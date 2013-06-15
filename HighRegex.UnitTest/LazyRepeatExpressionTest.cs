using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HighRegex
{
   [TestClass]
   public class LazyRepeatExpressionTest
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

      MockExpression<char> LookBackSupported = new MockExpression<char>{SupportsLookBack = true};
      MockExpression<char> LookBackNotSupported = new MockExpression<char>{SupportsLookBack = false};
      MockExpression<char> AnyLength = new MockExpression<char>{AnyLength = true};
      MockExpression<char> NotAnyLength = new MockExpression<char>{AnyLength = false};
      MockExpression<char> MatchLength0 = new MockExpression<char>{SupportsLookBack=true, PossibleMatchLengths=new []{0}};
      MockExpression<char> MatchLength1 = new MockExpression<char>{SupportsLookBack=true, PossibleMatchLengths=new []{1}};
      MockExpression<char> MatchLength2 = new MockExpression<char>{SupportsLookBack=true, PossibleMatchLengths=new []{2}};
      MockExpression<char> MatchLength3 = new MockExpression<char>{SupportsLookBack=true, PossibleMatchLengths=new []{3}};

      MockExpression<char> MatchLengths3to5 = new MockExpression<char>{SupportsLookBack=true, PossibleMatchLengths=new []{3,4,5}};
      
      /// <summary>
      ///Gets or sets the test context which provides
      ///information about and functionality for the current test run.
      ///</summary>
      public TestContext TestContext{get;set;}

      //
      // Constructor RepeatExpressionTest (IExpression<char> [])
      //
      [TestMethod]
      [ExpectedException (typeof(ArgumentNullException))]
      public void ConstructorThrowArgumentNullExceptionForNullArray()
      {
         var expression = new RepeatExpression<char> ((IExpression<char>) null);
      }

      [TestMethod]
      public void ConstructorSucceeds()
      {
         int min = 0;
         int max = int.MaxValue;
         var expression = new RepeatExpression<char> (m_A);
         Assert.AreEqual (min, expression.Minimum, "Minimum");
         Assert.AreEqual (max, expression.Maximum, "Maximum");
      }

      //
      // Constructor RepeatExpressionTest (IExpression<char> [], exact)
      //
      [TestMethod]
      [ExpectedException (typeof(ArgumentNullException))]
      public void ExactConstructorThrowArgumentNullExceptionForNullArray()
      {
         var expression = new RepeatExpression<char> ((IExpression<char>) null, 2);
      }

      [TestMethod]
      [ExpectedException (typeof(ArgumentOutOfRangeException))]
      public void ExactConstructorThrowArgumentOutOfRangeException()
      {
         var expression = new RepeatExpression<char> (m_any, -1);
      }

      [TestMethod]
      public void ExactConstructorSucceeds()
      {
         int min = 2;
         int max = min;
         var expression = new RepeatExpression<char> (m_A, min);
         Assert.AreEqual (min, expression.Minimum, "Minimum");
         Assert.AreEqual (max, expression.Maximum, "Maximum");
      }

      //
      // Constructor RepeatExpressionTest (IExpression<char> [], min, max)
      //
      [TestMethod]
      [ExpectedException (typeof(ArgumentNullException))]
      public void RangeConstructorThrowArgumentNullExceptionForNullArray()
      {
         var expression = new RepeatExpression<char> ((IExpression<char>) null);
      }

      [TestMethod]
      [ExpectedException (typeof(ArgumentOutOfRangeException))]
      public void RangeConstructorArgumentOutOfRangeExceptionForNegativeMinimum()
      {
         var expression = new RepeatExpression<char> (m_any, -1, 23);
      }

      [TestMethod]
      [ExpectedException (typeof(ArgumentOutOfRangeException))]
      public void RangeConstructorArgumentOutOfRangeExceptionForNegativeMax()
      {
         var expression = new RepeatExpression<char> (m_any, 1, -1);
      }

      [TestMethod]
      [ExpectedException (typeof(ArgumentException))]
      public void RangeConstructorArgumentExceptionForMaximumLessThanMinimum()
      {
         var expression = new RepeatExpression<char> (m_any, 4, 3);
      }

      [TestMethod]
      public void RangeConstructorSucceeds()
      {
         int min = 2;
         int max = min;
         var expression = new RepeatExpression<char> (m_A, min, max);
         Assert.AreEqual (min, expression.Minimum, "Minimum");
         Assert.AreEqual (max, expression.Maximum, "Maximum");
      }

      [TestMethod]
      public void GetMatchesFindsOneMatchForA()
      {
         RepeatExpression<char> expression = new RepeatExpression<char> (m_A);

         int index = 0;
         var list = AList;
         var matches = expression.GetMatches (list, index).ToList ();

         Assert.AreEqual (2, matches.Count (), "Count");
         Assert.AreEqual (0, matches.First ().Length, "First match should be empty");
         var match = matches.ElementAt(1);
         Assert.AreEqual (index, match.Index, "match.Index");
         Assert.AreEqual (1, match.Length, "match.Length");
         Assert.IsTrue (match.Success, "match.Success");
         Assert.AreEqual (1, match.Items.Count, "match.Items.Count");
         Assert.AreEqual (list[index], match.Items [0], "match.Items [0]");
      }

      [TestMethod]
      public void GetMatchesOf_Any_2_ForDigetsStartingAt5Returns56()
      {
         int index = 5;
         RepeatExpression<char> expression = new RepeatExpression<char> (m_any, 2);
         int length = 2;

         var list = DigetsList;
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
      public void GetMatchesFindsOneMatchesForEmpty()
      {
         RepeatExpression<char> expression = new RepeatExpression<char> (m_any);

         var matches = expression.GetMatches (EmptyList, 0).ToList ();

         Assert.AreEqual (1, matches.Count (), "Count");
         Assert.AreEqual (0, matches[0].Index, "Count");
         Assert.AreEqual (0, matches[0].Length, "Count");
      }

      [TestMethod]
      public void GetMatchesOfAnyQuestionStar_LikeRegex()
      {
         RepeatExpression<char> expression = new RepeatExpression<char> (m_any);

         expression.AssertMatches ("", ".*?");
      }

      [TestMethod]
      public void GetMatchesOf_A_3_ForAAABBBReturnsAAA()
      {
         int index = 0;
         int length = 3;
         RepeatExpression<char> expression = new RepeatExpression<char> (m_A, length);

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
      public void GetMatchesOf_A_2_ForAAABBBReturnsAA()
      {
         int index = 0;
         int length = 2;
         RepeatExpression<char> expression = new RepeatExpression<char> (m_A, length);

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
      public void GetMatchesOf_AorB_0_4_ForAAABBB()
      {
         int index = 0;
         int length = 4;
         RepeatExpression<char> expression = 
            new RepeatExpression<char> (
               new AlternationExpression<char> (
                  m_A, m_B),
               length);

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
            index++;
         }
      }

      [TestMethod]
      public void GetMatchesOf_AAorAB_0_4_ForAAABBB()
      {
         int index = 0;
         RepeatExpression<char> expression = 
            new RepeatExpression<char> (
               new AlternationExpression<char> (
                  new ListExpression<char>(m_A, m_A),
                  new ListExpression<char>(m_A, m_B, m_B, m_B)),
               0, 4);

         var list = AAABBB;
         var matches = expression.GetMatches (list, index).ToList ();

         string [] expectedValues = new [] {"", "AA", "AAABBB"};
         Assert.AreEqual (expectedValues.Length, matches.Count (), "Count");
         for (int i = 0; i < expectedValues.Length; i++)
         {
            var match = matches [i];
            string expected = expectedValues [i];
            Assert.AreEqual (index, match.Index, "match.Index at " + i);
            Assert.AreEqual (expected.Length, match.Length, "match.Length at " + i);
            Assert.IsTrue (match.Success, "match.Success at " + i);
            Assert.AreEqual (expected.Length, match.Items.Count, "match.Items.Count at " + i);
            
            Assert.AreEqual (expected, new string (match.Items.ToArray ()), "Value at " + i);
         }
      }

      [TestMethod]
      public void GetMatchesOf_AAorABBB_1_4_ForAAABBB()
      {
         int index = 0;
         RepeatExpression<char> expression = 
            new RepeatExpression<char> (
               new AlternationExpression<char> (
                  new ListExpression<char>(m_A, m_A),
                  new ListExpression<char>(m_A, m_B, m_B, m_B)),
               1, 4);

         var list = AAABBB;
         var matches = expression.GetMatches (list, index).ToList ();

         string [] expectedValues = new [] {"AA", "AAABBB"};
         Assert.AreEqual (expectedValues.Length, matches.Count (), "Count");
         for (int i = 0; i < expectedValues.Length; i++)
         {
            var match = matches [i];
            var expected = expectedValues [i];
            Assert.AreEqual (index, match.Index, "match.Index at " + i);
            Assert.AreEqual (expected.Length, match.Length, "match.Length at " + i);
            Assert.IsTrue (match.Success, "match.Success at " + i);
            Assert.AreEqual (expected, new string (match.Items.ToArray ()), "Value at " + i);
         }
      }

      [TestMethod]
      public void GetMatchesOf_AAorABorBB_1_2_ForAAABBB()
      {
         int index = 0;
         RepeatExpression<char> expression = 
            new RepeatExpression<char> (
               new AlternationExpression<char> (
                  new ListExpression<char>(m_A, m_A),
                  new ListExpression<char>(m_A, m_B),
                  new ListExpression<char>(m_B, m_B)),
               1, 2);

         var list = AAABBB;
         var matches = expression.GetMatches (list, index).ToList ();

         string [] expectedValues = new [] {"AA", "AAAB"};
         Assert.AreEqual (expectedValues.Length, matches.Count (), "Count");
         for (int i = 0; i < expectedValues.Length; i++)
         {
            var match = matches [i];
            var expected = expectedValues [i];
            Assert.AreEqual (index, match.Index, "match.Index at " + i);
            Assert.AreEqual (expected.Length, match.Length, "match.Length at " + i);
            Assert.IsTrue (match.Success, "match.Success at " + i);
            Assert.AreEqual (expected.Length, match.Items.Count, "match.Items.Count at " + i);
            
            Assert.AreEqual (expected, new string (match.Items.ToArray ()), "Value at " + i);
         }
      }

      [TestMethod]
      public void GetMatchesOfDigits_Any_4_8()
      {
         var expression = new RepeatExpression<char> (m_any, 4, 8);
         
         string [] expectedValues = new [] {"0123", "01234", "012345", "0123456", "01234567"};
         
         int index = 0;
         var list = DigetsList;
         
         var matches = expression.GetMatches (list, index).ToList ();

         Assert.AreEqual (expectedValues.Length, matches.Count (), "Count");
         for (int i = 0; i < expectedValues.Length; i++)
         {
            var match = matches [i];
            var expected = expectedValues [i];
            Assert.AreEqual (index, match.Index, "match.Index at " + i);
            Assert.AreEqual (expected.Length, match.Length, "match.Length at " + i);
            Assert.IsTrue (match.Success, "match.Success at " + i);
            Assert.AreEqual (expected.Length, match.Items.Count, "match.Items.Count at " + i);
            Assert.AreEqual (expected, new string (match.Items.ToArray ()), "Value at " + i);
         }
      }

      [TestMethod]
      public void GetMatchesOf_A_3_BForAAABBBReturnsAAAB()
      {
         int index = 0;
         int length = 4;
         var expression = 
            new ListExpression<char> (
               new RepeatExpression<char> (m_A, 3),
               m_B);

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

      [TestMethod]
      public void GetMatchesOf_A_3_BForAAABBB_GetEnumerator_MoveNextTooManyTimesThrows()
      {
         int index = 0;
         var expression = 
            new ListExpression<char> (
               new RepeatExpression<char> (m_A, 3),
               m_B);

         var list = AAABBB;
         var matches = expression.GetMatches (list, index).ToList ();
         var enumerator = matches.GetEnumerator ();
         var current = enumerator.Current;
         Assert.IsNull (current, "current");
         for (int i = 0; i < 100; i++)
         {
            enumerator.MoveNext ();
         }
      }

      [TestMethod]
      public void GetMatchesOf_A_ForAAABBB_LikeRegex()
      {
         var expression = new RepeatExpression<char> (m_A);

         expression.AssertMatches ("AAABBB", "A*?");
      }

      //
      // SupportsLookBack tests
      //
      [TestMethod]
      public void SupportsLookBack_For_LookBackSupported ()
      {
         var expression = new RepeatExpression<char> (LookBackSupported, 2,4);
         Assert.IsTrue (expression.SupportsLookBack);
      }

      [TestMethod]
      public void NotSupportsLookBack_For_LookBackNotSupported ()
      {
         var expression = new RepeatExpression<char> (LookBackNotSupported);
         Assert.IsFalse (expression.SupportsLookBack);
      }

      [TestMethod]
      public void NotSupportsLookBack_For_LookBackSupported_LookBackNotSupported ()
      {
         var expression = new RepeatExpression<char> (new AlternationExpression<char> (LookBackSupported, LookBackNotSupported), 2);
         Assert.IsFalse (expression.SupportsLookBack);
      }

      //
      // AnyLength tests
      //
      [TestMethod]
      public void AnyLength_For_AnyLength ()
      {
         var expression = new RepeatExpression<char> (AnyLength, 2);
         Assert.IsTrue (expression.AnyLength);
      }

      [TestMethod]
      public void NotAnyLength_For_NotAnyLength ()
      {
         var expression = new RepeatExpression<char> (NotAnyLength, 2);
         Assert.IsFalse (expression.AnyLength);
      }

      [TestMethod]
      public void AnyLength_For_AnyLength_NotAnyLength ()
      {
         var expression = new RepeatExpression<char> (
            new ListExpression<char> (AnyLength, NotAnyLength));
         Assert.IsTrue (expression.AnyLength);
      }

      //
      // MatchLength tests
      //
      [TestMethod]
      public void GetPossibleMatchLengths_ForEmpty ()
      {
         var expected = new int [] {0};
         var expression = new RepeatExpression<char> (new AlternationExpression<char>());
         expression.AssertPossibleMatchLengths (expected);
      }

      [TestMethod]
      public void GetPossibleMatchLengths_For1_4to6 ()
      {
         var expected = new int [] {4,5,6};
         var expression = new RepeatExpression<char> (MatchLength1, 4, 6);
         expression.AssertPossibleMatchLengths (expected);
      }

      [TestMethod]
      public void GetPossibleMatchLengths_For1_1 ()
      {
         var expected = new int [] {2};
         var expression = new RepeatExpression<char> (MatchLength1,2);
         expression.AssertPossibleMatchLengths (expected);
      }

      [TestMethod]
      public void GetPossibleMatchLengths_For_2_3to4 ()
      {
         var expected = new int [] {6,8};
         var expression = new RepeatExpression<char> (MatchLength2, 3,4);
         expression.AssertPossibleMatchLengths (expected);
      }

      [TestMethod]
      public void GetPossibleMatchLengths_For_3to5_2to3 ()
      {
         var expected = new int [] {3+3,3+3+3,3+3+4,3+3+5,3+4,3+4+5,3+5,3+5+5,4+5+5,5+5+5};
         var expression = new RepeatExpression<char> (MatchLengths3to5, 2,3);
         expression.AssertPossibleMatchLengths (expected);
      }

   }
}
