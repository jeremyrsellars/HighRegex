using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HighRegex
{
   [TestClass]
   public class LookBackExpressionTest
   {
      private readonly IClass<Char> m_any = new AnyClass<char> ();
      private readonly IClass<Char> m_A = new CharClass ('A');
      private readonly IClass<Char> m_B = new CharClass ('B');
      private readonly IClass<Char> [] m_digits = new []
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
      // Constructor LookBackExpressionTest (IExpression<char> [])
      //
      [TestMethod]
      [ExpectedException (typeof(ArgumentNullException))]
      public void ConstructorThrowArgumentNullExceptionForNullArray()
      {
         new LookBackExpression<char> ((IExpression<char>) null);
      }

      [TestMethod]
      public void ConstructorSucceeds()
      {
         var expression = new LookBackExpression<char> (m_A);
         Assert.IsFalse (expression.Negate);
      }


      [TestMethod]
      public void GetMatchesFindsOneMatchForA()
      {
         LookBackExpression<char> expression = new LookBackExpression<char> (m_A);

         string [] expectedValues = new [] {""};
         int index = 1;
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
      public void IsMatchAt5Of_LookBack_Lazy_Any_2_ForDigetsReturnsTrue()
      {
         int index = 5;
         LookBackExpression<char> expression = new LookBackExpression<char> (new RepeatExpression<char> (m_any, 2));
         int length = 0;

         var list = DigetsList;
         var ml = expression.IsMatchAt (list, index);
         Assert.IsTrue (ml.Success, "isMatch");
         Assert.AreEqual (length, ml.Length, "assertionLength");
      }

      [TestMethod]
      public void IsMatchAt1Of_LookBack_Greedy_Any_2_ForDigetsReturnsFalse()
      {
         int index = 1;
         LookBackExpression<char> expression = new LookBackExpression<char> (new GreedyRepeatExpression<char> (m_any, 2));
         int length = 0;

         var list = DigetsList;
         var ml = expression.IsMatchAt (list, index);
         Assert.IsFalse (ml.Success, "isMatch");
         Assert.AreEqual (length, ml.Length, "assertionLength");
      }

      [TestMethod]
      public void GetMatchesFindsNoMatchesForEmpty()
      {
         LookBackExpression<char> expression = new LookBackExpression<char> (m_any);

         var matches = expression.GetMatches (EmptyList, 0);

         Assert.AreEqual (0, matches.Count (), "Count");
      }

      [TestMethod]
      public void GetMatchesOf_LookBack_A_B_2_ForAAABBBReturnsBB()
      {
         int index = 3;
         int length = 2;
         var expression = 
            new ListExpression<char> (
               new LookBackExpression<char> (m_A),
               new RepeatExpression<char> (m_B, length));

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
      public void GetMatchesOf_LookBackAx3_LookBackAny_GreedyAny_B_ForAAABBBReturnsBBB()
      {
         int index = 3;
         int [] lengths = new [] {3,2,1};
         var expression = 
            new ListExpression<char> (
               new LookBackExpression<char> (
                  new RepeatExpression<char> (m_A, 3)),
//               new LookBackExpression<char> (m_any),
               new GreedyRepeatExpression<char> (m_any),
               m_B);

         var list = AAABBB;
         var matches = expression.GetMatches (list, index).ToList ();

         Assert.AreEqual (lengths.Length, matches.Count, "Count");
         for (int matchIndex = 0; matchIndex < matches.Count; matchIndex++)
         {
            var match = matches [matchIndex];
            Assert.AreEqual (index, match.Index, "match.Index");
            Assert.AreEqual (lengths[matchIndex], match.Length, "match.Length");
            Assert.IsTrue (match.Success, "match.Success");
            Assert.AreEqual (lengths[matchIndex], match.Items.Count, "match.Items.Count at");
            for (int i = 0; i < lengths[matchIndex]; i++)
            {
               Assert.AreEqual (list[index + i], match.Items [i], "match.Items [" + i + "]");
            }
         }
      }

      [TestMethod]
      public void GetMatchesOf_LookBack_5orAAA_GreedyAny_B_ForAAABBBReturnsBBB()
      {
         int index = 3;
         int [] lengths = new []{3,2,1};
         var expression = 
            new ListExpression<char> (
               new LookBackExpression<char> (
                  new AlternationExpression<char> (
                     m_digits [5],
                     new RepeatExpression<char> (m_A, 3))),
               new GreedyRepeatExpression<char> (m_any),
               m_B);

         var list = AAABBB;
         var matches = expression.GetMatches (list, index).ToList ();

         Assert.AreEqual (lengths.Length, matches.Count, "Count");
         for (int matchIndex = 0; matchIndex < matches.Count; matchIndex++)
         {
            var match = matches [matchIndex];
            Assert.AreEqual (index, match.Index, "match.Index");
            Assert.AreEqual (lengths[matchIndex], match.Length, "match.Length");
            Assert.IsTrue (match.Success, "match.Success");
            Assert.AreEqual (lengths[matchIndex], match.Items.Count, "match.Items.Count at");
            for (int i = 0; i < lengths[matchIndex]; i++)
            {
               Assert.AreEqual (list[index + i], match.Items [i], "match.Items [" + i + "]");
            }
         }
      }

      [TestMethod]
      public void GetMatchesOf_LookBack_Start_GreedyAny_B_ForAAABBBReturnsAAABBB()
      {
         int index = 0;
         int [] lengths = new []{6,5,4};
         var expression = 
            new ListExpression<char> (
               new LookBackExpression<char> (
                  new StartExpression<char> ()),
               new GreedyRepeatExpression<char> (m_any),
               m_B);

         var list = AAABBB;
         var matches = expression.GetMatches (list, index).ToList ();

         Assert.AreEqual (lengths.Length, matches.Count, "Count");
         for (int matchIndex = 0; matchIndex < matches.Count; matchIndex++)
         {
            var match = matches [matchIndex];
            Assert.AreEqual (index, match.Index, "match.Index");
            Assert.AreEqual (lengths[matchIndex], match.Length, "match.Length");
            Assert.IsTrue (match.Success, "match.Success");
            Assert.AreEqual (lengths[matchIndex], match.Items.Count, "match.Items.Count at");
            for (int i = 0; i < lengths[matchIndex]; i++)
            {
               Assert.AreEqual (list[index + i], match.Items [i], "match.Items [" + i + "]");
            }
         }
      }

      [TestMethod]
      public void GetMatchesOf_LookBack_List_AB_GreedyAny_B_ForAAABBBReturnsBB()
      {
         int index = 4;
         int [] lengths = {2,1};
         var expression = 
            new ListExpression<char> (
               new LookBackExpression<char> (
                  new ListExpression<char> (
                     m_A, m_B)),
               new GreedyRepeatExpression<char> (m_any),
               m_B);

         var list = AAABBB;
         var matches = expression.GetMatches (list, index).ToList ();

         Assert.AreEqual (lengths.Length, matches.Count (), "Count");
         for(int matchIndex = 0; matchIndex < lengths.Length; matchIndex++)
         {
            var match = matches[matchIndex];
            Assert.AreEqual (index, match.Index, "match.Index");
            Assert.AreEqual (lengths[matchIndex], match.Length, "match.Length");
            Assert.IsTrue (match.Success, "match.Success");
            Assert.AreEqual (lengths[matchIndex], match.Items.Count, "match.Items.Count at");
            for (int i = 0; i < lengths[matchIndex]; i++)
            {
               Assert.AreEqual (list[index + i], match.Items [i], "match.Items [" + i + "]");
            }
         }
      }

      [TestMethod]
      public void GetPossibleMatchLengthsOf_List_AB_LookAhead_BB_NegativeLookAhead_AA_At_4_Returns2()
      {
         int index = 4;
         int [] lengths = {1};
         var expression = 
//            new ListExpression<char> (
//               new LookBackExpression<char> (
                  new ListExpression<char> (
                     m_A, 
                     m_B, 
                     new LookAheadExpression<char> (new RepeatExpression<char>(m_B,2)), 
                     new NegativeLookAheadExpression<char> (new RepeatExpression<char>(m_A,2))
                     )
//                     ),
//               new GreedyRepeatExpression<char> (m_any)//,
//            )
//               m_B)
;

         var possibleLengths = expression.GetPossibleMatchLengths (index).ToList ();
         Assert.AreEqual (1, possibleLengths.Count, "Count");
         Assert.AreEqual (2, possibleLengths[0], "PossibleLength[0]");
      }

      [TestMethod]
      public void GetMatchesOf_LookBack_List_AB_LookAhead_BB_GreedyAny_B_ForAAABBBReturnsBB()
      {
         int index = 4;
         int [] lengths = {2,1};
         var expression = 
            new ListExpression<char> (
               new LookBackExpression<char> (
                  new ListExpression<char> (
                     m_A, 
                     m_B, 
                     new LookAheadExpression<char> (new RepeatExpression<char>(m_B,2)), 
                     new NegativeLookAheadExpression<char> (new RepeatExpression<char>(m_A,2))
                     )),
               new GreedyRepeatExpression<char> (m_any),
               m_B);

         var list = AAABBB;
         var matches = expression.GetMatches (list, index).ToList ();

         Assert.AreEqual (lengths.Length, matches.Count (), "Count");
         for(int matchIndex = 0; matchIndex < lengths.Length; matchIndex++)
         {
            var match = matches[matchIndex];
            Assert.AreEqual (index, match.Index, "match.Index");
            Assert.AreEqual (lengths[matchIndex], match.Length, "match.Length");
            Assert.IsTrue (match.Success, "match.Success");
            Assert.AreEqual (lengths[matchIndex], match.Items.Count, "match.Items.Count at");
            for (int i = 0; i < lengths[matchIndex]; i++)
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
            new LookBackExpression<char> (
               new MockExpression<char> {AnyLength = false, PossibleMatchLengths = new int[0], SupportsLookBack = true});

         var actual = expression.GetPossibleMatchLengths (100).ToList ();
         Assert.AreEqual (0, actual.Count, "Count");
      }

      [TestMethod]
      public void GetPossibleMatchLengthsReturns0ForOne ()
      {
         var expression =
            new LookBackExpression<char> (
               new MockExpression<char> {AnyLength = false, PossibleMatchLengths = new []{1}, SupportsLookBack = true});

         var actual = expression.GetPossibleMatchLengths (100).ToList ();
         Assert.AreEqual (1, actual.Count, "Count");
         Assert.AreEqual (0, actual[0], "value at [0]");
      }

      [TestMethod]
      public void GetPossibleMatchLengthsReturns0ForTwoThreeFive ()
      {
         var expression =
            new LookBackExpression<char> (
               new MockExpression<char> {AnyLength = false, PossibleMatchLengths = new []{2,3,5}, SupportsLookBack = true});

         var actual = expression.GetPossibleMatchLengths (100).ToList ();
         Assert.AreEqual (1, actual.Count, "Count");
         Assert.AreEqual (0, actual[0], "value at [0]");
      }

   }
}
