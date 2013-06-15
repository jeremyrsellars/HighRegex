using System;
using System.Collections.Generic;
using System.Text;

namespace RegSeqEx
{
   /// <summary>
   /// Adapts a StringBuilder for use with the RegularExpression engine.
   /// </summary>
   public class StringBuilderExpressionItemSource : IListExpressionItemSource<char>
   {
      /// <summary>
      /// Creates a new instance of <c>StringBuilderExpressionItemSource</c>.
      /// </summary>
      /// <param name="buf">The <c>StringBuilder</c> to adapt for use by RegularExpression.</param>
      public StringBuilderExpressionItemSource (StringBuilder buf)
      {
         if (buf == null)
            throw new ArgumentNullException ("buf");
         m_buf = buf;
      }

      /// <summary>
      /// The number of characters in the StringBuilder.
      /// </summary>
      public int Count
      {
         get
         {
            return m_buf.Length;
         }
      }
      
      /// <summary>
      /// Gets the character at the specified index.
      /// </summary>
      /// <param name="index">The index to get.</param>
      /// <returns>The character at the specified index.</returns>
      public char this[int index]
      {
         get
         {
            return m_buf [index];
         }
      }
      
      /// <summary>
      /// Gets the item at a particular index.
      /// </summary>
      /// <param name="index">
      /// The index of the item in the input sequence to get.
      /// </param>
      /// <returns>Gets the item at a particular index.</returns>
      /// <exception cref="System.ArgumentOutOfRangeException">If index is before the first item or after the last item.</exception>
      public char GetItemAtIndex (int index)
      {
         if (index >= m_buf.Length)
            throw new ArgumentOutOfRangeException ("index", index, null);

         if (index < 0)
            throw new ArgumentOutOfRangeException ("index", index, null);

         return m_buf [index];
      }
      
      /// <summary>
      /// Gets the value at the specified index.
      /// </summary>
      /// <param name="index">
      /// The index of the item in input sequence to get.
      /// </param>
      /// <param name="item">
      /// When this method returns, contains the item at the specified index, if the index is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.
      /// </param>
      /// <returns>Gets the item at a particular index.</returns>
      public bool TryGetItemAtIndex (int index, out char item)
      {
         if (index < 0 || index >= m_buf.Length)
         {
            item = default(char);
            return false;
         }

         item = m_buf [index];
         return true;
      }

      /// <summary>
      /// Gets a value indicating whether the index is the first in the input sequence.
      /// </summary>
      /// <param name="index">
      /// The index of the item in input sequence to check.
      /// </param>
      /// <returns>True if the index is the first index in the sequence.</returns>
      /// <remarks>
      /// In zero-based sequences, this method will only return true when <paramref name="index"/> is zero.
      /// </remarks>
      public bool IsAtStart (int index)
      {
         return index == 0;
      }
      
      /// <summary>
      /// Gets a value indicating whether the index is one past the last in the input sequence.
      /// </summary>
      /// <param name="index">
      /// The index of the item in input sequence to check.
      /// </param>
      /// <returns>True if the index is the one past the last index in the sequence.</returns>
      /// <remarks>
      /// This method will only return true when <paramref name="index"/> is equal to the length of the StringBuilder.
      /// </remarks>
      public bool IsAtEnd (int index)
      {
         return index == m_buf.Length;
      }
      
      /// <summary>
      /// Determines whether the specified index is in the range supported by Regular Expressions (the first item through one past the last item).
      /// </summary>
      /// <param name="index">
      /// The index of the item in input sequence to get.
      /// </param>
      /// <returns>
      /// True when the index is supported by Regular Expressions (e.g. the index is not before the first item and the index is not more than one past the last item), otherwise false.
      /// </returns>
      /// <remarks>
      /// For this input {0,1,2}, the output would be
      /// IsItemInRange (-1) --> false because it is before the first item.
      /// IsItemInRange (0) --> true
      /// IsItemInRange (1) --> true
      /// IsItemInRange (2) --> true
      /// IsItemInRange (3) --> true because it is 1 past the last item.
      /// IsItemInRange (4) --> false
      /// IsItemInRange (5) --> false
      /// The Includes one past the last item for the sake of the EndExpression.
      /// </remarks>
      public bool IsItemInRange (int index)
      {
         if (index < 0 || index > m_buf.Length)
            return false;

         return true;
      }

      /// <summary>
      /// Determines whether the specified index is 1 past the last index supported by Regular Expressions (2 past the last item).
      /// </summary>
      /// <param name="index">
      /// The index of the item in input sequence to get.
      /// </param>
      /// <returns></returns>
      /// <remarks>
      /// This method helps the RegularExpression engine test for correctness.
      /// For this input {0,1,2}, the output would be
      /// IsItemInRange (-1) --> false
      /// IsItemInRange (0) --> false
      /// IsItemInRange (1) --> false
      /// IsItemInRange (2) --> false
      /// IsItemInRange (3) --> false
      /// IsItemInRange (4) --> true because it is 2 past the last item.
      /// IsItemInRange (5) --> false
      /// </remarks>
      public bool IsIndexJustPastRange(int index)
      {
         int expected = m_buf.Length + 1;
         return index == expected;
      }
      
      /// <summary>
      /// Gets a part of the index sequence starting at <paramref name="index"/> that is <paramref name="length"/> items long.
      /// </summary>
      /// <param name="index">
      /// The starting index of the section of the original input sequence.
      /// </param>
      /// <param name="length">The length of the section.</param>
      /// <returns>Part of the original sequence.</returns>
      public IList<char> CreatePartialList(int index, int length)
      {
         var list = new PartialStringBuilder (m_buf, index, length);
         return list;
      }

      private StringBuilder m_buf;

      /// <summary>
      /// Represents a section of a string builder as an immutable IList of Char.
      /// </summary>
      internal class PartialStringBuilder : IList<char>
      {
         /// <summary>
         /// Creates an instance of PartialStringBuilder.
         /// </summary>
         /// <param name="buf">The source StringBuilder to expose part of.</param>
         /// <param name="index">The starting index of the StringBuilder to represent as Index 0.</param>
         /// <param name="count">The count of the IList.</param>
         public PartialStringBuilder (StringBuilder buf, int index, int count)
         {
            if (buf == null)
               throw new ArgumentNullException ("buf");
            if (index < 0 || index > buf.Length)
               throw new ArgumentOutOfRangeException ("startIndex", index, "startIndex must be at least 0 and less or equal to buf.Length.");
            if (count < 0)
               throw new ArgumentOutOfRangeException ("count", count, "count must be at least 0.");
            if (index + count > buf.Length)
               throw new ArgumentException ("index + count must be less than buf.Length.", "index + count");

            m_list = buf;
            m_index = index;
            m_count = count;
         }
         
         /// <summary>
         /// The starting index of the StringBuilder to represent as Index 0.
         /// </summary>
         public int Index
         {
            get
            {
               return m_index;
            }
         }

         #region IList<T> Members

         public int IndexOf(char item)
         {
            for (int thisIndex = 0, listIndex = 0; listIndex < m_count; thisIndex++, listIndex++)
            {
               if (StringComparer.InvariantCulture.Equals (m_list [listIndex], item))
                  return thisIndex;
            }
            return NOT_FOUND;
         }

         public void Insert(int index, char item)
         {
            throw CreateModificationsNotSupportedException ();
         }

         public void RemoveAt(int index)
         {
            throw CreateModificationsNotSupportedException ();
         }

         public char this[int index]
         {
            get
            {
               if (index < 0)
                  throw new ArgumentOutOfRangeException ("index", index, "Index must be at least 0.");
               if (index >= m_count)
                  throw new ArgumentOutOfRangeException ("index", index, "Index must be less that Count.");
               
               return m_list [ m_index + index];
            }
            set
            {
               throw CreateModificationsNotSupportedException ();
            }
         }

         #endregion

         #region ICollection<T> Members

         public void Add(char item)
         {
            throw CreateModificationsNotSupportedException ();
         }

         public void Clear()
         {
            throw CreateModificationsNotSupportedException ();
         }

         public bool Contains(char item)
         {
            return IndexOf (item) >= 0;
         }

         public void CopyTo(char[] array, int arrayIndex)
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

         public override string ToString()
         {
            return m_list.ToString (m_index, m_count);
         }

         /// <summary>
         /// Gets the number of characters in the sequence.
         /// </summary>
         public int Count
         {
            get { return m_count; }
         }

         public bool IsReadOnly
         {
            get { return true; }
         }

         public bool Remove(char item)
         {
            throw CreateModificationsNotSupportedException ();
         }

         #endregion

         #region IEnumerable<T> Members

         public IEnumerator<char> GetEnumerator()
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

         private IEnumerable<char> Items
         {
            get
            {
               for (int i = 0; i < m_count; i++)
                  yield return this [i];
            }
         }

         private const int NOT_FOUND = -1;
         
         private readonly StringBuilder m_list;
         private readonly int m_index;
         private readonly int m_count;
      }
   }
}
