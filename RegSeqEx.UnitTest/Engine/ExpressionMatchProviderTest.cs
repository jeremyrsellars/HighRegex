using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RegSeqEx.Engine
{
   [TestClass]
   public class ExpressionMatchProviderTest
   {
      private IClass<Char> m_expression = new AnyClass<char> ();
      private IClass<Char> m_five = new PredicateClass<char> (c => c == '5');

      public ListExpressionItemSource<char> EmptyList = "".ToListCursor ();
      public ListExpressionItemSource<char> AList = "A".ToListCursor ();
      public ListExpressionItemSource<char> DigetsList = "0123456789".ToListCursor ();
      
      private MockExpression<char> m_anyLength = new MockExpression<char> {AnyLength=true, SupportsLookBack=true, PossibleMatchLengthsFunc= max => AnyLengthMatchProvider.GetAscending (max)};
      private MockExpression<char> m_lookBack = new MockExpression<char> {AnyLength=true, SupportsLookBack=true, PossibleMatchLengthsFunc= max => AnyLengthMatchProvider.GetAscending (max)};
      private MockExpression<char> m_noLookBack = new MockExpression<char> {AnyLength=true, SupportsLookBack=false, PossibleMatchLengths=AnyLengthMatchProvider.NoMatches};

      
      /// <summary>
      ///Gets or sets the test context which provides
      ///information about and functionality for the current test run.
      ///</summary>
      public TestContext TestContext{get;set;}

      [TestMethod]
      [ExpectedException (typeof(ArgumentNullException))]
      public void GetMatchesConstructorThrowArgumentNullException()
      {
         var expression = new ExpressionMatchProvider<char> (null);
      }

      [TestMethod]
      public void GetMatchesFindsOneMatcheForA()
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

      [TestMethod]
      public void GetMatchesFindsOneMatchForDigits()
      {
         ExpressionMatchProvider<char> provider = new ExpressionMatchProvider<char> (m_expression);

         var matches = provider.GetMatches (DigetsList, 0);

         Assert.AreEqual (1, matches.Count (), "Count");
         
         int index = 0;
         foreach (var match in matches)
         {
            Assert.AreEqual (index, match.Index, "match.Index");
            Assert.AreEqual (1, match.Length, "match.Length");
            Assert.IsTrue (match.Success, "match.Success");
            Assert.AreEqual (1, match.Items.Count, "match.Items.Count");
            Assert.AreEqual (DigetsList[index], match.Items [0], "match.Items [0]");
            index++;
         }
      }

      [TestMethod]
      public void GetMatchesFindsNoMatchesForEmpty()
      {
         ExpressionMatchProvider<char> provider = new ExpressionMatchProvider<char> (m_expression);

         var matches = provider.GetMatches (EmptyList, 0);

         Assert.AreEqual (0, matches.Count (), "Count");
      }

      [TestMethod]
      public void GetMatchesOf5FindsOneMatchForDigitsStartingAt5()
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
      public void GetMatchesOf5FindsNoMatchForDigitsStartingAt8()
      {
         ExpressionMatchProvider<char> provider = new ExpressionMatchProvider<char> (m_five);

         var matches = provider.GetMatches (DigetsList, 8);

         Assert.AreEqual (0, matches.Count (), "Count");
      }

      [TestMethod]
      public void AnyLengthIsFalseForClass()
      {
         ExpressionMatchProvider<char> provider = new ExpressionMatchProvider<char> (m_five);

         Assert.IsFalse (provider.AnyLength);
      }

      [TestMethod]
      public void SupportsLookBackIsTrueForClass()
      {
         ExpressionMatchProvider<char> provider = new ExpressionMatchProvider<char> (m_five);

         Assert.IsTrue (provider.SupportsLookBack);
      }

      [TestMethod]
      public void GetPossibleMatchLengthsReturns1ForClass()
      {
         ExpressionMatchProvider<char> provider = new ExpressionMatchProvider<char> (m_five);

         var matchLengths = provider.GetPossibleMatchLengths (100).ToList ();
         
         Assert.AreEqual (1, matchLengths.Count, "Count");
         Assert.AreEqual (1, matchLengths[0], "list[0]");
      }

      [TestMethod]
      public void AnyLengthIsTrueForAnyLength()
      {
         ExpressionMatchProvider<char> provider = new ExpressionMatchProvider<char> (m_anyLength);
         
         int max = 100;
         var lengths = provider.GetPossibleMatchLengths (max);

         Assert.IsTrue (provider.AnyLength, "AnyLength");
         Assert.IsTrue (lengths is AnyLengthMatchProvider.AnyLengthEnumerable, "AnyLengthEnumerable");
         Assert.AreEqual(((AnyLengthMatchProvider.AnyLengthEnumerable) lengths).Maximum, max, "AnyLengthEnumerable.Maximum");
      }

      [TestMethod]
      public void SupportsLookBackIsTrueForLookBack()
      {
         ExpressionMatchProvider<char> provider = new ExpressionMatchProvider<char> (m_lookBack);

         int max = 100;
         var lengths = provider.GetPossibleMatchLengths (max);
         Assert.IsTrue (provider.SupportsLookBack);
         Assert.AreEqual(((AnyLengthMatchProvider.AnyLengthEnumerable) lengths).Maximum, max, "AnyLengthEnumerable.Maximum");
      }

      [TestMethod]
      public void SupportsLookBackIsFalseForNoLookBack()
      {
         ExpressionMatchProvider<char> provider = new ExpressionMatchProvider<char> (m_noLookBack);

         Assert.IsFalse (provider.SupportsLookBack);
         Assert.AreEqual(AnyLengthMatchProvider.NoMatches, provider.GetPossibleMatchLengths (100), "GetPossibleMatchLengths should be empty");
      }

   }
}
