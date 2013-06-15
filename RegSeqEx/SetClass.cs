namespace RegSeqEx
{
   /// <summary>
   /// Represents a regular expression that matches an item when any of the contained classes match the item.
   /// </summary>
   /// <typeparam name="T">The type of item matched by a regular expression</typeparam>
   public class SetClass<T> : IClass<T>
   {
      /// <summary>
      /// Creates a new instance of the SetClass that matches when any of the contained classes match the item.
      /// </summary>
      /// <param name="classes">The classes in the set.</param>
      public SetClass (params IClass<T> [] classes)
         : this (classes, false)
      {
      }

      /// <summary>
      /// Indicates whether the regular expression class matches the input.
      /// </summary>
      /// <param name="input">A single item.</param>
      /// <returns>true if the regular expression matches the input; otherwise false.</returns>
      public bool IsMatch (T input)
      {
         for (int i = 0; i < classes.Length; i++)
         {
            if (classes[i].IsMatch (input))
               return !negate;
         }
         return negate;
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
         T item;
         
         input.EnsureInputNotNull ();
         if (!input.TryGetItemAtIndex(index, out item))
            return MatchLength.NoMatch;

         return MatchLength.ClassMatchIf(IsMatch(item));
      }

      /// <summary>
      /// Creates a new instance of SetClass.
      /// </summary>
      /// <param name="classes">The classes in the set.</param>
      /// <param name="negate">Whether to negate the classes.</param>
      protected SetClass (IClass<T> [] classes, bool negate)
      {
         this.classes = classes;
         this.negate = negate;
      }

      readonly IClass<T> [] classes;
      readonly bool negate;
   }
}
