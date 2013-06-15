using System;
using System.Collections.Generic;

namespace HighRegex
{
   /// <summary>
   /// A zero-width assertion that the expression is not followed by the specified expression.
   /// </summary>
   /// <typeparam name="T">The type of item matched by a regular expression</typeparam>
   public class NegativeLookAheadExpression<T> : LookAheadExpression<T>
   {
      /// <summary>
      /// Creates a new instance of an expression that matches when <paramref name="expression"/> is not found following the specified index.
      /// </summary>
      /// <param name="expression">The asserted expression.</param>
      public NegativeLookAheadExpression (IExpression<T> expression)
         : base (expression, true)
      {
      }
   }
}
