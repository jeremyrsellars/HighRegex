using System;
using System.Collections.Generic;

namespace HighRegex.Engine
{
   /// <summary>
   /// Provides a ILookBackMatchProvider for any Regular Expression <c>IClass</c> object.
   /// </summary>
   /// <typeparam name="T">The type of item matched by a regular expression</typeparam>
   public class ClassLookBackMatchProvider<T> : ILookBackMatchProvider
   {
      /// <summary>
      /// Creates a new instance of <c>ClassLookBackMatchProvider</c> to expose <paramref name="class"/> for LookBehind.
      /// </summary>
      /// <param name="class">The class to expose to LookBehind.</param>
      public ClassLookBackMatchProvider (IClass<T> @class)
      {
         if (@class == null)
            throw new ArgumentNullException ("class");
         m_class = @class;
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
         get { return true; }
      }

      /// <summary>
      /// Gets a value indicating that no predictable patern can be determined for a pattern, so a lookbehind match could have any length.
      /// </summary>
      public bool AnyLength
      {
         get { return false; }
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
         yield return 1;
      }

      private IClass<T> m_class;
   }
}