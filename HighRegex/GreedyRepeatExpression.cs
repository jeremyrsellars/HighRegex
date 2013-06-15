using System;
using System.Collections.Generic;
using HighRegex.Engine;

namespace HighRegex
{
   /// <summary>
   /// Represents multiple consecutive occurrences of the contained regular expression.  Matches as many as possible.
   /// </summary>
   /// <typeparam name="T">The type of item matched by a regular expression</typeparam>
   public class GreedyRepeatExpression<T> : RepeatExpression<T>, IExpression<T>, IMatchProvider<T>
   {
      /// <summary>
      /// Creates a <c>GreedyRepeatExpression</c> that matches as many consecutive occurrences of <paramref name="expression"/> as possible.
      /// Any number of matches from 0 to Int32.MaxValue are sufficient to match.
      /// </summary>
      /// <param name="expression">The expression to match.</param>
      public GreedyRepeatExpression (IExpression<T> expression)
         : this (expression, MINIMUM, MAXIMUM)
      {
      }

      /// <summary>
      /// Creates a <c>GreedyRepeatExpression</c> that matches <paramref name="repititionCount"/> consecutive occurrences of <paramref name="expression"/>.
      /// </summary>
      /// <param name="expression">The expression to match.</param>
      /// <param name="repititionCount">The exact number of repititions required.</param>
      public GreedyRepeatExpression (IExpression<T> expression, int repititionCount)
         : this (expression, repititionCount, repititionCount)
      {
      }

      /// <summary>
      /// Creates a <c>GreedyRepeatExpression</c> that matches as many consecutive occurrences of <paramref name="expression"/> as possible.
      /// Any number of matches between <paramref name="minimum"/> and <paramref name="maximum"/> are sufficient to match.
      /// </summary>
      /// <param name="expression">The expression to match.</param>
      /// <param name="minimum">The least number of consecutive occurrences of <paramref name="expression"/> to match.</param>
      /// <param name="maximum">The greatest number of consecutive occurrences of <paramref name="expression"/> to match.</param>
      public GreedyRepeatExpression (IExpression<T> expression, int minimum, int maximum)
         : base (expression, minimum, maximum)
      {
      }

      /// <summary>
      /// Indicates whether the regular expression finds a match in the input beginning at the specified index.
      /// Indexes after <paramref name="index"/> will not be search.
      /// </summary>
      /// <param name="input">The items to search for a match</param>
      /// <param name="index">The index at which the expression must match.</param>
      /// <returns>A value indicating whether the regular expression matches the input at the specified index.</returns>
      protected override IEnumerable<Match<T>> GetMatchesCore(IExpressionItemSource<T> input, int index)
      {
         return new Enumerable (this, input, index);
      }

      internal class Enumerable : IEnumerable<Match<T>> 
      {
         internal Enumerable (GreedyRepeatExpression<T> expression, IExpressionItemSource<T> input, int index)
         {
            m_greedy = expression;
            m_input = input;
            m_index = index;
         }
         
         public IEnumerator<Match<T>> GetEnumerator()
         {
            return new Enumerator (this);
         }

         System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
         {
            return GetEnumerator ();
         }

         internal readonly GreedyRepeatExpression<T> m_greedy;
         internal readonly IExpressionItemSource<T> m_input;
         internal readonly int m_index;
      }
      
      internal class Enumerator : IEnumerator<Match<T>> 
      {
         internal Enumerator (Enumerable enumerable)
         {
            m_enumerable = enumerable;
            m_stack = new Stack<IEnumerator<Match<T>>> ();
            Reset ();
         }

         public Match<T> Current
         {
            get
            {
               return m_current;
            }
         }

         void IDisposable.Dispose()
         {
         }

         object System.Collections.IEnumerator.Current
         {
            get { return Current; }
         }

         public void Reset()
         {
            m_stack.Clear ();
            m_current = null;
            m_currentIndex = m_enumerable.m_index;
            m_state = EnumState.More;
         }

         public bool MoveNext()
         {
            for (;;)
            {
               switch (m_state)
               {
               case EnumState.More:
               {
                  while (m_stack.Count < m_enumerable.m_greedy.Maximum && More ())
                  {
                     // Continue;
                  }
                  int count = m_stack.Count;
                  int min = m_enumerable.m_greedy.Minimum;
                  if (count == 0)
                  {
                     if (min == 0)
                        m_state = EnumState.YieldEmpty;
                     else
                        m_state = EnumState.Complete;
                  }
                  else if (count >= min)
                  {
                     YieldReturn ();
                     return true;
                  }
                  else
                  {
                     m_state = EnumState.MoveNext;
                  }
                  break;
               }
               case EnumState.MoveNext:
               {
                  if (m_stack.Peek ().MoveNext ())
                  {
                     var current = m_stack.Peek ().Current;
                     m_currentIndex = current.Index + current.Length;
                     m_state = EnumState.More;
                     break;
                  }

                  m_stack.Pop ();
                  int count = m_stack.Count;
                  if (count > 0)
                  {
                     if (count >= m_enumerable.m_greedy.Minimum)
                     {
                        YieldReturn ();
                        return true;
                     }
                     // else state still equals MoveNext.
                     break;
                  }
                  else // if (m_stack.Count == 0)
                  {
                     int min = m_enumerable.m_greedy.Minimum;
                     if (min == 0)
                        m_state = EnumState.YieldEmpty;
                     else
                        m_state = EnumState.Complete;
                     break;
                  }
               }
               case EnumState.YieldEmpty:
                  YieldEmpty ();
                  return true;

               case EnumState.Complete:
                  return false;

               default:
                  throw new NotImplementedException (m_state.ToString ());
               }
            }
         }

         /// <summary>
         /// Advances one iteration and returns true, or returns false if the expression didn't match again.
         /// </summary>
         /// <returns></returns>
         private bool More ()
         {
            var enumerator = m_enumerable.m_greedy.m_expression.GetMatches (m_enumerable.m_input, m_currentIndex).GetEnumerator ();
            if (enumerator.MoveNext ())
            {
               m_stack.Push (enumerator);
               var current = enumerator.Current;
               m_currentIndex = current.Index + current.Length;
               return true;
            }
            return false;
         }
         
         private void YieldEmpty ()
         {
            int length = 0;
            m_current = new Match<T> (m_enumerable.m_input, m_enumerable.m_index, length, true);
            m_state = EnumState.Complete;
         }
         
         /// <summary>
         /// Returns a match for the match at the top of stack and sets the state to MoveNext.
         /// </summary>
         /// <returns></returns>
         private void YieldReturn ()
         {
            // The match should include everything from index
            // to the end of enumerator.Current.
            var current = m_stack.Peek ().Current;
            int length = current.Index - m_enumerable.m_index + current.Length;
            m_current = new Match<T> (m_enumerable.m_input, m_enumerable.m_index, length, true);
            m_state = EnumState.MoveNext;
         }
         
         private readonly Enumerable m_enumerable;
         private readonly Stack<IEnumerator<Match<T>>> m_stack;
         
         private Match<T> m_current;
         private int m_currentIndex;
         private EnumState m_state;
         
      }

      internal enum EnumState
      {
         More,
         MoveNext,
         TooMany,
         NotEnough,
         YieldEmpty,
         Complete,
      }

#if false
      /// <summary>
      /// Gets an Enumerable that iterates over the valid lengths of a match that are equal to or less than <paramref name="maxLength"/>.
      /// Only inputs of these lengths need to be tested, because length not returned would never match.
      /// </summary>
      /// <param name="maxLength">The maximum length match to return.</param>
      /// <returns>An enumerable of match lengths that the expression should be tested for a match, but does not include lengths that could never match per some cardinality constraint.</returns>
      /// <remarks>Implementations of <c>ILookBackMatchProvider</c> must return true from AnyLength or implement this method to be usefull.</remarks>
      protected override IEnumerable<int> GetPossibleMatchLengthsCore(int maxLength)
      {
         return new LengthEnumerable (this, maxLength);
      }
      
      internal class LengthEnumerable : IEnumerable<int> 
      {
         internal LengthEnumerable (GreedyRepeatExpression<T> expression, int maxLength)
         {
            m_greedy = expression;
            m_maxLength = maxLength;
         }
         
         public IEnumerator<int> GetEnumerator()
         {
            return new LengthEnumerator (this);
         }

         System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
         {
            return GetEnumerator ();
         }

         internal readonly GreedyRepeatExpression<T> m_greedy;
         internal readonly int m_maxLength;
      }
      
      internal class LengthEnumerator : IEnumerator<int> 
      {
         internal LengthEnumerator (LengthEnumerable enumerable)
         {
            m_enumerable = enumerable;
            m_stack = new Stack<IndexedEnumerator> ();
            m_expression = (ILookBackMatchProvider) enumerable.m_greedy.m_expression;
            Reset ();
         }

         public int Current
         {
            get
            {
               return m_current;
            }
         }

         void IDisposable.Dispose()
         {
         }

         object System.Collections.IEnumerator.Current
         {
            get { return Current; }
         }

         public void Reset()
         {
            m_stack.Clear ();
            m_current = -1;
            m_currentIndex = 0;
            m_state = EnumState.More;
            m_returnedZero = false;
         }

         public bool MoveNext()
         {
            for (;;)
            {
               switch (m_state)
               {
               case EnumState.More:
               {
                  bool skipZero = false;
                  while (m_stack.Count < m_enumerable.m_greedy.Maximum && More (ref skipZero))
                  {
                     // Continue;
                  }
                  int count = m_stack.Count;
                  int min = m_enumerable.m_greedy.Minimum;
                  if (count == 0)
                  {
                     if (min == 0)
                        m_state = EnumState.YieldEmpty;
                     else
                        m_state = EnumState.Complete;
                  }
                  else if (count >= min)
                  {
                     YieldReturn ();
                     return true;
                  }
                  else
                  {
                     m_state = EnumState.MoveNext;
                  }
                  break;
               }
               case EnumState.MoveNext:
               {
                  if (m_stack.Peek ().Enumerator.MoveNext ())
                  {
                     var ie = m_stack.Peek ();
                     var current = ie.Enumerator.Current;
                     m_currentIndex = ie.Index + current;
                     m_state = EnumState.More;
                     break;
                  }

                  m_stack.Pop ();
                  int count = m_stack.Count;
                  if (count > 0)
                  {
                     if (count >= m_enumerable.m_greedy.Minimum)
                     {
                        YieldReturn ();
                        return true;
                     }
                     // else state still equals MoveNext.
                     break;
                  }
                  else // if (m_stack.Count == 0)
                  {
                     int min = m_enumerable.m_greedy.Minimum;
                     if (min == 0)
                        m_state = EnumState.YieldEmpty;
                     else
                        m_state = EnumState.Complete;
                     break;
                  }
               }
               case EnumState.YieldEmpty:
                  YieldEmpty ();
                  return true;

               case EnumState.Complete:
                  return false;

               default:
                  throw new NotImplementedException (m_state.ToString ());
               }
            }
         }

         /// <summary>
         /// Advances one iteration and returns true, or returns false if the expression didn't match again.
         /// </summary>
         /// <returns></returns>
         private bool More (ref bool skipZero)
         {
            var enumerator = m_expression.GetPossibleMatchLengths (m_enumerable.m_maxLength - m_currentIndex).GetEnumerator ();
            bool movedNext;
            while ((movedNext = enumerator.MoveNext ()) && (skipZero |= enumerator.Current == 0) && (m_returnedZero || skipZero))
               skipZero = true;
            if (movedNext)
            {
               m_stack.Push (new IndexedEnumerator{Index=m_currentIndex, Enumerator = enumerator});
               var current = enumerator.Current;
               if (current == 0)
                  m_stack.Peek ().ReturnedZero = true;
               m_currentIndex += current;
               return true;
            }
            return false;
         }
         
         private void YieldEmpty ()
         {
            int length = 0;
            m_current = length;
            m_state = EnumState.Complete;
         }
         
         /// <summary>
         /// Returns a match for the match at the top of stack and sets the state to MoveNext.
         /// </summary>
         /// <returns></returns>
         private void YieldReturn ()
         {
            // The match should include everything from index
            // to the end of enumerator.Current.
            var ie = m_stack.Peek ();
            int length = ie.Enumerator.Current;
            m_current = length;
            if (length == 0)
               m_returnedZero = true;
            m_state = EnumState.MoveNext;
         }
         
         private readonly LengthEnumerable m_enumerable;
         private readonly ILookBackMatchProvider m_expression;
         private readonly Stack<IndexedEnumerator> m_stack;
         
         private int m_current;
         private int m_currentIndex;
         private EnumState m_state;
         private bool m_returnedZero;
         
      }
#endif
   }
}
