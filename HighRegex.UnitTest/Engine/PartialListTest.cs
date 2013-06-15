using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HighRegex.Engine
{
   [TestClass]
   public class PartialListTest
   {
      /// <summary>
      ///Gets or sets the test context which provides
      ///information about and functionality for the current test run.
      ///</summary>
      public TestContext TestContext{get;set;}

      public IList<char> EmptyList = "".ToCharArray ();
      public IList<char> AList = "A".ToCharArray ();
      public IList<char> DigetsList = "0123456789".ToCharArray ();

      [TestMethod]
      [ExpectedException (typeof(ArgumentNullException))]
      public void ConstrutorThrowsArgumentNullExceptionWhenInputIsNull()
      {
         var PartialList = new PartialList<char> (null, 0,0);
      }

      //
      // Index
      //
      [TestMethod]
      [ExpectedException (typeof(ArgumentOutOfRangeException))]
      public void ConstrutorThrowArgumentOutOfRangeExceptionWhenIndexIsNeg1()
      {
         var PartialList = new PartialList<char> (EmptyList, -1,0);
      }

      [TestMethod]
      [ExpectedException (typeof(ArgumentOutOfRangeException))]
      public void ConstrutorThrowArgumentOutOfRangeExceptionWhenIndexIsLessThanZero()
      {
         var PartialList = new PartialList<char> (EmptyList, int.MinValue,0);
      }

      [TestMethod]
      public void ConstrutorSucceedsWhenIndexIsJustOutOfRangeForEmpty()
      {
         var PartialList = new PartialList<char> (EmptyList, 0,0);
      }

      [TestMethod]
      public void ConstrutorSucceedsWhenIndexIsJustOutOfRangeForA()
      {
         var PartialList = new PartialList<char> (AList, 1,0);
      }

      //
      // Length
      //

      [TestMethod]
      [ExpectedException (typeof(ArgumentOutOfRangeException))]
      public void ConstrutorThrowArgumentOutOfRangeExceptionWhenLengthIsNeg1()
      {
         var PartialList = new PartialList<char> (EmptyList, 0, -1);
      }

      [TestMethod]
      [ExpectedException (typeof(ArgumentOutOfRangeException))]
      public void ConstrutorThrowArgumentOutOfRangeExceptionWhenLengthIsLessThanZero()
      {
         var PartialList = new PartialList<char> (EmptyList, 0, int.MinValue);
      }

      [TestMethod]
      [ExpectedException (typeof(ArgumentException))]
      public void ConstrutorThrowArgumentExceptionWhenLengthPlusIndexExceedsBoundsForEmpty()
      {
         var PartialList = new PartialList<char> (EmptyList, 0, 1);
      }

      [TestMethod]
      [ExpectedException (typeof(ArgumentException))]
      public void ConstrutorThrowArgumentOutOfRangeExceptionWhenLengthPlusIndexExceedsBoundsForDigets_0_11()
      {
         var PartialList = new PartialList<char> (DigetsList, 0, 11);
      }

      [TestMethod]
      [ExpectedException (typeof(ArgumentException))]
      public void ConstrutorThrowArgumentOutOfRangeExceptionWhenLengthPlusIndexExceedsBoundsForDigets_5_6()
      {
         var PartialList = new PartialList<char> (DigetsList, 5, 6);
      }

      [TestMethod]
      [ExpectedException (typeof(ArgumentException))]
      public void ConstrutorThrowArgumentOutOfRangeExceptionWhenLengthPlusIndexExceedsBoundsForDigets_5_10()
      {
         var PartialList = new PartialList<char> (DigetsList, 5, 10);
      }

      //
      // Properties
      //
      [TestMethod]
      public void ConstrutorSucceedsWhenLengthPlusIndexEqualsBoundsForDigets_0_10()
      {
         int index = 0;
         int length = 10;
         var PartialList = Create (DigetsList, index, length);
      }

      [TestMethod]
      public void ConstrutorSucceedsWhenLengthPlusIndexEqualsBoundsForDigets_5_5()
      {
         int index = 5;
         int length = 5;
         var PartialList = Create (DigetsList, index, length);
      }

      [TestMethod]
      public void ConstrutorSucceedsWhenLengthPlusIndexEqualsBoundsForDigets_4_6()
      {
         int index = 4;
         int length = 6;
         var PartialList = Create (DigetsList, index, length);
      }

      [TestMethod]
      public void ConstrutorSucceedsWhenLengthPlusIndexIsInBoundsForDigets_5_1()
      {
         int index = 5;
         int length = 1;
         var PartialList = Create (DigetsList, index, length);
      }
      
      private PartialList<T> Create<T> (IList<T> input, int index, int count)
      {
         PartialList<T> list = new PartialList<T> (input, index, count);
         Assert.AreEqual (index, list.Index, "index");
         Assert.AreEqual (count, list.Count, "count");
         
         Assert.AreEqual (count, list.Count, "PartialList.Items.Count");
         
         IEnumerator<T> enumeratorOfT = list.GetEnumerator ();
         IList<T> ilistOfT = (IList<T>) list;
         System.Collections.IEnumerator enumerator = list.GetEnumerator ();
         for (int inputIndex = index, itemIndex = 0; itemIndex < count; inputIndex++, itemIndex++)
         {
            // Test enumerator
            Assert.IsTrue (enumerator.MoveNext (), "enumerator");
            Assert.AreEqual (input[inputIndex], enumerator.Current, "enumerator.Current at itemIndex " + itemIndex);

            // Test enumeratorOfT
            Assert.IsTrue (enumeratorOfT.MoveNext (), "enumeratorOfT");
            Assert.AreEqual (input[inputIndex], enumeratorOfT.Current, "enumeratorOfT.Current at itemIndex " + itemIndex);
            
            // Test indexer
            Assert.AreEqual (input[inputIndex], list[itemIndex], "PartialList.Item at itemIndex " + itemIndex);
            
            // Test listOfT indexer
            Assert.AreEqual (input[inputIndex], ilistOfT[itemIndex], "IList<T>.Item at itemIndex " + itemIndex);
         }
         
         return list;
      }
   }
}
