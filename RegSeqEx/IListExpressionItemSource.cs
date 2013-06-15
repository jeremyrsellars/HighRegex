using System;
using System.Collections.Generic;

namespace RegSeqEx
{
   /// <summary>
   /// Provides Count and Indexer methods to the RegularExpressions engine.  Represents items in sequence.
   /// </summary>
   /// <typeparam name="T">The type of item matched by a regular expression</typeparam>
   public interface IListExpressionItemSource<T> : IExpressionItemSource<T>
   {
      /// <summary>
      /// Gets the item at the specified index.
      /// </summary>
      /// <param name="index">The index of the item to get.</param>
      /// <returns>The item at index.</returns>
      T this[int index]{get;}
      
      /// <summary>
      /// Gets the number of items in the sequence.
      /// </summary>
      int Count{get;}
   }
}
