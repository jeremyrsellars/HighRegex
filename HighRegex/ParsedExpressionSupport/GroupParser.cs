using System;

namespace HighRegex.ParsedExpressionSupport
{
   public class GroupParser<T>
   {
      private readonly Parser.TokenStream<T> tokens;
      private string groupStartToken;

      public GroupParser(Parser.TokenStream<T> tokens)
      {
         this.tokens = tokens;
      }

      public IExpression<T> ParseExpression ()
      {
         groupStartToken = tokens.Current;
         AdvancePastGroupOpen();
         IExpression<T> expression = ParseGroupContents();
         ThrowUnlessCurrentTokenIsGroupClose();

         return CreateExpressionOrThrowIfCurrentTokenDoesNotStartGroup(expression);
      }

      private void AdvancePastGroupOpen()
      {
         tokens.MoveNextOrThrow ();
      }

      private IExpression<T> ParseGroupContents()
      {
         return AlternationExpressionParser<T>.Parse(tokens);
      }

      private void ThrowUnlessCurrentTokenIsGroupClose()
      {
         if (tokens.Current != Parser.Token.GroupClose)
            throw CreateGroupCloseExpectedException();
      }

      private Exception CreateGroupCloseExpectedException()
      {
         return new InvalidExpressionException (
            "Expected ) ParenClose.  Found: " + tokens.Current);
      }

      private IExpression<T> CreateExpressionOrThrowIfCurrentTokenDoesNotStartGroup(
         IExpression<T> expression)
      {
         if (groupStartToken == Parser.Token.GroupOpen)
            return expression;
         return WrapExpressionWithAtomicLookAround(expression);
      }

      private IExpression<T> WrapExpressionWithAtomicLookAround(IExpression<T> expression)
      {
         switch (groupStartToken)
         {
            case Parser.Token.AtomicLookahead:
               return new LookAheadExpression<T> (expression);
            case Parser.Token.AtomicNegativeLookahead:
               return new NegativeLookAheadExpression<T> (expression);
            case Parser.Token.AtomicLookbehind:
               return new LookBackExpression<T> (expression);
            case Parser.Token.AtomicNegativeLookbehind:
               return new NegativeLookBackExpression<T> (expression);
         }

         throw CreateNotExpectedGroupStartExpression(groupStartToken);
      }

      private static Exception CreateNotExpectedGroupStartExpression(string startToken)
      {
         return new InvalidExpressionException(
            "Unsupported group beginner expression: " + startToken);
      }
   }
}
