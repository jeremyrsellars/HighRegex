using System;

namespace HighRegex.ParsedExpressionSupport
{
   public class ExpressionParsingException : Exception
   {
      public ExpressionParsingException (string message = "The expression could not be parsed.", Exception innerException = null)
         : base (message, innerException)
      {
      }
   }

   public class InvalidExpressionException : ExpressionParsingException
   {
      public InvalidExpressionException(string message)
         : base (message)
      {
      }
   }

   public class InvalidRepetitionExpressionException : InvalidExpressionException
   {
      public InvalidRepetitionExpressionException(string message = "The specified repitition is not well-formed.")
         : base (message)
      {
      }
   }

   public class AlgorithmAssertionFailedException : ExpressionParsingException
   {
      public AlgorithmAssertionFailedException (string message = "The expression could not be parsed because of an assertion failure.", Exception innerException = null)
         : base (message, innerException)
      {
      }
   }

   public class UndefinedNamedClassExpressionParsingException : InvalidExpressionException
   {
      public UndefinedNamedClassExpressionParsingException(string className)
         : base ("The named class is not defined: " + className)
      {
      }
   }

   public class UndefinedNamedExpressionExpressionParsingException : InvalidExpressionException
   {
      public UndefinedNamedExpressionExpressionParsingException(string className)
         : base ("The named expression is not defined: " + className)
      {
      }
   }

   public class InvalidTokenInClassExpressionParsingException : InvalidExpressionException
   {
      public InvalidTokenInClassExpressionParsingException(string token)
         : base ("This token is invalid in a class set: " + token)
      {
      }
   }
}
