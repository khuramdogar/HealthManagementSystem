using NUnit.Framework;

namespace TestIgnorableTests
{
   [TestFixture]
   public class NUnitExplicitTests
   {
      [Test]
      public void NUnitIsAbleToRun()
      {
         Assert.IsTrue(true);
      }

      [Test, Explicit]
      public void NUnitIsInvokeableViaResharper()
      {
         Assert.IsTrue(true);
      }
   }
}