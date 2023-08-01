using System;
using System.Collections.Generic;
using System.Linq;
using HMS.HealthTrack.Inventory.Test.Inventory.Mocks;
using HMS.HealthTrack.Web.Data.Helpers;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HMS.HealthTrack.Inventory.Test.Inventory
{
   [TestClass]
   public class InventoryStockCheckTests
   {
      [TestMethod]
      public void ItemIsNotInStock()
      {
         //Arrange
         var data = new List<Stock>().AsQueryable();
         var testData = new MockedInventoryStockData(new List<Product>(), data);

         //SUT
         var stockRepo = MockedInventoryRepositoryFactory.GetStockRepository(testData);

         Assert.IsFalse(stockRepo.ItemInStock(123, string.Empty));
      }

      //[TestMethod]
      public void GenericItemIsInStock()
      {
         //Arrange
         var data = new List<Stock>
         {
            new Stock {ProductId = 000, StockStatus = StockStatus.Available},
            new Stock {ProductId = 123, StockStatus = StockStatus.Available},
            new Stock {ProductId = 222, StockStatus = StockStatus.Available},
            new Stock {ProductId = 123},
            new Stock {ProductId = 123, StockStatus = StockStatus.Deducted},
         };

         var testData = new MockedInventoryStockData(new List<Product>(), data);

         //SUT
         var stockRepo = MockedInventoryRepositoryFactory.GetStockRepository(testData);

         Assert.AreEqual(5, stockRepo.FindAll().Count());
         Assert.AreEqual(1, stockRepo.GetProductStockBatches(123).Count());
         Assert.IsTrue(stockRepo.ItemInStock(123, string.Empty));
      }

      //[TestMethod]
      public void ItemUsedToBeInStock()
      {
         //Arrange
         var data = new List<Stock>
         {
            new Stock {ProductId = 000, StockStatus = StockStatus.Available},
            new Stock {ProductId = 123, StockStatus = StockStatus.Removed},
            new Stock {ProductId = 222, StockStatus = StockStatus.Deducted},
            new Stock {ProductId = 123},
            new Stock {ProductId = 123, StockStatus = StockStatus.Deducted},
         };

         var testData = new MockedInventoryStockData(new List<Product>(), data);

         //SUT
         var stockRepo = MockedInventoryRepositoryFactory.GetStockRepository(testData);

         Assert.AreEqual(5, stockRepo.FindAll().Count());
         Assert.AreEqual(0, stockRepo.GetProductStockBatches(123).Count());
         Assert.IsFalse(stockRepo.ItemInStock(123, string.Empty));
      }

      [TestMethod]
      public void CheckSerialNumberIsInStock()
      {
         //Arrange
         var serialNumber = Guid.NewGuid().ToString();
         var otherSerialNumber = Guid.NewGuid().ToString();

         var data = new List<Stock>
         {
            new Stock {ProductId = 000, StockStatus = StockStatus.Available},
            new Stock {ProductId = 123, StockStatus = StockStatus.Available, SerialNumber = serialNumber},
            new Stock {ProductId = 222, StockStatus = StockStatus.Deducted},
            new Stock {ProductId = 123, StockStatus = StockStatus.Available},
            new Stock {ProductId = 123, StockStatus = StockStatus.Deducted, SerialNumber = otherSerialNumber},
         };

         var testData = new MockedInventoryStockData(new List<Product>(), data);

         //SUT
         var stockRepo = MockedInventoryRepositoryFactory.GetStockRepository(testData);

         Assert.AreEqual(5, stockRepo.FindAll().Count());
         Assert.AreEqual(2, stockRepo.GetProductStockBatches(123).Count());
         Assert.IsTrue(stockRepo.ItemInStock(123, serialNumber));
      }

      [TestMethod]
      public void CheckSerialNumberNotIsInStock()
      {
         //Arrange
         var serialNumber = Guid.NewGuid().ToString();
         var otherSerialNumber = Guid.NewGuid().ToString();

         var data = new List<Stock>
         {
            new Stock {ProductId = 000, StockStatus = StockStatus.Available},
            new Stock {ProductId = 123, StockStatus = StockStatus.Deducted, SerialNumber = serialNumber},
            new Stock {ProductId = 222, StockStatus = StockStatus.Available},
            new Stock {ProductId = 123, StockStatus = StockStatus.Available},
            new Stock {ProductId = 123, StockStatus = StockStatus.Available, SerialNumber = otherSerialNumber},
         };

         var testData = new MockedInventoryStockData(new List<Product>(), data);

         //SUT
         var stockRepo = MockedInventoryRepositoryFactory.GetStockRepository(testData);

         Assert.AreEqual(5, stockRepo.FindAll().Count());
         Assert.AreEqual(2, stockRepo.GetProductStockBatches(123).Count());
         Assert.IsFalse(stockRepo.ItemInStock(123, serialNumber));
      }

      [TestMethod]
      public void DeductStockItem()
      {
         //Arrange
         var serialNumber = Guid.NewGuid().ToString();
         var otherSerialNumber = Guid.NewGuid().ToString();
         var itemDeduction = new ItemAdjustment { ProductId = 123, SerialNumber = serialNumber, Quantity = 1, StockLocationId = 1 };
         var product = new Product
         {
            ProductId = 123
         };

         var data = new List<Stock>
         {
            new Stock {ProductId = 123, StockStatus = StockStatus.Available, SerialNumber = serialNumber, Quantity = 1, StoredAt = 1},
            new Stock {ProductId = 123, StockStatus = StockStatus.Available, Quantity = 1, StoredAt = 1},
            new Stock {ProductId = 123, StockStatus = StockStatus.Available, SerialNumber = otherSerialNumber, Quantity = 1, StoredAt = 1},
         };

         var testData = new MockedInventoryStockData(new List<Product> { product }, data);

         //SUT
         var stockRepo = MockedInventoryRepositoryFactory.GetStockRepository(testData);
         var stockAdjustmentRepository = MockedInventoryRepositoryFactory.GetStockAdjustmentRepository(testData);
         var deductionHelper = new StockAdjustmentHelper(stockRepo, stockAdjustmentRepository);

         deductionHelper.AdjustItem(itemDeduction, product, "Unit test");

         Assert.AreEqual(2, stockRepo.GetProductStockBatches(123).Count());
         Assert.IsFalse(stockRepo.ItemInStock(123, serialNumber), "Item should no longer be in stock");
         Assert.IsTrue(stockRepo.ItemInStock(123, otherSerialNumber), "Item should still be in stock");
      }
   }
}