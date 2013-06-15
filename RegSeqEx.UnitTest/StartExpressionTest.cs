using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RegSeqEx
{
   [TestClass]
   public class StartExpressionTest
   {
      private StartExpression<Char> m_expression = new StartExpression<Char> ();
      
      /// <summary>
      ///Gets or sets the test context which provides
      ///information about and functionality for the current test run.
      ///</summary>
      public TestContext TestContext{get;set;}

      //
      // IsMatchAt
      //
      [TestMethod]
      public void IsMatchAtReturnsTrueForStartA()
      {
         MatchLength ml = m_expression.IsMatchAt ("A".ToListCursor (), 0);
         Assert.IsTrue (ml.Success);
         Assert.AreEqual (0, ml.Length, "length should be 0");
      }

      [TestMethod]
      public void IsMatchAtReturnsTrueForEndEmpty()
      {
         var ml = m_expression.IsMatchAt("".ToListCursor(), 0);
         Assert.IsTrue (ml.Success);
         Assert.AreEqual (0, ml.Length, "length should be 0");
      }

      [TestMethod]
      public void IsMatchAtReturnsFalseForOne()
      {
         MatchLength ml = m_expression.IsMatchAt ("".ToListCursor (), 1);
         Assert.IsFalse (ml.Success);
         Assert.AreEqual (0, ml.Length, "length should be 0");
      }

      [TestMethod]
      [ExpectedException (typeof (ArgumentNullException))]
      public void IsMatchAtThrowsArgumentNullException()
      {
         MatchLength ml = m_expression.IsMatchAt (null, 0);
         Assert.IsTrue (ml.Success);
         Assert.AreEqual (0, ml.Length, "length should be 0");
      }

      [TestMethod]
      public void IsMatchAtReturnsFalseWhenJustBeforeOfRange()
      {
         int length;
         MatchLength ml = m_expression.IsMatchAt ("".ToListCursor (), -1);
         Assert.IsFalse (ml.Success);
         Assert.AreEqual (0, ml.Length, "length should be 0");
      }

      [TestMethod]
      public void IsMatchAtReturnsFalseWhenJustOutOfRange()
      {
         int length;
         MatchLength ml = m_expression.IsMatchAt ("".ToListCursor (), 1);
         Assert.IsFalse (ml.Success);
         Assert.AreEqual (0, ml.Length, "length should be 0");
      }
      [TestMethod]
      public void IsMatchAtReturnsFalseWhenOutOfRange()
      {
         int length;
         MatchLength ml = m_expression.IsMatchAt ("".ToListCursor (), int.MaxValue);
         Assert.IsFalse (ml.Success);
         Assert.AreEqual (0, ml.Length, "length should be 0");
      }
      
      //
      // GetMatches
      //
      [TestMethod]
      public void GetMatchesReturnsOneForStartA()
      {
         var matches = m_expression.GetMatches ("A".ToListCursor (), 0);
         Assert.AreEqual (1, matches.Count (), "count");
         var match = matches.First ();
         Assert.AreEqual (0, match.Length, "length should be 0");
         Assert.AreEqual (0, match.Index, "index should be 0");
      }

      [TestMethod]
      public void GetMatchesReturnsOneForEmpty()
      {
         var matches = m_expression.GetMatches ("".ToListCursor (), 0);
         Assert.AreEqual (1, matches.Count (), "count");
         var match = matches.First ();
         Assert.AreEqual (0, match.Length, "length should be 0");
         Assert.AreEqual (0, match.Index, "index should be 0");
      }

      [TestMethod]
      public void GetMatchesReturnsNoneForOne()
      {
         var matches = m_expression.GetMatches ("".ToListCursor (), 1);
         Assert.AreEqual (0, matches.Count (), "count");
      }

      [TestMethod]
      [ExpectedException (typeof (ArgumentNullException))]
      public void GetMatchesThrowsArgumentNullException()
      {
         var matches = m_expression.GetMatches (null, 0);
         Assert.Fail ("expected exception");
      }

      [TestMethod]
      public void GetMatchesReturnsNoneWhenJustBeforeOfRange()
      {
         var matches = m_expression.GetMatches ("".ToListCursor (), -1);
         Assert.AreEqual (0, matches.Count (), "count");
      }

      [TestMethod]
      public void GetMatchesReturnsNoneWhenJustOutOfRange()
      {
         var matches = m_expression.GetMatches ("".ToListCursor (), 1);
         Assert.AreEqual (0, matches.Count (), "count");
      }
      [TestMethod]
      public void GetMatchesReturnsNoneWhenOutOfRange()
      {
         var matches = m_expression.GetMatches ("".ToListCursor (), int.MaxValue);
         Assert.AreEqual (0, matches.Count (), "count");
      }

      //
      // Regex Matches tests
      //
      [TestMethod]
      public void RegexMatchesForEmpty()
      {
         string regex = @"^";
         m_expression.AssertMatches ("", regex);
      }

      [TestMethod]
      public void RegexMatchesForAA()
      {
         string regex = @"^";
         m_expression.AssertMatches ("AA", regex);
      }
   }
}
