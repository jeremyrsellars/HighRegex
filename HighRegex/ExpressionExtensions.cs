using System;
using System.Collections.Generic;
using System.Linq;
using HighRegex.Engine;

namespace HighRegex
{
   /// <summary>
   /// The RegularExpression engine provides methods for determining whether input matches a pattern.
   /// </summary>
   public static class ExpressionEngine
   {
      /// <summary>
      /// Searches the specified input string for all occurrences of a regular expression, beginning at the specified starting position in the string.
      /// </summary>
      /// <typeparam name="T">The type of item matched by a regular expression</typeparam>
      /// <param name="expression">The RegularExpression pattern to match.</param>
      /// <param name="input">The sequence of items to be tested for a match.</param>
      /// <param name="startat">The character position in the input string at which to begin the search. </param>
      /// <returns>A <c>MatchCollection</c> of the <c>Match</c> objects found by the search.</returns>
      public static MatchCollection<T> Matches<T>(this IExpression<T> expression, IExpressionItemSource<T> input, int startat = 0)
      {
          var matches = GetMatches(expression, input, startat);
          var matchCollection = ToMatchValueCollection (matches);
          return matchCollection;
      }
      
      /// <summary>
      /// Searches an input sequence for an occurrence of a regular expression and returns the precise result as a single Match object.
      /// </summary>
      /// <typeparam name="T">The type of item matched by a regular expression.</typeparam>
      /// <param name="expression">The RegularExpression pattern to match.</param>
      /// <param name="input">The sequence of items to be tested for a match.</param>
      /// <param name="startat">The character position in the input string at which to begin the search. </param>
      /// <returns>A Regular Expression <c>Match</c> object.</returns>
      public static Match<T> Match<T> (this IExpression<T> expression, IExpressionItemSource<T> input, int startat = 0)
      {
         IMatchProvider<T> matchProvider = expression as IMatchProvider<T> ?? new ExpressionMatchProvider<T> (expression);
         Match<T> match = matchProvider.MatchesCore (input, startat).FirstOrDefault ();
         
         if (match == null)
            return new Match<T> (input, startat, 0, false);
         
         return match;
      }

      internal static void EnsureInputNotNull<T> (this IExpressionItemSource<T> input)
      {
         if (input == null)
            throw new ArgumentNullException ("input");
      }
      
      internal static IEnumerable<Match<T>> GetMatches<T> (this IExpression<T> expression, IExpressionItemSource<T> input, int startat)
      {
         IMatchProvider<T> matchProvider = expression as IMatchProvider<T> ?? new ExpressionMatchProvider<T> (expression);
         return GetMatches(matchProvider, input, startat);
      }
      
      internal static IEnumerable<Match<T>> GetMatches<T> (this IMatchProvider<T> expression, IExpressionItemSource<T> input, int startat)
      {
         input.EnsureInputNotNull ();
         
         return MatchesCore (expression, input, startat);
      }
      
      private static IEnumerable<Match<T>> MatchesCore<T> (this IMatchProvider<T> expression, IExpressionItemSource<T> input, int startat)
      {
         int index;

         // Try to use a count if we have one, otherwise, use IsItemInRange.
         int count = input is IListExpressionItemSource<T> ? ((IListExpressionItemSource<T>)input).Count : 0;

         // Go to just past the end of the list so we can match EndExpression.
         for (index = startat; index <= count || input.IsItemInRange (index); )
         {
            IEnumerable<Match<T>> matches = expression.GetMatches (input, index);
            Match<T> match = matches.FirstOrDefault ();
            
            if (match == null)
               index += 1;
            else if (match.Length == 0)
               index += 1;
            else
               index += match.Length;

            if (match != null)
               yield return match;
         }
         
         input.AssertIsIndexJustPastRange (index);
      }

      internal static void AssertIsIndexJustPastRange<T>(this IExpressionItemSource<T> input, int index)
      {
         if (!input.IsIndexJustPastRange (index))
            throw new InvalidOperationException ("An assertion has failed: index is not just past the end of input: " + index);
      }
      
      internal static MatchLength IsMatchAtCore<T> (this IMatchProvider<T> expression, IExpressionItemSource<T> input, int index)
      {
         if (expression == null)
            throw new ArgumentNullException ("expression");
         
         foreach (var match in expression.GetMatches (input, index))
         {
            if (match.Success) // Test shouldn't be necessary....
               return MatchLength.Of(match.Length);
         }
         
         return MatchLength.NoMatch;
      }
      
      /// <summary>
      /// Creates a <c>MatchCollection</c> from the matches in an enumerable.
      /// </summary>
      /// <typeparam name="T">The type of item matched by a regular expression</typeparam>
      /// <param name="enumerable">The Enumerable of matches to add to the <c>MatchCollection</c>.</param>
      /// <returns>A <c>MatchCollection</c>.</returns>
      public static MatchCollection<T> ToMatchValueCollection<T> (this IEnumerable<Match<T>> enumerable)
      {
         if (enumerable == null)
            throw new ArgumentNullException ("enumerable");
         
         MatchCollection<T> matches = new MatchCollection<T> ();
         foreach (var match in enumerable)
         {
            matches.Add (match);
         }
         
         matches.Lock ();
         return matches;
      }
      
      /// <summary>
      /// Gets the items matched in a MatchCollection as a List of <typeparamref name="T"/>.
      /// </summary>
      /// <typeparam name="T">The type of item matched by a regular expression</typeparam>
      /// <param name="matchCollection">A <c>MatchCollection</c> containing matched items.</param>
      /// <returns>A List of the matched items.</returns>
      public static List<T> ToList<T> (this MatchCollection<T> matchCollection)
      {
         if (matchCollection == null)
            throw new ArgumentNullException ("matchCollection");
         
         int length = 0;
         for (int i = 0; i < matchCollection.Count; i++)
         {
            length += matchCollection[i].Length;
         }
         
         List<T> list = new List<T> (length);
         for (int i = 0; i < matchCollection.Count; i++)
         {
            list.AddRange (matchCollection[i].Items);
         }
         
         return list;
      }

      internal static IEnumerable<Match<T>> GetMatches<T> (this IExpression<T> expression, IList<T> input, int startat)
      {
         IMatchProvider<T> matchProvider = expression as IMatchProvider<T> ?? new ExpressionMatchProvider<T> (expression);
         return GetMatches(matchProvider, new ListExpressionItemSource<T>(input), startat);
      }
   }
}
