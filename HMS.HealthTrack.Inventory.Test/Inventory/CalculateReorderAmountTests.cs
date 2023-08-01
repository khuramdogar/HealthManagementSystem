using HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HMS.HealthTrack.Inventory.Test.Inventory
{
   [TestClass]
   public class CalculateReorderAmountTests
   {
      [TestMethod]
      public void TestNoAmountsGiven()
      {
         var result = OrderableItemsUnitOfWork.CalculateReorderAmount(currentStock: 0, targetLevel: 0, minOrder: 0, multiplesOf: 0);
         Assert.AreEqual(0, result);
      }

      [TestMethod]
      public void TestStockLevelSpecified()
      {
         var result = OrderableItemsUnitOfWork.CalculateReorderAmount(currentStock: 10, targetLevel: 22, minOrder: 0, multiplesOf: 0);
         Assert.AreEqual(12, result);
      }

      [TestMethod]
      public void TestStockLevelSpecified_OverStocked()
      {
         var result = OrderableItemsUnitOfWork.CalculateReorderAmount(currentStock: 1000, targetLevel: 10, minOrder: 0, multiplesOf: 0);
         Assert.AreEqual(0, result);
      }

      [TestMethod]
      public void TestStockLevelSpecified_OverStocked_WithMin()
      {
         var result = OrderableItemsUnitOfWork.CalculateReorderAmount(currentStock: 1000, targetLevel: 10, minOrder: 10, multiplesOf: 0);
         Assert.AreEqual(0, result);
      }

      [TestMethod]
      public void TestStockLevelSpecified_OverStocked_WithMultiples()
      {
         var result = OrderableItemsUnitOfWork.CalculateReorderAmount(currentStock: 1000, targetLevel: 10, minOrder: 0, multiplesOf: 50);
         Assert.AreEqual(0, result);
      }

      [TestMethod]
      public void TestStockLevelSpecified_OverStocked_WithMultiples_WithMin()
      {
         var result = OrderableItemsUnitOfWork.CalculateReorderAmount(currentStock: 1000, targetLevel: 10, minOrder: 10, multiplesOf: 50);
         Assert.AreEqual(0, result);
      }

      [TestMethod]
      public void TestNeedStock_WithNoStock()
      {
         var result = OrderableItemsUnitOfWork.CalculateReorderAmount(currentStock: 0, targetLevel: 10, minOrder: 0, multiplesOf: 0);
         Assert.AreEqual(10, result);
      }

      [TestMethod]
      public void TestNeedStock_WithMinReached()
      {
         var result = OrderableItemsUnitOfWork.CalculateReorderAmount(currentStock: 5, targetLevel: 20, minOrder: 10, multiplesOf: 0);
         Assert.AreEqual(15, result);
      }

      [TestMethod]
      public void TestNeedStock_WithMinReached_Exactly()
      {
         var result = OrderableItemsUnitOfWork.CalculateReorderAmount(currentStock: 10, targetLevel: 20, minOrder: 10, multiplesOf: 0);
         Assert.AreEqual(10, result);
      }

      [TestMethod]
      public void TestNeedStock_WithMinNotReached()
      {
         var result = OrderableItemsUnitOfWork.CalculateReorderAmount(currentStock: 15, targetLevel: 20, minOrder: 10, multiplesOf: 0);
         Assert.AreEqual(10, result);
      }

      [TestMethod]
      public void TestNeedStock_WithMultiplesNotReached()
      {
         var result = OrderableItemsUnitOfWork.CalculateReorderAmount(currentStock: 15, targetLevel: 20, minOrder: 0, multiplesOf: 10);
         Assert.AreEqual(10, result);
      }

      [TestMethod]
      public void TestNeedStock_WithMultiplesAlreadyReached()
      {
         var result = OrderableItemsUnitOfWork.CalculateReorderAmount(currentStock: 10, targetLevel: 20, minOrder: 0, multiplesOf: 10);
         Assert.AreEqual(10, result);
      }

      [TestMethod]
      public void TestNeedStock_WithMultiplesWithMinLessThanMultiple()
      {
         var result = OrderableItemsUnitOfWork.CalculateReorderAmount(currentStock: 10, targetLevel: 20, minOrder: 5, multiplesOf: 10);
         Assert.AreEqual(10, result);
      }

      [TestMethod]
      public void TestNeedStock_WithMultiplesWithMinMoreThanMultiple()
      {
         var result = OrderableItemsUnitOfWork.CalculateReorderAmount(currentStock: 10, targetLevel: 20, minOrder: 50, multiplesOf: 10);
         Assert.AreEqual(50, result);
      }
   }
}
