using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HighRegex
{
   [TestClass]
   public class NegativeLookAheadExpressionTest
   {
      private IClass<Char> m_any = new AnyClass<char> ();
      private IClass<Char> m_A = new CharClass ('A');
      private IClass<Char> m_B = new CharClass ('B');
      private IClass<Char> [] m_digits = new []
      {
         new CharClass ('0'),
         new CharClass ('1'),
         new CharClass ('2'),
         new CharClass ('3'),
         new CharClass ('4'),
         new CharClass ('5'),
         new CharClass ('6'),
         new CharClass ('7'),
         new CharClass ('8'),
         new CharClass ('9'),
      };

      public ListExpressionItemSource<char> EmptyList = "".ToListCursor ();
      public ListExpressionItemSource<char> AList = "A".ToListCursor ();
      public ListExpressionItemSource<char> DigetsList = "0123456789".ToListCursor ();
      public ListExpressionItemSource<char> AAABBB = "AAABBB".ToListCursor ();

      
      /// <summary>
      ///Gets or sets the test context which provides
      ///information about and functionality for the current test run.
      ///</summary>
      public TestContext TestContext{get;set;}

      //
      // Constructor NegativeLookAheadExpressionTest (IExpression<char> [])
      //
      [TestMethod]
      [ExpectedException (typeof(ArgumentNullException))]
      public void ConstructorThrowArgumentNullExceptionForNullArray()
      {
         var expression = new NegativeLookAheadExpression<char> ((IExpression<char>) null);
      }

      [TestMethod]
      public void ConstructorSucceeds()
      {
         var expression = new NegativeLookAheadExpression<char> (m_A);
         Assert.IsTrue (expression.Negate);
      }


      [TestMethod]
      public void GetMatchesFindsOneMatchForB()
      {
         NegativeLookAheadExpression<char> expression = new NegativeLookAheadExpression<char> (m_B);

         string [] expectedValues = new [] {"",""};
         int index = 0;
         var list = AList;
         var matches = expression.GetMatches (list, index).ToList ();

         Assert.AreEqual (expectedValues.Length, matches.Count (), "Count");
         
         for (int i = 0; i < expectedValues.Length; i++)
         {
            var match = matches [i];
            var expected = expectedValues [i];
            Assert.AreEqual (index, match.Index, "match.Index");
            Assert.AreEqual (expected.Length, match.Length, "match.Length");
            Assert.IsTrue (match.Success, "match.Success");
            Assert.AreEqual (expected.Length, match.Items.Count, "match.Items.Count");
            Assert.AreEqual (expected, new string (match.Items.ToArray ()), "match.Items");
            index++;
         }
      }

      [TestMethod]
      public void IsMatchAt9Of_NegativeLookAhead_Greedy_Any_2_ForDigetsReturnsTrue()
      {
         int index = 9;
         NegativeLookAheadExpression<char> expression = new NegativeLookAheadExpression<char> (new GreedyRepeatExpression<char> (m_any, 2));
         int length = 0;

         var list = DigetsList;
         var ml = expression.IsMatchAt (list, index);
         Assert.IsTrue (ml.Success, "isMatch");
         Assert.AreEqual (length, ml.Length, "assertionLength");
      }

      [TestMethod]
      public void IsMatchAt5Of_NegativeLookAhead_Greedy_Any_2_ForDigetsReturnsFalse()
      {
         int index = 5;
         NegativeLookAheadExpression<char> expression = new NegativeLookAheadExpression<char> (new GreedyRepeatExpression<char> (m_any, 2));
         int length = 0;

         var list = DigetsList;
         var ml = expression.IsMatchAt (list, index);
         Assert.IsFalse (ml.Success, "isMatch");
         Assert.AreEqual (length, ml.Length, "assertionLength");
      }

      [TestMethod]
      public void GetMatchesFindsNoMatchesForEmpty()
      {
         NegativeLookAheadExpression<char> expression = new NegativeLookAheadExpression<char> (m_any);

         var matches = expression.GetMatches (EmptyList, 0).ToList ();

         Assert.AreEqual (1, matches.Count, "matches.Count");
         Assert.AreEqual (0, matches[0].Index, "matches[0].Index");
         Assert.AreEqual (0, matches[0].Length, "matches[0].Length");

      }

      [TestMethod]
      public void GetMatchesOf_GreedyA_NegativeLookAhead_B_ForAAABBBReturnsA()
      {
         var expression = 
            new ListExpression<char> (
               new GreedyRepeatExpression<char> (m_A),
               new NegativeLookAheadExpression<char> (m_B));

         expression.AssertMatches ("AAABBB", @"A*(?!B)");
      }

      [TestMethod]
      public void GetMatchesOf_GreedyA_NegativeLookAheadBx3_ForAAABBBReturnsAA()
      {
         //string [] expectedValues = new [] {"AA", "A", ""};
         //int index = 0;
         var expression = 
            new ListExpression<char> (
               new GreedyRepeatExpression<char> (m_A),
               new NegativeLookAheadExpression<char> (
                  new RepeatExpression<char> (m_B, 3)));

         expression.AssertMatches ("AAABBB", @"A*(?!B{3})");
         //var list = AAABBB;
         //var matches = expression.GetMatches (list, index).ToList ();

         //Assert.AreEqual (expectedValues.Length, matches.Count (), "Count");
         //for (int i = 0; i < matches.Count; i++)
         //{
         //   var match = matches[i];
         //   int length = expectedValues[i].Length;
         //   Assert.AreEqual (index, match.Index, "match.Index");
         //   Assert.AreEqual (length, match.Length, "match.Length");
         //   Assert.IsTrue (match.Success, "match.Success");
         //   Assert.AreEqual (expectedValues[i], new string (match.Items.ToArray()), "match.Items.Count at");
         //}
      }

   }
}
