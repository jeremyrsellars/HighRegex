namespace HighRegex.ParsedExpressionSupport
{
   public class RepetitionTokenParser<T>
   {
      // note: Greedy formats: X{1,1}  X{1}  X{,2}  X{,}  X{2,}
      // note: Lazy formats:   X{1,1}? X{1}? X{,2}? X{,}? X{2,}?

      private readonly Parser.TokenStream<T> tokens;
      private readonly IExpression<T> repeatedExpression;
      private bool lazy;
      private int minRepititionCount;
      private int maxRepititionCount;

      public RepetitionTokenParser(Parser.TokenStream<T> tokens, IExpression<T> repeatedExpression)
      {
         this.tokens = tokens;
         this.repeatedExpression = repeatedExpression;
      }

      public IExpression<T> ParseExpression()
      {
         EnsureCurrentTokenIsRepeatOpen();
         UseRemainingTokensToGetMinAndMaxRepitionCount();
         ThrowIfMaxIsLessThanMin();
         return CreateRepeatExpression();
      }

      private void EnsureCurrentTokenIsRepeatOpen()
      {
         if (tokens.Current != Parser.Token.RepeatOpen)
            throw new InvalidRepetitionExpressionException("Expected { BraceOpen.  Found: " + tokens.Current);
      }

      private void UseRemainingTokensToGetMinAndMaxRepitionCount()
      {
         SkipRepOpen();
         ParseRepetitionRangeTokens();
         lazy = tokens.Current == Parser.Token.LazyRepeatClose;
         EnsureCurrentTokenIsAVariantOfRepeatClose();
      }

      private void SkipRepOpen()
      {
         tokens.MoveNextOrThrow ();
      }

      private void ParseRepetitionRangeTokens()
      {
         var range = new RepetitionRangeTokenParser<T>(tokens).ParseRepetitionRange();
         minRepititionCount = range.Min;
         maxRepititionCount = range.Max;
      }

      private IExpression<T> CreateRepeatExpression()
      {
         if (RepeatExactlyOnce())
            return repeatedExpression;
         
         if (lazy || CanOptimizeBecauseMinEqualsMax())
            return CreateLazyRepeatExpression();
         return CreateGreedyRepeatExpression();
      }

      private bool RepeatExactlyOnce()
      {
         return minRepititionCount == 1 && maxRepititionCount == 1;
      }

      private bool CanOptimizeBecauseMinEqualsMax()
      {
         return minRepititionCount == maxRepititionCount;
      }

      private RepeatExpression<T> CreateLazyRepeatExpression()
      {
         return new RepeatExpression<T> (repeatedExpression, minRepititionCount, maxRepititionCount);
      }

      private GreedyRepeatExpression<T> CreateGreedyRepeatExpression()
      {
         return new GreedyRepeatExpression<T> (repeatedExpression, minRepititionCount, maxRepititionCount);
      }

      private void ThrowIfMaxIsLessThanMin()
      {
         if (maxRepititionCount < minRepititionCount)
            throw new InvalidRepetitionExpressionException(
               string.Format("max should be at least min. min:{0}, max:{1}", minRepititionCount, maxRepititionCount));
      }

      private bool IsCurrentTokenAVariantOfRepeatClose()
      {
         return tokens.Current == Parser.Token.RepeatClose
                || tokens.Current == Parser.Token.LazyRepeatClose;
      }

      private void EnsureCurrentTokenIsAVariantOfRepeatClose()
      {
         if (IsCurrentTokenAVariantOfRepeatClose())
            return;

         string msg = "Expected }.  Found: " + tokens.Current;
         System.Diagnostics.Debug.Assert(false, msg);
         throw new InvalidRepetitionExpressionException(msg);
      }
   }
}
