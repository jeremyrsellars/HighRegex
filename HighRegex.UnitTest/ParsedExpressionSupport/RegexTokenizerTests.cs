using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HighRegex.ParsedExpressionSupport
{
   [TestClass]
   public abstract class RegexTokenizerTest
   {
      protected abstract string[] ExpectedTokens { get; }
      protected RegexTokenizer tokenizer;

      [TestInitialize]
      public virtual void Init()
      {
         tokenizer = new RegexTokenizer(GetRegex());
      }

      [TestMethod]
      public void TokensAreAsExpected()
      {
         var tokens = tokenizer.Tokenize(GetTokenString()).ToList();
         CollectionAssert.AreEqual(ExpectedTokens, tokens);
      }

      protected abstract string GetTokenString();

      protected abstract Regex GetRegex();

      public abstract class SingleCharacterRegexTokenizerTest : RegexTokenizerTest
      {
         protected override Regex GetRegex()
         {
            return new Regex(".");
         }
         protected override string[] ExpectedTokens
         {
            get { return GetTokenString().Select(c => c.ToString()).ToArray(); }
         }
      }

      [TestClass]
      public class WhenTokenizingEmptyString : SingleCharacterRegexTokenizerTest
      {
         protected override string GetTokenString()
         {
            return "";
         }
      }

      [TestClass]
      public class WhenTokenizingSingleCharacterString : SingleCharacterRegexTokenizerTest
      {
         protected override string GetTokenString()
         {
            return "a";
         }
      }

      [TestClass]
      public class WhenTokenizingSeveralCharacters : SingleCharacterRegexTokenizerTest
      {
         protected override string GetTokenString()
         {
            return "aBcDeF";
         }
      }
   }
}
