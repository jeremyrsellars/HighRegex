using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RegSeqEx
{
   [TestClass]
   public class LookAheadExpressionTest
   {
      private IClass<Char> m_any = new AnyClass<char> ();
      private IClass<Char> m_a = new CharClass ('A');
      private IClass<Char> m_b = new CharClass ('B');
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
      // Constructor LookAheadExpressionTest (IExpression<char> [])
      //
      [TestMethod]
      [ExpectedException (typeof(ArgumentNullException))]
      public void ConstructorThrowArgumentNullExceptionForNullArray()
      {
         var expression = new LookAheadExpression<char> ((IExpression<char>) null);
      }

      [TestMethod]
      public void ConstructorSucceeds()
      {
         var expression = new LookAheadExpression<char> (m_a);
         Assert.IsFalse (expression.Negate);
      }


      [TestMethod]
      public void GetMatchesFindsOneMatchForA()
      {
         LookAheadExpression<char> expression = new LookAheadExpression<char> (m_a);

         string [] expectedValues = new [] {""};
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
         }
      }

      [TestMethod]
      public void IsMatchAt5Of_LookAhead_Greedy_Any_2_ForDigetsReturnsTrue()
      {
         int index = 5;
         LookAheadExpression<char> expression = new LookAheadExpression<char> (new GreedyRepeatExpression<char> (m_any, 2));
         int length = 0;

         var list = DigetsList;
         var ml = expression.IsMatchAt (list, index);
         Assert.IsTrue (ml.Success, "isMatch");
         Assert.AreEqual (length, ml.Length, "assertionLength");
      }

      [TestMethod]
      public void IsMatchAt9Of_LookAhead_Greedy_Any_2_ForDigetsReturnsFalse()
      {
         int index = 9;
         LookAheadExpression<char> expression = new LookAheadExpression<char> (new GreedyRepeatExpression<char> (m_any, 2));
         int length = 0;

         var list = DigetsList;
         var ml = expression.IsMatchAt (list, index);
         Assert.IsFalse (ml.Success, "isMatch");
         Assert.AreEqual (length, ml.Length, "assertionLength");
      }

      [TestMethod]
      public void GetMatchesFindsNoMatchesForEmpty()
      {
         LookAheadExpression<char> expression = new LookAheadExpression<char> (m_any);

         var matches = expression.GetMatches (EmptyList, 0);

         Assert.AreEqual (0, matches.Count (), "Count");
      }

      [TestMethod]
      public void GetMatchesOf_A_3_LookAhead_B_ForAAABBBReturnsAAA()
      {
         int index = 0;
         int length = 3;
         var expression = 
            new ListExpression<char> (
               new RepeatExpression<char> (m_a, length),
               new LookAheadExpression<char> (m_b));

         var list = AAABBB;
         var matches = expression.GetMatches (list, index).ToList ();

         Assert.AreEqual (1, matches.Count (), "Count");
         foreach (var match in matches)
         {
            Assert.AreEqual (index, match.Index, "match.Index");
            Assert.AreEqual (length, match.Length, "match.Length");
            Assert.IsTrue (match.Success, "match.Success");
            Assert.AreEqual (length, match.Items.Count, "match.Items.Count");
            for (int i = 0; i < length; i++)
            {
               Assert.AreEqual (list[index + i], match.Items [i], "match.Items [" + i + "]");
            }
         }
      }

      [TestMethod]
      public void GetMatchesOf_GreedyAny_LookAheadBx3_LookAheadAny_B_ForAAABBBReturnsAAAB()
      {
         int index = 0;
         int length = 4;
         var expression = 
            new ListExpression<char> (
               new GreedyRepeatExpression<char> (m_any),
               new LookAheadExpression<char> (
                  new RepeatExpression<char> (m_b, 3)),
               new LookAheadExpression<char> (m_any),
               m_b);

         var list = AAABBB;
         var matches = expression.GetMatches (list, index).ToList ();

         Assert.AreEqual (1, matches.Count (), "Count");
         foreach (var match in matches)
         {
            Assert.AreEqual (index, match.Index, "match.Index");
            Assert.AreEqual (length, match.Length, "match.Length");
            Assert.IsTrue (match.Success, "match.Success");
            Assert.AreEqual (length, match.Items.Count, "match.Items.Count at");
            for (int i = 0; i < length; i++)
            {
               Assert.AreEqual (list[index + i], match.Items [i], "match.Items [" + i + "]");
            }
         }
      }

      //
      // GetPossibleMatchLength tests
      //
      [TestMethod]
      public void GetPossibleMatchLengthsReturnsNoneForNone ()
      {
         var expression =
            new LookAheadExpression<char> (
               new MockExpression<char> {AnyLength = false, PossibleMatchLengths = new int[0], SupportsLookBack = true});

         var actual = expression.GetPossibleMatchLengths (100).ToList ();
         Assert.AreEqual (0, actual.Count, "Count");
      }

      [TestMethod]
      public void GetPossibleMatchLengthsReturns0ForOne ()
      {
         var expression =
            new LookAheadExpression<char> (
               new MockExpression<char> {AnyLength = false, PossibleMatchLengths = new []{1}, SupportsLookBack = true});

         var actual = expression.GetPossibleMatchLengths (100).ToList ();
         Assert.AreEqual (1, actual.Count, "Count");
         Assert.AreEqual (0, actual[0], "value at [0]");
      }

      [TestMethod]
      public void GetPossibleMatchLengthsReturns0ForTwoThreeFive ()
      {
         var expression =
            new LookAheadExpression<char> (
               new MockExpression<char> {AnyLength = false, PossibleMatchLengths = new []{2,3,5}, SupportsLookBack = true});

         var actual = expression.GetPossibleMatchLengths (100).ToList ();
         Assert.AreEqual (1, actual.Count, "Count");
         Assert.AreEqual (0, actual[0], "value at [0]");
      }

   }
}
