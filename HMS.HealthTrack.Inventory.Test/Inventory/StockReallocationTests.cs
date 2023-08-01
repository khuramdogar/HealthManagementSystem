using System;
using HMS.HealthTrack.Inventory.Test.Inventory.Mocks;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace HMS.HealthTrack.Inventory.Test.Inventory
{
   [TestClass]
   public class StockReallocationTests
   {
      private const int InitialQuantity = 10;
      private const int Quantity = 5;
      private const int OldLocation = 1;

      private readonly ItemAdjustment _itemAdjustment = new ItemAdjustment();


      private readonly StockLocation _newLocation = new StockLocation { LocationId = 2 };

      [TestMethod]
      public void SplitStock_NewStockCorrectValues()
      {
         var stock = new Stock
         {
            ProductId = 666,
            ReceivedQuantity = InitialQuantity,
            StockId = 123,
            Quantity = InitialQuantity,
         };

         var itemAdjustment = new ItemAdjustment();

         var newStock = StockAdjustmentUnitOfWork.SplitStock(stock, itemAdjustment, Quantity);

         Assert.AreEqual(InitialQuantity - Quantity, stock.ReceivedQuantity);
         Assert.AreEqual(InitialQuantity - Quantity, stock.Quantity);

         Assert.AreEqual(Quantity, newStock.ReceivedQuantity);
         Assert.AreEqual(Quantity, newStock.Quantity);

         Assert.AreEqual(stock.ProductId, newStock.ProductId);

         Assert.AreNotEqual(stock.StockId, newStock.StockId);
      }

      [TestMethod]
      public void ReallocateStockToLocation()
      {
         var stock = new Stock
         {
            ProductId = 666,
            ReceivedQuantity = InitialQuantity,
            StoredAt = OldLocation,
            Quantity = InitialQuantity,
         };

         var testData = new MockedInventoryStockData();
         var uow = MockedInventoryUnitOfWorkFactory.GetStockUnitOfWork(testData);

         var result = uow.ReallocateStock(stock, _newLocation, null, "test");

         Assert.IsTrue(result);
         Assert.AreEqual(_newLocation.LocationId, stock.StoredAt);
      }

      [TestMethod]
      public void ReallocateQuantityLargerThanStored()
      {
         var stock = new Stock
         {
            ProductId = 666,
            ReceivedQuantity = InitialQuantity,
            StoredAt = OldLocation,
            Quantity = InitialQuantity,
         };

         var testData = new MockedInventoryStockData();
         var uow = MockedInventoryUnitOfWorkFactory.GetStockUnitOfWork(testData);

         var result = uow.ReallocateStock(stock, _newLocation, InitialQuantity + 1, "test");
         Assert.IsFalse(result);
         Assert.AreEqual(InitialQuantity, stock.Quantity);
         Assert.AreEqual(InitialQuantity, stock.ReceivedQuantity);
         Assert.AreEqual(OldLocation, stock.StoredAt);
      }

      [TestMethod]
      public void PartialReallocate_NewValuesCorrect()
      {
         var stock = new Stock
         {
            ProductId = 666,
            ReceivedQuantity = InitialQuantity,
            StoredAt = OldLocation,
            Quantity = InitialQuantity,
         };

         var testData = new MockedInventoryStockData();
         var uow = MockedInventoryUnitOfWorkFactory.GetStockUnitOfWork(testData);

         var result = uow.ReallocateStock(stock, _newLocation, Quantity, "test");

         testData.MockedStock.Verify(c => c.Add(It.IsAny<Stock>()), Times.Once);

         Assert.IsTrue(result);
         Assert.AreEqual(InitialQuantity - Quantity, stock.Quantity);
         Assert.AreEqual(InitialQuantity - Quantity, stock.ReceivedQuantity);
         Assert.AreEqual(OldLocation, stock.StoredAt);
      }

      [TestMethod]
      public void ReallocateReservedStock()
      {
         var stock = new Stock
         {
            ProductId = 666,
            ReceivedQuantity = InitialQuantity,
            StoredAt = OldLocation,
            StockStatus = StockStatus.Reserved,
            Quantity = InitialQuantity,
         };
         var testData = new MockedInventoryStockData();
         var uow = MockedInventoryUnitOfWorkFactory.GetStockUnitOfWork(testData);

         var result = uow.ReallocateStock(stock, _newLocation, null, "test");

         Assert.IsTrue(result);
      }

      [TestMethod]
      public void ReallocateUnavailableStock_Fails()
      {
         var stock = new Stock
         {
            ProductId = 666,
            ReceivedQuantity = InitialQuantity,
            StoredAt = OldLocation,
            StockStatus = StockStatus.Deducted,
            Quantity = InitialQuantity,
         };
         var testData = new MockedInventoryStockData();
         var uow = MockedInventoryUnitOfWorkFactory.GetStockUnitOfWork(testData);

         var result = uow.ReallocateStock(stock, _newLocation, null, "test");

         Assert.IsFalse(result);
      }

      [TestMethod]
      public void ReallocateDeletedStock_Fails()
      {
         var stock = new Stock
         {
            DeletedOn = DateTime.Now,
            ProductId = 666,
            ReceivedQuantity = InitialQuantity,
            StoredAt = OldLocation,
            StockStatus = StockStatus.Available,
            Quantity = InitialQuantity,
         };
         var testData = new MockedInventoryStockData();
         var uow = MockedInventoryUnitOfWorkFactory.GetStockUnitOfWork(testData);

         var result = uow.ReallocateStock(stock, _newLocation, null, "test");

         Assert.IsFalse(result);
      }

      [TestMethod]
      public void ReallocateZeroQuantityStock_Fails()
      {
         var stock = new Stock
         {
            DeletedOn = DateTime.Now,
            ProductId = 666,
            ReceivedQuantity = InitialQuantity,
            StoredAt = OldLocation,
            StockStatus = StockStatus.Available,
            Quantity = 0,
         };
         var testData = new MockedInventoryStockData();
         var uow = MockedInventoryUnitOfWorkFactory.GetStockUnitOfWork(testData);

         var result = uow.ReallocateStock(stock, _newLocation, null, "test");

         Assert.IsFalse(result);
      }
   }
}
