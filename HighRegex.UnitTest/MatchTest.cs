using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HighRegex
{
   [TestClass]
   public class MatchTest
   {
      /// <summary>
      ///Gets or sets the test context which provides
      ///information about and functionality for the current test run.
      ///</summary>
      public TestContext TestContext{get;set;}

      public ListExpressionItemSource<char> EmptyList = "".ToListCursor ();
      public ListExpressionItemSource<char> AList = "A".ToListCursor ();
      public ListExpressionItemSource<char> DigetsList = "0123456789".ToListCursor ();

      [TestMethod]
      [ExpectedException (typeof(ArgumentNullException))]
      public void ConstrutorThrowsArgumentNullExceptionWhenInputIsNull()
      {
         Match<char> match = new Match<char> (null, 0,0, true);
      }

      //
      // Index
      //
      [TestMethod]
      [ExpectedException (typeof(ArgumentOutOfRangeException))]
      public void ConstrutorThrowArgumentOutOfRangeExceptionWhenIndexIsNeg1()
      {
         Match<char> match = new Match<char> (EmptyList, -1,0, true);
      }

      [TestMethod]
      [ExpectedException (typeof(ArgumentOutOfRangeException))]
      public void ConstrutorThrowArgumentOutOfRangeExceptionWhenIndexIsLessThanZero()
      {
         Match<char> match = new Match<char> (EmptyList, int.MinValue,0, true);
      }

      [TestMethod]
      public void ConstrutorDoesNotThrowExceptionWhenIndexIsJustOutOfRangeForEmpty()
      {
         Match<char> match = new Match<char> (EmptyList, 0,0, true);
      }

      [TestMethod]
      public void ConstrutorDoesNotThrowExceptionWhenIndexIsJustOutOfRangeForA()
      {
         Match<char> match = new Match<char> (AList, 1,0, true);
      }

      //
      // Length
      //

      [TestMethod]
      [ExpectedException (typeof(ArgumentOutOfRangeException))]
      public void ConstrutorThrowArgumentOutOfRangeExceptionWhenLengthIsNeg1()
      {
         Match<char> match = new Match<char> (EmptyList, 0, -1, true);
      }

      [TestMethod]
      [ExpectedException (typeof(ArgumentOutOfRangeException))]
      public void ConstrutorThrowArgumentOutOfRangeExceptionWhenLengthIsLessThanZero()
      {
         Match<char> match = new Match<char> (EmptyList, 0, int.MinValue, true);
      }

      [TestMethod]
      [ExpectedException (typeof(ArgumentException))]
      public void ConstrutorThrowArgumentExceptionWhenLengthPlusIndexExceedsBoundsForEmpty()
      {
         Match<char> match = new Match<char> (EmptyList, 0, 1, true);
      }

      [TestMethod]
      [ExpectedException (typeof(ArgumentException))]
      public void ConstrutorThrowArgumentExceptionWhenLengthPlusIndexExceedsBoundsForDigets_0_11()
      {
         Match<char> match = new Match<char> (DigetsList, 0, 11, true);
      }

      [TestMethod]
      [ExpectedException (typeof(ArgumentException))]
      public void ConstrutorThrowArgumentExceptionWhenLengthPlusIndexExceedsBoundsForDigets_5_6()
      {
         Match<char> match = new Match<char> (DigetsList, 5, 6, true);
      }

      [TestMethod]
      [ExpectedException (typeof(ArgumentException))]
      public void ConstrutorThrowArgumentExceptionWhenLengthPlusIndexExceedsBoundsForDigets_5_10()
      {
         Match<char> match = new Match<char> (DigetsList, 5, 10, true);
      }

      //
      // Success
      //
      [TestMethod]
      [ExpectedException (typeof(ArgumentException))]
      public void ConstrutorThrowArgumentExceptionWhenHasLengthAndFailure()
      {
         Match<char> match = new Match<char> (DigetsList, 5, 10, false);
      }

      //
      // Properties
      //
      [TestMethod]
      public void ConstrutorSucceedsWhenLengthPlusIndexEqualsBoundsForDigets_0_10()
      {
         int index = 0;
         int length = 10;
         bool success = true;
         Match<char> match = Create (DigetsList, index, length, success);
      }

      [TestMethod]
      public void ConstrutorSucceedsWhenLengthPlusIndexEqualsBoundsForDigets_4_6()
      {
         int index = 4;
         int length = 6;
         bool success = true;
         Match<char> match = Create (DigetsList, index, length, success);
      }

      [TestMethod]
      public void ConstrutorSucceedsWhenLengthPlusIndexEqualsBoundsForDigets_10_0()
      {
         int index = 10;
         int length = 0;
         bool success = true;
         Match<char> match = Create (DigetsList, index, length, success);
      }

      [TestMethod]
      public void ConstrutorSucceedsWhenLengthPlusIndexIsInBoundsForDigets_5_1()
      {
         int index = 5;
         int length = 1;
         bool success = true;
         Match<char> match = Create (DigetsList, index, length, success);
      }
      
      private Match<T> Create<T> (ListExpressionItemSource<T> input, int index, int length, bool success)
      {
         if (length > 0 && !success)
            Assert.Inconclusive ("When length is greater than zero, Success must be true.  Invalid test.");
         
         Match<T> match = new Match<T> (input, index, length, success);
         Assert.AreEqual (index, match.Index, "index");
         Assert.AreEqual (length, match.Length, "length");
         Assert.AreEqual (success, match.Success, "success");
         
         var matchItems = match.Items;
         Assert.IsNotNull (matchItems, "match.Items");
         Assert.IsTrue (ReferenceEquals (matchItems, match.Items), "match.Items should return the same instance every time.");
         Assert.AreEqual (length, matchItems.Count, "match.Items.Count");
         
         IEnumerator<T> enumerator = matchItems.GetEnumerator ();
         for (int inputIndex = index, itemIndex = 0; itemIndex < length; inputIndex++, itemIndex++)
         {
            // Test enumerator
            Assert.IsTrue (enumerator.MoveNext ());
            Assert.AreEqual (input[inputIndex], enumerator.Current, "Enumerator.Current at itemIndex " + itemIndex);
            
            // Test indexer
            Assert.AreEqual (input[inputIndex], match.Items [itemIndex], "match.Item at itemIndex " + itemIndex);
         }
         
         return match;
      }
   }
}
