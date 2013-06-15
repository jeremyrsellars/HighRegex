using System;
using System.Collections.Generic;
using System.Linq;
using RegSeqEx.Engine;

namespace RegSeqEx
{
   /// <summary>
   /// Represents multiples of the contained regular expression.  Matches as few as possible.
   /// </summary>
   /// <typeparam name="T">The type of item matched by a regular expression</typeparam>
   public class RepeatExpression<T> : IExpression<T>, IMatchProvider<T>, ILookBackMatchProvider
   {
      /// <summary>
      /// Creates a lazy <c>RepeatExpression</c> that matches as few consecutive occurrences of <paramref name="expression"/> as possible.
      /// Any number of matches from 0 to Int32.MaxValue are sufficient to match.
      /// </summary>
      /// <param name="expression">The expression to match.</param>
      public RepeatExpression (IExpression<T> expression)
         : this (expression, MINIMUM, MAXIMUM)
      {
      }

      /// <summary>
      /// Creates a lazy <c>RepeatExpression</c> that matches <paramref name="repititionCount"/> consecutive occurrences of <paramref name="expression"/>.
      /// </summary>
      /// <param name="expression">The expression to match.</param>
      /// <param name="repititionCount">The exact number of repititions required.</param>
      public RepeatExpression (IExpression<T> expression, int repititionCount)
         : this (expression, repititionCount, repititionCount)
      {
      }

      /// <summary>
      /// Creates a lazy <c>RepeatExpression</c> that matches as few consecutive occurrences of <paramref name="expression"/> as possible.
      /// Any number of matches between <paramref name="minimum"/> and <paramref name="maximum"/> are sufficient to match.
      /// </summary>
      /// <param name="expression">The expression to match.</param>
      /// <param name="minimum">The least number of consecutive occurrences of <paramref name="expression"/> to match.</param>
      /// <param name="maximum">The greatest number of consecutive occurrences of <paramref name="expression"/> to match.</param>
      public RepeatExpression (IExpression<T> expression, int minimum, int maximum)
      {
         if (expression == null)
            throw new ArgumentNullException ("expression");
         
         if (minimum < MINIMUM)
            throw new ArgumentOutOfRangeException ("minimum", minimum, "Minimum must be at least 0.");
         if (maximum < MINIMUM)
            throw new ArgumentOutOfRangeException ("maximum", maximum, "Maximum must be at least 0.");
         
         if (maximum < minimum)
            throw new ArgumentException ("maximum" , "Maximum must be at least 0.");
         
         Minimum = minimum;
         Maximum = maximum;
         m_expression = expression as IMatchProvider<T> ?? new ExpressionMatchProvider<T> (expression);
      }

      /// <summary>
      /// The least number of consecutive occurrences of the contained expression to match.
      /// </summary>
      public int Minimum{get;private set;}

      /// <summary>
      /// The greatest number of consecutive occurrences of the contained expression to match.
      /// </summary>
      public int Maximum{get;private set;}

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
      /// RepeatExpression could return several matches in the order they should be used.
      /// Performance note: The best performance will be achieved when backtracking is minimized.
      /// </remarks>
      public IEnumerable<Match<T>> GetMatches(IExpressionItemSource<T> input, int index)
      {
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
      
      internal const int MINIMUM = 0;
      internal const int MAXIMUM = int.MaxValue;

      /// <summary>
      /// Indicates whether the regular expression finds a match in the input beginning at the specified index.
      /// Indexes after <paramref name="index"/> will not be search.
      /// </summary>
      /// <param name="input">The items to search for a match</param>
      /// <param name="index">The index at which the expression must match.</param>
      /// <returns>A value indicating whether the regular expression matches the input at the specified index.</returns>
      protected virtual IEnumerable<Match<T>> GetMatchesCore(IExpressionItemSource<T> input, int index)
      {
         if (Minimum == 0)
            yield return new Match<T> (input, index, 0, true);
         
         // We have N expressions, so we need to traverse the matches in this order:
         //
         // foreach (Match a in m_expression.GetMatches(input, index))
         //    foreach (Match b in m_expression.GetMatches(input, a.Index + a.Length))
         //       foreach (Match c in m_expression.GetMatches(input, b.Index + b.Length))
         //          ...
         //          foreach (Match n in m_expression.GetMatches(input, m.Index + m.Length))
         //             yield return new Match (input, index, someLength, true);
         //
         // So, our looping will be a bit unusual.  We'll use a stack to hold the enumerators
         // for each expression.  When an expression has matches, we'll work with the first match,
         // then move on to the next expression in our list.
         
         // Stack stores enumerators for backtracking.  Each expression gets a position on the stack.
         Stack<IEnumerator<Match<T>>> stack = new Stack<IEnumerator<Match<T>>> ();
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
               enumerator = m_expression.GetMatches (input, currentIndex).GetEnumerator ();
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
               if (stack.Count >= Minimum && stack.Count <= Maximum)
               {
                  // The match should include everything from index
                  // to the end of enumerator.Current.
                  int length = current.Index - index + current.Length;
                  yield return new Match<T> (input, index, length, true);
               }
               if (stack.Count < Maximum)
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

      /// <summary>
      /// An IMatchProvider that wraps the contained Expression.
      /// </summary>
      protected readonly IMatchProvider<T> m_expression;

      #region ILookBackMatchProvider Members

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
            var expresssion = m_expression as ILookBackMatchProvider;
            
            if (expresssion == null || !expresssion.SupportsLookBack)
            {
               supportsLookback = false;
               return false;
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
            var expresssion = m_expression as ILookBackMatchProvider;
            
            if (expresssion == null || expresssion.AnyLength)
            {
               anyLength = true;
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
         if (Maximum == 0)
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
      protected virtual IEnumerable<int> GetPossibleMatchLengthsCore(int maxLength)
      {
         bool returnedZero = false;
         if (Minimum == 0)
         {
            yield return 0;
            returnedZero = true;
         }
         
         ILookBackMatchProvider expression = (ILookBackMatchProvider)m_expression;
         
         // We have N expressions, so we need to traverse the matches in this order:
         //
         // foreach (Match a in m_expression.GetMatches(input, index))
         //    foreach (Match b in m_expression.GetMatches(input, a.Index + a.Length))
         //       foreach (Match c in m_expression.GetMatches(input, b.Index + b.Length))
         //          ...
         //          foreach (Match n in m_expression.GetMatches(input, m.Index + m.Length))
         //             yield return new Match (input, index, someLength, true);
         //
         // So, our looping will be a bit unusual.  We'll use a stack to hold the enumerators
         // for each expression.  When an expression has matches, we'll work with the first match,
         // then move on to the next expression in our list.
         
         // Stack stores enumerators for backtracking.  Each expression gets a position on the stack.
         Stack<IndexedEnumerator> stack = new Stack<IndexedEnumerator> ();
         // The enumerator of matches for the current expression in the stack.
         IEnumerator<int> enumerator = null;
         // The index to begin getting matches for the next expression in the list.
         int currentIndex = 0;

         for (;;)
         {
            // When evaluating an expression for the first time, 
            // get an enumerator of the match lengths for the expression at the current index.
            // Push it onto the stack.. we'll see if it has any match lengths.
            if (enumerator == null)
            {
               enumerator = expression.GetPossibleMatchLengths (maxLength - currentIndex).GetEnumerator ();
               stack.Push (new IndexedEnumerator{Index=currentIndex, Enumerator = enumerator});
            }
            
            // If the current expression matched anything
            bool didMoveNext;
            while ((didMoveNext = enumerator.MoveNext ()) && ((returnedZero || stack.Peek ().ReturnedZero) && enumerator.Current == 0))
               ;
            if (didMoveNext)
            {
               // Calculate the new currentIndex.
               var current = enumerator.Current;
               if (current==0)
                  stack.Peek ().ReturnedZero = true;
               currentIndex = stack.Peek ().Index + current;
               
               // If we are evaluating the last expression in our list,
               // then we return a match...
               if (currentIndex <= maxLength)
               {
                  if (stack.Count >= Minimum && stack.Count <= Maximum)
                  {
                     if (currentIndex == 0)
                     {
                        if (!returnedZero)
                        {
                           returnedZero = true;
                           yield return 0;
                        }
                     }
                     else
                     {
                        yield return currentIndex;
                     }
                  }
                  if (stack.Count < Maximum)
                  {
                     // Not the last one in the list, so continue (evaluate the next expression).
                     enumerator = null;
                  }
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
      
      private class IndexedEnumerator
      {
         /// <summary>
         /// The index for which this enumerator was made.  (So maxLength - Index is the remaining space).
         /// </summary>
         public int Index;
         public bool ReturnedZero;
         public IEnumerator<int> Enumerator;
      }
      

      #endregion
      private bool? m_supportsLookback;
      private bool? m_anyLength;
   }
}
