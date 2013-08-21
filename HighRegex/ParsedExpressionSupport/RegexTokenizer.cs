using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace HighRegex.ParsedExpressionSupport
{
   public class RegexTokenizer
   {
      readonly Regex regex;

      public RegexTokenizer(Regex regex)
      {
         this.regex = regex;
      }

      public IEnumerable<string> Tokenize(string s)
      {
         for(var m = regex.Match(s); m.Success; m = m.NextMatch())
            yield return m.Value;
      }
   }
}
