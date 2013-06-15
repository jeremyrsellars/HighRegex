using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RegSeqEx
{
   [TestClass]
   public class EndExpressionTest
   {
      private EndExpression<Char> m_expression = new EndExpression<Char> ();
      
      /// <summary>
      ///Gets or sets the test context which provides
      ///information about and functionality for the current test run.
      ///</summary>
      public TestContext TestContext{get;set;}

      //
      // IsMatchAt
      //
      [TestMethod]
      public void IsMatchAtReturnsTrueForEndA()
      {
         MatchLength ml = m_expression.IsMatchAt ("A".ToListCursor (), 1);
         Assert.IsTrue (ml.Success);
         Assert.AreEqual (0, ml.Length, "length should be 0");
      }

      [TestMethod]
      public void IsMatchAtReturnsTrueForEndEmpty()
      {
         MatchLength ml = m_expression.IsMatchAt ("".ToListCursor (), 0);
         Assert.IsTrue (ml.Success);
         Assert.AreEqual (0, ml.Length, "length should be 0");
      }

      [TestMethod]
      [ExpectedException (typeof (ArgumentNullException))]
      public void IsMatchAtThrowsArgumentNullException()
      {
         m_expression.IsMatchAt (null, 0);
      }

      [TestMethod]
      public void IsMatchAtReturnsFalseWhenOutOfRangeBefore()
      {
         MatchLength ml = m_expression.IsMatchAt ("".ToListCursor (), -1);
         Assert.IsFalse (ml.Success);
         Assert.AreEqual (0, ml.Length, "length should be 0");
      }

      //
      // GetMatches
      //
      [TestMethod]
      public void GetMatchesReturnsOneForEndA()
      {
         var matches = m_expression.GetMatches ("A".ToListCursor (), 1);
         var match = matches.First ();
         Assert.AreEqual (0, match.Length, "length should be 0");
         Assert.AreEqual (1, match.Index, "index should be 1");
      }

      [TestMethod]
      public void GetMatchesReturnsOneForEndEmpty()
      {
         var matches = m_expression.GetMatches ("".ToListCursor (), 0);
         var match = matches.First ();
         Assert.AreEqual (0, match.Length, "length should be 0");
         Assert.AreEqual (0, match.Index, "index should be 0");
      }

      [TestMethod]
      [ExpectedException (typeof (ArgumentNullException))]
      public void GetMatchesThrowsArgumentNullException()
      {
         var matches = m_expression.GetMatches (null, 0);
      }

      [TestMethod]
      public void GetMatchesReturnsNoneWhenOutOfRangeBefore()
      {
         var matches = m_expression.GetMatches ("".ToListCursor (), -1);
         Assert.AreEqual (0, matches.Count (), "count");
      }

      //
      // Regex Matches tests
      //
      [TestMethod]
      public void RegexMatchesForEmpty()
      {
         string regex = @"$";
         m_expression.AssertMatches ("", regex);
      }

      [TestMethod]
      public void RegexMatchesForAA()
      {
         string regex = @"$";
         m_expression.AssertMatches ("AA", regex);
      }
   }
}
