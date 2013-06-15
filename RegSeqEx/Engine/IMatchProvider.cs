using System;
using System.Collections.Generic;

namespace RegSeqEx.Engine
{
   /// <summary>
   /// Provides matches to the Regular Expression engine for input at a specific index.
   /// </summary>
   /// <typeparam name="T">The type of item matched by a regular expression</typeparam>
   public interface IMatchProvider<T>
   {
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
      IEnumerable<Match<T>> GetMatches (IExpressionItemSource<T> input, int index);
   }
}