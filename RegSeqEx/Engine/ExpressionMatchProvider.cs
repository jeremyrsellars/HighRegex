using System;
using System.Collections.Generic;
using System.Linq;

namespace RegSeqEx.Engine
{
   /// <summary>
   /// Provides a ILookBackMatchProvider for any Regular Expression <c>IExpression</c> object.
   /// </summary>
   /// <typeparam name="T">The type of item matched by a regular expression</typeparam>
   public class ExpressionMatchProvider<T> : IMatchProvider<T>, ILookBackMatchProvider
   {
      /// <summary>
      /// Creates an instance of <c>ExpressionMatchProvider</c> for the specified <paramref name="expression"/>.
      /// </summary>
      /// <param name="expression">The expression to expose to LookBehind.</param>
      public ExpressionMatchProvider (IExpression<T> expression)
      {
         if (expression == null)
            throw new ArgumentNullException ("expression");
         
         m_expression = expression;
         if (expression is ILookBackMatchProvider)
            m_lookBack = (ILookBackMatchProvider)m_expression;
         else if (expression is IClass<T>)
            m_lookBack = new ClassLookBackMatchProvider<T> ((IClass<T>)expression);
         else
            m_lookBack = NotSupportedLookBackMatchProvider.Value;
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
      /// A class will yield 0 or 1 result, while an AlternationExpression or CardinalityExpression
      /// could return several matches in the order they should be used.
      /// Performance note: The best performance will be achieved when backtracking is minimized.
      /// </remarks>
      public IEnumerable<Match<T>> GetMatches(IExpressionItemSource<T> input, int index)
      {
         int length;
         if (m_expression is IMatchProvider<T>)
         {
            IMatchProvider<T> matchProvider = (IMatchProvider<T>)m_expression;
            foreach (Match<T> match in matchProvider.GetMatches (input, index))
            {
               yield return match;
            }
         }
         else
         {
            var ml = m_expression.IsMatchAt(input, index);
            if (ml.Success)
               yield return new Match<T> (input, index, ml.Length, true);
         }
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
         get { return m_lookBack.SupportsLookBack; }
      }

      /// <summary>
      /// Gets a value indicating that no predictable patern can be determined for a pattern, so a lookbehind match could have any length.
      /// </summary>
      public bool AnyLength
      {
         get { return m_lookBack.AnyLength; }
      }

      /// <summary>
      /// Gets an Enumerable that iterates over the valid lengths of a match that are equal to or less than <paramref name="maxLength"/>.
      /// Only inputs of these lengths need to be tested, because length not returned would never match.
      /// </summary>
      /// <param name="maxLength">The maximum length match to return.</param>
      /// <returns>An enumerable of match lengths that the expression should be tested for a match, but does not include lengths that could never match per some cardinality constraint.</returns>
      /// <remarks>Implementations of <c>ILookBackMatchProvider</c> must return true from AnyLength or implement this method to be usefull.</remarks>
      public IEnumerable<int> GetPossibleMatchLengths(int maxLength)
      {
         return m_lookBack.GetPossibleMatchLengths (maxLength);
      }
      
      private IExpression<T> m_expression;
      private ILookBackMatchProvider m_lookBack;
   }
}