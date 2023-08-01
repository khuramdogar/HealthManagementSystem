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
   public class StockAdjustmentTests
   {
      private Product Product
      {
         get { return new Product { ProductId = 123 }; }
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
      public void TestProcessStockTakeItem_MissingStock()
      {
         var testData = new MockedInventoryStockTakeData(new List<Product> { Product }, new List<Stock> { Stock },
            new List<StockAdjustment> { StockDeduction, StockReceipt }, StockAdjustmentStocks);

         var stockTake = StockTake;
         var testStockTakeItem = stockTake.StockTakeItems.First();
         testStockTakeItem.StockLevel = 4;

         //SUT
         var sut = new StockTakeUnitOfWork(testData.MockedContext.Object, testData.MockedLogger.Object, testData.MockedPropertyProvider.Object);

         sut.ProcessStockTake(stockTake);

         Assert.AreEqual(-3, testStockTakeItem.Adjustment, "Stock adjustment");
         Assert.AreEqual(7, testStockTakeItem.PreviousStockLevel, "Previous stock level");
         Assert.AreEqual(4, testStockTakeItem.NewStockLevel, "New stock level");
      }

      [TestMethod]
      public void TestProcessStockTakeItem_SurplusStock()
      {
         var testData = new MockedInventoryStockTakeData(new List<Product> { Product }, new List<Stock> { Stock },
            new List<StockAdjustment> { StockDeduction, StockReceipt }, StockAdjustmentStocks);

         var stockTake = StockTake;
         var testStockTakeItem = stockTake.StockTakeItems.First();
         testStockTakeItem.StockLevel = 9;

         //SUT
         var sut = new StockTakeUnitOfWork(testData.MockedContext.Object, testData.MockedLogger.Object, testData.MockedPropertyProvider.Object);

         sut.ProcessStockTake(stockTake);

         Assert.AreEqual(2, testStockTakeItem.Adjustment, "Stock adjustment");
         Assert.AreEqual(7, testStockTakeItem.PreviousStockLevel, "Previous stock level");
         Assert.AreEqual(9, testStockTakeItem.NewStockLevel, "New stock level");
      }

      [TestMethod]
      public void TestProcessStockTakeItem_MatchingStock()
      {
         var testData = new MockedInventoryStockTakeData(new List<Product> { Product }, new List<Stock> { Stock },
            new List<StockAdjustment> { StockDeduction, StockReceipt }, StockAdjustmentStocks);

         var stockTake = StockTake;
         var testStockTakeItem = stockTake.StockTakeItems.First();

         //SUT
         var sut = new StockTakeUnitOfWork(testData.MockedContext.Object, testData.MockedLogger.Object, testData.MockedPropertyProvider.Object);

         sut.ProcessStockTake(stockTake);

         Assert.AreEqual(0, testStockTakeItem.Adjustment, "Stock adjustment");
         Assert.AreEqual(7, testStockTakeItem.PreviousStockLevel, "Previous stock level");
         Assert.AreEqual(7, testStockTakeItem.NewStockLevel, "New stock level");
      }

      [TestMethod]
      public void TestProcessStockTakeItem_NoStock()
      {
         var testData = new MockedInventoryStockTakeData(new List<Product> { Product }, new List<Stock> { },
            new List<StockAdjustment> { }, new List<StockAdjustmentStock>());

         var stockTake = StockTake;
         var testStockTakeItem = stockTake.StockTakeItems.First();

         //SUT
         var sut = new StockTakeUnitOfWork(testData.MockedContext.Object, testData.MockedLogger.Object, testData.MockedPropertyProvider.Object);

         sut.ProcessStockTake(stockTake);

         Assert.AreEqual(7, testStockTakeItem.Adjustment, "Stock adjustment");
         Assert.AreEqual(0, testStockTakeItem.PreviousStockLevel, "Previous stock level");
         Assert.AreEqual(7, testStockTakeItem.NewStockLevel, "New stock level");
      }

      [TestMethod]
      public void TestProcessStockTakeItem_NoProduct()
      {
         var product = Product;
         product.DeletedOn = DateTime.Now.AddDays(-1);
         //Test Stock data
         var testData = new MockedInventoryStockTakeData(new List<Product> { product }, new List<Stock> { Stock },
            new List<StockAdjustment> { StockDeduction, StockReceipt }, StockAdjustmentStocks);

         var stockTake = StockTake;
         var testStockTakeItem = stockTake.StockTakeItems.First();

         //SUT
         var sut = new StockTakeUnitOfWork(testData.MockedContext.Object, testData.MockedLogger.Object, testData.MockedPropertyProvider.Object);

         sut.ProcessStockTake(stockTake);

         Assert.AreEqual(null, testStockTakeItem.Adjustment, "Stock adjustment");
         Assert.AreEqual(null, testStockTakeItem.PreviousStockLevel, "Previous stock level");
         Assert.AreEqual(null, testStockTakeItem.NewStockLevel, "New stock level");
         Assert.AreEqual(StockTakeItemStatus.Error, testStockTakeItem.Status, "Stock take item status");
      }

      [TestMethod]
      public void DeductionBewtweenCreationAndSubmission_MatchingStock()
      {
         // stock received 100 days ago
         // first deduction 14 days ago
         // stock take 7 days ago
         // second deduction 6 days ago

         var stock = Stock;
         stock.Quantity = 6; // was 7 before one was deducted

         var stockTake = StockTake;
         stockTake.StockTakeDate = DateTime.Now.AddDays(-7);
         var testStockTakeItem = stockTake.StockTakeItems.First();


         var deduction = StockDeduction;
         deduction.AdjustedOn = stockTake.StockTakeDate.AddDays(1);
         deduction.StockAdjustmentId = 5;
         deduction.Quantity = 1;

         var adjustmentStocks = StockAdjustmentStocks;
         adjustmentStocks.Add(new StockAdjustmentStock
         {
            StockAdjustmentId = deduction.StockAdjustmentId,
            StockId = stock.StockId
         });

         var testData = new MockedInventoryStockTakeData(new List<Product> { Product }, new List<Stock> { stock },
            new List<StockAdjustment> { StockDeduction, StockReceipt, deduction }, adjustmentStocks);

         var uow = new StockTakeUnitOfWork(testData.MockedContext.Object, testData.MockedLogger.Object, testData.MockedPropertyProvider.Object);
         uow.ProcessStockTake(stockTake);

         Assert.AreEqual(0, testStockTakeItem.Adjustment);
         Assert.AreEqual(7, testStockTakeItem.PreviousStockLevel, "Previous stock level");
         Assert.AreEqual(7, testStockTakeItem.NewStockLevel, "New stock level");
         Assert.AreEqual(6, stock.Quantity, "Current stock level");
         Assert.AreEqual(0, testStockTakeItem.StockAdjustments.Count, "New stock added");
      }

      [TestMethod]
      public void DeductionBewtweenCreationAndSubmission_WriteOffStock()
      {
         // stock received 100 days ago
         // first deduction 14 days ago
         // stock take 7 days ago
         // second deduction 6 days ago

         var stock = Stock;
         stock.Quantity = 6; // was 7 before one was deducted

         var stockTake = StockTake;
         stockTake.StockTakeDate = DateTime.Now.AddDays(-7);
         var testStockTakeItem = stockTake.StockTakeItems.First();
         testStockTakeItem.StockLevel = 5;


         var deduction = StockDeduction;
         deduction.AdjustedOn = stockTake.StockTakeDate.AddDays(1);
         deduction.StockAdjustmentId = 5;
         deduction.Quantity = 1;

         var adjustmentStocks = StockAdjustmentStocks;
         adjustmentStocks.Add(new StockAdjustmentStock
         {
            StockAdjustmentId = deduction.StockAdjustmentId,
            StockId = stock.StockId
         });

         var testData = new MockedInventoryStockTakeData(new List<Product> { Product }, new List<Stock> { stock },
            new List<StockAdjustment> { StockDeduction, StockReceipt, deduction }, adjustmentStocks);
         var uow = new StockTakeUnitOfWork(testData.MockedContext.Object, testData.MockedLogger.Object, testData.MockedPropertyProvider.Object);
         uow.ProcessStockTake(stockTake);

         Assert.AreEqual(-2, testStockTakeItem.Adjustment);
         Assert.AreEqual(7, testStockTakeItem.PreviousStockLevel, "Previous stock level");
         Assert.AreEqual(5, testStockTakeItem.NewStockLevel, "New stock level");
         Assert.AreEqual(4, stock.Quantity, "Current stock level");
         Assert.AreEqual(0, testStockTakeItem.StockAdjustments.Count, "New stock added");
         testData.MockedAdjustments.Verify(
            deductions => deductions.Add(It.Is<StockAdjustment>(d => d.Quantity == 2)));
      }

      [TestMethod]
      public void ConsumptionBewtweenCreationAndSubmission_SurplusStock()
      {
         // stock received 100 days ago
         // first deduction 14 days ago
         // stock take 7 days ago
         // second deduction 6 days ago

         var stock = Stock;
         stock.Quantity = 6; // was 7 before one was deducted

         var stockTake = StockTake;
         stockTake.StockTakeDate = DateTime.Now.AddDays(-7);
         var testStockTakeItem = stockTake.StockTakeItems.First();
         testStockTakeItem.StockLevel = 10;


         var deduction = StockDeduction;
         deduction.AdjustedOn = stockTake.StockTakeDate.AddDays(1);
         deduction.StockAdjustmentId = 5;
         deduction.Quantity = 1;

         var adjustmentStocks = StockAdjustmentStocks;
         adjustmentStocks.Add(new StockAdjustmentStock
         {
            StockAdjustmentId = deduction.StockAdjustmentId,
            StockId = stock.StockId
         });

         var testData = new MockedInventoryStockTakeData(new List<Product> { Product }, new List<Stock> { stock },
            new List<StockAdjustment> { StockDeduction, StockReceipt, deduction }, adjustmentStocks);
         var uow = new StockTakeUnitOfWork(testData.MockedContext.Object, testData.MockedLogger.Object, testData.MockedPropertyProvider.Object);
         uow.ProcessStockTake(stockTake);

         Assert.AreEqual(3, testStockTakeItem.Adjustment);
         Assert.AreEqual(7, testStockTakeItem.PreviousStockLevel, "Previous stock level");
         Assert.AreEqual(10, testStockTakeItem.NewStockLevel, "New stock level");
         Assert.AreEqual(6, stock.Quantity, "Current stock level");

         Assert.AreEqual(1, testStockTakeItem.StockAdjustments.Count, "Stock adjustment added");
         Assert.AreEqual(3, testStockTakeItem.StockAdjustments.First().Quantity);
         Assert.IsTrue(testStockTakeItem.StockAdjustments.First().IsPositive);
         Assert.AreEqual(AdjustmentSource.StockTake, testStockTakeItem.StockAdjustments.First().Source);

         Assert.AreEqual(1, testStockTakeItem.StockAdjustments.First().StockAdjustmentStocks.Count);

         var surplusStock = testStockTakeItem.StockAdjustments.First().StockAdjustmentStocks.First().Stock;
         Assert.AreEqual(3, surplusStock.Quantity);
         Assert.AreEqual(3, surplusStock.ReceivedQuantity);
         Assert.AreEqual(StockStatus.Available, surplusStock.StockStatus);
         Assert.AreEqual(1, surplusStock.StoredAt);
         Assert.AreEqual(testStockTakeItem.ProductId, surplusStock.ProductId);
      }

      [TestMethod]
      public void ReceiptBetweenCreationAndSubmission_MatchingStockLevel()
      {
         // stock received 100 days ago
         // first deduction 14 days ago
         // stock take 7 days ago
         // second deduction 6 days ago

         var stock = Stock;
         stock.Quantity = 9; // was 7 before two received

         var stockTake = StockTake;
         stockTake.StockTakeDate = DateTime.Now.AddDays(-7);
         var testStockTakeItem = stockTake.StockTakeItems.First();

         var receipt = StockReceipt;
         receipt.AdjustedOn = stockTake.StockTakeDate.AddDays(1);
         receipt.StockAdjustmentId = 5;
         receipt.Quantity = 2;

         var adjustmentStocks = StockAdjustmentStocks;
         adjustmentStocks.Add(new StockAdjustmentStock
         {
            StockAdjustmentId = receipt.StockAdjustmentId,
            StockId = stock.StockId
         });

         var testData = new MockedInventoryStockTakeData(new List<Product> { Product }, new List<Stock> { stock },
            new List<StockAdjustment> { StockDeduction, StockReceipt, receipt }, adjustmentStocks);

         var uow = new StockTakeUnitOfWork(testData.MockedContext.Object, testData.MockedLogger.Object, testData.MockedPropertyProvider.Object);
         uow.ProcessStockTake(stockTake);

         Assert.AreEqual(0, testStockTakeItem.Adjustment);
         Assert.AreEqual(7, testStockTakeItem.PreviousStockLevel, "Previous stock level");
         Assert.AreEqual(7, testStockTakeItem.NewStockLevel, "New stock level");
         Assert.AreEqual(0, testStockTakeItem.StockAdjustments.Count, "New stock added");

      }

      [TestMethod]
      public void ReceiptBetweenCreationAndSubmission_WriteOff()
      {
         // stock received 100 days ago
         // first deduction 14 days ago
         // stock take 7 days ago
         // second deduction 6 days ago

         var stock = Stock;
         stock.Quantity = 9; // was 7 before two received

         var stockTake = StockTake;
         stockTake.StockTakeDate = DateTime.Now.AddDays(-7);
         var testStockTakeItem = stockTake.StockTakeItems.First();
         testStockTakeItem.StockLevel = 5;

         var receipt = StockReceipt;
         receipt.AdjustedOn = stockTake.StockTakeDate.AddDays(1);
         receipt.StockAdjustmentId = 5;
         receipt.Quantity = 2;

         var adjustmentStocks = StockAdjustmentStocks;
         adjustmentStocks.Add(new StockAdjustmentStock
         {
            StockAdjustmentId = receipt.StockAdjustmentId,
            StockId = stock.StockId
         });

         var testData = new MockedInventoryStockTakeData(new List<Product> { Product }, new List<Stock> { stock },
            new List<StockAdjustment> { StockDeduction, StockReceipt, receipt }, adjustmentStocks);

         var uow = new StockTakeUnitOfWork(testData.MockedContext.Object, testData.MockedLogger.Object, testData.MockedPropertyProvider.Object);

         uow.ProcessStockTake(stockTake);

         Assert.AreEqual(-2, testStockTakeItem.Adjustment);
         Assert.AreEqual(7, testStockTakeItem.PreviousStockLevel, "Previous stock level");
         Assert.AreEqual(5, testStockTakeItem.NewStockLevel, "New stock level");
         Assert.AreEqual(7, stock.Quantity, "Current stock level");
         testData.MockedAdjustments.Verify(
            deductions => deductions.Add(It.Is<StockAdjustment>(d => d.Quantity == 2 && !d.IsPositive && d.Source == AdjustmentSource.StockTake)), Times.Once());
      }

      [TestMethod]
      public void ReceiptBetweenCreationAndSubmission_Surplus()
      {
         // stock received 100 days ago
         // first deduction 14 days ago
         // stock take 7 days ago
         // second deduction 6 days ago

         var stock = Stock;
         stock.Quantity = 9; // was 7 before two received

         var stockTake = StockTake;
         stockTake.StockTakeDate = DateTime.Now.AddDays(-7);
         var testStockTakeItem = stockTake.StockTakeItems.First();
         testStockTakeItem.StockLevel = 10;

         var receipt = StockReceipt;
         receipt.AdjustedOn = stockTake.StockTakeDate.AddDays(1);
         receipt.StockAdjustmentId = 5;
         receipt.Quantity = 2;

         var adjustmentStocks = StockAdjustmentStocks;
         adjustmentStocks.Add(new StockAdjustmentStock
         {
            StockAdjustmentId = receipt.StockAdjustmentId,
            StockId = stock.StockId
         });

         var testData = new MockedInventoryStockTakeData(new List<Product> { Product }, new List<Stock> { stock },
            new List<StockAdjustment> { StockDeduction, StockReceipt, receipt }, adjustmentStocks);
         var uow = new StockTakeUnitOfWork(testData.MockedContext.Object, testData.MockedLogger.Object, testData.MockedPropertyProvider.Object);

         uow.ProcessStockTake(stockTake);

         Assert.AreEqual(3, testStockTakeItem.Adjustment);
         Assert.AreEqual(7, testStockTakeItem.PreviousStockLevel, "Previous stock level");
         Assert.AreEqual(10, testStockTakeItem.NewStockLevel, "New stock level");
         Assert.AreEqual(2, receipt.Quantity, "Received stock level");

         Assert.AreEqual(1, testStockTakeItem.StockAdjustments.Count, "Stock adjustment added");
         Assert.AreEqual(3, testStockTakeItem.StockAdjustments.First().Quantity);
         Assert.IsTrue(testStockTakeItem.StockAdjustments.First().IsPositive);
         Assert.AreEqual(AdjustmentSource.StockTake, testStockTakeItem.StockAdjustments.First().Source);

         Assert.AreEqual(1, testStockTakeItem.StockAdjustments.First().StockAdjustmentStocks.Count);

         var surplusStock = testStockTakeItem.StockAdjustments.First().StockAdjustmentStocks.First().Stock;
         Assert.AreEqual(3, surplusStock.Quantity);
         Assert.AreEqual(3, surplusStock.ReceivedQuantity);
         Assert.AreEqual(StockStatus.Available, surplusStock.StockStatus);
         Assert.AreEqual(1, surplusStock.StoredAt);
         Assert.AreEqual(testStockTakeItem.ProductId, surplusStock.ProductId);
      }
   }
}
