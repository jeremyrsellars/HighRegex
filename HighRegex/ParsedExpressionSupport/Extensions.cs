using System.Collections.Generic;

namespace HighRegex.ParsedExpressionSupport
{
   static class Extensions
   {
      public static void AddUnlessNull<T>(this List<T> list, T item)
         where T : class
      {
         if (item != null)
            list.Add(item);
      }
   }
}