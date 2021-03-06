﻿using System;
using System.Collections.Generic;
using HighRegex.Engine;

namespace HighRegex
{
   class MockExpression<T> : IExpression<T>, ILookBackMatchProvider
   {

      #region ILookBackMatchProvider<char> Members

      /// <summary>
      /// Gets a value indicating whether the <c>ILookBackMatchProvider</c> can be used to find a match that preceeds the current index.
      /// </summary>
      /// <remarks>
      /// If an expression that would otherwise support lookback is comprised of an expression
      /// that does not support lookback, this returns false.
      /// </remarks>
      public bool SupportsLookBack
      {
         get;internal set;
      }

      /// <summary>
      /// Gets a value indicating that no predictable patern can be determined for a pattern, so a lookbehind match could have any length.
      /// </summary>
      public bool AnyLength
      {
         get; internal set;
      }

      public IEnumerable<int> PossibleMatchLengths{get;set;}
      public Func<int,IEnumerable<int>> PossibleMatchLengthsFunc{get;set;}

      /// <summary>
      /// Gets an Enumerable that iterates over the valid lengths of a match that are equal to or less than <paramref name="maxLength"/>.
      /// Only inputs of these lengths need to be tested, because length not returned would never match.
      /// </summary>
      /// <param name="maxLength">The maximum length match to return.</param>
      /// <returns>An enumerable of match lengths that the expression should be tested for a match, but does not include lengths that could never match per some cardinality constraint.</returns>
      /// <remarks>Implementations of <c>ILookBackMatchProvider</c> must return true from AnyLength or implement this method to be usefull.</remarks>
      public IEnumerable<int> GetPossibleMatchLengths(int maxLength)
      {
         return PossibleMatchLengths ?? PossibleMatchLengthsFunc (maxLength);
      }

      #endregion

      #region IExpression<T> Members

      MatchLength IExpression<T>.IsMatchAt(IExpressionItemSource<T> input, int index)
      {
         throw new NotImplementedException();
      }

      #endregion
   }
}
