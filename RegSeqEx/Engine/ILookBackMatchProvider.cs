using System;
using System.Collections.Generic;

namespace RegSeqEx.Engine
{
   /// <summary>
   /// Provides support for expressions that match patterns that preceed the current index.
   /// </summary>
   /// <remarks>
   /// While this interface has no generic type arguments, it is intended for use by implementers of IExpression of T.
   /// Implementations of IExpression must implement this interface in order to be used by a lookbehind expression.
   /// To be usefull, implementations should return True for AnyLength or return possible match lengths.
   /// </remarks>
   public interface ILookBackMatchProvider
   {
      /// <summary>
      /// Gets a value indicating whether the <c>ILookBackMatchProvider</c> can be used to find a match that preceeds the current index.
      /// </summary>
      /// <remarks>
      /// If an expression that would otherwise support lookback is comprised of an expression
      /// that does not support lookback, this returns false.
      /// </remarks>
      bool SupportsLookBack{get;}
      
      /// <summary>
      /// Gets a value indicating that no predictable patern can be determined for a pattern, so a lookbehind match could have any length.
      /// </summary>
      bool AnyLength{get;}
      
      /// <summary>
      /// Gets an Enumerable that iterates over the valid lengths of a match that are equal to or less than <paramref name="maxLength"/>.
      /// Only inputs of these lengths need to be tested, because length not returned would never match.
      /// </summary>
      /// <param name="maxLength">The maximum length match to return.</param>
      /// <returns>An enumerable of match lengths that the expression should be tested for a match, but does not include lengths that could never match per some cardinality constraint.</returns>
      /// <remarks>Implementations of <c>ILookBackMatchProvider</c> must return true from AnyLength or implement this method to be usefull.</remarks>
      IEnumerable<int> GetPossibleMatchLengths(int maxLength);
   }
}