using System.Collections.Generic;
using HighRegex.ParsedExpressionSupport;


namespace HighRegex
{
   public class ParsedExpression<T> : IExpression<T>
   {
      private readonly IExpression<T> m_expression;
      private readonly string m_text;

      static IExpression<T> ParseExpression (
         string [] tokens, 
         Dictionary<string,Parser.ExpressionDefinition<T>> otherExpressions, 
         Dictionary<string,object> parameterValues)
      {
         return AlternationExpressionParser<T>.Parse (
            new Parser.TokenStream<T> (tokens, otherExpressions, parameterValues));
      }

      public ParsedExpression (
         Dictionary<string, Parser.ExpressionDefinition<T>> otherExpressions,
         Dictionary<string,object> parameterValues, 
         string expressionText)
      {
         string [] tokens = Parser.Tokenize (expressionText);
         m_expression = ParseExpression (tokens, otherExpressions, parameterValues);
         m_text = expressionText;
      }

      public string Text
      {
         get { return m_text; }
      }

      public IExpression<T> EnclosedExpression
      {
         get { return m_expression;  }
      }

      public IEnumerable<IExpression<T>> Expressions
      {
         get { return new [] {m_expression}; }
      }

      public MatchLength IsMatchAt(IExpressionItemSource<T> input, int index)
      {
         return m_expression.IsMatchAt (input, index);
      }
   }
}