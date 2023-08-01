using System.Collections.Generic;
using System.Linq;
using HMS.HealthTrack.Inventory.Test.Inventory.Mocks;
using HMS.HealthTrack.Web.Data.Exceptions;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MSTestExtensions;

namespace HMS.HealthTrack.Inventory.Test.Inventory
{
   [TestClass]
   public class AdjustReceivedStock
   {
      private const int LocationId = 6;
      private const string Username = "UnitTest";

      private Stock Stock
      {
         get
         {
            return new Stock
            {
               IsNegative = false,
               Quantity = 8,
               ReceivedQuantity = 10,
               ProductId = 123,
               StockId = 1,
               StockStatus = StockStatus.Available,
               StoredAt = LocationId,
            };
         }
      }

      private StockAdjustment ReceiptAdjustment
      {
         get
         {
            return new StockAdjustment
            {
               IsPositive = true,
               OrderItemId = 2,
               Quantity = 10,
               Source = AdjustmentSource.Order,
               StockAdjustmentStocks = new List<StockAdjustmentStock>
               {
                  StockAdjustmentStock
               }
            };
         }
      }

      public StockAdjustmentStock StockAdjustmentStock
      {
         get
         {
            return new StockAdjustmentStock
            {
               Stock = Stock,
            };
         }
      }

      [TestMethod]
      public void NoChangeInQuantity()
      {
         var newQuantity = ReceiptAdjustment.Quantity;
         var receipt = ReceiptAdjustment;

         var testData = new MockedStockAdjustmentData();
         var uow = MockedInventoryUnitOfWorkFactory.GetStockAdjustmentUnitOfWork(testData);
         uow.AdjustReceivedStock(receipt, newQuantity, Username);

         // stock unchanged
         Assert.AreEqual(Stock.Quantity, receipt.StockAdjustmentStocks.First().Stock.Quantity);
         Assert.AreEqual(Stock.ReceivedQuantity, receipt.StockAdjustmentStocks.First().Stock.ReceivedQuantity);

         // no stock added
         testData.MockedStock.Verify(ms => ms.Add(It.IsAny<Stock>()), Times.Never);
      }

      [TestMethod]
      public void AddNewStock()
      {
         var extraStock = 2;
         var newQuantity = ReceiptAdjustment.Quantity + extraStock;
         var receipt = ReceiptAdjustment;

         var testData = new MockedStockAdjustmentData();
         var uow = MockedInventoryUnitOfWorkFactory.GetStockAdjustmentUnitOfWork(testData);
         uow.AdjustReceivedStock(receipt, newQuantity, Username);

         // stock unchanged
         Assert.AreEqual(Stock.Quantity, receipt.StockAdjustmentStocks.First().Stock.Quantity);
         Assert.AreEqual(Stock.ReceivedQuantity, receipt.StockAdjustmentStocks.First().Stock.ReceivedQuantity);

         // new stock added
         Assert.AreEqual(2, receipt.StockAdjustmentStocks.Count());
         var newStock = receipt.StockAdjustmentStocks.Single(s => s.Stock.StockId == 0).Stock;
         Assert.AreEqual(Stock.ProductId, newStock.ProductId);
         Assert.AreEqual(extraStock, newStock.Quantity);
         Assert.AreEqual(extraStock, newStock.ReceivedQuantity);
         Assert.AreEqual(StockStatus.Available, newStock.StockStatus);
         Assert.AreEqual(LocationId, newStock.StoredAt);

         Assert.AreEqual(newQuantity, receipt.Quantity);
      }

      [TestMethod]
      public void DecreaseStock_NoStockAvailable()
      {
         var lessStock = 4;
         var newQuantity = Stock.Quantity - lessStock;
         var receipt = ReceiptAdjustment;
         receipt.StockAdjustmentStocks.First().Stock.Quantity = 0;
         receipt.StockAdjustmentStocks.First().Stock.StockStatus = StockStatus.Deducted;

         var testData = new MockedStockAdjustmentData();
         var uow = MockedInventoryUnitOfWorkFactory.GetStockAdjustmentUnitOfWork(testData);
         ExceptionAssert.Throws<StockException>(() => uow.AdjustReceivedStock(receipt, newQuantity, Username));

         // stock unchanged
         Assert.AreEqual(0, receipt.StockAdjustmentStocks.First().Stock.Quantity);
         Assert.AreEqual(Stock.ReceivedQuantity, receipt.StockAdjustmentStocks.First().Stock.ReceivedQuantity);
      }

      [TestMethod]
      public void DecreaseStock_NotEnoughStockAvailable()
      {
         var newQuantity = Stock.Quantity - Stock.Quantity + 1;
         var receipt = ReceiptAdjustment;
         receipt.StockAdjustmentStocks.First().Stock.Quantity = 0;
         receipt.StockAdjustmentStocks.First().Stock.StockStatus = StockStatus.Deducted;

         var testData = new MockedStockAdjustmentData();
         var uow = MockedInventoryUnitOfWorkFactory.GetStockAdjustmentUnitOfWork(testData);
         ExceptionAssert.Throws<StockException>(() => uow.AdjustReceivedStock(receipt, newQuantity, Username));

         // stock unchanged
         Assert.AreEqual(0, receipt.StockAdjustmentStocks.First().Stock.Quantity);
         Assert.AreEqual(Stock.ReceivedQuantity, receipt.StockAdjustmentStocks.First().Stock.ReceivedQuantity);
      }

      [TestMethod]
      public void DecreaseStock_DecreasesAvailableStock()
      {
         var lessStock = 4;
         var newQuantity = ReceiptAdjustment.Quantity - lessStock;
         var receipt = ReceiptAdjustment;

         var testData = new MockedStockAdjustmentData();
         var uow = MockedInventoryUnitOfWorkFactory.GetStockAdjustmentUnitOfWork(testData);
         uow.AdjustReceivedStock(receipt, newQuantity, Username);

         Assert.AreEqual(newQuantity, receipt.Quantity, "Adjustment quantity");
         Assert.AreEqual(Stock.Quantity - lessStock, receipt.StockAdjustmentStocks.First().Stock.Quantity,
            "stock quantity");
         Assert.AreEqual(Stock.ReceivedQuantity - lessStock,
            receipt.StockAdjustmentStocks.First().Stock.ReceivedQuantity, "stock received quantity");
      }

      [TestMethod]
      public void DecreaseStock_IgnoresSerialStock_DecreasesAvailableStock()
      {
         var lessStock = 4;
         var newQuantity = ReceiptAdjustment.Quantity - lessStock;
         var receipt = ReceiptAdjustment;
         var secondStock = Stock;

         secondStock.Quantity = 1;
         secondStock.ReceivedQuantity = 1;
         secondStock.SerialNumber = "SERIAL";

         receipt.StockAdjustmentStocks.First().Stock.Quantity -= 1;
         receipt.StockAdjustmentStocks.First().Stock.ReceivedQuantity -= 1;

         receipt.StockAdjustmentStocks.Add(new StockAdjustmentStock
         {
            Stock = secondStock
         });

         var testData = new MockedStockAdjustmentData();
         var uow = MockedInventoryUnitOfWorkFactory.GetStockAdjustmentUnitOfWork(testData);
         uow.AdjustReceivedStock(receipt, newQuantity, Username);

         Assert.AreEqual(newQuantity, receipt.Quantity, "Adjustment quantity");
         Assert.AreEqual(Stock.Quantity - lessStock - 1, receipt.StockAdjustmentStocks.First().Stock.Quantity,
            "stock quantity");
         Assert.AreEqual(Stock.ReceivedQuantity - lessStock - 1,
            receipt.StockAdjustmentStocks.First().Stock.ReceivedQuantity, "stock received quantity");
      }

      [TestMethod]
      public void DecreaseStock_IgnoresBatchStock_DecreasesAvailableStock()
      {
         var lessStock = 4;
         var newQuantity = ReceiptAdjustment.Quantity - lessStock;
         var receipt = ReceiptAdjustment;
         var secondStock = Stock;
         secondStock.BatchNumber = "BATCH";
         secondStock.Quantity = 2;
         secondStock.ReceivedQuantity = 2;

         receipt.StockAdjustmentStocks.First().Stock.Quantity -= 2;
         receipt.StockAdjustmentStocks.First().Stock.ReceivedQuantity -= 2;

         receipt.StockAdjustmentStocks.Add(new StockAdjustmentStock
         {
            Stock = secondStock
         });

         var testData = new MockedStockAdjustmentData();
         var uow = MockedInventoryUnitOfWorkFactory.GetStockAdjustmentUnitOfWork(testData);
         uow.AdjustReceivedStock(receipt, newQuantity, Username);

         Assert.AreEqual(newQuantity, receipt.Quantity, "Adjustment quantity");
         Assert.AreEqual(Stock.Quantity - lessStock - 2, receipt.StockAdjustmentStocks.First().Stock.Quantity,
            "stock quantity");
         Assert.AreEqual(Stock.ReceivedQuantity - lessStock - 2,
            receipt.StockAdjustmentStocks.First().Stock.ReceivedQuantity, "stock received quantity");
      }

      [TestMethod]
      public void DecreaseStock_DecreasesAvailableStock_SubtractAllStock()
      {
         var lessStock = Stock.Quantity;
         var newQuantity = ReceiptAdjustment.Quantity - lessStock;
         var receipt = ReceiptAdjustment;

         var testData = new MockedStockAdjustmentData();
         var uow = MockedInventoryUnitOfWorkFactory.GetStockAdjustmentUnitOfWork(testData);
         uow.AdjustReceivedStock(receipt, newQuantity, Username);

         Assert.AreEqual(newQuantity, receipt.Quantity, "Adjustment quantity");
         Assert.AreEqual(Stock.Quantity - lessStock, receipt.StockAdjustmentStocks.First().Stock.Quantity,
            "stock quantity");
         Assert.AreEqual(Stock.ReceivedQuantity - lessStock,
            receipt.StockAdjustmentStocks.First().Stock.ReceivedQuantity, "stock received quantity");
      }

      [TestMethod]
      public void DecreaseStock_DecreasesAvailableStock_SubtractAllReceivedStock()
      {
         var newQuantity = 0;
         var receipt = ReceiptAdjustment;
         receipt.StockAdjustmentStocks.First().Stock.Quantity = receipt.Quantity;

         var testData = new MockedStockAdjustmentData();
         var uow = MockedInventoryUnitOfWorkFactory.GetStockAdjustmentUnitOfWork(testData);
         uow.AdjustReceivedStock(receipt, newQuantity, Username);

         Assert.AreEqual(newQuantity, receipt.Quantity, "Adjustment quantity");
         Assert.AreEqual(newQuantity, receipt.StockAdjustmentStocks.First().Stock.Quantity, "stock quantity");
         Assert.AreEqual(newQuantity, receipt.StockAdjustmentStocks.First().Stock.ReceivedQuantity,
            "stock received quantity");
      }
   }
}