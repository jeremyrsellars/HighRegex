using System;
using System.Collections.Generic;

namespace RegSeqEx
{
   /// <summary>
   /// An immutable object representing a location and length in a sequence
   /// that matches an expression.
   /// Represents the results from a single regular expression match.
   /// </summary>
   /// <typeparam name="T">The type of item matched by a regular expression</typeparam>
   public class Match<T>
   {
      /// <summary>
      /// Gets a value indicating whether the match is successful.
      /// </summary>
      public bool Success
      {
         get
         {
            return m_success;
         }
      }
      
      /// <summary>
      /// The position in the original list where the first item
      /// of the match was found.
      /// </summary>
      public int Index
      {
         get
         {
            return m_index;
         }
      }
      
      /// <summary>
      /// The length of the match.
      /// </summary>
      public int Length
      {
         get
         {
            return m_length;
         }
      }
      
      /// <summary>
      /// Gets the captured substring from the input list.
      /// </summary>
      public IList<T> Items
      {
         get
         {
            return m_items;
         }
      }

      internal Match (IExpressionItemSource<T> input, int index, int length, bool success)
      {
         if (input == null)
            throw new ArgumentNullException ("input");
         
         if (!input.IsItemInRange (index))
            throw new ArgumentOutOfRangeException ("index", index, "Must be at least 0 and less than or equal to number of items.");
         
         if (length < 0)
            throw new ArgumentOutOfRangeException ("length", length, "Must be at least 0.");
         if (!input.IsItemInRange (index + length))
            throw new ArgumentException ("index + length must be less than or equal to the number of items.", "length");

         if (length > 0 && !success)
            throw new ArgumentException ("Success must be true when length is greater than 0.", "success");

         m_input = input;
         m_index = index;
         m_length = length;
         m_success = success;
         m_items = input.CreatePartialList (index, length);
      }
      
      private readonly bool m_success;
      private readonly int m_index;
      private readonly int m_length;
      private readonly IExpressionItemSource<T> m_input;
      private readonly IList<T> m_items;
   }
}
