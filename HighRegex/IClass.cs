using System;
using System.Collections.Generic;

namespace HighRegex
{
   /// <summary>
   /// Represents a regular expression that matches a single item.
   /// </summary>
   /// <typeparam name="T">The type of item matched by a regular expression</typeparam>
   public interface IClass<T> : IExpression<T>
   {
      /// <summary>
      /// Indicates whether the regular expression class matches the input.
      /// </summary>
      /// <param name="input">A single item.</param>
      /// <returns>true if the regular expression matches the input; otherwise false.</returns>
      bool IsMatch (T input);
   }
}
