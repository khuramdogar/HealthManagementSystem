using System;
using System.Collections.Generic;
using System.Linq;
using HMS.HealthTrack.Inventory.Test.Inventory.Mocks;
using HMS.HealthTrack.Web.Data.Helpers;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace HMS.HealthTrack.Inventory.Test.Inventory
{
   [TestClass]
   public class StockSplittingForDeduction
   {
      [TestMethod]
      public void Consume_HasSerialNumber_Stock_NoSerial_SplitsOldestStock()
      {
         var locationId = 1;

         // stock age calculated by CreatedOn field
         var olderStock = new Stock
         {
            CreatedOn = DateTime.Now.AddDays(-3),
            ProductId = 123,
            Quantity = 2,
            ReceivedQuantity = 2,
            StockId = 555,
            StockStatus = StockStatus.Available,
            StoredAt = locationId,
         };

         var newerStock = new Stock
         {
            CreatedOn = DateTime.Now.AddDays(-1),
            ProductId = 123,
            Quantity = 5,
            ReceivedQuantity = 5,
            StockId = 666,
            StockStatus = StockStatus.Available,
            StoredAt = locationId,
         };

         var stocks = new List<Stock>
         {
            olderStock,
            newerStock
         };

         var product = new Product { ProductId = 123 };
         var testData = new MockedInventoryStockData(new List<Product> { product }, stocks);
         var stockRepo = MockedInventoryRepositoryFactory.GetStockRepository(testData);
         var stockAdjustmentRepository = MockedInventoryRepositoryFactory.GetStockAdjustmentRepository(testData);
         var deductionHelper = new StockAdjustmentHelper(stockRepo, stockAdjustmentRepository);

         //Test
         deductionHelper.AdjustItem(new ItemAdjustment { ProductId = 123, Quantity = 1, SerialNumber = "SERIAL", StockLocationId = locationId }, product, "UnitTest");

         //Verify
         testData.MockedAdjustments.Verify(
            i => i.Add(It.Is<StockAdjustment>(sa => sa.Quantity == 1 && !sa.IsPositive)), Times.Once(),
            "Stock was deduction");
         testData.MockedAdjustments.Verify(i => i.Add(It.Is<StockAdjustment>(sa => sa.StockAdjustmentStocks.First().Stock.Quantity == 0 && sa.StockAdjustmentStocks.First().Stock.ReceivedQuantity == 1 && !sa.StockAdjustmentStocks.First().Stock.IsNegative && sa.StockAdjustmentStocks.First().Stock.SerialNumber == "SERIAL" && sa.StockAdjustmentStocks.First().Stock.StoredAt == locationId && sa.StockAdjustmentStocks.First().Stock.StockStatus == StockStatus.Deducted && sa.StockAdjustmentStocks.First().Stock.StockId == 0)), Times.Once(), "Verify split stock values");

         //Old stock should be unaffected
         Assert.AreEqual(StockStatus.Available, olderStock.StockStatus);
         Assert.IsNull(olderStock.SerialNumber);
         Assert.AreEqual(1, olderStock.Quantity);

         // Newer stock should be untouched
         Assert.AreEqual(StockStatus.Available, newerStock.StockStatus);
         Assert.IsNull(newerStock.SerialNumber);
         Assert.AreEqual(5, newerStock.Quantity);
      }


      /// <summary>
      /// Check that when there is a stock item that has been deducted down to a quantity of 1 and is then split has the initial stock
      /// entry updated to be deducted with a quantity of 0.
      /// </summary>
      [TestMethod]
      public void Consume_HasSerialNumber_Stock_NoSerial_SplitsLastRemainingQuantityOfStock_OldestStock()
      {
         var locationId = 1;
         var historicStockAdjustment = new StockAdjustment { Quantity = 1 };
         // stock age calculated by CreatedOn field
         var olderStock = new Stock
         {
            CreatedOn = DateTime.Now.AddDays(-3),
            ProductId = 123,
            Quantity = 1,
            ReceivedQuantity = 2,
            StockStatus = StockStatus.Available,
            StoredAt = locationId,
         };

         olderStock.StockAdjustmentStocks.Add(new StockAdjustmentStock
         {
            StockAdjustment = historicStockAdjustment
         });

         var newerStock = new Stock
         {
            CreatedOn = DateTime.Now.AddDays(-1),
            ProductId = 123,
            Quantity = 5,
            ReceivedQuantity = 5,
            StockStatus = StockStatus.Available,
            StoredAt = locationId,
         };

         var stocks = new List<Stock>
         {
            olderStock,
            newerStock
         };

         var product = new Product { ProductId = 123 };
         var testData = new MockedInventoryStockData(new List<Product> { product }, stocks);

         //SUT
         var stockRepo = MockedInventoryRepositoryFactory.GetStockRepository(testData);
         var stockAdjustmentRepository = MockedInventoryRepositoryFactory.GetStockAdjustmentRepository(testData);
         var deductionHelper = new StockAdjustmentHelper(stockRepo, stockAdjustmentRepository);

         //Test
         deductionHelper.AdjustItem(new ItemAdjustment { ProductId = 123, Quantity = 1, SerialNumber = "SERIAL", StockLocationId = 1 }, product, "UnitTest");

         //Verify
         testData.MockedAdjustments.Verify(i => i.Add(It.Is<StockAdjustment>(sa => sa.Quantity == 1 && !sa.IsPositive)), Times.Once(), "New consumption of 1 item with serial");

         //Verify
         testData.MockedAdjustments.Verify(
            i => i.Add(It.Is<StockAdjustment>(sa => sa.Quantity == 1 && !sa.IsPositive)), Times.Once(),
            "Stock was deduction");
         testData.MockedAdjustments.Verify(i => i.Add(It.Is<StockAdjustment>(sa => sa.StockAdjustmentStocks.First().Stock.Quantity == 0 && sa.StockAdjustmentStocks.First().Stock.ReceivedQuantity == 1 && !sa.StockAdjustmentStocks.First().Stock.IsNegative && sa.StockAdjustmentStocks.First().Stock.SerialNumber == "SERIAL" && sa.StockAdjustmentStocks.First().Stock.StoredAt == locationId && sa.StockAdjustmentStocks.First().Stock.StockStatus == StockStatus.Deducted && sa.StockAdjustmentStocks.First().Stock.StockId == 0)), Times.Once(), "Verify split stock values");

         //Old stock should be affected
         Assert.AreEqual(StockStatus.Deducted, olderStock.StockStatus);
         Assert.IsNull(olderStock.SerialNumber);
         Assert.AreEqual(0, olderStock.Quantity);
         Assert.AreEqual(1, olderStock.ReceivedQuantity);

         // Newer stock should be untouched
         Assert.AreEqual(StockStatus.Available, newerStock.StockStatus);
         Assert.IsNull(newerStock.SerialNumber);
         Assert.AreEqual(5, newerStock.Quantity);
      }
   }
}