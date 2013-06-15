using System;

namespace RegSeqEx
{
   /// <summary>
   /// Represents a class that negates the contained regular expression class.
   /// </summary>
   /// <typeparam name="T">The type of item matched by a regular expression</typeparam>
   public class NotClass<T> : IClass<T>
   {
      /// <summary>
      /// Creates a new instance of NotClass to negate the specified RegularExpression class.
      /// </summary>
      /// <param name="class">The RegularExpression class to negate</param>
      public NotClass (IClass<T> @class)
      {
         if (@class == null)
            throw new ArgumentNullException ("class");
         this.@class = @class;
      }
      
      /// <summary>
      /// Indicates whether the regular expression class matches the input.
      /// </summary>
      /// <param name="input">A single item.</param>
      /// <returns>true if the regular expression matches the input; otherwise false.</returns>
      public bool IsMatch(T input)
      {
         bool isMatch = @class.IsMatch (input);
         return !isMatch;
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
      public MatchLength IsMatchAt(IExpressionItemSource<T> input, int index)
      {
         T item;
         
         input.EnsureInputNotNull ();
         if (!input.TryGetItemAtIndex(index, out item))
            return MatchLength.NoMatch;

         return MatchLength.ClassMatchIf(IsMatch(item));
      }
      
      readonly IClass<T> @class;
   }
}
