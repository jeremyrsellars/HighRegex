using System;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RegSeqEx
{
   /// <summary>
   /// Summary description for RegexTest
   /// </summary>
   [TestClass]
   public class RegexTest
   {
      public RegexTest()
      {
         //
         // TODO: Add constructor logic here
         //
      }

      private TestContext testContextInstance;

      /// <summary>
      ///Gets or sets the test context which provides
      ///information about and functionality for the current test run.
      ///</summary>
      public TestContext TestContext
      {
         get
         {
            return testContextInstance;
         }
         set
         {
            testContextInstance = value;
         }
      }
      
      Regex m_regexStartOfString = new Regex ("^");
      Regex m_regexEndOfString = new Regex ("$");

      [TestMethod]
      public void StartOfString()
      {
         Assert.AreEqual (0, m_regexStartOfString.Match ("").Index);
         Assert.AreEqual (0, m_regexStartOfString.Match (" ").Index);
      }

      [TestMethod]
      public void EndOfString()
      {
         Assert.AreEqual (0, m_regexEndOfString.Match ("").Index);
         Assert.AreEqual (1, m_regexEndOfString.Match (" ").Index);
      }
      
      public const string AAABBAB = "aaabbab";
      
      [TestMethod]
      public void MatchFailure()
      {
         Regex asb = new Regex ("a+b");
         
         Match match = asb.Match ("AAAAAAAAAAA");
         
         Assert.IsFalse (match.Success, "match.Success");
         Assert.AreEqual (0, match.Index, "matches.Index");
         Assert.AreEqual (0, match.Length, "matches.Length");
      }
      
      [TestMethod]
      public void MatchOptionalEmpty()
      {
         Regex asb = new Regex ("a*");
         
         Match match = asb.Match ("");
         
         Assert.IsTrue (match.Success, "match.Success");
         Assert.AreEqual (0, match.Index, "matches.Index");
         Assert.AreEqual (0, match.Length, "matches.Length");
      }
      
      [TestMethod]
      public void MatchCollectionASB()
      {
         Regex asb = new Regex ("a+b");
         
         MatchCollection matches = asb.Matches (AAABBAB);
         
         Assert.AreEqual (2, matches.Count, "matches.Count");
         Assert.AreEqual (0, matches[0].Index, "matches[0].Index");
         Assert.AreEqual (5, matches[1].Index, "matches[1].Index");
      }
      
      [TestMethod]
      public void MatchCollectionAA()
      {
         Regex asb = new Regex ("aa");
         
         MatchCollection matches = asb.Matches (AAABBAB);
         
         Assert.AreEqual (1, matches.Count, "matches.Count");
         Assert.AreEqual (0, matches[0].Index, "matches[0].Index");
      }
      
      [TestMethod]
      public void MatchCollectionAAorBB()
      {
         Regex asb = new Regex ("(aa)|(bb)");
         
         MatchCollection matches = asb.Matches (AAABBAB);
         
         Assert.AreEqual (2, matches.Count, "matches.Count");
         Assert.AreEqual (0, matches[0].Index, "matches[0].Index");
         Assert.AreEqual (3, matches[1].Index, "matches[0].Index");
      }
      
      [TestMethod]
      public void MatchCollectionAAdotBB()
      {
         Regex asb = new Regex ("a.b");
         
         MatchCollection matches = asb.Matches (AAABBAB);
         
         Assert.AreEqual (1, matches.Count, "matches.Count");
         Assert.AreEqual (1, matches[0].Index, "matches[0].Index");
      }
      
      [TestMethod]
      public void MatchCollectionAstarAAdotBB()
      {
         Regex asb = new Regex ("a*a.b");
         
         MatchCollection matches = asb.Matches (AAABBAB);
         
         Assert.AreEqual (1, matches.Count, "matches.Count");
         Assert.AreEqual (0, matches[0].Index, "matches[0].Index");
      }
      
      [TestMethod]
      public void MatchCollectionAAorDotPlus()
      {
         Regex asb = new Regex ("aa|.+");
         
         MatchCollection matches = asb.Matches (AAABBAB);
         
         Assert.AreEqual (2, matches.Count, "matches.Count");
         Assert.AreEqual (0, matches[0].Index, "matches[0].Index");
         Assert.AreEqual (2, matches[1].Index, "matches[0].Index");
      }
      
      [TestMethod]
      public void MatchCollection_Ax5orAx16_plus()
      {
         Regex asb = new Regex ("(A{5}|A{16})+");
         
         MatchCollection matches = asb.Matches (new string ('A', 19));
         
         Assert.AreEqual (1, matches.Count, "matches.Count");
         Assert.AreEqual (0, matches[0].Index, "matches[0].Index");
         Assert.AreEqual (15, matches[0].Length, "matches[0].Length");
      }
      
      [TestMethod]
      public void MatchCollection_Ax5orAx16orAx3_plus()
      {
         Regex asb = new Regex ("(A{5}|A{16}|A{2})+");
         
         MatchCollection matches = asb.Matches (new string ('A', 17));
         
         Assert.AreEqual (1, matches.Count, "matches.Count");
         Assert.AreEqual (0, matches[0].Index, "matches[0].Index");
         Assert.AreEqual (17, matches[0].Length, "matches[0].Length");
      }
      
      [TestMethod]
      public void MatchCollection_OnlyLookAhead()
      {
         Regex asb = new Regex ("(?=ABC)");
         
         MatchCollection matches = asb.Matches ("ABCDEF123456789ABCDEF");
         
         Assert.AreEqual (2, matches.Count, "matches.Count");
         Assert.AreEqual (0, matches[0].Index, "matches[0].Index");
         Assert.AreEqual (0, matches[0].Length, "matches[0].Length");
         Assert.AreEqual (15, matches[1].Index, "matches[0].Index");
         Assert.AreEqual (0, matches[1].Length, "matches[0].Length");
      }
      
      [TestMethod]
      public void MatchCollection_LookAheadNotAny()
      {
         Regex asb = new Regex ("(?!.)");
         
         MatchCollection matches = asb.Matches ("");
         
         Assert.AreEqual (1, matches.Count, "matches.Count");
         Assert.AreEqual (0, matches[0].Index, "matches[0].Index");
         Assert.AreEqual (0, matches[0].Length, "matches[0].Length");
      }
      
      [TestMethod]
      public void MatchCollection_LookAheadNotB()
      {
         Regex asb = new Regex ("(?!b)");
         
         MatchCollection matches = asb.Matches ("A");
         
         Assert.AreEqual (2, matches.Count, "matches.Count");
         Assert.AreEqual (0, matches[0].Index, "matches[0].Index");
         Assert.AreEqual (0, matches[0].Length, "matches[0].Length");
         Assert.AreEqual (1, matches[1].Index, "matches[0].Index");
         Assert.AreEqual (0, matches[1].Length, "matches[0].Length");
      }
   }
}
