using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HighRegex
{
   [TestClass]
   public class AnyClassStringTest
   {
      private IClass<string> m_expression= new AnyClass<string> ();
      
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
      public void ReturnsTrueForNull()
      {
         Assert.IsTrue (m_expression.IsMatch (null));
      }
   }
}
