namespace HighRegex
{
   public class ExpressionName<T>
   {
      public string Name { get; set; }
      public IExpression<T> Expression { get; set; }
   }
}