using System;
using System.Collections.Generic;

namespace HighRegex.Engine
{
   /// <summary>
   /// Provides an IMatchProvider for regular expressions that do not provide any matches.
   /// </summary>
   /// <typeparam name="T">The type of item matched by a regular expression</typeparam>
   public class NullMatchProvider<T> : IMatchProvider<T>
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
      /// NullMatchProvider will yield 0 results.
      /// </remarks>
      public IEnumerable<Match<T>> GetMatches(IExpressionItemSource<T> input, int index)
      {
         T item;
         input.TryGetItemAtIndex (index, out item);
         
         return EmptyMatches;
      }
      
      /// <summary>
      /// Gets an empty IEnumerable of Matches.
      /// </summary>
      public static readonly IEnumerable<Match<T>> EmptyMatches = new Match<T> [0];
   }
}