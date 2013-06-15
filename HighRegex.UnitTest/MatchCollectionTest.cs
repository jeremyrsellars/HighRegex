using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HighRegex
{
   [TestClass]
   public class MatchCollectionTest
   {
      public MatchCollectionTest ()
      {
         Match<char> match;
         
         try
         {
            match = new Match<char> ("0123456789".ToListCursor (), 0, 10, true);
         }
         catch (Exception e)
         {
            Assert.Inconclusive (e.ToString ());
            throw;
         }
         m_allDigitsMatch = match;

         try
         {
            match = new Match<char> ("0123456789".ToListCursor (), 0, 0, false);
         }
         catch (Exception e)
         {
            Assert.Inconclusive (e.ToString ());
            throw;
         }
         m_noDigitsMatch = match;
      }
      
      private Match<char> m_allDigitsMatch;
      private Match<char> m_noDigitsMatch;
      
      /// <summary>
      ///Gets or sets the test context which provides
      ///information about and functionality for the current test run.
      ///</summary>
      public TestContext TestContext{get;set;}

      [TestMethod]
      public void ConstrutorSucceeds()
      {
         MatchCollection<char> matches = new MatchCollection<char> ();
      }

      [TestMethod]
      public void IsReadonly()
      {
         MatchCollection<object> matches = new MatchCollection<object> ();
         matches.VerifyListIsReadonly ();
      }

      [TestMethod]
      [ExpectedException (typeof (ArgumentNullException))]
      public void InternalAddThrowsArgumentNullException()
      {
         MatchCollection<char> matches = new MatchCollection<char> ();
         
         matches.Add (null);
      }

      [TestMethod]
      public void InternalAddSucceeds()
      {
         MatchCollection<char> matches = new MatchCollection<char> ();
         
         matches.Add (m_allDigitsMatch);
         matches.Add (m_noDigitsMatch);
      }

      [TestMethod]
      public void InternalAddSucceedsWhenLocked()
      {
         MatchCollection<char> matches = new MatchCollection<char> ();
         
         matches.Lock ();
      }

      [TestMethod]
      public void InternalAddSucceedsWhenAddedThenLocked()
      {
         MatchCollection<char> matches = new MatchCollection<char> ();
         
         matches.Add (m_noDigitsMatch);
         
         matches.Lock ();
      }

      [TestMethod]
      [ExpectedException(typeof(InvalidOperationException))]
      public void InternalAddFailsWhenLockedAndAdded()
      {
         MatchCollection<char> matches = new MatchCollection<char> ();
         
         matches.Lock ();
         
         // Fails
         matches.Add (m_allDigitsMatch);
      }

      [TestMethod]
      [ExpectedException(typeof(InvalidOperationException))]
      public void InternalAddFailsWhenAddedLockedAndAdded()
      {
         MatchCollection<char> matches = new MatchCollection<char> ();
         
         matches.Add (m_noDigitsMatch);

         matches.Lock ();
         
         // Fails
         matches.Add (m_allDigitsMatch);
      }

   }
   
   static class TestExtensions
   {
      public static ListExpressionItemSource<char> ToListCursor(this string s)
      {
         var cursor = new ListExpressionItemSource<char> (s.ToCharArray ());
         return cursor;
      }
   }
}
