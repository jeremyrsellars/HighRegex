using System;
using System.Collections.Generic;

namespace HighRegex
{
   public struct MatchLength
   {
      public static readonly MatchLength NoMatch = new MatchLength();

      public static MatchLength Of(int length)
      {
         return new MatchLength(length);
      }

      public static MatchLength EmptyMatch(bool success)
      {
         return new MatchLength(0, success);
      }

      public static MatchLength ClassMatchIf(bool success)
      {
         return new MatchLength(success ? 1 : 0, success);
      }

      public MatchLength(int length, bool success = true)
      {
         Length = length;
         Success = success;
      }

      public readonly bool Success;
      public readonly int Length;
   }
   /// <summary>
   /// Represents a regular expression capable of matching input at a specific index.
   /// </summary>
   /// <typeparam name="T">The type of item matched by a regular expression</typeparam>
   /// <remarks>
   /// This interface is usually not used called directly, but rather used
   /// by the extension methods defined in ExpressionExtensions.
   /// </remarks>
   public interface IExpression<T>
   {
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
      MatchLength IsMatchAt (IExpressionItemSource<T> input, int index);
   }
}
