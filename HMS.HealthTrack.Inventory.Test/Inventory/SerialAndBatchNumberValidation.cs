using System.Collections.Generic;
using System.Collections.ObjectModel;
using HMS.HealthTrack.Inventory.Common.Constants;
using HMS.HealthTrack.Inventory.Test.Inventory.Mocks;
using HMS.HealthTrack.Web.Data.Exceptions;
using HMS.HealthTrack.Web.Data.Helpers;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace HMS.HealthTrack.Inventory.Test.Inventory
{
   [TestClass]
   public class SerialAndBatchNumberValidation
   {
      private StockAdjustmentHelper GetStockDeductionHelper(MockedInventoryStockData data)
      {
         var stockRepo = MockedInventoryRepositoryFactory.GetStockRepository(data);
         var stockAdjustmentRepository = MockedInventoryRepositoryFactory.GetStockAdjustmentRepository(data);
         return new StockAdjustmentHelper(stockRepo, stockAdjustmentRepository);
      }

      [TestMethod]
      public void Consume_NoSettings_Product()
      {
         //Test data
         var product = new Product { ProductId = 123, ProductSettings = new Collection<StockSetting>() };
         var stock = new List<Stock> { new Stock { ProductId = 123, Quantity = 1, StoredAt = 1 } };
         var testConsumptionRecord = new ItemAdjustment { ProductId = 123, Quantity = 1, StockLocationId = 1 };

         //Arrange
         var testData = new MockedInventoryStockData(new List<Product> { product }, stock);
         var deductionHelper = GetStockDeductionHelper(testData);

         //Act
         deductionHelper.AdjustItem(testConsumptionRecord, product, "UnitTest");

         //Verify
         testData.MockedAdjustments.Verify(c => c.Add(It.IsAny<StockAdjustment>()), Times.Once());
      }


      [TestMethod]
      [Ignore] // TODO: Remove this when properly implementing the requirement of batch and serial numbers
      [ExpectedException(typeof(StockException))]
      public void Consume_SerialRequired_Product_Without_Serial()
      {
         //Test data
         var product = new Product { ProductId = 123, ProductSettings = new Collection<StockSetting> { new StockSetting { SettingId = InventoryConstants.StockSettings.RequiresSerialNumber } } };
         var stock = new List<Stock> { new Stock { ProductId = 123, Quantity = 1 } };
         var testConsumptionRecord = new ItemAdjustment { ProductId = 123, Quantity = 1 };

         //Arrange
         var testData = new MockedInventoryStockData(new List<Product> { product }, stock);
         var deductionHelper = GetStockDeductionHelper(testData);

         //Act
         deductionHelper.AdjustItem(testConsumptionRecord, product, "UnitTest");
      }

      [TestMethod]
      public void Consume_SerialRequired_Product_With_Serial()
      {
         //Test data
         var product = new Product { ProductId = 123, ProductSettings = new Collection<StockSetting> { new StockSetting { SettingId = InventoryConstants.StockSettings.RequiresSerialNumber } } };
         var stock = new List<Stock> { new Stock { ProductId = 123, Quantity = 1, StoredAt = 1 } };
         var testConsumptionRecord = new ItemAdjustment { ProductId = 123, Quantity = 1, SerialNumber = "TEST SERIAL NUMBER", StockLocationId = 1 };

         //Arrange
         var testData = new MockedInventoryStockData(new List<Product> { product }, stock);
         var deductionHelper = GetStockDeductionHelper(testData);

         //Act
         deductionHelper.AdjustItem(testConsumptionRecord, product, "UnitTest");

         //Verify
         testData.MockedAdjustments.Verify(c => c.Add(It.IsAny<StockAdjustment>()), Times.Once());
      }



      [TestMethod]
      [Ignore] // TODO: Remove this when properly implementing the requirement of batch and serial numbers
      [ExpectedException(typeof(StockException))]
      public void Consume_BatchRequired_Product_Without_Batch()
      {
         //Test data
         var product = new Product { ProductId = 123, ProductSettings = new Collection<StockSetting> { new StockSetting { SettingId = InventoryConstants.StockSettings.RequiresBatchNumber } } };
         var stock = new List<Stock> { new Stock { ProductId = 123, Quantity = 1 } };
         var testConsumptionRecord = new ItemAdjustment { ProductId = 123, Quantity = 1 };

         //Arrange
         var testData = new MockedInventoryStockData(new List<Product> { product }, stock);
         var deductionHelper = GetStockDeductionHelper(testData);

         //Act
         deductionHelper.AdjustItem(testConsumptionRecord, product, "UnitTest");
      }

      [TestMethod]
      public void Consume_BatchRequired_Product_With_Batch()
      {
         //Test data
         var product = new Product { ProductId = 123, ProductSettings = new Collection<StockSetting> { new StockSetting { SettingId = InventoryConstants.StockSettings.RequiresBatchNumber } } };
         var stock = new List<Stock> { new Stock { ProductId = 123, Quantity = 1, StoredAt = 1 } };
         var testConsumptionRecord = new ItemAdjustment { ProductId = 123, Quantity = 1, BatchNumber = "BATCH NUMBER", StockLocationId = 1 };

         //Arrange
         var testData = new MockedInventoryStockData(new List<Product> { product }, stock);
         var deductionHelper = GetStockDeductionHelper(testData);

         //Act
         deductionHelper.AdjustItem(testConsumptionRecord, product, "UnitTest");

         //Verify
         testData.MockedAdjustments.Verify(c => c.Add(It.IsAny<StockAdjustment>()), Times.Once());
      }
   }
}
