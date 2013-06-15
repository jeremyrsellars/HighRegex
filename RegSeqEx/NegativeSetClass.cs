using System;
using System.Collections.Generic;

namespace RegSeqEx
{
   /// <summary>
   /// Represents a regular expression class that matches when none of the contained classes match the item.
   /// </summary>
   /// <typeparam name="T">The type of item matched by a regular expression</typeparam>
   public class NegativeSetClass<T> : SetClass<T>
   {
      /// <summary>
      /// Creates a new instance of the NegativeSetClass that matches when none of the contained classes match the item.
      /// </summary>
      /// <param name="classes">The classes in the set.</param>
      public NegativeSetClass (params IClass<T> [] classes)
         : base (classes, true)
      {
      }
   }
}
