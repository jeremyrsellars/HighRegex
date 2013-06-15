using System;
using System.Collections.Generic;
using HighRegex.Engine;

namespace HighRegex
{
   /// <summary>
   /// Presents the data in an IEnumerable to the RegularExpression engine caching the items as needed.
   /// </summary>
   /// <typeparam name="T">The type of item matched by a regular expression</typeparam>
   public class EnumerableExpressionItemSource<T> : IExpressionItemSource<T>
   {
      /// <summary>
      /// Creates an instance of EnumerableExpressionItemSource that enables the RegularExpression engine to match the items in the specified <paramref name="enumerable"/>.
      /// </summary>
      /// <param name="enumerable">The items to pattern match.</param>
      public EnumerableExpressionItemSource (IEnumerable<T> enumerable)
      {
         if (enumerable == null)
            throw new ArgumentNullException ("enumerable");
         m_enumerable = enumerable;
         m_list = new List<T> ();
      }
      
      /// <summary>
      /// Gets the item at a particular index.
      /// </summary>
      /// <param name="index">
      /// The index of the item in the input sequence to get.
      /// </param>
      /// <returns>Gets the item at a particular index.</returns>
      /// <exception cref="System.ArgumentOutOfRangeException">If index is before the first item or after the last item.</exception>
      public T GetItemAtIndex (int index)
      {
         EnsureCached (index);
         
         if (index >= m_list.Count)
            throw new ArgumentOutOfRangeException ("index", index, null);

         if (index < 0)
            throw new ArgumentOutOfRangeException ("index", index, null);

         return m_list [index];
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
      public bool TryGetItemAtIndex (int index, out T item)
      {
         EnsureCached (index);
         
         if (index < 0 || index >= m_list.Count)
         {
            item = default(T);
            return false;
         }

         item = m_list [index];
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
      /// In zero-based sequences, this method will only return true when <paramref name="index"/> is equal to the number of items in the sequence.
      /// </remarks>
      public bool IsAtEnd (int index)
      {
         EnsureCached (index);
         
         return index == m_list.Count;
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
         EnsureCached (index);
         
         if (index < 0 || index > m_list.Count)
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
         if (index <= m_list.Count)
            return false;
         EnsureCached (index);
         
         int expected = m_list.Count + 1;
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
      public IList<T> CreatePartialList(int index, int length)
      {
         EnsureCached (index + length);
         
         var list = new PartialList<T> (m_list, index, length);
         return list;
      }

      private bool EnsureCached(int index)
      {
         // See if we already have it cached.
         int count = m_list.Count;
         if (index < count)
            return true;
         
         // If we have fully cached the list, then the index is out of range.
         if (m_fullyCached)
            return false;
         
         // Ensure the list has enough capacity;
         if (m_list.Capacity <= index)
         {
            int cap = m_list.Capacity;
            if (cap == 0)
               cap = 16;
            while (cap < index)
               cap *= 2;
            m_list.Capacity = cap;
         }
         
         // First time?  Set up enumerator.
         if (m_enumerator == null)
            m_enumerator = m_enumerable.GetEnumerator ();
         
         // Add anything from the current position to the specified index.
         while (m_list.Count <= index)
         {
            if (!m_enumerator.MoveNext ())
            {
               m_fullyCached = true;
               return false;
            }

            m_list.Add (m_enumerator.Current);
         }
         
         return true;
      }
      
      private IEnumerable<T> m_enumerable;
      private IEnumerator<T> m_enumerator;
      private bool m_fullyCached;
      private List<T> m_list;
   }
}
