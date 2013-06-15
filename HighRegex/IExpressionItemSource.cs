using System;
using System.Collections.Generic;

namespace HighRegex
{
   /// <summary>
   /// Provides input to the RegularExpressions engine.  Represents items in sequence.
   /// </summary>
   /// <typeparam name="T">The type of item matched by a regular expression</typeparam>
   public interface IExpressionItemSource<T>
   {
      /// <summary>
      /// Gets the item at a particular index.
      /// </summary>
      /// <param name="index">
      /// The index of the item in the input sequence to get.
      /// </param>
      /// <returns>Gets the item at a particular index.</returns>
      /// <exception cref="System.ArgumentOutOfRangeException">If index is before the first item or after the last item.</exception>
      T GetItemAtIndex (int index);
      
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
      bool TryGetItemAtIndex (int index, out T item);
      
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
      bool IsAtStart (int index);
      
      /// <summary>
      /// Gets a value indicating whether the index is one past the last in the input sequence.
      /// </summary>
      /// <param name="index">
      /// The index of the item in input sequence to check.
      /// </param>
      /// <returns>True if the index is the one past the last index in the sequence.</returns>
      /// <remarks>
      /// In zero-based sequences, this method will only return true when <paramref name="index"/> is equal to the number of items in the sequence.
      /// For this input {0,1,2}, the output would be
      /// IsItemInRange (-1) --> false
      /// IsItemInRange (0) --> false
      /// IsItemInRange (1) --> false
      /// IsItemInRange (2) --> false
      /// IsItemInRange (3) --> true because it is just past the last item.
      /// IsItemInRange (4) --> false
      /// IsItemInRange (5) --> false
      /// The Includes one past the last item for the sake of the EndExpression.
      /// </remarks>
      bool IsAtEnd (int index);
      
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
      bool IsItemInRange (int index);
      
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
      bool IsIndexJustPastRange (int index);

      /// <summary>
      /// Gets a part of the index sequence starting at <paramref name="index"/> that is <paramref name="length"/> items long.
      /// </summary>
      /// <param name="index">
      /// The starting index of the section of the original input sequence.
      /// </param>
      /// <param name="length">The length of the section.</param>
      /// <returns>Part of the original sequence.</returns>
      IList<T> CreatePartialList(int index, int length);
   }
}
