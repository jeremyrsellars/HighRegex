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
}