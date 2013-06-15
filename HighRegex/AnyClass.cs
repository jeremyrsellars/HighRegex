using System;
using System.Collections.Generic;

namespace HighRegex
{
   /// <summary>
   /// Represents a regular expression that matches any single item.
   /// </summary>
   /// <typeparam name="T">The type of item matched by a regular expression</typeparam>
   public class AnyClass<T> : IClass<T>
   {
      /// <summary>
      /// Indicates whether the regular expression class matches the input.
      /// </summary>
      /// <param name="input">A single item.</param>
      /// <returns>true if the regular expression matches the input; otherwise false.</returns>
      public bool IsMatch (T input)
      {
         return true;
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
         input.EnsureInputNotNull ();

         T discard;

         return MatchLength.ClassMatchIf(input.TryGetItemAtIndex(index, out discard));
      }
   }
}
