using System;
using System.Collections.Generic;
using HighRegex.Engine;

namespace HighRegex
{
   /// <summary>
   /// A zero-width assertion that the expression is followed by the specified expression.
   /// </summary>
   /// <typeparam name="T">The type of item matched by a regular expression</typeparam>
   public class LookAheadExpression<T> : IExpression<T>, ILookBackMatchProvider
   {
      /// <summary>
      /// Creates an expression that has no width, but will match if contained expression matches.
      /// </summary>
      /// <param name="expression">The expression to compare with the input items at the current position.</param>
      public LookAheadExpression (IExpression<T> expression)
         : this (expression, false)
      {
      }

      /// <summary>
      /// Creates an expression that has no width, but will match if contained expression matches.
      /// </summary>
      /// <param name="expression">The expression to compare with the input items at the current position.</param>
      /// <param name="negate">Inheritors may negate the base functionality by supplying true.</param>
      protected internal LookAheadExpression (IExpression<T> expression, bool negate)
      {
         if (expression == null)
            throw new ArgumentNullException ("expression");
         
         Negate = negate;

         m_lookBack = m_expression as ILookBackMatchProvider ?? new ExpressionMatchProvider<T> (expression);
         m_expression = m_lookBack as IMatchProvider<T> ?? new ExpressionMatchProvider<T> (expression);
      }

      /// <summary>
      /// Gets a value indicating the IsMatch is negated.  When negated, the expression matches when
      /// the contained expression does not match the input at the current index.
      /// </summary>
      public bool Negate{get;private set;}

      /// <summary>
      /// Indicates whether the regular expression finds a match in the input beginning at the specified index.
      /// Indexes after <paramref name="index"/> will not be search.
      /// </summary>
      /// <param name="input">The items to search for a match</param>
      /// <param name="index">The index at which the expression must match.</param>
      /// <returns>A value indicating whether the regular expression matches the input at the specified index.</returns>
      /// <remarks>
      /// This interface is usually not used called directly, but rather used
      /// by the extension methods defined in ExpressionExtensions.
      /// To find matches at indexes after <paramref name="index"/>, use ExpressionExtensions.Matches
      /// or ExpressionExtensions.Match.
      /// </remarks>
      public MatchLength IsMatchAt (IExpressionItemSource<T> input, int index)
      {
         bool isMatch = m_expression.IsMatchAtCore (input, index).Success;
         return MatchLength.EmptyMatch(Negate ^ isMatch);
      }
      
      /// <summary>
      /// Gets a value indicating whether the <c>ILookBackMatchProvider</c> can be used to find a match that preceeds the current index.
      /// </summary>
      /// <remarks>
      /// If an expression that would otherwise support lookback is comprised of an expression
      /// that does not support lookback, this returns false.
      /// </remarks>
      public bool SupportsLookBack
      {
         get
         {
            return m_lookBack.SupportsLookBack;
         }
      }

      /// <summary>
      /// Gets a value indicating that no predictable patern can be determined for a pattern, so a lookbehind match could have any length.
      /// </summary>
      public bool AnyLength
      {
         get
         {
            return false;
         }
      }

      /// <summary>
      /// Gets an Enumerable that iterates over the valid lengths of a match that are equal to or less than <paramref name="maxLength"/>.
      /// Only inputs of these lengths need to be tested, because length not returned would never match.
      /// </summary>
      /// <param name="maxLength">The maximum length match to return.</param>
      /// <returns>An enumerable of match lengths that the expression should be tested for a match, but does not include lengths that could never match per some cardinality constraint.</returns>
      /// <remarks>Implementations of <c>ILookBackMatchProvider</c> must return true from AnyLength or implement this method to be usefull.</remarks>
      public IEnumerable<int> GetPossibleMatchLengths(int maxLength)
      {
         foreach (int matchLength in m_lookBack.GetPossibleMatchLengths (maxLength))
         {
            // Doesn't matter what the length is, we'll return zero because we're an atomic assertion.
            yield return 0;
            yield break;
         }
      }

      /// <summary>
      /// An IMatchProvider that wraps the contained Expression.
      /// </summary>
      protected readonly IMatchProvider<T> m_expression;

      /// <summary>
      /// An ILookBackMatchProvider that wraps the contained Expression.
      /// </summary>
      protected readonly ILookBackMatchProvider m_lookBack;
   }
}
