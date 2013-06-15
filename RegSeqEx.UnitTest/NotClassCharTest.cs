using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RegSeqEx
{
   [TestClass]
   public class NotClassCharTest
   {
      readonly IClass<Char> notA= new NotClass<Char> (new CharClass ('A'));
      readonly IClass<Char> notFail= new NotClass<Char> (new AssertionFailsClass<char> ());
      
      /// <summary>
      ///Gets or sets the test context which provides
      ///information about and functionality for the current test run.
      ///</summary>
      public TestContext TestContext{get;set;}

      //
      // Constructor
      //
      [TestMethod]
      [ExpectedException(typeof(ArgumentNullException))]
      public void ConstructorThrowsArgumentNullException()
      {
         var expression = new NotClass<char> (null);
      }

      //
      // IsMatch
      //
      [TestMethod]
      public void IsMatchOfNotAReturnsFalseForA()
      {
         Assert.IsFalse (notA.IsMatch ('A'));
      }

      [TestMethod]
      public void IsMatchOfNotAReturnsTrueForB()
      {
         Assert.IsTrue (notA.IsMatch ('B'));
      }

      [TestMethod]
      public void IsMatchOfNotAReturnsTrueForNull()
      {
         Assert.IsTrue (notA.IsMatch ('\0'));
      }

      //
      // IsMatchAt
      //
      [TestMethod]
      public void IsMatchAtOfNotAReturnsFalseForA()
      {
         MatchLength ml = notA.IsMatchAt ("AAAAAAAAA".ToListCursor (), 3);
         Assert.IsFalse (ml.Success);
         Assert.AreEqual (0, ml.Length, "length");
      }

      [TestMethod]
      public void IsMatchAtOfNotAReturnsTrueForB()
      {
         MatchLength ml = notA.IsMatchAt ("BBBBBBBBBB".ToListCursor (), 0);
         Assert.IsTrue (ml.Success);
         Assert.AreEqual (1, ml.Length, "length");
      }

      [TestMethod]
      public void IsMatchAtOfNotAReturnsTrueForNull()
      {
         MatchLength ml = notA.IsMatchAt ("\0\0\0\0\0".ToListCursor (), 0);
         Assert.IsTrue (ml.Success);
         Assert.AreEqual (1, ml.Length, "length");
      }

      [TestMethod]
      public void IsMatchAtReturnsTrueForNull()
      {
         MatchLength ml = notA.IsMatchAt ("\0".ToListCursor (), 0);
         Assert.IsTrue (ml.Success);
         Assert.AreEqual (1, ml.Length, "length");
      }

      [TestMethod]
      [ExpectedException (typeof (ArgumentNullException))]
      public void IsMatchAtThrowsArgumentNullException()
      {
         MatchLength ml = notFail.IsMatchAt (null, 0);
         Assert.IsTrue (ml.Success);
         Assert.AreEqual (1, ml.Length, "length");
      }

      [TestMethod]
      public void IsMatchAtReturnsFalseWhenOutOfRangeBefore()
      {
         MatchLength ml = notFail.IsMatchAt ("".ToListCursor (), -1);
         Assert.IsFalse (ml.Success);
         Assert.AreEqual (0, ml.Length, "length");
      }

      [TestMethod]
      public void IsMatchAtReturnsFalseWhenJustOutOfRange()
      {
         MatchLength ml = notFail.IsMatchAt ("".ToListCursor (), 0);
         Assert.IsFalse (ml.Success);
         Assert.AreEqual (0, ml.Length, "length");
      }

      [TestMethod]
      public void IsMatchAtReturnsFalseWhenOutOfRange()
      {
         MatchLength ml = notFail.IsMatchAt ("".ToListCursor (), int.MaxValue);
         Assert.IsFalse (ml.Success);
         Assert.AreEqual (0, ml.Length, "length");
      }
      
      //
      // Regex Matches tests
      //
      [TestMethod]
      public void RegexMatches_SameFor_NotB()
      {
         IExpression<char> expression = new NotClass<char> (new CharClass ('B'));
         expression.AssertMatches ("AAABAB", @"[^B]");
      }
   }
}
