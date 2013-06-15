using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RegSeqEx
{
   [TestClass]
   public class EnumerableExpressionItemSourceTest
   {
      private IClass<Char> m_any = new AnyClass<char> ();
      private IClass<Char> m_A = new CharClass ('A');
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

      public EnumerableExpressionItemSource<char> EmptyList = new EnumerableExpressionItemSource<char>("");
      public EnumerableExpressionItemSource<char> AList = new EnumerableExpressionItemSource<char>("A");
      public EnumerableExpressionItemSource<char> DigetsList = new EnumerableExpressionItemSource<char>("0123456789");
      public EnumerableExpressionItemSource<char> AllCharacters = new EnumerableExpressionItemSource<char>(EnumerateAllCharactersThenThrow);
      
      private static IEnumerable<char> EnumerateAllCharactersThenThrow
      {
         get
         {
            for (char c = char.MinValue; c < char.MaxValue; c++)
               yield return c;
            throw new InvalidOperationException ("Enumerating past end of characters");
         }
      }

      
      /// <summary>
      ///Gets or sets the test context which provides
      ///information about and functionality for the current test run.
      ///</summary>
      public TestContext TestContext{get;set;}

      //
      // Constructor
      //
      [TestMethod]
      [ExpectedException (typeof(ArgumentNullException))]
      public void ConstructorThrowsArgumentNullExceptionForNullArray()
      {
         var input = new EnumerableExpressionItemSource<char> ((char[]) null);
      }

      [TestMethod]
      public void ParamsConstructorSucceedsForNonNullInput()
      {
         var input = new EnumerableExpressionItemSource<char> ("0123456789".ToCharArray ());
      }

      //
      // Input tests
      //
      [TestMethod]
      public void MatchAFindsOneMatchForA()
      {
         var expression = m_A;

         int index = 0;
         var list = AList;
         var match = expression.Match (list, index);

         Assert.IsTrue (match.Success, "match.Success");
         Assert.AreEqual (index, match.Index, "match.Index");
         Assert.AreEqual (1, match.Length, "match.Length");
         Assert.AreEqual (1, match.Items.Count, "match.Items.Count");
         Assert.AreEqual ('A', match.Items [0], "match.Items [0]");
      }

      [TestMethod]
      public void MatchAFindsOneMatchForAllCharacters()
      {
         var expression = m_A;
         
         int index = (int)'A';
         var list = AllCharacters;
         var match = expression.Match (list, index);

         Assert.IsTrue (match.Success, "match.Success");
         Assert.AreEqual (index, match.Index, "match.Index");
         Assert.AreEqual (1, match.Length, "match.Length");
         Assert.AreEqual (1, match.Items.Count, "match.Items.Count");
         Assert.AreEqual ('A', match.Items [0], "match.Items [0]");
      }

      [TestMethod]
      public void GetMatchesFindsNoMatchesForEmpty()
      {
         var expression = new AlternationExpression<char> (m_A);

         var match = expression.Match (EmptyList, 0);

         Assert.IsFalse (match.Success, "Success");
      }

      [TestMethod]
      public void MatchDigetsFindsOneMatchForAllCharacters()
      {
         var expression = new ListExpression<char> (m_digits.ToArray ());
         
         int index = (int)'0';
         var list = AllCharacters;
         var match = expression.Match (list, index);

         Assert.IsTrue (match.Success, "match.Success");
         Assert.AreEqual (index, match.Index, "match.Index");
         Assert.AreEqual (m_digits.Length, match.Length, "match.Length");
         Assert.AreEqual (m_digits.Length, match.Items.Count, "match.Items.Count");
         Assert.AreEqual ("0123456789", new string(match.Items.ToArray ()), "match.Items");
      }

      [TestMethod]
      public void MatchAInFirst100FindsMatchForAllCharacters()
      {
         var expression = 
            new ListExpression<char> (
               new StartExpression<char> (), 
               new RepeatExpression<char> (
                  new NotClass<char> (m_A),
                  0, 100),
                  m_A);
         
         int index = 0;
         int length = (int)'A' + 1;
         var list = AllCharacters;
         var match = expression.Match (list, index);

         Assert.IsTrue (match.Success, "match.Success");
         Assert.AreEqual (index, match.Index, "match.Index");
         Assert.AreEqual (length, match.Length, "match.Length");
         Assert.AreEqual (length, match.Items.Count, "match.Items.Count");
      }

      //[TestMethod]
      //public void GetMatchesOfAor8or5orAnyFindsTwoMatchesForDigitsStartingAt5()
      //{
      //   AlternationExpression<char> expression = new AlternationExpression<char> (m_a,m_digits[8],m_digits[5],m_any);

      //   int index = 5;
      //   var list = DigetsList;
      //   var matches = expression.GetMatches (list, index);

      //   Assert.AreEqual (2, matches.Count (), "Count");
      //   foreach (var match in matches)
      //   {
      //      Assert.AreEqual (index, match.Index, "match.Index");
      //      Assert.AreEqual (1, match.Length, "match.Length");
      //      Assert.IsTrue (match.Success, "match.Success");
      //      Assert.AreEqual (1, match.Items.Count, "match.Items.Count");
      //      Assert.AreEqual (list[index], match.Items [0], "match.Items [0]");
      //   }
      //}

      //[TestMethod]
      //public void GetMatchesOf5orAFindsNoMatchForDigitsStartingAt8()
      //{
      //   AlternationExpression<char> expression = new AlternationExpression<char> (m_digits[5],m_a);

      //   var matches = expression.GetMatches (DigetsList, 8);

      //   Assert.AreEqual (0, matches.Count (), "Count");
      //}

      //[TestMethod]
      //[ExpectedException (typeof (ArgumentNullException))]
      //public void GetMatchesOfNullThrowsArgumentNullException()
      //{
      //   AlternationExpression<char> expression = new AlternationExpression<char> (m_digits[5],m_a);

      //   var matches = expression.GetMatches (null, 8);
      //}

      //[TestMethod]
      //public void GetMatchesOfEmptyFindsOneZeroLengthMatchForDigitsStartingAt8()
      //{
      //   AlternationExpression<char> expression = new AlternationExpression<char> ();

      //   int index = 8;
      //   var list = DigetsList;
      //   int length = 0;
      //   var matches = expression.GetMatches (list, index);

      //   Assert.AreEqual (1, matches.Count (), "Count");
         
      //   var match = matches.First ();
      //   Assert.AreEqual (index, match.Index, "match.Index");
      //   Assert.AreEqual (length, match.Length, "match.Length");
      //   Assert.IsTrue (match.Success, "match.Success");
      //   Assert.AreEqual (length, match.Items.Count, "match.Items.Count");
      //}

      //[TestMethod]
      //public void GetMatchesOf_AnyOr56_ForDigitsStartingAt5()
      //{
      //   int index = 5;
      //   AlternationExpression<char> expression = 
      //      new AlternationExpression<char>(
      //         m_any, 
      //         new ListExpression<char>(m_digits[5],m_digits[6]));

      //   var list = DigetsList;
      //   var matches = expression.GetMatches (list, index).ToList ();
         
      //   string[] expectedMatches = new [] {"5","56"};

      //   Assert.AreEqual (expectedMatches.Length, matches.Count (), "Count");
      //   int matchesIndex = 0;
      //   foreach (var match in matches)
      //   {
      //      string expected = expectedMatches [matchesIndex];
      //      Assert.AreEqual (index, match.Index, "match.Index");
      //      Assert.AreEqual (expected.Length, match.Length, "match.Length");
      //      Assert.IsTrue (match.Success, "match.Success");
      //      Assert.AreEqual (expected.Length, match.Items.Count, "match.Items.Count");
      //      string matchedString = new string (match.Items.ToArray ());
      //      Assert.AreEqual (expected, matchedString, "matched string does not match expected value.");

      //      matchesIndex++;
      //   }
      //}

      //[TestMethod]
      //public void GetMatchesOfDigits_AnyOr5OrAny()
      //{
      //   IExpression<char> [] expressions = new [] {m_any, m_digits[5], m_any};
      //   var expression = new AlternationExpression<char> (expressions);
         
      //   string expected = "5";

      //   int index = 5;
      //   var list = DigetsList;
         
      //   var matches = expression.GetMatches (list, index).ToList ();

      //   Assert.AreEqual (3, matches.Count (), "Count");
      //   foreach (var match in matches)
      //   {
      //      Assert.AreEqual (index, match.Index, "match.Index");
      //      Assert.AreEqual (expected.Length, match.Length, "match.Length");
      //      Assert.IsTrue (match.Success, "match.Success");
      //      Assert.AreEqual (expected.Length, match.Items.Count, "match.Items.Count");
      //      string matchedString = new string (match.Items.ToArray ());
      //      Assert.AreEqual (expected, matchedString, "matched string does not match expected value.");
      //   }
      //}
      
      ////
      //// Regex Matches tests
      ////
      //[TestMethod]
      //public void RegexMatches_AAorAB()
      //{
      //   IExpression<char> expression = 
      //      new AlternationExpression<char>(
      //         new ListExpression<char> (
      //            new CharClass ('A'),
      //            new CharClass ('A')),
      //         new ListExpression<char> (
      //            new CharClass ('A'),
      //            new CharClass ('B'))
      //            );
      //   string regex = @"(AA)|(AB)";
      //   expression.AssertMatches ("AAABAB", regex);
      //   expression.AssertMatches ("00000000000000AAABAB000000", regex);
      //}

      //[TestMethod]
      //public void RegexMatches_StartOrEndForAAABBB()
      //{
      //   IExpression<char> expression = 
      //      new AlternationExpression<char>(
      //         new StartExpression<char> (),
      //         new EndExpression<char> ());
      //   string regex = @"^|$";
      //   expression.AssertMatches ("AAABBB", regex);
      //}

      //[TestMethod]
      //public void RegexMatches_StartOrEndForEmpty()
      //{
      //   IExpression<char> expression = 
      //      new AlternationExpression<char>(
      //         new StartExpression<char> (),
      //         new EndExpression<char> ());
      //   string regex = @"^|$";
      //   expression.AssertMatches ("", regex);
      //}
   }
}
