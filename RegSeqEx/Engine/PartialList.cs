using System;
using System.Collections.Generic;

namespace RegSeqEx.Engine
{
   /// <summary>
   /// Represents a section of an existing IList as an immutable IList.
   /// </summary>
   /// <typeparam name="T">The type of item matched by a regular expression</typeparam>
   public class PartialList<T> : IList<T>
   {
      /// <summary>
      /// Creates a new instance of <c>PartialList</c>.
      /// </summary>
      /// <param name="wrappedList">The <c>StringBuilder</c> to adapt for use by RegularExpression.</param>
      /// <param name="index">/// The starting index of the IList to represent as Index 0.</param>
      /// <param name="count">The number of items in the PartialList.</param>
      public PartialList (IList<T> wrappedList, int index, int count)
      {
         if (wrappedList == null)
            throw new ArgumentNullException ("wrappedList");
         if (index < 0 || index > wrappedList.Count)
            throw new ArgumentOutOfRangeException ("startIndex", index, "startIndex must be at least 0 and less or equal to wrappedList.Count.");
         if (count < 0)
            throw new ArgumentOutOfRangeException ("count", count, "count must be at least 0.");
         if (index + count > wrappedList.Count)
            throw new ArgumentException ("index + count must be less than wrappedList.Count.", "index + count");

         m_list = wrappedList;
         m_index = index;
         m_count = count;
      }
      
      /// <summary>
      /// The starting index of the IList to represent as Index 0.
      /// </summary>
      public int Index
      {
         get
         {
            return m_index;
         }
      }

      /// <summary>
      /// Gets the item at the specified index.
      /// </summary>
      /// <param name="index">The index of the item to get.</param>
      /// <returns>The item at the specified index.</returns>
      public T this[int index]
      {
         get
         {
            if (index < 0)
               throw new ArgumentOutOfRangeException ("index", index, "Index must be at least 0.");
            if (index >= m_count)
               throw new ArgumentOutOfRangeException ("index", index, "Index must be less that Count.");
            
            return m_list [ m_index + index];
         }
      }
      
      #region IList<T> Members

      /// <summary>
      /// Gets the relative index of the specified item.
      /// </summary>
      /// <param name="item">The item to look for.</param>
      /// <returns>The index of the specified item, relative to the begining of this IList.</returns>
      public int IndexOf(T item)
      {
         for (int thisIndex = 0, listIndex = 0; listIndex < m_count; thisIndex++, listIndex++)
         {
            if (StringComparer.InvariantCulture.Equals (m_list [listIndex], item))
               return thisIndex;
         }
         return NOT_FOUND;
      }

      void IList<T>.Insert(int index, T item)
      {
         throw CreateModificationsNotSupportedException ();
      }

      void IList<T>.RemoveAt(int index)
      {
         throw CreateModificationsNotSupportedException ();
      }

      T IList<T>.this[int index]
      {
         get
         {
            return this[index];
         }
         set
         {
            throw CreateModificationsNotSupportedException ();
         }
      }

      #endregion

      #region ICollection<T> Members

      void ICollection<T>.Add(T item)
      {
         throw CreateModificationsNotSupportedException ();
      }

      void ICollection<T>.Clear()
      {
         throw CreateModificationsNotSupportedException ();
      }

      /// <summary>
      /// Determines whether the List contains the specified item.
      /// </summary>
      /// <param name="item">The item to look for.</param>
      /// <returns>True if the <c>PartialList</c> contains the specified item, otherwise false.</returns>
      public bool Contains(T item)
      {
         return IndexOf (item) >= 0;
      }

      /// <summary>
      /// Copies the contents of the PartialList to the Array.
      /// </summary>
      /// <param name="array">The destination array.</param>
      /// <param name="arrayIndex">The index of the destination array to start copying.</param>
      public void CopyTo(T[] array, int arrayIndex)
      {
         if (array == null)
            throw new ArgumentNullException ("array");
         if (arrayIndex < 0)
            throw new ArgumentOutOfRangeException ("arrayIndex");
         if (array.Rank > 1)
            throw new ArgumentException ("array is multidimensional");
         if (Count + arrayIndex > array.Length)
            throw new ArgumentException ("The number of elements is greater than the available space.");

         for (int thisIndex = 0, aindex = arrayIndex; thisIndex < m_count; thisIndex++, aindex++)
         {
            array[aindex] = this [thisIndex];
         }
      }

      /// <summary>
      /// Gets the number of items in the list.
      /// </summary>
      public int Count
      {
         get { return m_count; }
      }

      /// <summary>
      /// Gets a value indicating that the collection may not be modified.
      /// </summary>
      public bool IsReadOnly
      {
         get { return true; }
      }

      bool ICollection<T>.Remove(T item)
      {
         throw CreateModificationsNotSupportedException ();
      }

      #endregion

      #region IEnumerable<T> Members

      /// <summary>
      /// Gets an enumerator that enumerates over the values in the PartialList.
      /// </summary>
      /// <returns>An enumerator that enumerates over the values in the PartialList.</returns>
      public IEnumerator<T> GetEnumerator()
      {
         return Items.GetEnumerator ();
      }

      #endregion

      #region IEnumerable Members

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
         return GetEnumerator ();
      }

      #endregion
      
      private Exception CreateModificationsNotSupportedException()
      {
         return new NotSupportedException("Modifications to this list are not supported");
      }

      private IEnumerable<T> Items
      {
         get
         {
            for (int i = 0; i < m_count; i++)
               yield return this [i];
         }
      }

      private const int NOT_FOUND = -1;
      
      private readonly IList<T> m_list;
      private readonly int m_index;
      private readonly int m_count;
   }
}