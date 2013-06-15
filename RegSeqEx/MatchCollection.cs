using System;
using System.Collections.Generic;

namespace RegSeqEx
{
   /// <summary>
   /// Represents the set of successful matches found by iteratively applying a regular expression pattern to the input.
   /// </summary>
   /// <typeparam name="T">The type of item matched by a regular expression</typeparam>
   public class MatchCollection<T> : IList<Match<T>>
   {
      /// <summary>
      /// Creates an instance of MatchCollection.
      /// </summary>
      public MatchCollection ()
      {
         m_list = new List<Match<T>> ();
      }

      /// <summary>
      /// Gets the number of items in the sequence.
      /// </summary>
      public int Count
      {
         get { return m_list.Count; }
      }

      /// <summary>
      /// Gets the item at the specified index.
      /// </summary>
      /// <param name="index">The index of the item to get.</param>
      /// <returns>The item at index.</returns>
      public Match<T> this[int index]
      {
         get
         {
            return m_list [index];
         }
      }

      /// <summary>
      /// Gets the index of the the specified <c>Match</c>, <paramref name="item"/>.
      /// </summary>
      /// <param name="item">The match to locate in the collection.</param>
      /// <returns>The index of the the specified <c>Match</c>, <paramref name="item"/> if it is found, otherwise -1.</returns>
      public int IndexOf(Match<T> item)
      {
         return m_list.IndexOf (item);
      }

      /// <summary>
      /// Determines whether the collection contains the specified match.
      /// </summary>
      /// <param name="item">The match to locate in the collection.</param>
      /// <returns>True if the match is in the collection, otherwise false.</returns>
      public bool Contains(Match<T> item)
      {
         return IndexOf (item) >= 0;
      }

      /// <summary>
      /// Copies the contents of the collection to an array at the specified index.
      /// </summary>
      /// <param name="array">The destination array.</param>
      /// <param name="arrayIndex">The starting index to copy into.</param>
      public void CopyTo(Match<T>[] array, int arrayIndex)
      {
         m_list.CopyTo (array, arrayIndex);
      }

      #region IList<Match<T>> Members

      void IList<Match<T>>.Insert(int index, Match<T> item)
      {
         throw CreateModificationsNotSupportedException ();
      }

      void IList<Match<T>>.RemoveAt(int index)
      {
         throw CreateModificationsNotSupportedException ();
      }

      Match<T> IList<Match<T>>.this[int index]
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

      #region ICollection<Match<T>> Members

      void ICollection<Match<T>>.Add(Match<T> item)
      {
         throw CreateModificationsNotSupportedException ();
      }

      void ICollection<Match<T>>.Clear()
      {
         throw CreateModificationsNotSupportedException ();
      }

      bool ICollection<Match<T>>.IsReadOnly
      {
         get { return true; }
      }

      bool ICollection<Match<T>>.Remove(Match<T> item)
      {
         throw CreateModificationsNotSupportedException ();
      }

      #endregion

      #region IEnumerable<T> Members

      /// <summary>
      /// Enumerates the items in the collection.
      /// </summary>
      /// <returns>An enumerator of the matches in the collection.</returns>
      public IEnumerator<Match<T>> GetEnumerator()
      {
         return m_list.GetEnumerator ();
      }

      #endregion

      #region IEnumerable Members

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
         return GetEnumerator ();
      }

      #endregion
      
      internal bool IsLocked
      {
         get
         {
            return m_isLocked;
         }
      }
      
      internal void Lock ()
      {
         m_isLocked = true;
      }
      
      internal void Add (Match<T> item)
      {
         if (item == null)
            throw new ArgumentNullException ("item");
         
         if (m_isLocked)
            throw new InvalidOperationException ("Cannot add another match after MatchCollection has been locked.");

         m_list.Add (item);
      }
      
      private Exception CreateModificationsNotSupportedException()
      {
         return new NotSupportedException("Modifications to this list are not supported");
      }
      
      private List<Match<T>> m_list;
      private bool m_isLocked;
   }
}
