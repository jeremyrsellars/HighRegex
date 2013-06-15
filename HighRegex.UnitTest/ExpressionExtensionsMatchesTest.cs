using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HighRegex.Engine;

namespace HighRegex
{
   [TestClass]
   public class ExpressionExtensionsMatchesTest
   {
      private IClass<Char> m_expression = new AnyClass<char> ();
      private IClass<Char> m_five = new PredicateClass<char> (c => c == '5');

      public ListExpressionItemSource<char> EmptyList = "".ToListCursor();
      public ListExpressionItemSource<char> AList = "A".ToListCursor ();
      public ListExpressionItemSource<char> DigetsList = "0123456789".ToListCursor ();

      
      /// <summary>
      ///Gets or sets the test context which provides
      ///information about and functionality for the current test run.
      ///</summary>
      public TestContext TestContext{get;set;}

      [TestMethod]
      [ExpectedException (typeof(ArgumentNullException))]
      public void ConstructorThrowArgumentNullException()
      {
         var expression = new ExpressionMatchProvider<char> (null);
      }

      [TestMethod]
      public void MatchesFindsOneMatcheForA()
      {
         ExpressionMatchProvider<char> provider = new ExpressionMatchProvider<char> (m_expression);

         var matches = provider.GetMatches (AList, 0);

         Assert.AreEqual (1, matches.Count (), "Count");
         var match = matches.First ();
         Assert.AreEqual (0, match.Index, "match.Index");
         Assert.AreEqual (1, match.Length, "match.Length");
         Assert.IsTrue (match.Success, "match.Success");
         Assert.AreEqual (1, match.Items.Count, "match.Items.Count");
         Assert.AreEqual (AList[0], match.Items [0], "match.Items [0]");
      }

      //[TestMethod]
      //public void MatchesFindsTenMatchesForDigits()
      //{
      //   ExpressionMatchProvider<char> provider = new ExpressionMatchProvider<char> (m_expression);

      //   var matches = provider.Matches (DigetsList, 0);

      //   Assert.AreEqual (10, matches.Count (), "Count");
         
      //   int index = 0;
      //   foreach (var match in matches)
      //   {
      //      Assert.AreEqual (index, match.Index, "match.Index");
      //      Assert.AreEqual (1, match.Length, "match.Length");
      //      Assert.IsTrue (match.Success, "match.Success");
      //      Assert.AreEqual (1, match.Items.Count, "match.Items.Count");
      //      Assert.AreEqual (DigetsList[index], match.Items [0], "match.Items [0]");
      //      index++;
      //   }
      //}

      [TestMethod]
      public void MatchesFindsNoMatchesForEmpty()
      {
         ExpressionMatchProvider<char> provider = new ExpressionMatchProvider<char> (m_expression);

         var matches = provider.GetMatches (EmptyList, 0);

         Assert.AreEqual (0, matches.Count (), "Count");
      }

      [TestMethod]
      public void MatchesOf5FindsOneMatchForDigitsStartingAt5()
      {
         ExpressionMatchProvider<char> provider = new ExpressionMatchProvider<char> (m_five);

         var matches = provider.GetMatches (DigetsList, 5);

         Assert.AreEqual (1, matches.Count (), "Count");

         var match = matches.First ();
         Assert.AreEqual (5, match.Index, "match.Index");
         Assert.AreEqual (1, match.Length, "match.Length");
         Assert.IsTrue (match.Success, "match.Success");
         Assert.AreEqual (1, match.Items.Count, "match.Items.Count");
         Assert.AreEqual ('5', match.Items [0], "match.Items [0]");
      }

      [TestMethod]
      public void MatchesOf5FindsNoMatchForDigitsStartingAt8()
      {
         ExpressionMatchProvider<char> provider = new ExpressionMatchProvider<char> (m_five);

         var matches = provider.GetMatches (DigetsList, 8);

         Assert.AreEqual (0, matches.Count (), "Count");
      }


   }
}
