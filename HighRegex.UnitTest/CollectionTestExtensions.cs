using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using HighRegex.Engine;

namespace HighRegex
{
   public static class CollectionTestExtensions
   {
      public static void VerifyEnumeratorsAgainstIndexer<T> (this IList<T> list)
      {
         VerifyEnumerators (list,list);
      }

      public static void VerifyEnumerators<T> (this IList<T> list, IList<T> expectedInput)
      {
         Assert.IsNotNull (list, "list should not be null");

         //
         // Check count against expected input.
         //
         int count = expectedInput.Count;
         Assert.AreEqual (count, list.Count, "Count");

         //
         // Check enumerators and indexers against expected input.
         //
         IEnumerator<T> enumeratorOfT = list.GetEnumerator ();
         System.Collections.IEnumerator enumerator = list.GetEnumerator ();
         for (int i = 0; i < count; i++)
         {
            // Test enumerator
            Assert.IsTrue (enumerator.MoveNext ());
            Assert.AreEqual (expectedInput[i], enumerator.Current, "enumerator.Current at index " + i);
            
            // Test enumerator
            Assert.IsTrue (enumeratorOfT.MoveNext ());
            Assert.AreEqual (expectedInput[i], enumeratorOfT.Current, "enumeratorOfT.Current at index " + i);
            
            // Test indexer
            Assert.AreEqual (expectedInput [i], list[i], "list.Item at index " + i);
         }
         
         // Make sure no more items are enumerated.
         Assert.IsFalse (enumerator.MoveNext (), "enumerator");
         Assert.IsFalse (enumeratorOfT.MoveNext (), "enumeratorOfT");
      }

      public static void VerifyListIsReadonly<T> (this IList<T> list)
      {
         Assert.IsNotNull (list, "list should not be null");

         AssertThrowsNotSupportedException (
            (Action)(() => 
               list.RemoveAt (0)
               ),
            "IList<T>.RemoveAt");

         AssertThrowsNotSupportedException (
            (Action)(() => 
               list.Insert (0, default (T))
               ),
            "IList<T>.Insert");

         VerifyCollectionIsReadonly (list);
      }
      
      public static void VerifyCollectionIsReadonly<T> (this ICollection<T> list)
      {
         Assert.IsNotNull (list, "list should not be null");

         AssertThrowsNotSupportedException (
            (Action)(() => 
               list.Add (default (T))
               ),
            "ICollection<T>.Add");

         AssertThrowsNotSupportedException (
            (Action)(() => 
               list.Remove (default (T))
               ),
            "ICollection<T>.Remove");

         AssertThrowsNotSupportedException (
            (Action)(() => 
               list.Clear()
               ),
            "ICollection<T>.Clear");

         Assert.IsTrue (list.IsReadOnly, "ICollection<T>.IsReadOnly");
      }
      
      public static void AssertThrowsNotSupportedException (this Action action, string detail)
      {
         AssertThrows (action, typeof (NotSupportedException), detail);
      }
      
      public static void AssertThrows (this Action action, Type type, string detail)
      {
         try
         {
            action ();
         }
         catch (Exception e)
         {
            if (type.IsAssignableFrom (e.GetType ()))
            {
               Assert.IsInstanceOfType (e, type, "Expected instance of " + type + ".  " + detail);
               return;
            }
         }
         Assert.Fail ("Expected instance of " + type + ".  " + detail);
      }
      
      
      public static void AssertPossibleMatchLengths(this ILookBackMatchProvider expression, IList<int> expected)
      {
         var actual = expression.GetPossibleMatchLengths (100).ToList ();
         Assert.AreEqual (expected.Count, actual.Count, "Count");
         for (int i = 0; i < actual.Count; i++)
            Assert.AreEqual (expected [i], actual [i], "Element at " + i);
      }
   }
}
