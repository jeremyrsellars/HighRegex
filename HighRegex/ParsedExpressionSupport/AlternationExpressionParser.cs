using System.Collections.Generic;

namespace HighRegex.ParsedExpressionSupport
{
   public class AlternationExpressionParser<T>
   {
      readonly Parser.TokenStream<T> tokens;

      static IExpression<T> ParseListExpression (Parser.TokenStream<T> tokens)
      {
         var list = new ListExpressionParser<T>(tokens).ParseExpressions();
         if (list.Count == 1)
            return list [0];
         return new ListExpression<T>(list.ToArray ());
      }

      static bool IsEndOfSubExpression(string token)
      {
         return token == Parser.Token.EndOfStream
                || token == Parser.Token.GroupClose;
      }

      static void ThrowIfNotAnAlternationPipe(string token)
      {
         System.Diagnostics.Debug.Assert(token == Parser.Token.Pipe, "Not supported: " + token);
         if (token != Parser.Token.Pipe)
            throw new AlgorithmAssertionFailedException ("Not supported: " + token);
      }

      internal static IExpression<T> Parse(Parser.TokenStream<T> tokens)
      {
         var list = new AlternationExpressionParser<T>(tokens).ParseExpressions();
         if (list.Count == 1)
            return list [0];
         return new AlternationExpression<T> (list.ToArray ());
      }

      public AlternationExpressionParser(Parser.TokenStream<T> tokens)
      {
         this.tokens = tokens;
      }

      public List<IExpression<T>> ParseExpressions ()
      {
         var list = new List<IExpression<T>>();
         do
         {
            list.Add (ParseListExpression (tokens));
            if (IsEndOfSubExpression(tokens.Current))
               break;
            ThrowIfNotAnAlternationPipe(tokens.Current);
         } while (tokens.MoveNext ());

         return list;
      }
   }
}
