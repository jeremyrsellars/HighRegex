namespace HighRegex.ParsedExpressionSupport
{
   public class RepetitionRangeTokenParser<T>
   {
      public class RepetitionRange
      {
         public int Min;
         public int Max;
      }

      private readonly Parser.TokenStream<T> tokens;
      private bool alreadyFoundComma;
      private int? explicitMax;

      private int GetMinimumFromCurrentToken()
      {
         return IsCurrentTokenComma() ? 0 : ParseCurrentTokenAsRepetitionCount();
      }

      private int UseRemainingTokensToFindMaximum(int minimum)
      {
         alreadyFoundComma = IsCurrentTokenComma();
         for (AdvanceOrThrow (); IsNotACloseToken(); AdvanceOrThrow ())
            ProcessCurrentTokenToFindMaximum();
         return CalculateMaximum(minimum);
      }

      private int CalculateMaximum(int minimum)
      {
         return explicitMax ?? (alreadyFoundComma ? int.MaxValue : minimum);
      }

      private void AdvanceOrThrow()
      {
         tokens.MoveNextOrThrow();
      }

      private bool IsNotACloseToken()
      {
         return !IsCurrentACloseToken();
      }

      private bool IsCurrentACloseToken()
      {
         return tokens.Current == Parser.Token.RepeatClose
                || tokens.Current == Parser.Token.LazyRepeatClose;
      }

      private void ProcessCurrentTokenToFindMaximum()
      {
         if (IsCurrentTokenComma())
            ProcessComma();
         else
            ProcessMaximum();
      }

      private bool IsCurrentTokenComma()
      {
         return tokens.Current == Parser.Token.Comma;
      }

      private void ProcessComma()
      {
         if (alreadyFoundComma)
            throw CreateAlreadyFoundComma();
         alreadyFoundComma = true;
      }

      private InvalidRepetitionExpressionException CreateAlreadyFoundComma()
      {
         throw new InvalidRepetitionExpressionException(
            "Expected number or }.  Found: " + tokens.Current);
      }

      private void ProcessMaximum()
      {
         if (explicitMax.HasValue)
            throw CreateAlreadyFoundMax();
         explicitMax = ParseCurrentTokenAsRepetitionCount();
      }

      private InvalidRepetitionExpressionException CreateAlreadyFoundMax()
      {
         return new InvalidRepetitionExpressionException (
            "Expected }.  Found: " + tokens.Current);
      }

      private int ParseCurrentTokenAsRepetitionCount()
      {
         int repititionCount;
         if (!int.TryParse (tokens.Current, out repititionCount))
            throw CreateExpectedNumber();
         return repititionCount;
      }

      private InvalidRepetitionExpressionException CreateExpectedNumber()
      {
         return new InvalidRepetitionExpressionException (
            "Expected number.  Found: " + tokens.Current);
      }

      public RepetitionRangeTokenParser(Parser.TokenStream<T> tokens)
      {
         this.tokens = tokens;
      }

      public RepetitionRange ParseRepetitionRange()
      {
         int min = GetMinimumFromCurrentToken();
         int max = UseRemainingTokensToFindMaximum(min);
         return new RepetitionRange {Min = min, Max = max};
      }
   }
}
