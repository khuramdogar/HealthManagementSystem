using System.Collections.Generic;
using System.Linq;
using HMS.HealthTrack.Inventory.Test.Inventory.Mocks;
using HMS.HealthTrack.Web.Data.Exceptions;
using HMS.HealthTrack.Web.Data.Helpers;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MSTestExtensions;

namespace HMS.HealthTrack.Inventory.Test.Inventory
{
   [TestClass]
   public class SerialAndBatchDeduction
   {
      private StockAdjustmentHelper GetStockDeductionHelper(MockedInventoryStockData data)
      {
         var stockRepo = MockedInventoryRepositoryFactory.GetStockRepository(data);
         var stockAdjustmentRepository = MockedInventoryRepositoryFactory.GetStockAdjustmentRepository(data);
         return new StockAdjustmentHelper(stockRepo, stockAdjustmentRepository);
      }

      [TestMethod]
      public void Consume_No_Serial_No_Batch_No_Stock_EmptyLocation()
      {
         //Test data
         var product = new Product { ProductId = 123 };
         var adjustment = new ItemAdjustment() { ProductId = 123, Quantity = 1 };
         var testData = new MockedInventoryStockData(new List<Product> { product }, new List<Stock>());

         //SUT
         var deductionHelper = GetStockDeductionHelper(testData);

         //Test
         ExceptionAssert.Throws<StockException>(() => deductionHelper.AdjustItem(adjustment, product, "UnitTest"));

         //Verify
         testData.MockedStock.Verify(c => c.Add(It.IsAny<Stock>()), Times.Never(), "No new stock");
         testData.MockedAdjustments.Verify(m => m.Add(It.IsAny<StockAdjustment>()), Times.Never,
            "No adjustment recorded");
      }

      #region adjustment with No Serial and No Batch

      [TestMethod]
      public void Consume_NoSerial_NoBatch_NoStock_CreatesNegativeStock()
      {
         //Test data
         var product = new Product { ProductId = 123 };
         var adjustment = new ItemAdjustment() { ProductId = 123, Quantity = 1, StockLocationId = 1 };

         var testData = new MockedInventoryStockData(new List<Product> { product }, new List<Stock>());

         //SUT
         var deductionHelper = GetStockDeductionHelper(testData);

         //Test
         deductionHelper.AdjustItem(adjustment, product, "UnitTest");

         //Verify
         Assert.AreEqual(1, product.Stocks.Count(), "Addition of negative stock entity to product");
         Assert.AreEqual(true, product.Stocks.First().IsNegative, "Stock is created as Negative Stock");
         Assert.AreEqual(1, product.Stocks.First().ReceivedQuantity, "Received quantity");
         Assert.AreEqual(0, product.Stocks.First().Quantity, "Available stock");
         Assert.AreEqual(adjustment.SerialNumber, product.Stocks.First().SerialNumber,
            "Serial number added to negative stock");
         Assert.AreEqual(adjustment.BatchNumber, product.Stocks.First().BatchNumber,
            "Batch number added to negative stock");
         Assert.AreEqual(StockStatus.Deducted, product.Stocks.First().StockStatus, "Stock has been deducted");

         //Verify
         testData.MockedAdjustments.Verify(m => m.Add(It.Is<StockAdjustment>(c => c.Quantity == 1)), Times.Once(),
            "adjustment recorded");
      }

      [TestMethod]
      public void Consume_No_Serial_No_Batch_StockWith_NoSerial_NoBatch()
      {
         //Test data
         var product = new Product { ProductId = 123 };
         var adjustment = new ItemAdjustment() { ProductId = 123, Quantity = 1, StockLocationId = 1 };
         var stock = new Stock { ProductId = 123, Quantity = 1, StoredAt = 1 };

         var testData = new MockedInventoryStockData(new List<Product> { product }, new List<Stock> { stock });
         //SUT
         var deductionHelper = GetStockDeductionHelper(testData);

         //Test
         deductionHelper.AdjustItem(adjustment, product, "UnitTest");

         //Verify
         testData.MockedStock.Verify(c => c.Add(It.IsAny<Stock>()), Times.Never, "No stock added");
         Assert.AreEqual(0, stock.Quantity, "Deducted from stock");
         testData.MockedAdjustments.Verify(m => m.Add(It.Is<StockAdjustment>(c => c.Quantity == 1)), Times.Once(),
            "adjustment recorded");
      }

      [TestMethod]
      public void Consume_No_Serial_No_Batch_Stock_With_Serial()
      {
         //Test data
         var product = new Product { ProductId = 123 };
         var adjustment = new ItemAdjustment() { ProductId = 123, Quantity = 1, StockLocationId = 1 };
         var stock = new Stock { ProductId = 123, Quantity = 1, SerialNumber = "SERIAL", StoredAt = 1 };

         var testData = new MockedInventoryStockData(new List<Product> { product }, new List<Stock> { stock });

         //SUT
         var deductionHelper = GetStockDeductionHelper(testData);

         //Test
         ExceptionAssert.Throws<AmbiguousStockException>(
            () => deductionHelper.AdjustItem(adjustment, product, "UnitTest"));

         //Verify
         testData.MockedStock.Verify(c => c.Add(It.IsAny<Stock>()), Times.Never, "Not added to stock");
         Assert.AreEqual(1, stock.Quantity, "No stock deduction");
         testData.MockedAdjustments.Verify(m => m.Add(It.IsAny<StockAdjustment>()), Times.Never(),
            "No adjustment recorded");
      }

      [TestMethod]
      public void Consume_No_Serial_No_Batch_Stock_With_Batch()
      {
         //Test data
         var product = new Product { ProductId = 123 };
         var adjustment = new ItemAdjustment() { ProductId = 123, Quantity = 1, StockLocationId = 1 };
         var stock = new Stock { ProductId = 123, Quantity = 1, BatchNumber = "BATCH", StoredAt = 1 };

         var testData = new MockedInventoryStockData(new List<Product> { product }, new List<Stock> { stock });

         //SUT
         var deductionHelper = GetStockDeductionHelper(testData);

         //Test
         ExceptionAssert.Throws<AmbiguousStockException>(
            () => deductionHelper.AdjustItem(adjustment, product, "UnitTest"));

         //Verify
         testData.MockedStock.Verify(c => c.Add(It.IsAny<Stock>()), Times.Never, "Not added to stock");
         testData.MockedAdjustments.Verify(m => m.Add(It.IsAny<StockAdjustment>()), Times.Never(),
            "No adjustment recorded");
      }

      [TestMethod]
      public void Consume_No_Serial_No_Batch_Stock_With_SerialAndBatch()
      {
         //Test data
         var product = new Product { ProductId = 123 };
         var adjustment = new ItemAdjustment() { ProductId = 123, Quantity = 1, StockLocationId = 1 };
         var stock = new Stock
         {
            ProductId = 123,
            Quantity = 1,
            BatchNumber = "BATCH",
            SerialNumber = "SERIAL",
            StoredAt = 1
         };

         var testData = new MockedInventoryStockData(new List<Product> { product }, new List<Stock> { stock });

         //SUT
         var deductionHelper = GetStockDeductionHelper(testData);

         //Test
         ExceptionAssert.Throws<AmbiguousStockException>(
            () => deductionHelper.AdjustItem(adjustment, product, "UnitTest"));


         //Verify
         testData.MockedStock.Verify(c => c.Add(It.IsAny<Stock>()), Times.Never, "Not added to stock");
         testData.MockedAdjustments.Verify(m => m.Add(It.IsAny<StockAdjustment>()), Times.Never(),
            "No adjustment recorded");
      }

      #endregion

      #region adjustment with Serial Number but No Batch

      [TestMethod]
      public void Consume_Serial_NoBatch_Stock_With_MatchSerial_NoBatch()
      {
         //Test data
         var product = new Product { ProductId = 123 };
         var adjustment = new ItemAdjustment()
         {
            ProductId = 123,
            Quantity = 1,
            SerialNumber = "SERIAL",
            StockLocationId = 1
         };
         var stock = new Stock { ProductId = 123, Quantity = 1, SerialNumber = "SERIAL", StoredAt = 1 };

         var testData = new MockedInventoryStockData(new List<Product> { product }, new List<Stock> { stock });

         //SUT
         var deductionHelper = GetStockDeductionHelper(testData);

         //Test
         deductionHelper.AdjustItem(adjustment, product, "UnitTest");

         //Verify
         testData.MockedStock.Verify(c => c.Add(It.IsAny<Stock>()), Times.Never, "Verify no stock added");
         Assert.AreEqual(0, stock.Quantity, "Verify stock deducted");
         Assert.AreEqual("SERIAL", stock.SerialNumber, "Verify serial number recorded in stock");
         testData.MockedAdjustments.Verify(m => m.Add(It.Is<StockAdjustment>(c => c.Quantity == 1)), Times.Once(),
            "Verify adjustment was recorded");
      }

      [TestMethod]
      public void Consume_HasSerial_NoBatch_Stock_With_MatchSerial_HasBatch()
      {
         //Test data
         var product = new Product { ProductId = 123 };
         var adjustment = new ItemAdjustment()
         {
            ProductId = 123,
            Quantity = 1,
            SerialNumber = "SERIAL",
            StockLocationId = 1
         };
         var stock = new Stock
         {
            ProductId = 123,
            Quantity = 1,
            SerialNumber = "SERIAL",
            BatchNumber = "BATCH",
            StoredAt = 1
         };

         var testData = new MockedInventoryStockData(new List<Product> { product }, new List<Stock> { stock });

         //SUT
         var deductionHelper = GetStockDeductionHelper(testData);

         //Test
         deductionHelper.AdjustItem(adjustment, product, "UnitTest");

         //Verify
         testData.MockedStock.Verify(c => c.Add(It.IsAny<Stock>()), Times.Never, "Verify no stock added");
         Assert.AreEqual(0, stock.Quantity, "Verify stock deducted");
         Assert.AreEqual("SERIAL", stock.SerialNumber, "Verify serial number recorded in stock");
         Assert.AreEqual("BATCH", stock.BatchNumber, "Verify batch number recorded in stock");
         testData.MockedAdjustments.Verify(m => m.Add(It.Is<StockAdjustment>(c => c.Quantity == 1)), Times.Once(),
            "Verify adjustment was recorded");
      }

      [TestMethod]
      public void Consume_Serial_NoBatch_Stock_With_NoSerial_NoBatch()
      {
         //Test data
         var product = new Product { ProductId = 123 };
         var adjustment = new ItemAdjustment()
         {
            ProductId = 123,
            Quantity = 1,
            SerialNumber = "SERIAL",
            StockLocationId = 1
         };
         var stock = new Stock { ProductId = 123, Quantity = 1, StoredAt = 1 };

         var testData = new MockedInventoryStockData(new List<Product> { product }, new List<Stock> { stock });

         //SUT
         var deductionHelper = GetStockDeductionHelper(testData);

         //Test
         deductionHelper.AdjustItem(adjustment, product, "UnitTest");

         //Verify
         testData.MockedStock.Verify(c => c.Add(It.IsAny<Stock>()), Times.Never, "Verify no stock added");
         Assert.AreEqual(0, stock.Quantity, "Verify stock deducted");
         Assert.AreEqual("SERIAL", stock.SerialNumber, "Verify serial number recorded in stock");
         Assert.IsNull(stock.BatchNumber, "Verify batch number is unchanged");
         testData.MockedAdjustments.Verify(m => m.Add(It.Is<StockAdjustment>(c => c.Quantity == 1)), Times.Once(),
            "Verify adjustment was recorded");
      }

      [TestMethod]
      public void Consume_Serial_NoBatch_Stock_With_MatchingSerial_NoBatch()
      {
         //Test data
         var product = new Product { ProductId = 123 };
         var adjustment = new ItemAdjustment()
         {
            ProductId = 123,
            Quantity = 1,
            SerialNumber = "SERIAL",
            StockLocationId = 1
         };
         var stock = new Stock { ProductId = 123, Quantity = 1, SerialNumber = "SERIAL", StoredAt = 1 };

         var testData = new MockedInventoryStockData(new List<Product> { product }, new List<Stock> { stock });

         //SUT
         var deductionHelper = GetStockDeductionHelper(testData);

         //Test
         deductionHelper.AdjustItem(adjustment, product, "UnitTest");

         //Verify
         testData.MockedStock.Verify(c => c.Add(It.IsAny<Stock>()), Times.Never, "No stock added");
         //Should not be added to stock
         Assert.AreEqual(0, stock.Quantity, "Verify stock deducted");
         testData.MockedAdjustments.Verify(m => m.Add(It.Is<StockAdjustment>(c => c.Quantity == 1)), Times.Once(),
            "Verify adjustment was recorded"); //adjustment record should be added
      }

      [TestMethod]
      public void Consume_HasSerial_No_Batch_Stock_With_HasNonMatchingSerial_NoBatch()
      {
         //Test data
         var product = new Product { ProductId = 123 };
         var adjustment = new ItemAdjustment()
         {
            ProductId = 123,
            Quantity = 1,
            SerialNumber = "CONSUMUPTION SERIAL",
            StockLocationId = 1
         };
         var stock = new Stock { ProductId = 123, Quantity = 1, SerialNumber = "ORIGINAL SERIAL", StoredAt = 1 };

         var testData = new MockedInventoryStockData(new List<Product> { product }, new List<Stock> { stock });

         //SUT
         var deductionHelper = GetStockDeductionHelper(testData);

         //Test
         ExceptionAssert.Throws<AmbiguousStockException>(
            () => deductionHelper.AdjustItem(adjustment, product, "UnitTest"));

         //Verify
         testData.MockedStock.Verify(c => c.Add(It.IsAny<Stock>()), Times.Never, "Not added to stock");
         Assert.AreEqual(1, stock.Quantity, "Verify stock level is not affected");
         Assert.AreEqual("ORIGINAL SERIAL", stock.SerialNumber, "Verify stock serial is not affected");
         testData.MockedAdjustments.Verify(m => m.Add(It.IsAny<StockAdjustment>()), Times.Never(),
            "No adjustment recorded");
      }

      [TestMethod]
      public void Consume_HasSerial_No_Batch_Stock_With_HasNonMatchingSerial_HasBatch()
      {
         //Test data
         var product = new Product { ProductId = 123 };
         var adjustment = new ItemAdjustment()
         {
            ProductId = 123,
            Quantity = 1,
            SerialNumber = "CONSUMUPTION SERIAL",
            StockLocationId = 1
         };
         var stock = new Stock
         {
            ProductId = 123,
            Quantity = 1,
            SerialNumber = "ORIGINAL SERIAL",
            BatchNumber = "STOCK BATCH",
            StoredAt = 1
         };

         var testData = new MockedInventoryStockData(new List<Product> { product }, new List<Stock> { stock });

         //SUT
         var deductionHelper = GetStockDeductionHelper(testData);

         //Test
         ExceptionAssert.Throws<AmbiguousStockException>(
            () => deductionHelper.AdjustItem(adjustment, product, "UnitTest"));

         //Verify
         testData.MockedStock.Verify(c => c.Add(It.IsAny<Stock>()), Times.Never, "Not added to stock");
         Assert.AreEqual(1, stock.Quantity, "Verify stock level is not affected");
         Assert.AreEqual("ORIGINAL SERIAL", stock.SerialNumber, "Verify stock serial is not affected");
         Assert.AreEqual("STOCK BATCH", stock.BatchNumber, "Verify stock batch is not affected");
         testData.MockedAdjustments.Verify(m => m.Add(It.IsAny<StockAdjustment>()), Times.Never(),
            "No adjustment recorded");
      }

      [TestMethod]
      public void Consume_HasSerial_No_Batch_NoStock_CreatesNegativeStock()
      {
         //Test data
         var product = new Product { ProductId = 123 };
         var adjustment = new ItemAdjustment()
         {
            ProductId = 123,
            Quantity = 1,
            SerialNumber = "CONSUMUPTION SERIAL",
            StockLocationId = 1
         };

         var testData = new MockedInventoryStockData(new List<Product> { product }, new List<Stock>());

         //SUT
         var deductionHelper = GetStockDeductionHelper(testData);

         //Test
         deductionHelper.AdjustItem(adjustment, product, "UnitTest");

         //Verify
         Assert.AreEqual(1, product.Stocks.Count()); // negative stock added to the product
         Assert.AreEqual(true, product.Stocks.First().IsNegative);
         Assert.AreEqual(StockStatus.Deducted, product.Stocks.First().StockStatus);
         Assert.AreEqual(adjustment.SerialNumber, product.Stocks.First().SerialNumber);
         testData.MockedAdjustments.Verify(m => m.Add(It.Is<StockAdjustment>(c => c.Quantity == 1)), Times.Once(),
            "adjustment recorded");
      }

      [TestMethod]
      public void Consume_Serial_NoBatch_Stock_With_MatchSerial_NoBatch_DifferentStorageLocation_CreatesNegativeStock()
      {
         //Test data
         var product = new Product { ProductId = 123 };
         var adjustment = new ItemAdjustment()
         {
            ProductId = 123,
            Quantity = 1,
            SerialNumber = "SERIAL",
            StockLocationId = 1
         };
         var stock = new Stock { ProductId = 123, Quantity = 1, SerialNumber = "SERIAL", StoredAt = 2 };

         var testData = new MockedInventoryStockData(new List<Product> { product }, new List<Stock> { stock });

         //SUT
         var deductionHelper = GetStockDeductionHelper(testData);

         deductionHelper.AdjustItem(adjustment, product, "UnitTest");

         //Verify
         Assert.AreEqual(1, stock.Quantity, "Verify stock not deducted");
         Assert.AreEqual(1, product.Stocks.Count(), "Addition of negative stock entity to product");
         Assert.AreEqual(true, product.Stocks.First().IsNegative, "Stock is created as Negative Stock");
         Assert.AreEqual(1, product.Stocks.First().ReceivedQuantity, "Received quantity");
         Assert.AreEqual(0, product.Stocks.First().Quantity, "Available stock");
         Assert.AreEqual(adjustment.SerialNumber, product.Stocks.First().SerialNumber,
            "Serial number added to negative stock");
         Assert.AreEqual(adjustment.BatchNumber, product.Stocks.First().BatchNumber,
            "Batch number added to negative stock");
         Assert.AreEqual(StockStatus.Deducted, product.Stocks.First().StockStatus, "Stock has been deducted");

         //Verify
         testData.MockedAdjustments.Verify(m => m.Add(It.Is<StockAdjustment>(c => c.Quantity == 1)), Times.Once(),
            "adjustment recorded");
      }

      #endregion

      #region adjustment with No Serial Number but has Batch

      [TestMethod]
      public void Consume_NoSerial_HasBatch_Stock_With_NoSerial_MatchingBatch()
      {
         //Test data
         var product = new Product { ProductId = 123 };
         var adjustment = new ItemAdjustment()
         {
            ProductId = 123,
            Quantity = 1,
            BatchNumber = "BATCH",
            StockLocationId = 1
         };
         var stock = new Stock { ProductId = 123, Quantity = 1, BatchNumber = "BATCH", StoredAt = 1 };

         var testData = new MockedInventoryStockData(new List<Product> { product }, new List<Stock> { stock });

         //SUT
         var deductionHelper = GetStockDeductionHelper(testData);

         //Test
         deductionHelper.AdjustItem(adjustment, product, "UnitTest");

         //Verify
         testData.MockedStock.Verify(c => c.Add(It.IsAny<Stock>()), Times.Never, "Verify no stock added");
         Assert.AreEqual(0, stock.Quantity, "Verify stock deducted");
         Assert.AreEqual("BATCH", stock.BatchNumber, "Verify batch number recorded in stock");
         testData.MockedAdjustments.Verify(m => m.Add(It.Is<StockAdjustment>(c => c.Quantity == 1)), Times.Once(),
            "Verify adjustment was recorded");
      }


      [TestMethod]
      public void Consume_NoSerial_HasBatch_Stock_With_NoSerial_NoBatch()
      {
         //Test data
         var product = new Product { ProductId = 123 };
         var adjustment = new ItemAdjustment()
         {
            ProductId = 123,
            Quantity = 1,
            BatchNumber = "BATCH",
            StockLocationId = 1
         };
         var stock = new Stock { ProductId = 123, Quantity = 1, StoredAt = 1 };

         var testData = new MockedInventoryStockData(new List<Product> { product }, new List<Stock> { stock });

         //SUT
         var deductionHelper = GetStockDeductionHelper(testData);

         //Test
         deductionHelper.AdjustItem(adjustment, product, "UnitTest");

         //Verify
         testData.MockedStock.Verify(c => c.Add(It.IsAny<Stock>()), Times.Never, "Verify no stock added");
         Assert.AreEqual(0, stock.Quantity, "Verify stock deducted");
         Assert.AreEqual("BATCH", stock.BatchNumber, "Verify batch number recorded in stock");
         testData.MockedAdjustments.Verify(m => m.Add(It.Is<StockAdjustment>(c => c.Quantity == 1)), Times.Once(),
            "Verify adjustment was recorded");
      }


      [TestMethod]
      public void Consume_NoSerial_HasBatch_Stock_With_HasSerial_MatchingBatch()
      {
         //Test data
         var product = new Product { ProductId = 123 };
         var adjustment = new ItemAdjustment()
         {
            ProductId = 123,
            Quantity = 1,
            BatchNumber = "BATCH",
            StockLocationId = 1
         };
         var stock = new Stock
         {
            ProductId = 123,
            Quantity = 1,
            SerialNumber = "ORIGINAL SERIAL",
            BatchNumber = "BATCH",
            StoredAt = 1
         };

         var testData = new MockedInventoryStockData(new List<Product> { product }, new List<Stock> { stock });

         //SUT
         var deductionHelper = GetStockDeductionHelper(testData);

         //Test
         ExceptionAssert.Throws<AmbiguousStockException>(
            () => deductionHelper.AdjustItem(adjustment, product, "UnitTest"));

         //Verify
         testData.MockedStock.Verify(c => c.Add(It.IsAny<Stock>()), Times.Never, "Not added to stock");
         Assert.AreEqual(1, stock.Quantity, "Verify stock level is not affected");
         Assert.AreEqual("ORIGINAL SERIAL", stock.SerialNumber, "Verify stock serial is not affected");
         Assert.AreEqual("BATCH", stock.BatchNumber, "Verify stock batch is not affected");
         testData.MockedAdjustments.Verify(m => m.Add(It.IsAny<StockAdjustment>()), Times.Never(),
            "No adjustment recorded");
      }

      [TestMethod]
      public void Consume_NoSerial_HasBatch_Stock_With_NoSerial_HasNonMatchingBatch()
      {
         //Test data
         var product = new Product { ProductId = 123 };
         var adjustment = new ItemAdjustment()
         {
            ProductId = 123,
            Quantity = 1,
            BatchNumber = "adjustment BATCH",
            StockLocationId = 1
         };
         var stock = new Stock { ProductId = 123, Quantity = 1, BatchNumber = "BATCH", StoredAt = 1 };

         var testData = new MockedInventoryStockData(new List<Product> { product }, new List<Stock> { stock });

         //SUT
         var deductionHelper = GetStockDeductionHelper(testData);

         //Test
         ExceptionAssert.Throws<AmbiguousStockException>(
            () => deductionHelper.AdjustItem(adjustment, product, "UnitTest"));

         //Verify
         testData.MockedStock.Verify(c => c.Add(It.IsAny<Stock>()), Times.Never, "Not added to stock");
         Assert.AreEqual(1, stock.Quantity, "Verify stock level is not affected");
         Assert.AreEqual("BATCH", stock.BatchNumber, "Verify stock batch is not affected");
         testData.MockedAdjustments.Verify(m => m.Add(It.IsAny<StockAdjustment>()), Times.Never(),
            "No adjustment recorded");
      }


      [TestMethod]
      public void Consume_NoSerial_HasBatch_Stock_With_HasSerial_NoBatch()
      {
         //Test data
         var product = new Product { ProductId = 123 };
         var adjustment = new ItemAdjustment()
         {
            ProductId = 123,
            Quantity = 1,
            BatchNumber = "BATCH",
            StockLocationId = 1
         };
         var stock = new Stock { ProductId = 123, Quantity = 1, SerialNumber = "SERIAL", StoredAt = 1 };

         var testData = new MockedInventoryStockData(new List<Product> { product }, new List<Stock> { stock });

         //SUT
         var deductionHelper = GetStockDeductionHelper(testData);

         //Test
         ExceptionAssert.Throws<AmbiguousStockException>(
            () => deductionHelper.AdjustItem(adjustment, product, "UnitTest"));

         //Verify
         testData.MockedStock.Verify(c => c.Add(It.IsAny<Stock>()), Times.Never, "Not added to stock");
         Assert.AreEqual(1, stock.Quantity, "Verify stock level is not affected");
         Assert.AreEqual("SERIAL", stock.SerialNumber, "Verify stock serial is not affected");
         testData.MockedAdjustments.Verify(m => m.Add(It.IsAny<StockAdjustment>()), Times.Never(),
            "No adjustment recorded");
      }

      [TestMethod]
      public void Consume_NoSerial_HasBatch_NoStock_CreatesNegativeStock()
      {
         //Test data
         var product = new Product { ProductId = 123 };
         var adjustment = new ItemAdjustment()
         {
            ProductId = 123,
            Quantity = 1,
            BatchNumber = "BATCH",
            StockLocationId = 1
         };

         var testData = new MockedInventoryStockData(new List<Product> { product }, new List<Stock>());

         //SUT
         var deductionHelper = GetStockDeductionHelper(testData);

         //Test
         deductionHelper.AdjustItem(adjustment, product, "UnitTest");

         //Verify
         Assert.AreEqual(1, product.Stocks.Count()); // negative stock added to the product
         Assert.AreEqual(true, product.Stocks.First().IsNegative);
         Assert.AreEqual(StockStatus.Deducted, product.Stocks.First().StockStatus);
         Assert.AreEqual(adjustment.BatchNumber, product.Stocks.First().BatchNumber);
         testData.MockedAdjustments.Verify(m => m.Add(It.Is<StockAdjustment>(c => c.Quantity == 1)), Times.Once(),
            "adjustment recorded");
      }

      #endregion

      #region adjustment with Serial and Batch

      [TestMethod]
      public void Consume_HasSerial_HasBatch_Stock_MatchingSerial_MatchingBatch()
      {
         //Test data
         var product = new Product { ProductId = 123 };
         var adjustment = new ItemAdjustment()
         {
            ProductId = 123,
            Quantity = 1,
            SerialNumber = "Serial",
            BatchNumber = "Batch",
            StockLocationId = 1
         };
         var stock = new Stock
         {
            ProductId = 123,
            Quantity = 1,
            SerialNumber = "Serial",
            BatchNumber = "Batch",
            StoredAt = 1
         };

         var testData = new MockedInventoryStockData(new List<Product> { product }, new List<Stock> { stock });

         //SUT
         var deductionHelper = GetStockDeductionHelper(testData);

         //Test
         deductionHelper.AdjustItem(adjustment, product, "UnitTest");

         //Verify
         testData.MockedStock.Verify(c => c.Add(It.IsAny<Stock>()), Times.Never, "No stock added");
         Assert.AreEqual(0, stock.Quantity, "Deducted from stock");
         Assert.AreEqual("Serial", stock.SerialNumber, "Serial recorded");
         Assert.AreEqual("Batch", stock.BatchNumber, "Batch recorded");
         testData.MockedAdjustments.Verify(m => m.Add(It.Is<StockAdjustment>(c => c.Quantity == 1)), Times.Once(),
            "adjustment recorded");
      }

      [TestMethod]
      public void Consume_HasSerial_HasBatch_Stock_MatchingSerial_NoBatch()
      {
         //Test data
         var product = new Product { ProductId = 123 };
         var adjustment = new ItemAdjustment()
         {
            ProductId = 123,
            Quantity = 1,
            SerialNumber = "Serial",
            BatchNumber = "Batch",
            StockLocationId = 1
         };
         var stock = new Stock { ProductId = 123, Quantity = 1, SerialNumber = "Serial", StoredAt = 1 };

         var testData = new MockedInventoryStockData(new List<Product> { product }, new List<Stock> { stock });

         //SUT
         var deductionHelper = GetStockDeductionHelper(testData);

         //Test
         deductionHelper.AdjustItem(adjustment, product, "UnitTest");

         //Verify
         testData.MockedStock.Verify(c => c.Add(It.IsAny<Stock>()), Times.Never, "No stock added");
         Assert.AreEqual(0, stock.Quantity, "Deducted from stock");
         Assert.AreEqual("Serial", stock.SerialNumber, "Serial recorded");
         Assert.AreEqual("Batch", stock.BatchNumber, "Batch recorded");
         testData.MockedAdjustments.Verify(m => m.Add(It.Is<StockAdjustment>(c => c.Quantity == 1)), Times.Once(),
            "adjustment recorded");
      }

      [TestMethod]
      public void Consume_HasSerial_HasBatch_Stock_NoSerial_NoBatch()
      {
         //Test data
         var product = new Product { ProductId = 123 };
         var adjustment = new ItemAdjustment()
         {
            ProductId = 123,
            Quantity = 1,
            SerialNumber = "Serial",
            BatchNumber = "Batch",
            StockLocationId = 1
         };
         var stock = new Stock { ProductId = 123, Quantity = 1, StoredAt = 1 };

         var testData = new MockedInventoryStockData(new List<Product> { product }, new List<Stock> { stock });

         //SUT
         var deductionHelper = GetStockDeductionHelper(testData);

         //Test
         deductionHelper.AdjustItem(adjustment, product, "UnitTest");

         //Verify
         testData.MockedStock.Verify(c => c.Add(It.IsAny<Stock>()), Times.Never, "No stock added");
         Assert.AreEqual(0, stock.Quantity, "Deducted from stock");
         Assert.AreEqual("Serial", stock.SerialNumber, "Serial recorded");
         Assert.AreEqual("Batch", stock.BatchNumber, "Batch recorded");
         testData.MockedAdjustments.Verify(m => m.Add(It.Is<StockAdjustment>(c => c.Quantity == 1)), Times.Once(),
            "adjustment recorded");
      }


      [TestMethod]
      public void Consume_HasSerial_HasBatch_Stock_With_NoSerial_MatchingBatch()
      {
         //Test data
         var product = new Product { ProductId = 123 };
         var adjustment = new ItemAdjustment()
         {
            ProductId = 123,
            Quantity = 1,
            SerialNumber = "Serial",
            BatchNumber = "Batch",
            StockLocationId = 1
         };
         var stock = new Stock { ProductId = 123, Quantity = 1, BatchNumber = "Batch", StoredAt = 1 };

         var testData = new MockedInventoryStockData(new List<Product> { product }, new List<Stock> { stock });

         //SUT
         var deductionHelper = GetStockDeductionHelper(testData);

         //Test
         ExceptionAssert.Throws<AmbiguousStockException>(
            () => deductionHelper.AdjustItem(adjustment, product, "UnitTest"));

         //Verify
         testData.MockedStock.Verify(c => c.Add(It.IsAny<Stock>()), Times.Never, "Not added to stock");
         Assert.AreEqual(1, stock.Quantity, "Verify stock level is not affected");
         Assert.AreEqual("Batch", stock.BatchNumber, "Verify batch is not affected");
         Assert.IsNull(stock.SerialNumber, "Verify serial is not affected");
         testData.MockedAdjustments.Verify(m => m.Add(It.IsAny<StockAdjustment>()), Times.Never(),
            "No adjustment recorded");
      }

      [TestMethod]
      public void Consume_HasSerial_HasBatch_Stock_With_NoSerial_NonMatchingBatch()
      {
         //Test data
         var product = new Product { ProductId = 123 };
         var adjustment = new ItemAdjustment()
         {
            ProductId = 123,
            Quantity = 1,
            SerialNumber = "Serial",
            BatchNumber = "Batch",
            StockLocationId = 1
         };
         var stock = new Stock { ProductId = 123, Quantity = 1, BatchNumber = "STOCK BATCH", StoredAt = 1 };

         var testData = new MockedInventoryStockData(new List<Product> { product }, new List<Stock> { stock });

         //SUT
         var deductionHelper = GetStockDeductionHelper(testData);

         //Test
         ExceptionAssert.Throws<AmbiguousStockException>(
            () => deductionHelper.AdjustItem(adjustment, product, "UnitTest"));

         //Verify
         testData.MockedStock.Verify(c => c.Add(It.IsAny<Stock>()), Times.Never, "Not added to stock");
         Assert.AreEqual(1, stock.Quantity, "Verify stock level is not affected");
         Assert.AreEqual("STOCK BATCH", stock.BatchNumber, "Verify batch is not affected");
         Assert.IsNull(stock.SerialNumber, "Verify serial is not affected");
         testData.MockedAdjustments.Verify(m => m.Add(It.IsAny<StockAdjustment>()), Times.Never(),
            "No adjustment recorded");
      }

      [TestMethod]
      public void Consume_HasSerial_HasBatch_Stock_With_MatchingSerial_NonMatchingBatch()
      {
         //Test data
         var product = new Product { ProductId = 123 };
         var adjustment = new ItemAdjustment()
         {
            ProductId = 123,
            Quantity = 1,
            SerialNumber = "Serial",
            BatchNumber = "Batch",
            StockLocationId = 1
         };
         var stock = new Stock { ProductId = 123, Quantity = 1, BatchNumber = "STOCK BATCH", StoredAt = 1 };

         var testData = new MockedInventoryStockData(new List<Product> { product }, new List<Stock> { stock });

         //SUT
         var deductionHelper = GetStockDeductionHelper(testData);

         //Test
         ExceptionAssert.Throws<AmbiguousStockException>(
            () => deductionHelper.AdjustItem(adjustment, product, "UnitTest"));

         //Verify
         testData.MockedStock.Verify(c => c.Add(It.IsAny<Stock>()), Times.Never, "Not added to stock");
         Assert.AreEqual(1, stock.Quantity, "Verify stock level is not affected");
         Assert.AreEqual("STOCK BATCH", stock.BatchNumber, "Verify batch is not affected");
         Assert.IsNull(stock.SerialNumber, "Verify serial is not affected");
         testData.MockedAdjustments.Verify(m => m.Add(It.IsAny<StockAdjustment>()), Times.Never(),
            "No adjustment recorded");
      }

      [TestMethod]
      public void Consume_HasSerial_HasBatch_Stock_With_NonMatchingSerial_MatchingBatch()
      {
         //Test data
         var product = new Product { ProductId = 123 };
         var adjustment = new ItemAdjustment()
         {
            ProductId = 123,
            Quantity = 1,
            SerialNumber = "Serial",
            BatchNumber = "Batch",
            StockLocationId = 1
         };
         var stock = new Stock
         {
            ProductId = 123,
            Quantity = 1,
            BatchNumber = "Batch",
            SerialNumber = "STOCK SERIAL",
            StoredAt = 1
         };

         var testData = new MockedInventoryStockData(new List<Product> { product }, new List<Stock> { stock });

         //SUT
         var deductionHelper = GetStockDeductionHelper(testData);

         //Test
         ExceptionAssert.Throws<AmbiguousStockException>(
            () => deductionHelper.AdjustItem(adjustment, product, "UnitTest"));

         //Verify
         testData.MockedStock.Verify(c => c.Add(It.IsAny<Stock>()), Times.Never, "Not added to stock");
         Assert.AreEqual(1, stock.Quantity, "Verify stock level is not affected");
         Assert.AreEqual("Batch", stock.BatchNumber, "Verify batch is not affected");
         Assert.AreEqual("STOCK SERIAL", stock.SerialNumber, "Verify serial is not affected");
         testData.MockedAdjustments.Verify(m => m.Add(It.IsAny<StockAdjustment>()), Times.Never(),
            "No adjustment recorded");
      }

      [TestMethod]
      public void Consume_HasSerial_HasBatch_Stock_With_NonMatchingSerial_NoBatch()
      {
         //Test data
         var product = new Product { ProductId = 123 };
         var adjustment = new ItemAdjustment()
         {
            ProductId = 123,
            Quantity = 1,
            SerialNumber = "Serial",
            BatchNumber = "Batch",
            StockLocationId = 1
         };
         var stock = new Stock { ProductId = 123, Quantity = 1, SerialNumber = "STOCK SERIAL", StoredAt = 1 };

         var testData = new MockedInventoryStockData(new List<Product> { product }, new List<Stock> { stock });

         //SUT
         var deductionHelper = GetStockDeductionHelper(testData);

         //Test
         ExceptionAssert.Throws<AmbiguousStockException>(
            () => deductionHelper.AdjustItem(adjustment, product, "UnitTest"));

         //Verify
         testData.MockedStock.Verify(c => c.Add(It.IsAny<Stock>()), Times.Never, "Not added to stock");
         Assert.AreEqual(1, stock.Quantity, "Verify stock level is not affected");
         Assert.IsNull(stock.BatchNumber, "Verify batch is not affected");
         Assert.AreEqual("STOCK SERIAL", stock.SerialNumber, "Verify serial is not affected");
         testData.MockedAdjustments.Verify(m => m.Add(It.IsAny<StockAdjustment>()), Times.Never(),
            "No adjustment recorded");
      }

      [TestMethod]
      public void Consume_HasSerial_HasBatch_NoStock_CreatesNegativeStock()
      {
         //Test data
         var product = new Product { ProductId = 123 };
         var adjustment = new ItemAdjustment()
         {
            ProductId = 123,
            Quantity = 1,
            SerialNumber = "Serial",
            BatchNumber = "Batch",
            StockLocationId = 1
         };

         var testData = new MockedInventoryStockData(new List<Product> { product }, new List<Stock>());

         //SUT
         var deductionHelper = GetStockDeductionHelper(testData);

         //Test
         deductionHelper.AdjustItem(adjustment, product, "UnitTest");

         Assert.AreEqual(1, product.Stocks.Count(), "Addition of negative stock entity to product");
         Assert.AreEqual(true, product.Stocks.First().IsNegative, "Stock is created as Negative Stock");
         Assert.AreEqual(1, product.Stocks.First().ReceivedQuantity, "Received quantity");
         Assert.AreEqual(0, product.Stocks.First().Quantity, "Available stock");
         Assert.AreEqual(adjustment.SerialNumber, product.Stocks.First().SerialNumber,
            "Serial number added to negative stock");
         Assert.AreEqual(adjustment.BatchNumber, product.Stocks.First().BatchNumber,
            "Batch number added to negative stock");
         Assert.AreEqual(StockStatus.Deducted, product.Stocks.First().StockStatus, "Stock has been deducted");

         //Verify
         testData.MockedAdjustments.Verify(m => m.Add(It.Is<StockAdjustment>(c => c.Quantity == 1)), Times.Once(),
            "adjustment recorded");
      }

      #endregion
   }
}