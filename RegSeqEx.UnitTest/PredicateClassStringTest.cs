using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RegSeqEx
{
   [TestClass]
   public class PredicateClassStringTest
   {
      private IClass<string> m_expression= new PredicateClass<string> (input => input == "A");
      
      /// <summary>
      ///Gets or sets the test context which provides
      ///information about and functionality for the current test run.
      ///</summary>
      public TestContext TestContext{get;set;}

      [TestMethod]
      public void ReturnsTrueForA()
      {
         Assert.IsTrue (m_expression.IsMatch ("A"));
      }

      [TestMethod]
      public void ReturnsFalseForB()
      {
         Assert.IsFalse (m_expression.IsMatch ("B"));
      }

      [TestMethod]
      public void ReturnsFalseForNull()
      {
         Assert.IsFalse (m_expression.IsMatch (null));
      }
   }
}
