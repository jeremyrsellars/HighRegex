using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using HighRegex.ParsedExpressionSupport;

namespace HighRegex
{
   public static class Parser
   {
      public static class Token
      {
         public const string AtomicLookahead = "(?=";
         public const string AtomicNegativeLookahead = "(?!";
         public const string AtomicLookbehind = "(?<=";
         public const string AtomicNegativeLookbehind = "(?<!";
         public const string GroupOpen = "(";
         public const string GroupClose = ")";
         public const string ClassSetOpen = "[";
         public const string NegatedClassSetOpen = "[^";
         public const string ClassSetClose = "]";
         public const string RepeatOpen = "{";
         public const string RepeatClose = "}";
         public const string LazyRepeatClose = "}?";
         public const string Caret = "^";
         public const string Dollar = "$";
         public const string GreedyStar = "*";
         public const string LazyStar = "*?";
         public const string GreedyPlus = "+";
         public const string LazyPlus = "+?";
         public const string GreedyQuestion = "?";
         public const string LazyQuestion = "??";
         public const string Dot = ".";
         public const string Pipe = "|";
         public const string Comma = ",";
         public const string EndOfStream = "{EndOfStream}";
      }
      public class ParseResult<TData>
      {
         public ParseResult(IEnumerable<ExpressionName<TData>> expressions, IEnumerable<string> preamble)
         {
            if (expressions == null)
               throw new ArgumentNullException("expressions");
            Expressions = expressions;
            Preamble = preamble == null ? Enumerable.Empty<string>() : preamble.ToList ();
         }
         public IEnumerable<ExpressionName<TData>> Expressions { get; private set; }
         public IEnumerable<string> Preamble { get; private set; }
      }
      public class ExpressionDefinition<T>
      {
         public string Type{get;set;}
         public string Name{get;set;}
         public string Contents{get;set;}
         public IExpression<T> GetExpression (Dictionary<string,ExpressionDefinition<T>> otherExpressions, Dictionary<string,object> parameterValues)
         {
            Type expressionItemType = null;
            foreach (var assembly in GetAssemblies())
            {
               foreach (var type in assembly.GetTypes())
               {
                  if (type.FullName == Type)
                  {
                     expressionItemType = type;
                     break;
                  }
                  if (type.FullName == Type + "`1")
                  {
                     expressionItemType = type.MakeGenericType (typeof (T));
                     break;
                  }
               }
               if (expressionItemType != null)
                  break;
            }

            if (expressionItemType == null)
               expressionItemType = System.Type.GetType(Type);

            if (expressionItemType == null)
               throw new InvalidOperationException("Type could not be found: " + Type);
            if (expressionItemType.IsGenericTypeDefinition)
               expressionItemType = expressionItemType.MakeGenericType (typeof (T));

            foreach (ConstructorInfo ctor in expressionItemType.GetConstructors())
            {
               var @params = ctor.GetParameters ();
               if (@params.Length == 0 && string.IsNullOrEmpty(Contents))
                  return (IExpression<T>)ctor.Invoke (null);
               object [] invokeParams = new object[@params.Length];
               bool usedArg = false;
               bool ok = true;
               for (int i = 0; i < @params.Length; i++)
               {
                  var param = @params [i];
                  if (param.Name == "parameterValues")
                  {
                     invokeParams [i] = parameterValues;
                  }
                  else if (param.Name == "otherExpressions")
                  {
                     invokeParams [i] = otherExpressions;
                  }
                  else if (!parameterValues.TryGetValue(param.Name, out invokeParams[i]))
                  {
                     if (usedArg)
                     {
                        ok = false;
                        break;
                     }
                     if (param.ParameterType == typeof(string))
                     {
                        invokeParams [i] = Contents;
                        usedArg = true;
                     }
                     else
                     {
                        ok = false;
                     }
                  }
               }
               if (ok)
                  return (IExpression<T>)ctor.Invoke (invokeParams);
            }
            throw new InvalidOperationException ("Could not find usable constructor for " + expressionItemType);
         }

         private static IEnumerable<Assembly> GetAssemblies ()
         {
            var regexAssem = typeof (AnyClass<int>).Assembly;
            yield return regexAssem;
            var thisAssem = typeof (ExpressionDefinition<T>).Assembly;
            yield return thisAssem;
            foreach (Assembly assem in AppDomain.CurrentDomain.GetAssemblies())
            {
               if (assem != regexAssem && assem != thisAssem)
                  yield return assem;
            }
         }
      }

      public static ParseResult<T> ParseContents<T> (string fileContents, Dictionary<string,object> parameterValues)
      {
         List<ExpressionDefinition<T>> expressions = new List<ExpressionDefinition<T>> ();
         List<string> preamble = new List<string>();
         foreach (var line in fileContents.Split(new [] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
         {
            string remainder;
            if (line.StartsWith("ExpressionType=", out remainder))
            {
               Type genericType = Type.GetType (remainder, true);
               if (genericType != typeof(T))
                  throw new NotSupportedException ("Type is not supported: " + genericType);
            }
            else if (line.StartsWith(";", out remainder))
            {
               // Comment
            }
            else if (string.IsNullOrEmpty (line))
            {
               continue;
            }
            else if (line.Trim().StartsWith("Import "))
            {
               string assembly = line.Trim().Substring("Import ".Length);
               if (assembly.EndsWith (".dll", StringComparison.OrdinalIgnoreCase)
                  || assembly.EndsWith (".exe", StringComparison.OrdinalIgnoreCase))
                  Assembly.LoadFrom(assembly);
               else
                  Assembly.Load(assembly);
            }
            else if (line.Trim().StartsWith("Preamble "))
            {
               string command = line.Trim().Substring("Preamble ".Length);
               preamble.Add(command);
            }
            else
            {
               // Class/Expression Declaration
               int indexOfSpace = line.IndexOf (' ');
               int indexOfEquals = line.IndexOf ('=');
               string type = line.Substring (0, indexOfSpace);
               string name = line.Substring (indexOfSpace + 1, indexOfEquals - type.Length - 1);
               string expression = line.Substring (indexOfEquals + 1);

               expressions.Add (new ExpressionDefinition<T> {Type = type, Name = name, Contents = expression});
            }
         }

         var expDict = BuildExpressionDictionaryWithImprovedException(expressions);
         var expressionNames = expressions
            .Select(exp => new ExpressionName<T>{Expression = exp.GetExpression (expDict, parameterValues), Name = exp.Name})
            .ToList();

         return new ParseResult<T>(expressionNames, preamble);
      }

      private static Dictionary<string, ExpressionDefinition<T>> BuildExpressionDictionaryWithImprovedException<T>(IEnumerable<ExpressionDefinition<T>> expressions)
      {
         string name = null;
         try
         {
            return expressions.ToDictionary(def => name = def.Name);
         }
         catch (ArgumentException ae)
         {
            throw new ArgumentException(string.Format("{0}  Is {1} a duplicate?", ae.Message, name), ae);
         }
      }

      private static bool StartsWith(this string text, string s, out string remainder)
      {
         if (text.StartsWith(s))
         {
            remainder = text.Substring (s.Length);
            return true;
         }
         remainder = null;
         return false;
      }

      public static string [] Tokenize (string text)
      {
         return Tokenizer.Tokenize(text).ToArray ();
      }

      public class TokenStream<T>
      {
         public TokenStream (string [] tokens, Dictionary<string, ExpressionDefinition<T>> otherExpressions, Dictionary<string,object> parameterValues)
         {
            if (tokens == null)
               throw new ArgumentNullException ("tokens");
            m_tokens = tokens;
            m_otherExpressions = otherExpressions;
            m_parameterValues = parameterValues;
         }

         public string Current
         {
            get
            {
               if (m_index == m_tokens.Length)
                  return Token.EndOfStream;
               return m_tokens [m_index];
            }
         }

         public bool MoveNext ()
         {
            if (m_index == m_tokens.Length)
               return false;
            m_index++;
            return true;
         }

         public void MoveNextOrThrow ()
         {
            if (!MoveNext ())
               throw new IndexOutOfRangeException ("Past end of stream.");
         }

         public Dictionary<string, ExpressionDefinition<T>> OtherExpressions
         {
            get
            {
               return m_otherExpressions;
            }
         }

         public Dictionary<string,object> ParameterValues
         {
            get
            {
               return m_parameterValues;
            }
         }

         private readonly Dictionary<string, ExpressionDefinition<T>> m_otherExpressions;
         private readonly Dictionary<string, object> m_parameterValues;

         private int m_index;
         private readonly string [] m_tokens;
      }

      static readonly RegexTokenizer Tokenizer = new RegexTokenizer(new Regex (@"\(\?\<?[=!]|\[\^?|[?+*]\?|\}\??|[\^\$()\]|{,?*]|[a-zA-Z0-9_]+|\{\d*,\d*\}|\s|."));
   }
}
