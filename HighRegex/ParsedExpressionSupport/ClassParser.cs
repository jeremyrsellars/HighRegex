using System;
using System.Collections.Generic;

namespace HighRegex.ParsedExpressionSupport
{
   public class ClassParser<T>
   {
      readonly Parser.TokenStream<T> tokens;

      static bool IsEndOfClassExpression(string token)
      {
         return token == Parser.Token.ClassSetClose
            || token == Parser.Token.EndOfStream;
      }

      static bool IsSpaceOrTab(string token)
      {
         return token == " " || token == "\t";
      }

      static bool IsInvalidInsideClassExpression(string token)
      {
         switch (token)
         {
            case Parser.Token.Caret:
            case Parser.Token.ClassSetOpen:
            case Parser.Token.NegatedClassSetOpen:
            case Parser.Token.Dollar:
            case Parser.Token.GroupOpen:
            case Parser.Token.Pipe:
            case Parser.Token.AtomicLookahead:
            case Parser.Token.AtomicNegativeLookahead:
            case Parser.Token.AtomicLookbehind:
            case Parser.Token.AtomicNegativeLookbehind:
            case Parser.Token.LazyQuestion:
            case Parser.Token.GreedyQuestion:
            case Parser.Token.LazyStar:
            case Parser.Token.GreedyStar:
            case Parser.Token.LazyPlus:
            case Parser.Token.GreedyPlus:
               return true;
            default:
               return false;
         }
      }

      List<IClass<T>> ParseClassItems()
      {
         List<IClass<T>> classes = new List<IClass<T>> ();

         do
         {
            if (IsEndOfClassExpression(tokens.Current))
               break;
            classes.AddUnlessNull(CreateClassForValidTokenOrThrow(tokens.Current));
         } while (tokens.MoveNext ());

         return classes;
      }

      IClass<T> CreateClassForValidTokenOrThrow(string token)
      {
         if (IsSpaceOrTab(token))
            return null;
         if (IsInvalidInsideClassExpression(token))
            throw CreateInvalidExpressionTokenInClass(token);

         Parser.ExpressionDefinition<T> expDef;
         if (tokens.OtherExpressions.TryGetValue(token, out expDef))
            return CreateExpression(expDef);

         throw CreateNamedClassNotFound(token);
      }

      static Exception CreateNamedClassNotFound(string className)
      {
         return new UndefinedNamedClassExpressionParsingException(className);
      }

      static Exception CreateInvalidExpressionTokenInClass(string token)
      {
         return new InvalidTokenInClassExpressionParsingException (token);
      }

      IClass<T> CreateExpression(Parser.ExpressionDefinition<T> expDef)
      {
         return (IClass<T>)expDef.GetExpression (tokens.OtherExpressions, tokens.ParameterValues);
      }
      
      static IClass<T> CreateOptimizedPositiveClass(List<IClass<T>> list)
      {
         if (list.Count == 1)
            return list[0];
         return new SetClass<T>(list.ToArray ());
      }

      static IClass<T> CreateOptimizedNegativeClass(List<IClass<T>> list)
      {
         if (list.Count == 1)
            return new NotClass<T>(list[0]);
         return new NegativeSetClass<T>(list.ToArray ());
      }

      void ThrowUnlessCurrentlyOnClassSetClose()
      {
         if (tokens.Current != Parser.Token.ClassSetClose)
            throw new InvalidExpressionException ("Expected ] ClassSetClose.  Found: " + tokens.Current);
      }

      void ThrowUnlessCurrentlyOnClassSetOpen()
      {
         if (tokens.Current != Parser.Token.ClassSetOpen && tokens.Current != Parser.Token.NegatedClassSetOpen)
            throw new InvalidExpressionException ("Expected [ ClassSetOpen.  Found: " + tokens.Current);
      }

      public ClassParser(Parser.TokenStream<T> tokens)
      {
         this.tokens = tokens;
      }

      public IClass<T> ParseClass ()
      {
         ThrowUnlessCurrentlyOnClassSetOpen();
         bool negate = tokens.Current == Parser.Token.NegatedClassSetOpen;
         tokens.MoveNextOrThrow ();
         List<IClass<T>> list = ParseClassItems();
         ThrowUnlessCurrentlyOnClassSetClose();
         return negate ? CreateOptimizedNegativeClass(list) : CreateOptimizedPositiveClass(list);
      }
   }
}
