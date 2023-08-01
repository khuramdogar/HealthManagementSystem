using System;
using System.Collections.Generic;
using System.Linq;
using HMS.HealthTrack.Inventory.Test.Inventory.Mocks;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace HMS.HealthTrack.Inventory.Test.Inventory.StockTakes
{
   [TestClass]
   public class StockTakeProcessingTests
   {
      private const string Username = "UnitTest";

      private Product Product
      {
         get
         {
            return new Product
            {
               ProductId = 123,
               AutoReorderSetting = ReorderSettings.DoNotReorder,
               ManageStock = false,
               TargetStockLevel = 100,
               ReorderThreshold = 50
            };
         }
      }

      private Stock Stock
      {
         get
         {
            return new Stock
            {
               Product = Product,
               ProductId = 123,
               Quantity = 7,
               ReceivedQuantity = 11,
               StockId = 1,
               StockStatus = StockStatus.Available,
               StoredAt = 1
            };
         }
      }

      private StockAdjustment StockDeduction
      {
         get
         {
            return new StockAdjustment
            {
               AdjustedOn = DateTime.Now.AddDays(-14),
               IsPositive = false,
               Quantity = 4,
               StockAdjustmentId = 2,
            };
         }
      }


      private StockAdjustment StockReceipt
      {
         get
         {
            return new StockAdjustment
            {
               AdjustedOn = DateTime.Now.AddDays(-100),
               IsPositive = true,
               Quantity = 11,
               StockAdjustmentId = 1,
            };
         }
      }

      private List<StockAdjustmentStock> StockAdjustmentStocks
      {
         get
         {
            return new List<StockAdjustmentStock>
            {
               new StockAdjustmentStock
               {
                  StockAdjustmentId = 1,
                  StockId = 1
               },
               new StockAdjustmentStock
               {
                  StockAdjustmentId = 2,
                  StockId = 1
               }
            };
         }
      }

      private StockTake StockTake
      {
         get
         {
            return new StockTake
            {
               LocationId = 1,
               StockTakeDate = DateTime.Now,
               StockTakeItems = new List<StockTakeItem>
               {
                  new StockTakeItem
                  {
                     ProductId = Product.ProductId,
                     StockLevel = 7
                  }
               }
            };
         }
      }

      [TestMethod]
      public void TestStockTakeProcessing_WriteOff()
      {
         var testData = new MockedInventoryStockTakeData(new List<Product> { Product }, new List<Stock> { Stock },
            new List<StockAdjustment> { StockDeduction, StockReceipt }, StockAdjustmentStocks);

         var stockTake = StockTake;
         var testStockTakeItem = stockTake.StockTakeItems.First();
         testStockTakeItem.StockLevel = 4;

         //SUT
         var sut = new StockTakeUnitOfWork(testData.MockedContext.Object, testData.MockedLogger.Object, testData.MockedPropertyProvider.Object);

         sut.ProcessStockTake(stockTake);

         testData.MockedStock.Verify(s => s.Add(It.IsAny<Stock>()), Times.Never, "New Stock");
         testData.MockedAdjustments.Verify(s => s.Add(It.IsAny<StockAdjustment>()), Times.Once(), "Item deduction");
         testData.MockedAdjustments.Verify(
            s =>
               s.Add(
                  It.Is<StockAdjustment>(
                     sa => !sa.IsPositive && sa.Quantity == 3 && sa.Source == AdjustmentSource.StockTake)), Times.Once(),
            "Item deduction");
         Assert.AreEqual(StockTakeStatus.Complete, stockTake.Status);
      }

      [TestMethod]
      public void TestStockTakeProcessing_WriteOff_UpdateProductManageStock()
      {
         var product = Product;
         var testData = new MockedInventoryStockTakeData(new List<Product> { product }, new List<Stock> { Stock },
            new List<StockAdjustment> { StockDeduction, StockReceipt }, StockAdjustmentStocks);

         var stockTake = StockTake;
         var testStockTakeItem = stockTake.StockTakeItems.First();
         testStockTakeItem.StockLevel = 4;

         //SUT
         var sut = new StockTakeUnitOfWork(testData.MockedContext.Object, testData.MockedLogger.Object, testData.MockedPropertyProvider.Object);

         sut.ProcessStockTake(stockTake);

         Assert.IsTrue(product.ManageStock);
         Assert.AreEqual(ReorderSettings.SpecifyLevels, product.AutoReorderSetting);
         Assert.AreEqual(testStockTakeItem.StockLevel, product.TargetStockLevel);
         Assert.AreEqual(testStockTakeItem.StockLevel - 1, product.ReorderThreshold);
      }

      [TestMethod]
      public void TestStockTakeProcessing_WriteOff_AllDeleted()
      {
         var testData = new MockedInventoryStockTakeData(new List<Product> { Product }, new List<Stock> { Stock },
            new List<StockAdjustment> { StockDeduction, StockReceipt }, StockAdjustmentStocks);

         var stockTake = StockTake;
         var testStockTakeItem = stockTake.StockTakeItems.First();
         testStockTakeItem.StockLevel = 4;
         testStockTakeItem.DeletedBy = Username;
         testStockTakeItem.DeletedOn = DateTime.Now;

         //SUT
         var sut = new StockTakeUnitOfWork(testData.MockedContext.Object, testData.MockedLogger.Object, testData.MockedPropertyProvider.Object);

         sut.ProcessStockTake(stockTake);

         testData.MockedStock.Verify(s => s.Add(It.IsAny<Stock>()), Times.Never, "New Stock");
         testData.MockedAdjustments.Verify(s => s.Add(It.IsAny<StockAdjustment>()), Times.Never(), "Item deduction");
         Assert.AreEqual(StockTakeStatus.Complete, stockTake.Status);
      }

      [TestMethod]
      public void TestStockTakeProcessing_WriteOff_Deleted()
      {
         var testData = new MockedInventoryStockTakeData(new List<Product> { Product }, new List<Stock> { Stock },
            new List<StockAdjustment> { StockDeduction, StockReceipt }, StockAdjustmentStocks);

         var stockTake = StockTake;
         var testStockTakeItem = stockTake.StockTakeItems.First();
         testStockTakeItem.StockLevel = 4;

         var testDeletedItem = StockTake.StockTakeItems.First();
         testDeletedItem.DeletedBy = Username;
         testDeletedItem.DeletedOn = DateTime.Now;
         testDeletedItem.StockTakeItemId = 5;
         stockTake.StockTakeItems.Add(testDeletedItem);

         //SUT
         var sut = new StockTakeUnitOfWork(testData.MockedContext.Object, testData.MockedLogger.Object, testData.MockedPropertyProvider.Object);

         sut.ProcessStockTake(stockTake);

         testData.MockedStock.Verify(s => s.Add(It.IsAny<Stock>()), Times.Never, "New Stock");
         testData.MockedAdjustments.Verify(s => s.Add(It.IsAny<StockAdjustment>()), Times.Once(), "Item deduction");
         testData.MockedAdjustments.Verify(s => s.Add(It.Is<StockAdjustment>(sa => !sa.IsPositive && sa.Quantity == 3)), Times.Once(), "Item deduction");
         Assert.AreEqual(StockTakeStatus.Complete, stockTake.Status);
      }

      [TestMethod]
      public void TestStockTakeProcessing_Surplus()
      {
         var testData = new MockedInventoryStockTakeData(new List<Product> { Product }, new List<Stock> { Stock },
            new List<StockAdjustment> { StockDeduction, StockReceipt }, StockAdjustmentStocks);

         var stockTake = StockTake;
         var testStockTakeItem = stockTake.StockTakeItems.First();
         testStockTakeItem.StockLevel = 9;

         //SUT
         var sut = new StockTakeUnitOfWork(testData.MockedContext.Object, testData.MockedLogger.Object, testData.MockedPropertyProvider.Object);

         sut.ProcessStockTake(stockTake);

         Assert.AreEqual(1, testStockTakeItem.StockAdjustments.Count, "Stock adjustment added");
         Assert.AreEqual(2, testStockTakeItem.StockAdjustments.First().Quantity);
         Assert.IsTrue(testStockTakeItem.StockAdjustments.First().IsPositive);
         Assert.AreEqual(AdjustmentSource.StockTake, testStockTakeItem.StockAdjustments.First().Source);

         Assert.AreEqual(1, testStockTakeItem.StockAdjustments.First().StockAdjustmentStocks.Count);

         var surplusStock = testStockTakeItem.StockAdjustments.First().StockAdjustmentStocks.First().Stock;
         Assert.AreEqual(2, surplusStock.Quantity);
         Assert.AreEqual(2, surplusStock.ReceivedQuantity);
         Assert.AreEqual(StockStatus.Available, surplusStock.StockStatus);
         Assert.AreEqual(1, surplusStock.StoredAt);
         Assert.AreEqual(testStockTakeItem.ProductId, surplusStock.ProductId);
      }

      [TestMethod]
      public void TestStockTakeProcessing_Surplus_UpdateProductManageStock()
      {
         var product = Product;

         var testData = new MockedInventoryStockTakeData(new List<Product> { product }, new List<Stock> { Stock },
    new List<StockAdjustment> { StockDeduction, StockReceipt }, StockAdjustmentStocks);

         var stockTake = StockTake;
         var testStockTakeItem = stockTake.StockTakeItems.First();
         testStockTakeItem.StockLevel = 9;

         //SUT
         var sut = new StockTakeUnitOfWork(testData.MockedContext.Object, testData.MockedLogger.Object, testData.MockedPropertyProvider.Object);

         sut.ProcessStockTake(stockTake);

         Assert.IsTrue(product.ManageStock);
         Assert.AreEqual(ReorderSettings.SpecifyLevels, product.AutoReorderSetting);
         Assert.AreEqual(testStockTakeItem.StockLevel, product.TargetStockLevel);
         Assert.AreEqual(testStockTakeItem.StockLevel - 1, product.ReorderThreshold);
      }

      [TestMethod]
      public void TestStockTakeProcessing_NoChange()
      {
         var testData = new MockedInventoryStockTakeData(new List<Product> { Product }, new List<Stock> { Stock },
      new List<StockAdjustment> { StockDeduction, StockReceipt }, StockAdjustmentStocks);

         var stockTake = StockTake;

         //SUT
         var sut = new StockTakeUnitOfWork(testData.MockedContext.Object, testData.MockedLogger.Object, testData.MockedPropertyProvider.Object);

         sut.ProcessStockTake(stockTake);

         testData.MockedStock.Verify(s => s.Add(It.IsAny<Stock>()), Times.Never, "New Stock");
         testData.MockedAdjustments.Verify(s => s.Add(It.IsAny<StockAdjustment>()), Times.Never, "Item deduction");
      }

      [TestMethod]
      public void TestStockTakeProcessing_NoChange_UpdateProductManageStock()
      {
         var product = Product;

         var testData = new MockedInventoryStockTakeData(new List<Product> { product }, new List<Stock> { Stock },
            new List<StockAdjustment> { StockDeduction, StockReceipt }, StockAdjustmentStocks);

         var stockTake = StockTake;
         var testStockTakeItem = stockTake.StockTakeItems.First();

         //SUT
         var sut = new StockTakeUnitOfWork(testData.MockedContext.Object, testData.MockedLogger.Object,
            testData.MockedPropertyProvider.Object);

         sut.ProcessStockTake(stockTake);

         Assert.IsTrue(product.ManageStock);
         Assert.AreEqual(ReorderSettings.SpecifyLevels, product.AutoReorderSetting);
         Assert.AreEqual(testStockTakeItem.StockLevel, product.TargetStockLevel);
         Assert.AreEqual(testStockTakeItem.StockLevel - 1, product.ReorderThreshold);
      }

      [TestMethod]
      public void TestStockTakeProcessing_NoChange_ProductWithManageStockUntouched()
      {
         var product = Product;
         product.AutoReorderSetting = ReorderSettings.SpecifyLevels;
         product.ManageStock = true;

         var testData = new MockedInventoryStockTakeData(new List<Product> { product }, new List<Stock> { Stock },
            new List<StockAdjustment> { StockDeduction, StockReceipt }, StockAdjustmentStocks);

         var stockTake = StockTake;

         //SUT
         var sut = new StockTakeUnitOfWork(testData.MockedContext.Object, testData.MockedLogger.Object,
            testData.MockedPropertyProvider.Object);

         sut.ProcessStockTake(stockTake);

         Assert.IsTrue(product.ManageStock);
         Assert.AreEqual(ReorderSettings.SpecifyLevels, product.AutoReorderSetting);
         Assert.AreEqual(Product.TargetStockLevel, product.TargetStockLevel);
         Assert.AreEqual(Product.ReorderThreshold, product.ReorderThreshold);
      }
   }
}
