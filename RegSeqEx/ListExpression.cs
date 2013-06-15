using System;
using System.Collections.Generic;
using System.Linq;
using RegSeqEx.Engine;

namespace RegSeqEx
{
   /// <summary>
   /// Represents a regular expression capable of matching an ordered list of expressions.
   /// </summary>
   /// <typeparam name="T">The type of item matched by a regular expression</typeparam>
   public class ListExpression<T> : IExpression<T>, IMatchProvider<T>, ILookBackMatchProvider
   {
      /// <summary>
      /// Creates a new <c>ListExpression</c> that will match if <paramref name="first"/> matches and is immediately followed by <paramref name="second"/>.
      /// </summary>
      /// <param name="first">The first <c>IExpression</c> to match.</param>
      /// <param name="second">The <c>IExpression</c> that follows the first.</param>
      public ListExpression (IExpression<T> first, IExpression<T> second)
      {
         if (first == null)
            throw new ArgumentNullException ("first");
         if (second == null)
            throw new ArgumentNullException ("second");

         // We need IMatchProviders, so if each expression isn't one,
         // create a wrapper.
         m_expressions = new [] {
            first as IMatchProvider<T> ?? new ExpressionMatchProvider<T> (first),
            second as IMatchProvider<T> ?? new ExpressionMatchProvider<T> (second)};
      }
      
      /// <summary>
      /// Creates a new <c>ListExpression</c> that will match if all the <c>IExpression</c> objects in <paramref name="expressions"/> match in sequence.
      /// </summary>
      /// <param name="expressions">The ordered list of <c>IExpression</c>s that must match in sequence.</param>
      public ListExpression (params IExpression<T> [] expressions)
      {
         if (expressions == null)
            throw new ArgumentNullException ("expressions");

         if (expressions.Contains (null))
            throw new ArgumentNullException ("expressions");

         // We need IMatchProviders, so if each expression isn't one,
         // create a wrapper.
         m_expressions = new IMatchProvider<T> [expressions.Length];
         for (int i = 0; i < expressions.Length; i++)
         {
            var expression = expressions [i];
            m_expressions [i] = expression as IMatchProvider<T> ?? new ExpressionMatchProvider<T> (expression);
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
         get
         {
            if (m_supportsLookback.HasValue)
               return m_supportsLookback.Value;

            bool supportsLookback = true;
            for (int i = 0; i < m_expressions.Length; i++)
            {
               var expresssion = m_expressions[i] as ILookBackMatchProvider;
               
               if (expresssion == null || !expresssion.SupportsLookBack)
               {
                  supportsLookback = false;
                  break;
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
      protected IEnumerable<int> GetPossibleMatchLengthsCore(int maxLength)
      {
         // An empty ListExpression should return a zero-length match at the specified index.
         if (m_expressions.Length == 0)
         {
            yield return 0;
            yield break;
         }

         // We have N expressions, so we need to traverse the matches in this order:
         //
         // foreach (Match a in m_expressions[0].GetMatches(input, index))
         //    foreach (Match b in m_expressions[1].GetMatches(input, a.Index + a.Length))
         //       foreach (Match c in m_expressions[2].GetMatches(input, b.Index + b.Length))
         //          ...
         //          foreach (Match n in m_expressions[N].GetMatches(input, m.Index + m.Length))
         //             yield return new Match (input, index, someLength, true);
         //
         // So, our looping will be a bit unusual.  We'll use a stack to hold the enumerators
         // for each expression.  When an expression has match lengths, we'll work with the first match length,
         // then move on to the next expression in our list.
         
         // Stack stores enumerators for backtracking.  Each expression gets a position on the stack.
         Stack<IndexedEnumerator> stack = new Stack<IndexedEnumerator> (m_expressions.Length);
         // The enumerator of matches for the current expression in the stack.
         IEnumerator<int> enumerator = null;
         // The index to begin getting matches for the next expression in the list.
         int currentIndex = 0;

         for (;;)
         {
            // When evaluating an expression for the first time, 
            // get an enumerator of the matches for the expression at the current index.
            // Push it onto the stack.. we'll see if it matches anything.
            if (enumerator == null)
            {
               enumerator = GetLookBackAt (stack.Count).GetPossibleMatchLengths (maxLength - currentIndex).GetEnumerator ();
               stack.Push (new IndexedEnumerator {Enumerator=enumerator,Index=currentIndex});
            }
            
            // If the current expression matched anything
            if (enumerator.MoveNext ())
            {
               // Calculate the new currentIndex.
               var current = enumerator.Current;
               currentIndex = stack.Peek ().Index + current;
               
               // If we are evaluating the last expression in our list,
               // then we return a match...
               if (stack.Count == m_expressions.Length)
               {
                  // The match should include everything from index
                  // to the end of enumerator.Current.
                  yield return currentIndex;
               }
               else
               {
                  // Not the last one in the list, so continue (evaluate the next expression).
                  enumerator = null;
               }
            }
            else
            {
               // No more results for this expression, so backtrack to the previous expression.
               stack.Pop ();
               if (stack.Count == 0)
                  yield break;
               enumerator = stack.Peek ().Enumerator;
            }
         }
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
      /// ListExpression may return esveral matches in the order they should be used.
      /// Performance note: The best performance will be achieved when backtracking is minimized.
      /// </remarks>
      public IEnumerable<Match<T>> GetMatches(IExpressionItemSource<T> input, int index)
      {
         input.EnsureInputNotNull ();

         if (!input.IsItemInRange (index))
            return NullMatchProvider<T>.EmptyMatches;
         return GetMatchesCore (input, index);
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
      
      private IEnumerable<Match<T>> GetMatchesCore(IExpressionItemSource<T> input, int index)
      {
         // An empty ListExpression should return a zero-length match at the specified index.
         if (m_expressions.Length == 0)
         {
            yield return new Match<T> (input, index, 0, true);
            yield break;
         }

         // We have N expressions, so we need to traverse the matches in this order:
         //
         // foreach (Match a in m_expressions[0].GetMatches(input, index))
         //    foreach (Match b in m_expressions[1].GetMatches(input, a.Index + a.Length))
         //       foreach (Match c in m_expressions[2].GetMatches(input, b.Index + b.Length))
         //          ...
         //          foreach (Match n in m_expressions[N].GetMatches(input, m.Index + m.Length))
         //             yield return new Match (input, index, someLength, true);
         //
         // So, our looping will be a bit unusual.  We'll use a stack to hold the enumerators
         // for each expression.  When an expression has matches, we'll work with the first match,
         // then move on to the next expression in our list.
         
         // Stack stores enumerators for backtracking.  Each expression gets a position on the stack.
         Stack<IEnumerator<Match<T>>> stack = new Stack<IEnumerator<Match<T>>> (m_expressions.Length);
         // The enumerator of matches for the current expression in the stack.
         IEnumerator<Match<T>> enumerator = null;
         // The index to begin getting matches for the next expression in the list.
         int currentIndex = index;

         for (;;)
         {
            // When evaluating an expression for the first time, 
            // get an enumerator of the matches for the expression at the current index.
            // Push it onto the stack.. we'll see if it matches anything.
            if (enumerator == null)
            {
               enumerator = m_expressions [stack.Count].GetMatches (input, currentIndex).GetEnumerator ();
               stack.Push (enumerator);
            }
            
            // If the current expression matched anything
            if (enumerator.MoveNext ())
            {
               // Calculate the new currentIndex.
               var current = enumerator.Current;
               currentIndex = current.Index + current.Length;
               
               // If we are evaluating the last expression in our list,
               // then we return a match...
               if (stack.Count == m_expressions.Length)
               {
                  // The match should include everything from index
                  // to the end of enumerator.Current.
                  int length = current.Index - index + current.Length;
                  yield return new Match<T> (input, index, length, true);
               }
               else
               {
                  // Not the last one in the list, so continue (evaluate the next expression).
                  enumerator = null;
               }
            }
            else
            {
               // No more results for this expression, so backtrack to the previous expression.
               stack.Pop ();
               if (stack.Count == 0)
                  yield break;
               enumerator = stack.Peek ();
            }
         }
      }

      private class IndexedEnumerator
      {
         /// <summary>
         /// The index for which this enumerator was made.  (So maxLength - Index is the remaining space).
         /// </summary>
         public int Index;
         //public bool ReturnedZero;
         public IEnumerator<int> Enumerator;
      }
      
      private ILookBackMatchProvider GetLookBackAt (int index)
      {
         if (m_lookbacks == null)
            m_lookbacks = new ILookBackMatchProvider [m_expressions.Length];

         if (m_lookbacks[index] != null)
            return m_lookbacks[index];

         ILookBackMatchProvider provider = 
            m_expressions[index] as ILookBackMatchProvider ?? NotSupportedLookBackMatchProvider.Value;

         m_lookbacks[index] = provider;
         return provider;
      }
      
      private IMatchProvider<T> [] m_expressions;
      private ILookBackMatchProvider [] m_lookbacks;

      private bool? m_supportsLookback;
      private bool? m_anyLength;
   }
}
