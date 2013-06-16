using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HighRegex
{
   [TestClass]
   public class ListExpressionTest
   {
      private IClass<Char> m_any = new AnyClass<char> ();
      private IClass<Char> m_a = new CharClass ('A');
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

      MockExpression<char> LookBackSupported = new MockExpression<char>{SupportsLookBack = true};
      MockExpression<char> LookBackNotSupported = new MockExpression<char>{SupportsLookBack = false};
      MockExpression<char> AnyLength = new MockExpression<char>{AnyLength = true};
      MockExpression<char> NotAnyLength = new MockExpression<char>{AnyLength = false};
      MockExpression<char> MatchLength0 = new MockExpression<char>{SupportsLookBack=true, PossibleMatchLengths=new []{0}};
      MockExpression<char> MatchLength1 = new MockExpression<char>{SupportsLookBack=true, PossibleMatchLengths=new []{1}};
      MockExpression<char> MatchLength2 = new MockExpression<char>{SupportsLookBack=true, PossibleMatchLengths=new []{2}};
      MockExpression<char> MatchLength3 = new MockExpression<char>{SupportsLookBack=true, PossibleMatchLengths=new []{3}};

      MockExpression<char> MatchLengths3to5 = new MockExpression<char>{SupportsLookBack=true, PossibleMatchLengths=new []{3,4,5}};
      MockExpression<char> MatchLengths2or8 = new MockExpression<char>{SupportsLookBack=true, PossibleMatchLengths=new []{2,8}};

      
      /// <summary>
      ///Gets or sets the test context which provides
      ///information about and functionality for the current test run.
      ///</summary>
      public TestContext TestContext{get;set;}

      //
      // Constructor ListExpressionTest (params IExpression<char> [])
      //
      [TestMethod]
      [ExpectedException (typeof(ArgumentNullException))]
      public void ParamsConstructorThrowArgumentNullExceptionForNullArray()
      {
         new ListExpression<char> ((IExpression<char>[]) null);
      }

      [TestMethod]
      [ExpectedException (typeof(ArgumentNullException))]
      public void ParamsConstructorThrowArgumentNullException()
      {
         new ListExpression<char> ((IExpression<char>)null);
      }

      [TestMethod]
      public void ParamsConstructorSucceedsForNonNullInput()
      {
         IExpression<char> [] expressions = new [] {m_a, m_any};
         new ListExpression<char> (expressions);
      }

      [TestMethod]
      public void ParamsConstructorCopiesParamsArraySoParamsArrayModificationDoesNotAffectConstructedExpression()
      {
         IExpression<char> [] expressions = new [] {m_any, m_any, m_any, m_any, m_any, m_any};
         var expression = new ListExpression<char> (expressions);
         
         string expected = "012345";
         
         int index = 0;
         var list = DigetsList;
         
         // alter array
         expressions [0] = m_digits[5];
         expressions [1] = m_digits[8];
         expressions [2] = m_a;
         
         var matches = expression.GetMatches (list, index).ToList ();

         Assert.AreEqual (1, matches.Count (), "Count");
         foreach (var match in matches)
         {
            Assert.AreEqual (index, match.Index, "match.Index");
            Assert.AreEqual (expressions.Length, match.Length, "match.Length");
            Assert.IsTrue (match.Success, "match.Success");
            Assert.AreEqual (expressions.Length, match.Items.Count, "match.Items.Count");
            string matchedString = new string (match.Items.ToArray ());
            Assert.AreEqual (expected, matchedString, "matched string does not match expected value.");
         }
      }

      //
      // Constructor ListExpressionTest (IExpression<char> first, IExpression<char> second)
      //
      [TestMethod]
      [ExpectedException (typeof(ArgumentNullException))]
      public void ConstructorThrowArgumentNullExceptionForFirst()
      {
         var expression = new ListExpression<char> ((IExpression<char>) null, m_a);
      }

      [TestMethod]
      [ExpectedException (typeof(ArgumentNullException))]
      public void ConstructorThrowArgumentNullExceptionForSecond()
      {
         var expression = new ListExpression<char> (m_a, (IExpression<char>) null);
      }

      [TestMethod]
      public void ConstructorSucceedsForNonNullInput()
      {
         var expression = new ListExpression<char> (m_a, m_any);
      }

      [TestMethod]
      [ExpectedException (typeof (ArgumentNullException))]
      public void GetMatchesOfNullThrowsArgumentNullException()
      {
         ListExpression<char> expression = new ListExpression<char> (m_digits[5],m_a);

         var matches = expression.GetMatches (null, 8);
      }

      [TestMethod]
      public void GetMatchesFindsOneMatchForA()
      {
         ListExpression<char> expression = new ListExpression<char> (m_a);

         int index = 0;
         var list = AList;
         var matches = expression.GetMatches (list, index).ToList ();

         Assert.AreEqual (1, matches.Count (), "Count");
         var match = matches.First ();
         Assert.AreEqual (index, match.Index, "match.Index");
         Assert.AreEqual (1, match.Length, "match.Length");
         Assert.IsTrue (match.Success, "match.Success");
         Assert.AreEqual (1, match.Items.Count, "match.Items.Count");
         Assert.AreEqual (list[index], match.Items [0], "match.Items [0]");
      }

      [TestMethod]
      public void GetMatchesOf56ForDigetsStartingAt5()
      {
         int index = 5;
         ListExpression<char> expression = new ListExpression<char> (m_digits[index], m_digits[index+1]);
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
      public void GetMatchesFindsNoMatchesForEmpty()
      {
         ListExpression<char> expression = new ListExpression<char> (m_any);

         var matches = expression.GetMatches (EmptyList, 0);

         Assert.AreEqual (0, matches.Count (), "Count");
      }

      [TestMethod]
      public void GetMatchesOf56789ForDigitsStartingAt5()
      {
         int index = 5;
         int length = 0;
         ListExpression<char> expression = new ListExpression<char> (
            m_digits[index+length++], 
            m_digits[index+length++],
            m_digits[index+length++],
            m_digits[index+length++],
            m_digits[index+length++]
            );

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
      public void GetMatchesOf_AnyOr56_AnyOr6789_ForDigitsStartingAt5()
      {
         int index = 5;
         ListExpression<char> expression = new ListExpression<char> (
            new AlternationExpression<char>(m_any, new ListExpression<char>(m_digits[5],m_digits[6])),
            new AlternationExpression<char>(m_any, new ListExpression<char>(m_digits[6],m_digits[7],m_digits[8],m_digits[9]))
            );

         var list = DigetsList;
         var matches = expression.GetMatches (list, index).ToList ();
         
         string[] expectedMatches = new [] {"56","56789","567"};

         Assert.AreEqual (expectedMatches.Length, matches.Count (), "Count");
         int matchesIndex = 0;
         foreach (var match in matches)
         {
            string expected = expectedMatches [matchesIndex];
            Assert.AreEqual (index, match.Index, "match.Index");
            Assert.AreEqual (expected.Length, match.Length, "match.Length");
            Assert.IsTrue (match.Success, "match.Success");
            Assert.AreEqual (expected.Length, match.Items.Count, "match.Items.Count");
            string matchedString = new string (match.Items.ToArray ());
            Assert.AreEqual (expected, matchedString, "matched string does not match expected value.");

            matchesIndex++;
         }
      }

      [TestMethod]
      public void GetMatchesOfEmptyFindsOneZeroLengthMatchForDigitsStartingAt8()
      {
         ListExpression<char> expression = new ListExpression<char> ();

         int index = 8;
         var list = DigetsList;
         int length = 0;
         var matches = expression.GetMatches (list, index).ToList ();

         Assert.AreEqual (1, matches.Count (), "Count");
         
         var match = matches.First ();
         Assert.AreEqual (index, match.Index, "match.Index");
         Assert.AreEqual (length, match.Length, "match.Length");
         Assert.IsTrue (match.Success, "match.Success");
         Assert.AreEqual (length, match.Items.Count, "match.Items.Count");
      }

      [TestMethod]
      public void GetMatchesOfDigits_Any_Any_Any()
      {
         IExpression<char> [] expressions = new [] {m_any, m_any, m_any};
         var expression = new ListExpression<char> (expressions);
         
         string expected = "012";
         
         int index = 0;
         var list = DigetsList;
         
         var matches = expression.GetMatches (list, index).ToList ();

         Assert.AreEqual (1, matches.Count (), "Count");
         var match = matches.First ();
         
         Assert.AreEqual (index, match.Index, "match.Index");
         Assert.AreEqual (expected.Length, match.Length, "match.Length");
         Assert.IsTrue (match.Success, "match.Success");
         Assert.AreEqual (expected.Length, match.Items.Count, "match.Items.Count");
         string matchedString = new string (match.Items.ToArray ());
         Assert.AreEqual (expected, matchedString, "matched string does not match expected value.");
      }

      [TestMethod]
      public void GetMatchesOfDigits_AnyOr5OrAny_Any()
      {
         IExpression<char> [] expressions = new [] {m_any, m_digits[5], m_any};
         var expression = new ListExpression<char> (new AlternationExpression<char> (expressions), m_any);
         
         string expected = "56";

         int index = 5;
         var list = DigetsList;
         
         var matches = expression.GetMatches (list, index).ToList ();

         Assert.AreEqual (3, matches.Count (), "Count");
         foreach (var match in matches)
         {
            Assert.AreEqual (index, match.Index, "match.Index");
            Assert.AreEqual (expected.Length, match.Length, "match.Length");
            Assert.IsTrue (match.Success, "match.Success");
            Assert.AreEqual (expected.Length, match.Items.Count, "match.Items.Count");
            string matchedString = new string (match.Items.ToArray ());
            Assert.AreEqual (expected, matchedString, "matched string does not match expected value.");
         }
      }

      //
      // Regex Matches tests
      //
      [TestMethod]
      public void RegexMatches_AAdot()
      {
         IExpression<char> expression = 
            new ListExpression<char> (
               new CharClass ('A'),
               new CharClass ('A'),
               new AnyClass<char> ());
         string regex = @"AA.";
         expression.AssertMatches ("AAABAB", regex);
         expression.AssertMatches ("00A0AA00000000AAABAB000000", regex);
      }

      [TestMethod]
      public void RegexMatches_AAdot_LookaheadNotA()
      {
         IExpression<char> expression = 
            new ListExpression<char> (
               new CharClass ('A'),
               new CharClass ('A'),
               new AnyClass<char> (),
               new NegativeLookAheadExpression<char>(m_a));
         string regex = @"AA.(?!A)";
         expression.AssertMatches ("AAABAB", regex);
         expression.AssertMatches ("00A0AA00000000AAABAB000000", regex);
      }

      //
      // SupportsLookBack tests
      //
      [TestMethod]
      public void SupportsLookBack_For_LookBackSupported ()
      {
         var expression = new ListExpression<char> (LookBackSupported);
         Assert.IsTrue (expression.SupportsLookBack);
      }

      [TestMethod]
      public void NotSupportsLookBack_For_LookBackNotSupported ()
      {
         var expression = new ListExpression<char> (LookBackNotSupported);
         Assert.IsFalse (expression.SupportsLookBack);
      }

      [TestMethod]
      public void NotSupportsLookBack_For_LookBackSupported_LookBackNotSupported ()
      {
         var expression = new ListExpression<char> (LookBackSupported, LookBackNotSupported);
         Assert.IsFalse (expression.SupportsLookBack);
      }

      //
      // AnyLength tests
      //
      [TestMethod]
      public void AnyLength_For_AnyLength ()
      {
         var expression = new ListExpression<char> (AnyLength);
         Assert.IsTrue (expression.AnyLength);
      }

      [TestMethod]
      public void NotAnyLength_For_NotAnyLength ()
      {
         var expression = new ListExpression<char> (NotAnyLength);
         Assert.IsFalse (expression.AnyLength);
      }

      [TestMethod]
      public void AnyLength_For_AnyLength_NotAnyLength ()
      {
         var expression = new ListExpression<char> (AnyLength, NotAnyLength);
         Assert.IsTrue (expression.AnyLength);
      }

      //
      // MatchLength tests
      //
      [TestMethod]
      public void GetPossibleMatchLengths_ForEmpty ()
      {
         var expected = new int [] {0};
         var expression = new ListExpression<char> ();
         expression.AssertPossibleMatchLengths (expected);
      }

      [TestMethod]
      public void GetPossibleMatchLengths_ForEmptyAlternation ()
      {
         var expected = new int [] {0};
         var expression = new ListExpression<char> (new AlternationExpression<char>());
         expression.AssertPossibleMatchLengths (expected);
      }

      [TestMethod]
      public void GetPossibleMatchLengths_For12 ()
      {
         var expected = new int [] {3};
         var expression = new ListExpression<char> (MatchLength1, MatchLength2);
         expression.AssertPossibleMatchLengths (expected);
      }

      [TestMethod]
      public void GetPossibleMatchLengths_For123 ()
      {
         var expected = new int [] {6};
         var expression = new ListExpression<char> (MatchLength1, MatchLength2, MatchLength3);
         expression.AssertPossibleMatchLengths (expected);
      }

      [TestMethod]
      public void GetPossibleMatchLengths_For_2or8_3to5 ()
      {
         var expected = new int [] {5,6,7,11,12,13};
         var expression = new ListExpression<char> (MatchLengths2or8,MatchLengths3to5);
         expression.AssertPossibleMatchLengths (expected);
      }

      [TestMethod]
      public void GetPossibleMatchLengths_For_22222 ()
      {
         var expected = new int [] {10};
         var expression = new ListExpression<char> (MatchLength2, MatchLength2, MatchLength2, MatchLength2, MatchLength2);
         expression.AssertPossibleMatchLengths (expected);
      }

      [TestMethod]
      public void GetPossibleMatchLengths_For_2_3to5_2_2or8_2 ()
      {
         var expected = new int [] {11,17,12,18,13,19};
         var expression = new ListExpression<char> (MatchLength2, MatchLengths3to5, MatchLength2, MatchLengths2or8, MatchLength2);
         expression.AssertPossibleMatchLengths (expected);
      }

      [TestMethod]
      public void GetPossibleMatchLengths_For_Start_2_3to5_2_2or8_2_End ()
      {
         var expected = new int [] {11,17,12,18,13,19};
         var expression = new ListExpression<char> (new StartExpression<char> (), MatchLength2, MatchLengths3to5, MatchLength2, MatchLengths2or8, MatchLength2, new EndExpression<char> ());
         expression.AssertPossibleMatchLengths (expected);
      }
   }
}
