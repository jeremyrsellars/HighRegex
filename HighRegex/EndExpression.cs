using System.Collections.Generic;
using HighRegex.Engine;

namespace HighRegex
{
   /// <summary>
   /// A zero-width assertion that matches the start of the input.
   /// </summary>
   /// <typeparam name="T">The type of item matched by a regular expression</typeparam>
   public class EndExpression<T> : IExpression<T>, IMatchProvider<T>, ILookBackMatchProvider
   {
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
      public MatchLength IsMatchAt(IExpressionItemSource<T> input, int index)
      {
         input.EnsureInputNotNull ();
         
         return MatchLength.EmptyMatch(input.IsAtEnd (index));
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
      /// EndExpression will yield 0 or 1 result.
      /// </remarks>
      public IEnumerable<Match<T>> GetMatches(IExpressionItemSource<T> input, int index)
      {
         var ml = IsMatchAt (input, index);
         if (!ml.Success)
            return EmptyEnumerable;

         Match<T> match = new Match<T> (input, index, ml.Length, true);
         return new [] {match};
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
            return true;
         }
      }

      /// <summary>
      /// Gets a value indicating that no predictable patern can be determined for a pattern, so a lookbehind match could have any length.
      /// </summary>
      public bool AnyLength
      {
         get
         {
            return false;
         }
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
         yield return 0;
      }

      static readonly IEnumerable<Match<T>> EmptyEnumerable = new Match<T> [0];
   }
}
