using System.Collections.Generic;
using System.Linq;
using HMS.HealthTrack.Inventory.Test.Inventory.Mocks;
using HMS.HealthTrack.Web.Data.Exceptions;
using HMS.HealthTrack.Web.Data.Helpers;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MSTestExtensions;

namespace HMS.HealthTrack.Inventory.Test.Inventory
{
   [TestClass]
   public class StockDeductionTests
   {
      private StockAdjustmentHelper GetStockDeductionHelper(MockedInventoryStockData data)
      {
         var stockRepo = MockedInventoryRepositoryFactory.GetStockRepository(data);
         var stockAdjustmentRepository = MockedInventoryRepositoryFactory.GetStockAdjustmentRepository(data);
         return new StockAdjustmentHelper(stockRepo, stockAdjustmentRepository);
      }

      [TestMethod]
      public void Consume_Single_Generic_Item_With_Zero_Quantity()
      {
         //Arrange stock
         var stockItem = new Stock
         {
            ProductId = 123,
            StockStatus = StockStatus.Available,
            Quantity = 1,
            StoredAt = 1
         };

         var product = new Product
         {
            ProductId = 123
         };

         var testData = new MockedInventoryStockData(new List<Product>(), new List<Stock> { stockItem });
         var deductionHelper = GetStockDeductionHelper(testData);

         //Act
         ExceptionAssert.Throws<StockException>(
            () =>
               deductionHelper.AdjustItem(new ItemAdjustment { ProductId = 123, Quantity = 0, StockLocationId = 1 },
                  product, "Unit test"));

         //Verify
         testData.MockedAdjustments.Verify(c => c.Add(It.IsAny<StockAdjustment>()), Times.Never());

         //Assertions
         Assert.AreEqual(StockStatus.Available, stockItem.StockStatus);
         Assert.AreEqual(1, stockItem.Quantity);
      }

      [TestMethod]
      public void Consume_Single_Generic_Item()
      {
         var product = new Product { ProductId = 123 };

         //Arrange stock
         var stockItem = new Stock
         {
            ProductId = 123,
            StockStatus = StockStatus.Available,
            Quantity = 1,
            StoredAt = 1
         };

         var testData = new MockedInventoryStockData(new List<Product>() { product }, new List<Stock> { stockItem });
         var deductionHelper = GetStockDeductionHelper(testData);

         //Act
         deductionHelper.AdjustItem(new ItemAdjustment { ProductId = 123, Quantity = 1, StockLocationId = 1 }, product, "Unit test");

         //Verify
         testData.MockedAdjustments.Verify(c => c.Add(It.IsAny<StockAdjustment>()), Times.Once());

         //Assertions
         Assert.AreEqual(StockStatus.Deducted, stockItem.StockStatus);
         Assert.AreEqual(0, stockItem.Quantity);
      }

      [TestMethod]
      public void Consume_OutOfStock_Item_CreatesNegativeStock()
      {
         var product = new Product { ProductId = 123 };
         var itemDeduction = new ItemAdjustment
         {
            ProductId = 123,
            Quantity = 3,
            StockLocationId = 1
         };

         //Arrange stock
         var testData = new MockedInventoryStockData(new List<Product>() { product }, new List<Stock>());
         var deductionHelper = GetStockDeductionHelper(testData);

         //Act
         deductionHelper.AdjustItem(itemDeduction, product, "Unit test");

         //Verify consumption and stock have not been added
         Assert.AreEqual(1, product.Stocks.Count(), "Addition of negative stock entity to product");
         Assert.AreEqual(true, product.Stocks.First().IsNegative, "Stock is created as Negative Stock");
         Assert.AreEqual(3, product.Stocks.First().ReceivedQuantity, "Received quantity");
         Assert.AreEqual(0, product.Stocks.First().Quantity, "Available stock");
         Assert.AreEqual(StockStatus.Deducted, product.Stocks.First().StockStatus, "Stock has been deducted");

         //Verify
         testData.MockedAdjustments.Verify(m => m.Add(It.Is<StockAdjustment>(c => c.Quantity == 3)), Times.Once(), "Consumption recorded");
      }

      [TestMethod]
      public void Consume_OutOfStock_NonExistantItem_ThrowsException()
      {
         var testData = new MockedInventoryStockData(new List<Product>(), new List<Stock>());

         //Act
         var deductionHelper = GetStockDeductionHelper(testData);
         ExceptionAssert.Throws<StockException>(
            () =>
               deductionHelper.AdjustItem(new ItemAdjustment { ProductId = 123, Quantity = 1, StockLocationId = 1 }, null,
                  "Unit test"));

         //Verify consumption and stock have not been added
         testData.MockedAdjustments.Verify(c => c.Add(It.IsAny<StockAdjustment>()), Times.Never());
         testData.MockedStock.Verify(s => s.Add(It.IsAny<Stock>()), Times.Never);
         testData.MockedProducts.Verify(p => p.Add(It.IsAny<Product>()), Times.Never);
      }

      [TestMethod]
      public void Consume_More_Stock_Than_Exists_CreatesNegativeStock()
      {
         //Arrange stock
         var stockItem = new Stock { ProductId = 123, Quantity = 2, StoredAt = 1 };
         var product = new Product { ProductId = 123, ManageStock = true };
         var itemDeduction = new ItemAdjustment
         {
            ProductId = 123,
            Quantity = 3,
            StockLocationId = 1
         };

         var testData = new MockedInventoryStockData(new List<Product>() { product }, new List<Stock> { stockItem });
         var deductionHelper = GetStockDeductionHelper(testData);
         //Act
         deductionHelper.AdjustItem(itemDeduction, product, "Unit test");

         //Verify consumption and stock have been added
         Assert.AreEqual(StockStatus.Deducted, stockItem.StockStatus);
         Assert.AreEqual(0, stockItem.Quantity);

         Assert.AreEqual(1, product.Stocks.Count(), "Addition of negative stock entity to product");
         Assert.AreEqual(true, product.Stocks.First().IsNegative, "Stock is created as Negative Stock");
         Assert.AreEqual(1, product.Stocks.First().ReceivedQuantity, "Received quantity");
         Assert.AreEqual(0, product.Stocks.First().Quantity, "Available stock");
         Assert.AreEqual(StockStatus.Deducted, product.Stocks.First().StockStatus, "Stock has been deducted");

         //Verify
         testData.MockedAdjustments.Verify(m => m.Add(It.IsAny<StockAdjustment>()), Times.Once);
         testData.MockedAdjustments.Verify(m => m.Add(It.Is<StockAdjustment>(c => c.Quantity == 3)), Times.Once(), "Consumption recorded");
      }

      [TestMethod]
      public void Test_Stock_Quantity_Deduction()
      {
         //Arrange stock
         var stockItem = new Stock { ProductId = 123, Quantity = 6, StoredAt = 1 };
         var product = new Product { ProductId = 123 };
         var testData = new MockedInventoryStockData(new List<Product>() { product }, new List<Stock> { stockItem });
         var deductionHelper = GetStockDeductionHelper(testData);

         //Act
         deductionHelper.AdjustItem(new ItemAdjustment { ProductId = 123, Quantity = 3, StockLocationId = 1 }, product, "Unit test");

         //Verify consumption and stock have been added
         testData.MockedAdjustments.Verify(c => c.Add(It.IsAny<StockAdjustment>()), Times.Exactly(1));
         testData.MockedStock.Verify(s => s.Add(It.IsAny<Stock>()), Times.Never);

         Assert.AreEqual(StockStatus.Available, stockItem.StockStatus);
         Assert.AreEqual(3, stockItem.Quantity);
      }

      [TestMethod]
      public void Test_Oldest_Stock_Deducted()
      {
         var product = new Product { ProductId = 123 };

         //Arrange stock
         var newerStockItem = new Stock
         {
            ProductId = 123,
            Quantity = 6,
            StockId = 1,
            StoredAt = 1,
         };
         var olderStockItem = new Stock
         {
            ProductId = 123,
            Quantity = 6,
            StockId = 2,
            StoredAt = 1,
         };
         var testData = new MockedInventoryStockData(new List<Product>() { product }, new List<Stock> { newerStockItem, olderStockItem });
         var deductionHelper = GetStockDeductionHelper(testData);

         //Act
         var consumption = new ItemAdjustment
         {
            ProductId = 123,
            Quantity = 3,
            StockLocationId = 1
         };

         deductionHelper.AdjustItem(consumption, product, "Unit test");

         //Verify consumption has been added
         testData.MockedAdjustments.Verify(c => c.Add(It.IsAny<StockAdjustment>()), Times.Exactly(1));

         //Verify correct stock entry has been deducted
         Assert.AreEqual(6, newerStockItem.Quantity);
         Assert.AreEqual(3, olderStockItem.Quantity);
      }

      [TestMethod]
      public void SplitStock_KeepsReferenceToPositiveAdjustment()
      {
         var stock = new Stock
         {
            Quantity = 10,
            ReceivedQuantity = 10,
         };

         var adjustment = new StockAdjustment
         {
            IsPositive = true,
            Quantity = 10
         };

         stock.StockAdjustmentStocks.Add(new StockAdjustmentStock
         {
            StockAdjustment = adjustment
         });

         var itemConsumption = new ItemAdjustment
         {
            Quantity = 2
         };

         var testData = new MockedInventoryStockData();
         var repo = MockedInventoryRepositoryFactory.GetStockRepository(testData);

         var newStock = StockAdjustmentUnitOfWork.SplitStock(stock, itemConsumption, 1);

         Assert.AreEqual(1, newStock.StockAdjustmentStocks.Count(), "The number of adjustments associated with the stock");
         Assert.IsTrue(newStock.StockAdjustmentStocks.Single().StockAdjustment.IsPositive, "Check the adjustment is positive");
      }

      [TestMethod]
      public void SplitStock_SplitsWithNoPositiveAdjustment()
      {
         var stock = new Stock
         {
            Quantity = 10,
            ReceivedQuantity = 10,
         };

         var itemConsumption = new ItemAdjustment
         {
            Quantity = 2
         };

         var newStock = StockAdjustmentUnitOfWork.SplitStock(stock, itemConsumption, 1);
         Assert.AreEqual(0, newStock.StockAdjustmentStocks.Count());
      }

      [TestMethod]
      public void Deduct_MultipleStockEntries_SingleAdjustment()
      {
         var product = new Product { ProductId = 123 };

         //Arrange stock
         var stockItem = new Stock
         {
            ProductId = 123,
            StockStatus = StockStatus.Available,
            Quantity = 1,
            StoredAt = 1
         };

         var stockItem_v2 = new Stock
         {
            ProductId = 123,
            StockStatus = StockStatus.Available,
            Quantity = 2,
            StoredAt = 1
         };

         var testData = new MockedInventoryStockData(new List<Product>() { product }, new List<Stock> { stockItem, stockItem_v2 });
         var deductionHelper = GetStockDeductionHelper(testData);

         //Act
         deductionHelper.AdjustItem(new ItemAdjustment { ProductId = 123, Quantity = 3, StockLocationId = 1 }, product, "Unit test");

         //Verify
         testData.MockedAdjustments.Verify(c => c.Add(It.IsAny<StockAdjustment>()), Times.Once());

         //Assertions
         Assert.AreEqual(StockStatus.Deducted, stockItem.StockStatus);
         Assert.AreEqual(0, stockItem.Quantity);

         Assert.AreEqual(StockStatus.Deducted, stockItem.StockStatus);
         Assert.AreEqual(0, stockItem.Quantity);
      }
   }
}