using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HighRegex
{
   [TestClass]
   public class AlternationExpressionTest
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
      
      /// <summary>
      ///Gets or sets the test context which provides
      ///information about and functionality for the current test run.
      ///</summary>
      public TestContext TestContext{get;set;}

      //
      // Constructor AlternationExpressionTest (params IExpression<char> [])
      //
      [TestMethod]
      [ExpectedException (typeof(ArgumentNullException))]
      public void ParamsConstructorThrowArgumentNullExceptionForNullArray()
      {
         var expression = new AlternationExpression<char> ((IExpression<char>[]) null);
      }

      [TestMethod]
      [ExpectedException (typeof(ArgumentNullException))]
      public void ParamsConstructorThrowArgumentNullException()
      {
         var expression = new AlternationExpression<char> ((IExpression<char>)null);
      }

      [TestMethod]
      public void ParamsConstructorSucceedsForNonNullInput()
      {
         IExpression<char> [] expressions = new [] {m_digits[5], m_digits[8]};
         var expression = new AlternationExpression<char> (expressions);
      }

      [TestMethod]
      public void ParamsConstructorCopiesParamsArraySoParamsArrayModificationDoesNotAffectConstructedExpression()
      {
         IExpression<char> [] expressions = new [] {m_any, m_any, m_any};
         var expression = new AlternationExpression<char> (expressions);
         
         int index = 0;
         var list = DigetsList;
         
         // alter array
         expressions [0] = m_digits[5];
         expressions [1] = m_digits[8];
         expressions [2] = m_a;
         
         var matches = expression.GetMatches (list, index);

         Assert.AreEqual (3, matches.Count (), "Count");
         foreach (var match in matches)
         {
            Assert.AreEqual (index, match.Index, "match.Index");
            Assert.AreEqual (1, match.Length, "match.Length");
            Assert.IsTrue (match.Success, "match.Success");
            Assert.AreEqual (1, match.Items.Count, "match.Items.Count");
            Assert.AreEqual (list[index], match.Items [0], "match.Items [0]");
         }
      }

      //
      // Constructor AlternationExpressionTest (IExpression<char> first, IExpression<char> second)
      //
      [TestMethod]
      [ExpectedException (typeof(ArgumentNullException))]
      public void ConstructorThrowArgumentNullExceptionForFirst()
      {
         var expression = new AlternationExpression<char> ((IExpression<char>) null, m_digits[5]);
      }

      [TestMethod]
      [ExpectedException (typeof(ArgumentNullException))]
      public void ConstructorThrowArgumentNullExceptionForSecond()
      {
         var expression = new AlternationExpression<char> (m_digits[5], (IExpression<char>) null);
      }

      [TestMethod]
      public void ConstructorSucceedsForNonNullInput()
      {
         var expression = new AlternationExpression<char> (m_digits[5], m_digits[8]);
      }

      [TestMethod]
      public void GetMatchesFindsOneMatchForA()
      {
         AlternationExpression<char> expression = new AlternationExpression<char> (m_a);

         int index = 0;
         var list = AList;
         var matches = expression.GetMatches (list, index);

         Assert.AreEqual (1, matches.Count (), "Count");
         var match = matches.First ();
         Assert.AreEqual (index, match.Index, "match.Index");
         Assert.AreEqual (1, match.Length, "match.Length");
         Assert.IsTrue (match.Success, "match.Success");
         Assert.AreEqual (1, match.Items.Count, "match.Items.Count");
         Assert.AreEqual (list[index], match.Items [0], "match.Items [0]");
      }

      [TestMethod]
      public void GetMatchesFindsTwoMatchesFor5orAny()
      {
         AlternationExpression<char> expression = new AlternationExpression<char> (m_digits[5], m_any);

         var list = DigetsList;
         int index = 5;
         var matches = expression.GetMatches (list, index);

         Assert.AreEqual (2, matches.Count (), "Count");
         foreach (var match in matches)
         {
            Assert.AreEqual (index, match.Index, "match.Index");
            Assert.AreEqual (1, match.Length, "match.Length");
            Assert.IsTrue (match.Success, "match.Success");
            Assert.AreEqual (1, match.Items.Count, "match.Items.Count");
            Assert.AreEqual (list[index], match.Items [0], "match.Items [0]");
         }
      }

      [TestMethod]
      public void GetMatchesFindsNoMatchesForEmpty()
      {
         AlternationExpression<char> expression = new AlternationExpression<char> (m_any);

         var matches = expression.GetMatches (EmptyList, 0);

         Assert.AreEqual (0, matches.Count (), "Count");
      }

      [TestMethod]
      public void GetMatchesOfAor8or5orAnyFindsTwoMatchesForDigitsStartingAt5()
      {
         AlternationExpression<char> expression = new AlternationExpression<char> (m_a,m_digits[8],m_digits[5],m_any);

         int index = 5;
         var list = DigetsList;
         var matches = expression.GetMatches (list, index);

         Assert.AreEqual (2, matches.Count (), "Count");
         foreach (var match in matches)
         {
            Assert.AreEqual (index, match.Index, "match.Index");
            Assert.AreEqual (1, match.Length, "match.Length");
            Assert.IsTrue (match.Success, "match.Success");
            Assert.AreEqual (1, match.Items.Count, "match.Items.Count");
            Assert.AreEqual (list[index], match.Items [0], "match.Items [0]");
         }
      }

      [TestMethod]
      public void GetMatchesOf5orAFindsNoMatchForDigitsStartingAt8()
      {
         AlternationExpression<char> expression = new AlternationExpression<char> (m_digits[5],m_a);

         var matches = expression.GetMatches (DigetsList, 8);

         Assert.AreEqual (0, matches.Count (), "Count");
      }

      [TestMethod]
      [ExpectedException (typeof (ArgumentNullException))]
      public void GetMatchesOfNullThrowsArgumentNullException()
      {
         AlternationExpression<char> expression = new AlternationExpression<char> (m_digits[5],m_a);

         var matches = expression.GetMatches (null, 8);
      }

      [TestMethod]
      public void GetMatchesOfEmptyFindsOneZeroLengthMatchForDigitsStartingAt8()
      {
         AlternationExpression<char> expression = new AlternationExpression<char> ();

         int index = 8;
         var list = DigetsList;
         int length = 0;
         var matches = expression.GetMatches (list, index);

         Assert.AreEqual (1, matches.Count (), "Count");
         
         var match = matches.First ();
         Assert.AreEqual (index, match.Index, "match.Index");
         Assert.AreEqual (length, match.Length, "match.Length");
         Assert.IsTrue (match.Success, "match.Success");
         Assert.AreEqual (length, match.Items.Count, "match.Items.Count");
      }

      [TestMethod]
      public void GetMatchesOf_AnyOr56_ForDigitsStartingAt5()
      {
         int index = 5;
         AlternationExpression<char> expression = 
            new AlternationExpression<char>(
               m_any, 
               new ListExpression<char>(m_digits[5],m_digits[6]));

         var list = DigetsList;
         var matches = expression.GetMatches (list, index).ToList ();
         
         string[] expectedMatches = new [] {"5","56"};

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
      public void GetMatchesOfDigits_AnyOr5OrAny()
      {
         IExpression<char> [] expressions = new [] {m_any, m_digits[5], m_any};
         var expression = new AlternationExpression<char> (expressions);
         
         string expected = "5";

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
      public void RegexMatches_AAorAB()
      {
         IExpression<char> expression = 
            new AlternationExpression<char>(
               new ListExpression<char> (
                  new CharClass ('A'),
                  new CharClass ('A')),
               new ListExpression<char> (
                  new CharClass ('A'),
                  new CharClass ('B'))
                  );
         string regex = @"(AA)|(AB)";
         expression.AssertMatches ("AAABAB", regex);
         expression.AssertMatches ("00000000000000AAABAB000000", regex);
      }

      [TestMethod]
      public void RegexMatches_StartOrEndForAAABBB()
      {
         IExpression<char> expression = 
            new AlternationExpression<char>(
               new StartExpression<char> (),
               new EndExpression<char> ());
         string regex = @"^|$";
         expression.AssertMatches ("AAABBB", regex);
      }

      [TestMethod]
      public void RegexMatches_StartOrEndForEmpty()
      {
         IExpression<char> expression = 
            new AlternationExpression<char>(
               new StartExpression<char> (),
               new EndExpression<char> ());
         string regex = @"^|$";
         expression.AssertMatches ("", regex);
      }

      //
      // SupportsLookBack tests
      //
      [TestMethod]
      public void SupportsLookBack_ForEmpty ()
      {
         var expression = new AlternationExpression<char> ();
         Assert.IsTrue (expression.SupportsLookBack);
      }

      [TestMethod]
      public void SupportsLookBack_For_LookBackSupported ()
      {
         var expression = new AlternationExpression<char> (LookBackSupported);
         Assert.IsTrue (expression.SupportsLookBack);
      }

      [TestMethod]
      public void NotSupportsLookBack_For_LookBackNotSupported ()
      {
         var expression = new AlternationExpression<char> (LookBackNotSupported);
         Assert.IsFalse (expression.SupportsLookBack);
      }

      [TestMethod]
      public void NotSupportsLookBack_For_LookBackSupported_LookBackNotSupported ()
      {
         var expression = new AlternationExpression<char> (LookBackSupported, LookBackNotSupported);
         Assert.IsFalse (expression.SupportsLookBack);
      }

      //
      // AnyLength tests
      //
      [TestMethod]
      public void AnyLength_ForEmpty ()
      {
         var expression = new AlternationExpression<char> ();
         Assert.IsFalse (expression.AnyLength);
      }

      [TestMethod]
      public void AnyLength_For_AnyLength ()
      {
         var expression = new AlternationExpression<char> (AnyLength);
         Assert.IsTrue (expression.AnyLength);
      }

      [TestMethod]
      public void NotAnyLength_For_NotAnyLength ()
      {
         var expression = new AlternationExpression<char> (NotAnyLength);
         Assert.IsFalse (expression.AnyLength);
      }

      [TestMethod]
      public void AnyLength_For_AnyLength_NotAnyLength ()
      {
         var expression = new AlternationExpression<char> (AnyLength, NotAnyLength);
         Assert.IsTrue (expression.AnyLength);
      }

      //
      // MatchLength tests
      //
      [TestMethod]
      public void GetPossibleMatchLengths_ForEmpty ()
      {
         var expected = new int [] {0};
         var expression = new AlternationExpression<char> ();
         expression.AssertPossibleMatchLengths (expected);
      }

      [TestMethod]
      public void GetPossibleMatchLengths_For1 ()
      {
         var expected = new int [] {1};
         var expression = new AlternationExpression<char> (MatchLength1);
         expression.AssertPossibleMatchLengths (expected);
      }

      [TestMethod]
      public void GetPossibleMatchLengths_For1_1 ()
      {
         var expected = new int [] {1};
         var expression = new AlternationExpression<char> (MatchLength1,MatchLength1);
         expression.AssertPossibleMatchLengths (expected);
      }

      [TestMethod]
      public void GetPossibleMatchLengths_For1_2 ()
      {
         var expected = new int [] {1,2};
         var expression = new AlternationExpression<char> (MatchLength1,MatchLength2);
         expression.AssertPossibleMatchLengths (expected);
      }

      [TestMethod]
      public void GetPossibleMatchLengths_For0_2_3to5 ()
      {
         var expected = new int [] {0, 2, 3,4,5};
         var expression = new AlternationExpression<char> (MatchLength0,MatchLength2,MatchLengths3to5);
         expression.AssertPossibleMatchLengths (expected);
      }

   }
}
