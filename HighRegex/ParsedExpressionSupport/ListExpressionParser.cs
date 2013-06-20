using System;
using System.Collections.Generic;

namespace HighRegex.ParsedExpressionSupport
{
   class ListExpressionParser<T>
   {
      interface IToken
      {
         bool Supports(string token);
         IExpression<T> CreateExpression(Parser.TokenStream<T> tokens);
      }

      class StartToken : IToken
      {
         public bool Supports(string token)
         {
            return Parser.Token.Caret == token;
         }
         public IExpression<T> CreateExpression(Parser.TokenStream<T> tokens)
         {
            return new StartExpression<T>();
         }
      }

      class EndToken : IToken
      {
         public bool Supports(string token)
         {
            return Parser.Token.Dollar == token;
         }
         public IExpression<T> CreateExpression(Parser.TokenStream<T> tokens)
         {
            return new EndExpression<T>();
         }
      }

      class DotToken : IToken
      {
         public bool Supports(string token)
         {
            return Parser.Token.Dot == token;
         }
         public IExpression<T> CreateExpression(Parser.TokenStream<T> tokens)
         {
            return new AnyClass<T>();
         }
      }

      class GroupToken : IToken
      {
         public bool Supports(string token)
         {
            return
               token == Parser.Token.AtomicLookahead
               || token == Parser.Token.AtomicNegativeLookahead
               || token == Parser.Token.AtomicLookbehind
               || token == Parser.Token.AtomicNegativeLookbehind
               || token == Parser.Token.GroupOpen;
         }
         public IExpression<T> CreateExpression(Parser.TokenStream<T> tokens)
         {
            return new GroupParser<T>(tokens).ParseExpression();
         }
      }

      class ClassToken : IToken
      {
         public bool Supports(string token)
         {
            return
               token == Parser.Token.NegatedClassSetOpen
               || token == Parser.Token.ClassSetOpen;
         }
         public IExpression<T> CreateExpression(Parser.TokenStream<T> tokens)
         {
            return new ClassParser<T>(tokens).ParseClass();
         }
      }

      class WhitespaceToken : IToken
      {
         public bool Supports(string token)
         {
            return token == " " || token == "\t";
         }
         public IExpression<T> CreateExpression(Parser.TokenStream<T> tokens)
         {
            return null;
         }
      }

      interface IModifierToken
      {
         bool Supports(string token);
         IExpression<T> CreateExpression(Parser.TokenStream<T> tokens, IExpression<T> containedExpression);
      }

      class LazyQuestionToken : IModifierToken
      {
         public bool Supports(string token)
         {
            return token == Parser.Token.LazyQuestion;
         }
         public IExpression<T> CreateExpression(Parser.TokenStream<T> tokens, IExpression<T> containedExpression)
         {
            return new RepeatExpression<T> (containedExpression, 0, 1);
         }
      }

      class GreedyQuestionToken : IModifierToken
      {
         public bool Supports(string token)
         {
            return token == Parser.Token.GreedyQuestion;
         }
         public IExpression<T> CreateExpression(Parser.TokenStream<T> tokens, IExpression<T> containedExpression)
         {
            return new GreedyRepeatExpression<T> (containedExpression, 0, 1);
         }
      }

      class LazyStarToken : IModifierToken
      {
         public bool Supports(string token)
         {
            return token == Parser.Token.LazyStar;
         }
         public IExpression<T> CreateExpression(Parser.TokenStream<T> tokens, IExpression<T> containedExpression)
         {
            return new RepeatExpression<T> (containedExpression, 0, Int32.MaxValue);
         }
      }

      class GreedyStarToken : IModifierToken
      {
         public bool Supports(string token)
         {
            return token == Parser.Token.GreedyStar;
         }
         public IExpression<T> CreateExpression(Parser.TokenStream<T> tokens, IExpression<T> containedExpression)
         {
            return new GreedyRepeatExpression<T> (containedExpression, 0, Int32.MaxValue);
         }
      }

      class LazyPlusToken : IModifierToken
      {
         public bool Supports(string token)
         {
            return token == Parser.Token.LazyPlus;
         }
         public IExpression<T> CreateExpression(Parser.TokenStream<T> tokens, IExpression<T> containedExpression)
         {
            return new RepeatExpression<T> (containedExpression, 1, Int32.MaxValue);
         }
      }

      class GreedyPlusToken : IModifierToken
      {
         public bool Supports(string token)
         {
            return token == Parser.Token.GreedyPlus;
         }
         public IExpression<T> CreateExpression(Parser.TokenStream<T> tokens, IExpression<T> containedExpression)
         {
            return new GreedyRepeatExpression<T> (containedExpression, 1, Int32.MaxValue);
         }
      }

      class RepeatOpenToken : IModifierToken
      {
         public bool Supports(string token)
         {
            return token == Parser.Token.RepeatOpen;
         }
         public IExpression<T> CreateExpression(Parser.TokenStream<T> tokens, IExpression<T> containedExpression)
         {
            return new RepetitionTokenParser<T>(tokens, containedExpression)
               .ParseExpression();
         }
      }

      static readonly IToken[] TokenHandlers = new IToken[] {new WhitespaceToken(), new StartToken (), new EndToken (), new GroupToken(), new ClassToken (), new DotToken ()};
      static readonly IModifierToken[] ModifierTokenHandlers = new IModifierToken[] {new LazyQuestionToken(), new GreedyQuestionToken(), new LazyStarToken(), new GreedyStarToken(), new LazyPlusToken(), new GreedyPlusToken(), new RepeatOpenToken ()};

      readonly Parser.TokenStream<T> tokenStream;
      readonly List<IExpression<T>> list = new List<IExpression<T>>();
      private IExpression<T> previousExpression;

      public ListExpressionParser(Parser.TokenStream<T> tokenStream)
      {
         this.tokenStream = tokenStream;
      }

      public List<IExpression<T>> ParseExpressions ()
      {
         do
         {
            if (IsEndOfSubExpression(tokenStream.Current))
               break;
            ProcessToken(tokenStream.Current);
         } while (tokenStream.MoveNext ());

         return list;
      }

      private static bool IsEndOfSubExpression (string token)
      {
         return
            token == Parser.Token.EndOfStream
            || token == Parser.Token.GroupClose
            || token == Parser.Token.Pipe;
      }

      private void ProcessToken (string token)
      {
         bool handled =
            ProcessAsDiscreteToken(token)
            || ProcessAsModifierToken(token)
            || ProcessAsNamedExpression(token);
         if (!handled)
            throw CreateNamedExpressionIsNotDefinedException(token);
      }

      private bool ProcessAsDiscreteToken(string token)
      {
         var simpleTokenHandler = FindSimpleTokenHandlerThatSupports(token);
         if (simpleTokenHandler != null)
         {
            previousExpression = simpleTokenHandler.CreateExpression(tokenStream);
            list.AddUnlessNull(previousExpression);
            return true;
         }
         return false;
      }

      private static IToken FindSimpleTokenHandlerThatSupports(string current)
      {
         foreach (var tokenHandler in TokenHandlers)
         {
            if (tokenHandler.Supports(current))
               return tokenHandler;
         }
         return null;
      }

      private bool ProcessAsModifierToken(string token)
      {
         var tokenHandler = FindModifierTokenHandlerThatSupports(token);
         if (tokenHandler != null)
         {
            previousExpression = tokenHandler.CreateExpression(tokenStream, previousExpression);
            RemoveLastEntry();
            list.AddUnlessNull(previousExpression);
            return true;
         }
         return false;
      }

      private static IModifierToken FindModifierTokenHandlerThatSupports(string current)
      {
         foreach (var tokenHandler in ModifierTokenHandlers)
         {
            if (tokenHandler.Supports(current))
               return tokenHandler;
         }
         return null;
      }

      private bool ProcessAsNamedExpression(string token)
      {
         Parser.ExpressionDefinition<T> expDef;
         if (tokenStream.OtherExpressions.TryGetValue(token, out expDef))
         {
            previousExpression = expDef.GetExpression(tokenStream.OtherExpressions,
                                                      tokenStream.ParameterValues);
            list.AddUnlessNull(previousExpression);
            return true;
         }
         return false;
      }

      private static Exception CreateNamedExpressionIsNotDefinedException(string token)
      {
         return new UndefinedNamedExpressionExpressionParsingException(token);
      }

      private void RemoveLastEntry()
      {
         list.RemoveAt(list.Count - 1);
      }
   }
}
