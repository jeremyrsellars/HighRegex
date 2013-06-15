using System;
using System.Linq;
using System.Text.RegularExpressions;
using STR=System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using E=RegSeqEx;

namespace RegSeqEx
{
   static class RegexTestExtensions
   {
      public static void AssertMatches (this IExpression<char> expression, string input, string regex)
      {
         AssertMatches (expression, input, new Regex (regex), 0);
      }
      public static void AssertMatches (this IExpression<char> expression, string input, Regex regex)
      {
         AssertMatches (expression, input, regex, 0);
      }
      public static void AssertMatches (this IExpression<char> expression, string input, Regex regex, int startat)
      {
         STR.MatchCollection regexMatches = regex.Matches (input, startat);
         //IMatchProvider<char> matchProvider = expression as IMatchProvider<char> ?? new ExpressionMatchProvider<char> (expression);
         E.MatchCollection<char> eMatches = expression.GetMatches (input.ToCharArray (), startat).ToMatchValueCollection ();
         
         AssertMatches (eMatches, regexMatches);
      }
      public static void AssertMatches (E.MatchCollection<char> matches, STR.MatchCollection regexMatches)
      {
         if (matches == null && regexMatches == null)
            return;
         if (matches == null)
            Assert.Fail ("matches == null");
         if (regexMatches == null)
            Assert.Fail ("regexMatches == null");
         
         Assert.AreEqual (regexMatches.Count, matches.Count, "matches.Count");
         for (int i = 0; i < regexMatches.Count; i++)
         {
            var rMatch = regexMatches[i];
            var eMatch = matches[i];
            
            Assert.AreEqual (rMatch.Index, eMatch.Index, "Index of match " + i);
            Assert.AreEqual (rMatch.Length, eMatch.Length, "Length of match " + i);
            
            string rValue = rMatch.Value;
            string eValue = new string (eMatch.Items.ToArray ());
            Assert.AreEqual (rValue.Length, eValue.Length, "Value of match " + i);
         }
      }
   }
}
