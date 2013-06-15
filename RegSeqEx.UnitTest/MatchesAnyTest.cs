using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RegSeqEx
{
   [TestClass]
   public class MatchesAnyTest
   {
      readonly IClass<Char> expression = new AnyClass<Char> ();
      
      /// <summary>
      ///Gets or sets the test context which provides
      ///information about and functionality for the current test run.
      ///</summary>
      public TestContext TestContext{get;set;}

      [TestMethod]
      public void IsMatchReturnsTrueForA()
      {
         Assert.IsTrue (expression.IsMatch ('A'));
      }

      [TestMethod]
      public void IsMatchReturnsTrueForNull()
      {
         Assert.IsTrue (expression.IsMatch ('\0'));
      }

      [TestMethod]
      public void IsMatchAtReturnsTrueForA()
      {
         MatchLength ml = expression.IsMatchAt ("A".ToListCursor (), 0);
         Assert.IsTrue (ml.Success);
         Assert.AreEqual (1, ml.Length, "length");
      }

      [TestMethod]
      public void IsMatchAtReturnsTrueForNull()
      {
         MatchLength ml = expression.IsMatchAt ("\0".ToListCursor (), 0);
         Assert.IsTrue (ml.Success);
         Assert.AreEqual (1, ml.Length, "length");
      }

      [TestMethod]
      [ExpectedException (typeof (ArgumentNullException))]
      public void IsMatchAtThrowsArgumentNullException()
      {
         MatchLength ml = expression.IsMatchAt (null, 0);
         Assert.IsTrue (ml.Success);
         Assert.AreEqual (1, ml.Length, "length");
      }

      [TestMethod]
      public void IsMatchAtReturnsFalseWhenOutOfRangeBefore()
      {
         MatchLength ml = expression.IsMatchAt ("".ToListCursor (), -1);
         Assert.IsFalse (ml.Success);
         Assert.AreEqual (0, ml.Length, "length");
      }

      [TestMethod]
      public void IsMatchAtReturnsFalseWhenJustOutOfRange()
      {
         MatchLength ml = expression.IsMatchAt ("".ToListCursor (), 0);
         Assert.IsFalse (ml.Success);
         Assert.AreEqual (0, ml.Length, "length");
      }

      [TestMethod]
      public void IsMatchAtReturnsFalseWhenOutOfRange()
      {
         MatchLength ml = expression.IsMatchAt ("".ToListCursor (), int.MaxValue);
         Assert.IsFalse (ml.Success);
         Assert.AreEqual (0, ml.Length, "length");
      }
   }
}
