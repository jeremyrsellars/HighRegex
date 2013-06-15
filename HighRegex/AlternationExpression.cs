using System;
using System.Collections.Generic;
using System.Linq;
using HighRegex.Engine;

namespace HighRegex
{
   /// <summary>
   /// A single regular expression that matches one of several possible regular expressions.
   /// </summary>
   /// <typeparam name="T">The type of item matched by a regular expression</typeparam>
   public class AlternationExpression<T> : IExpression<T>, IMatchProvider<T>, ILookBackMatchProvider
   {
      /// <summary>
      /// Creates an <c>AlternationExpression</c> that <paramref name="first"/> attempts to match, then if that does not succeed, matches <paramref name="second"/>.
      /// </summary>
      /// <param name="first">The first Regular Expression pattern to match.</param>
      /// <param name="second">The first Regular Expression pattern to match.</param>
      public AlternationExpression (IExpression<T> first, IExpression<T> second)
      {
         if (first == null)
            throw new ArgumentNullException ("first");
         if (second == null)
            throw new ArgumentNullException ("second");

         m_expressions = new [] {first, second};
      }
      
      /// <summary>
      /// Creates an <c>AlternationExpression</c> that attempts to match any one of the <paramref name="expressions"/> in the order provided.
      /// </summary>
      /// <param name="expressions">The Regular Expression patterns to match in the order to be checked.</param>
      public AlternationExpression (params IExpression<T> [] expressions)
      {
         if (expressions == null)
            throw new ArgumentNullException ("expressions");

         if (expressions.Contains (null))
            throw new ArgumentNullException ("expressions");

         // Clone the list so the caller can independently modify their array
         // in case they call without using the params semantics.
         m_expressions = (IExpression<T> []) expressions.Clone ();
      }

      /// <summary>
      /// Gets a value indicating whether the <c>ILookBackMatchProvider</c> can be used to find a match that preceeds the current index.
      /// </summary>
      /// <remarks>
      /// If an expression that would otherwise support lookback is comprised of an expression
      /// that does not support lookback, this returns false.
      /// </remarks>
      public bool SupportsLookBack
      {
         get
         {
            if (m_supportsLookback.HasValue)
               return m_supportsLookback.Value;

            bool supportsLookback = true;
            for (int i = 0; i < m_expressions.Length; i++)
            {
               var expresssion = m_expressions[i] as ILookBackMatchProvider;
               if (expresssion == null && m_expressions[i] is IClass<T>)
                  expresssion = new ClassLookBackMatchProvider<T> ((IClass<T>)m_expressions[i]);
               
               if (expresssion == null || !expresssion.SupportsLookBack)
               {
                  supportsLookback = false;
                  return false;
               }
            }

            m_supportsLookback = supportsLookback;
            return supportsLookback;
         }
      }

      /// <summary>
      /// Gets a value indicating that no predictable patern can be determined for a pattern, so a lookbehind match could have any length.
      /// </summary>
      public bool AnyLength
      {
         get
         {
            if (m_anyLength.HasValue)
               return m_anyLength.Value;

            bool anyLength = false;
            for (int i = 0; i < m_expressions.Length; i++)
            {
               var expresssion = m_expressions[i] as ILookBackMatchProvider;
               
               if (expresssion == null || expresssion.AnyLength)
               {
                  anyLength = true;
                  break;
               }
            }

            m_anyLength = anyLength;
            return anyLength;
         }
      }

      /// <summary>
      /// Gets an Enumerable that iterates over the valid lengths of a match that are equal to or less than <paramref name="maxLength"/>.
      /// Only inputs of these lengths need to be tested, because length not returned would never match.
      /// </summary>
      /// <param name="maxLength">The maximum length match to return.</param>
      /// <returns>An enumerable of match lengths that the expression should be tested for a match, but does not include lengths that could never match per some cardinality constraint.</returns>
      /// <remarks>Implementations of <c>ILookBackMatchProvider</c> must return true from AnyLength or implement this method to be usefull.</remarks>
      public IEnumerable<int> GetPossibleMatchLengths (int maxLength)
      {
         if (!SupportsLookBack)
            return AnyLengthMatchProvider.NoMatches;
         if (AnyLength)
            return AnyLengthMatchProvider.GetAscending (maxLength);
         if (m_expressions.Length == 0)
            return AnyLengthMatchProvider.GetAscending (0);
         return GetPossibleMatchLengthsCore (maxLength).Distinct ();
      }

      /// <summary>
      /// Gets an Enumerable that iterates over the valid lengths of a match that are equal to or less than <paramref name="maxLength"/>.
      /// Only inputs of these lengths need to be tested, because length not returned would never match.
      /// SupportsLookBack and AnyLength have already been considered by the base class.
      /// </summary>
      /// <param name="maxLength">The maximum length match to return.</param>
      /// <returns>An enumerable of match lengths that the expression should be tested for a match, but does not include lengths that could never match per some cardinality constraint.</returns>
      /// <remarks>Implementations of <c>ILookBackMatchProvider</c> must return true from AnyLength or implement this method to be usefull.</remarks>
      private IEnumerable<int> GetPossibleMatchLengthsCore(int maxLength)
      {
         foreach (var expression in m_expressions.Cast<ILookBackMatchProvider> ())
         {
            foreach (int length in expression.GetPossibleMatchLengths (maxLength))
               yield return length;
         }
      }
      
      /// <summary>
      /// Indicates whether the regular expression finds a match in the input beginning at the specified index.
      /// Indexes after <paramref name="index"/> will not be search.
      /// </summary>
      /// <param name="input">The items to search for a match</param>
      /// <param name="index">The index at which the expression must match.</param>
      /// <returns>A value indicating whether the regular expression matches the input at the specified index.</returns>
      /// <remarks>
      /// This interface is usually not used called directly, but rather used
      /// by the extension methods defined in ExpressionExtensions.
      /// To find matches at indexes after <paramref name="index"/>, use ExpressionExtensions.Matches
      /// or ExpressionExtensions.Match.
      /// </remarks>
      public MatchLength IsMatchAt (IExpressionItemSource<T> input, int index)
      {
         return this.IsMatchAtCore(input, index);
      }
      
      /// <summary>
      /// Gets matches that begin at the specified index.  The matches are returned in the order of preference.
      /// The only reason the enumerator.MoveNext should be called more than once is if the entire expression
      /// is not a match for the first result, resulting in back-tracking.  Backtracking may call MoveNext again 
      /// find other options.
      /// </summary>
      /// <param name="input">The items to search for a match</param>
      /// <param name="index">The index at which the expression must match.</param>
      /// <returns>Matches that begin at the specified index.</returns>
      /// <remarks>
      /// AlternationExpression could return several matches in the order they should be used.
      /// Performance note: The best performance will be achieved when backtracking is minimized.
      /// </remarks>
      public IEnumerable<Match<T>> GetMatches(IExpressionItemSource<T> input, int index)
      {
         input.EnsureInputNotNull ();
         if (!input.IsItemInRange (index))
            return NullMatchProvider<T>.EmptyMatches;
         return GetMatchesCore (input, index);
      }

      private IEnumerable<Match<T>> GetMatchesCore(IExpressionItemSource<T> input, int index)
      {
         int length;

         if (m_expressions.Length == 0)
         {
            yield return new Match<T> (input, index, 0, true);
            yield break;
         }
         
         for (int i = 0; i < m_expressions.Length; i++)
         {
            var expression = m_expressions[i];
            if (expression is IMatchProvider<T>)
            {
               IMatchProvider<T> matchProvider = (IMatchProvider<T>)expression;
               foreach (Match<T> match in matchProvider.GetMatches (input, index))
               {
                  yield return match;
               }
            }
            else
            {
               var ml = expression.IsMatchAt(input, index);
               if (ml.Success)
                  yield return new Match<T> (input, index, ml.Length, true);
            }
         }
      }
      
      private IExpression<T> [] m_expressions;
      private bool? m_supportsLookback;
      private bool? m_anyLength;
   }
}
