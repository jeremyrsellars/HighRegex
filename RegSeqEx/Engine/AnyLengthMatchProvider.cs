using System;
using System.Collections.Generic;

namespace RegSeqEx.Engine
{
   internal static class AnyLengthMatchProvider
   {
      public abstract class AnyLengthEnumerable : IEnumerable<int>
      {
         public AnyLengthEnumerable (int max)
         {
            Maximum = max;
         }
         
         public int Maximum{get; private set;}
         public abstract IEnumerator<int> GetEnumerator();
         
         System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
         {
            return GetEnumerator ();
         }
      }
      
      public class AscendingAnyLengthEnumerable : AnyLengthEnumerable
      {
         public AscendingAnyLengthEnumerable (int max)
            : base (max)
         {
         }
         
         public override IEnumerator<int> GetEnumerator()
         {
            return GetAscendingCore (Maximum).GetEnumerator ();
         }
      }
      
      public class DescendingAnyLengthEnumerable : AnyLengthEnumerable
      {
         public DescendingAnyLengthEnumerable (int max)
            : base (max)
         {
         }
         
         public override IEnumerator<int> GetEnumerator()
         {
            return GetDescendingCore (Maximum).GetEnumerator ();
         }
      }
      
      public static AscendingAnyLengthEnumerable GetAscending (int max)
      {
         return new AscendingAnyLengthEnumerable(max);
      }
      
      public static DescendingAnyLengthEnumerable GetDescending (int max)
      {
         return new DescendingAnyLengthEnumerable(max);
      }

      private static IEnumerable<int> GetAscendingCore (int max)
      {
         for (int i = 0; i <= max; i++)
            yield return i;
      }

      private static IEnumerable<int> GetDescendingCore (int max)
      {
         for (int i = max; i >= 0; i--)
            yield return i;
      }

      public static readonly int [] NoMatches = new int [0];
   }
}