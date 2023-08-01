using System;
using System.Collections.Generic;
using System.Linq;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Inventory.Common.Constants;
using HMS.HealthTrack.Inventory.Test.Inventory.Mocks;
using HMS.HealthTrack.Web.Data.Exceptions;
using HMS.HealthTrack.Web.Data.Helpers;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MSTestExtensions;

namespace HMS.HealthTrack.Inventory.Test.Inventory
{
   [TestClass]
   public class ReceiveStockTests
   {
      private const string Username = "UnitTest";
      private const int LocationId = 1;
      private const string SerialNumber = "SerialNumber";
      private const string BatchNumber = "BatchNumber";

      private Product Product
      {
         get
         {
            return new Product
            {
               ManageStock = true,
               PriceModelId = 6,
               Prices = new List<ProductPrice>
               {
                 new ProductPrice
                 {
                    ProductId = 123,
                    PriceTypeId = 1,
                    BuyPrice = 42,
                 } 
               },
               ProductId = 123,
               ProductSettings = new List<StockSetting>(),
            };
         }
      }

      private ItemAdjustment TestAdjustment
      {
         get
         {
            return StockAdjustmentHelper.CreateItemAdjustment(Product.ProductId,
               LocationId, 6, true, AdjustmentSource.Web, Username);
         }
      }

      [TestMethod]
      public void Receive_Single_Item()
      {
         //Arrange
         var testData = new MockedInventoryStockData(new List<Product> { Product }, new List<Stock>());
         var uow = MockedInventoryUnitOfWorkFactory.GetStockUnitOfWork(testData);

         var adjustment = TestAdjustment;
         uow.ReceiveNewStock(adjustment, null, Username);

         //Verify
         testData.MockedStock.Verify(c => c.Add(It.IsAny<Stock>()), Times.Once());
      }

      [TestMethod]
      public void Receive_Single_Item_ManageStockFalse()
      {
         var product = Product;
         product.ManageStock = false;

         //Arrange
         var testData = new MockedInventoryStockData(new List<Product> { product }, new List<Stock>());
         var uow = MockedInventoryUnitOfWorkFactory.GetStockUnitOfWork(testData);

         var adjustment = TestAdjustment;
         ExceptionAssert.Throws<StockException>(() => uow.ReceiveNewStock(adjustment, null, Username));

         //Verify
         testData.MockedAdjustments.Verify(c => c.Add(It.IsAny<StockAdjustment>()), Times.Never);
         testData.MockedStock.Verify(c => c.Add(It.IsAny<Stock>()), Times.Never);
      }

      [TestMethod]
      public void Receive_Single_Item_ExpirySet()
      {
         //Arrange
         var testData = new MockedInventoryStockData(new List<Product> { Product }, new List<Stock>());
         var uow = MockedInventoryUnitOfWorkFactory.GetStockUnitOfWork(testData);

         var adjustment = TestAdjustment;
         uow.ReceiveNewStock(adjustment, DateTime.Now.AddDays(7), Username);

         //Verify
         testData.MockedStock.Verify(c => c.Add(It.IsAny<Stock>()), Times.Once());
         testData.MockedStock.Verify(c => c.Add(It.Is<Stock>(s => s.ExpiresOn != null)), Times.Once());
      }

      [TestMethod]
      public void Receive_Single_Item_TaxRateSet()
      {
         //Arrange
         var testData = new MockedInventoryStockData(new List<Product> { Product }, new List<Stock>());
         var uow = MockedInventoryUnitOfWorkFactory.GetStockUnitOfWork(testData);

         var adjustment = TestAdjustment;
         uow.ReceiveNewStock(adjustment, null, Username);

         //Verify
         testData.MockedStock.Verify(c => c.Add(It.IsAny<Stock>()), Times.Once());
         testData.MockedStock.Verify(c => c.Add(It.Is<Stock>(s => s.TaxRateOnReceipt != null)), Times.Once());
      }

      [TestMethod]
      [ExpectedException(typeof(StockException))]
      public void Receive_ZeroQuantityItem()
      {
         var testData = new MockedInventoryStockData(new List<Product> { Product }, new List<Stock>());
         var uow = MockedInventoryUnitOfWorkFactory.GetStockUnitOfWork(testData);

         var adjustment = TestAdjustment;
         adjustment.Quantity = 0;
         //Act
         uow.ReceiveNewStock(adjustment, null, Username);
      }

      [TestMethod]
      public void Receive_NegativeQuantityItem()
      {
         var testData = new MockedInventoryStockData(new List<Product> { Product }, new List<Stock>());
         var uow = MockedInventoryUnitOfWorkFactory.GetStockUnitOfWork(testData);

         var adjustment = TestAdjustment;
         adjustment.Quantity = -1;

         ExceptionAssert.Throws<StockException>(() => uow.ReceiveNewStock(adjustment, null, Username));

         //Verify
         testData.MockedStock.Verify(c => c.Add(It.IsAny<Stock>()), Times.Never);
      }

      [TestMethod]
      public void Receive_Multiple_Items()
      {
         //Arrange
         var testData = new MockedInventoryStockData(new List<Product> { Product }, new List<Stock>());
         var uow = MockedInventoryUnitOfWorkFactory.GetStockUnitOfWork(testData);

         var adjustment = TestAdjustment;
         adjustment.Quantity = 100;

         uow.ReceiveNewStock(adjustment, null, Username);

         //Verify
         testData.MockedStock.Verify(c => c.Add(It.IsAny<Stock>()), Times.Once());
      }

      [TestMethod]
      public void Receive_Item_Check_Status()
      {
         //Arrange
         var testData = new MockedInventoryStockData(new List<Product> { Product }, new List<Stock>());
         var uow = MockedInventoryUnitOfWorkFactory.GetStockUnitOfWork(testData);

         uow.ReceiveNewStock(TestAdjustment, null, Username);

         //Verify
         testData.MockedStock.Verify(c => c.Add(It.IsAny<Stock>()), Times.Once());
         testData.MockedStock.Verify(c => c.Add(It.Is<Stock>(s => s.StockStatus == StockStatus.Available)), Times.Once());
      }

      [TestMethod]
      public void Receive_Stock_With_NonExistant_Product()
      {
         //Arrange
         var testData = new MockedInventoryStockData();
         var uow = MockedInventoryUnitOfWorkFactory.GetStockUnitOfWork(testData);

         ExceptionAssert.Throws<StockException>(() => uow.ReceiveNewStock(TestAdjustment, null, Username));

         //Verify
         testData.MockedStock.Verify(c => c.Add(It.IsAny<Stock>()), Times.Never());
         testData.MockedProducts.Verify(p => p.Add(It.IsAny<Product>()), Times.Never());
      }

      [TestMethod]
      public void Receive_Stock_With_Existing_Product()
      {
         var testData = new MockedInventoryStockData(new List<Product> { Product }, new List<Stock>());
         var uow = MockedInventoryUnitOfWorkFactory.GetStockUnitOfWork(testData);

         uow.ReceiveNewStock(TestAdjustment, null, Username);

         //Verify
         testData.MockedStock.Verify(c => c.Add(It.IsAny<Stock>()), Times.Once());
         testData.MockedProducts.Verify(p => p.Add(It.IsAny<Product>()), Times.Never());
      }

      [TestMethod]
      public void Receive_Stock_With_SerialNumber_Qty_Check()
      {
         var testData = new MockedInventoryStockData(new List<Product> { Product }, new List<Stock>());
         var uow = MockedInventoryUnitOfWorkFactory.GetStockUnitOfWork(testData);

         var adjustment = TestAdjustment;
         adjustment.Quantity = 1;
         adjustment.SerialNumber = SerialNumber;

         uow.ReceiveNewStock(adjustment, null, Username);

         //Verify
         testData.MockedStock.Verify(c => c.Add(It.IsAny<Stock>()), Times.Once());
      }

      [TestMethod]
      public void Receive_Stock_With_SerialNumber_And_Bad_Qty()
      {
         //Arrange
         var testData = new MockedInventoryStockData(new List<Product> { Product }, new List<Stock>());
         var uow = MockedInventoryUnitOfWorkFactory.GetStockUnitOfWork(testData);

         var adjustment = TestAdjustment;
         adjustment.SerialNumber = SerialNumber;

         ExceptionAssert.Throws<StockException>(() => uow.ReceiveNewStock(adjustment, null, Username));

         //Verify
         testData.MockedStock.Verify(c => c.Add(It.IsAny<Stock>()), Times.Never());
      }


      [TestMethod]
      public void Receive_Stock_With_Price_Model()
      {
         //Arrange
         var testData = new MockedInventoryStockData(new List<Product> { Product }, new List<Stock>());
         var uow = MockedInventoryUnitOfWorkFactory.GetStockUnitOfWork(testData);

         uow.ReceiveNewStock(TestAdjustment, null, Username);

         //Verify
         testData.MockedStock.Verify(c => c.Add(It.IsAny<Stock>()), Times.Once());
         testData.MockedStock.Verify(c => c.Add(It.Is<Stock>(s => s.PriceModelOnReceipt == Product.PriceModelId)), Times.Once());
      }

      [TestMethod]
      public void Receive_SerialRequired_Product()
      {
         //Arrange
         var product = Product;
         product.ProductSettings.Add(new StockSetting { SettingId = InventoryConstants.StockSettings.RequiresSerialNumber });
         var testData = new MockedInventoryStockData(new List<Product> { Product }, new List<Stock>());
         var uow = MockedInventoryUnitOfWorkFactory.GetStockUnitOfWork(testData);

         var adjustment = TestAdjustment;
         adjustment.Quantity = 1;
         adjustment.SerialNumber = SerialNumber;

         uow.ReceiveNewStock(adjustment, null, Username);

         //Verify
         testData.MockedStock.Verify(c => c.Add(It.IsAny<Stock>()), Times.Once());
         testData.MockedStock.Verify(c => c.Add(It.Is<Stock>(s => s.SerialNumber == SerialNumber)), Times.Once());
      }

      [TestMethod]
      public void Receive_BatchRequired_Product()
      {
         var product = Product;
         product.ProductSettings.Add(new StockSetting { SettingId = InventoryConstants.StockSettings.RequiresBatchNumber });
         var testData = new MockedInventoryStockData(new List<Product> { Product }, new List<Stock>());
         var uow = MockedInventoryUnitOfWorkFactory.GetStockUnitOfWork(testData);

         var adjustment = TestAdjustment;
         adjustment.Quantity = 10;
         adjustment.BatchNumber = BatchNumber;

         uow.ReceiveNewStock(adjustment, null, Username);

         //Verify
         testData.MockedStock.Verify(c => c.Add(It.IsAny<Stock>()), Times.Once());
         testData.MockedStock.Verify(c => c.Add(It.Is<Stock>(s => s.BatchNumber == BatchNumber)), Times.Once());
      }

      [TestMethod]
      public void Receive_BatchRequired_Product_Without_BatchNumber()
      {
         var product = Product;
         product.ProductSettings.Add(new StockSetting { SettingId = InventoryConstants.StockSettings.RequiresBatchNumber });

         var testData = new MockedInventoryStockData(new List<Product> { product }, new List<Stock>());
         var uow = MockedInventoryUnitOfWorkFactory.GetStockUnitOfWork(testData);

         //Should allow you to receive it but not consume without a batch
         uow.ReceiveNewStock(TestAdjustment, null, Username);
         testData.MockedStock.Verify(c => c.Add(It.IsAny<Stock>()), Times.Once());
      }

      // Test receive order item closed requests
      [TestMethod]
      public void Receive_OrderItem()
      {
         //Arrange
         var request = new ProductStockRequest
         {
            StockRequestId = 1,
            ProductId = 123,
            RequestedQuantity = 1,
            RequestStatus = RequestStatus.Ordered
         };

         var orderItem = new OrderItem
         {
            OrderItemId = 1,
            ProductId = 123,
            Product = new Product
            {
               ManageStock = true,
               ProductId = 123,
            },
            Quantity = 2,
         };
         var requestSource = new OrderItemSource
         {
            OrderItem = orderItem,
            OrderItemId = 1,
            ProductStockRequest = request,
            StockRequestId = 1,
            Quantity = 1
         };

         orderItem.OrderItemSources.Add(requestSource);

         var mockedRequests = MockingHelper.GetMockedDbSet(new List<ProductStockRequest> { request }.AsQueryable());
         var mockedOrderItems = MockingHelper.GetMockedDbSet(new List<OrderItem> { orderItem }.AsQueryable());
         var mockedOrderItemSources = MockingHelper.GetMockedDbSet(new List<OrderItemSource>().AsQueryable());

         var mockedContext = MockingHelper.GetMockedContext<ProductStockRequest, InventoryContext>(mockedRequests, context => context.ProductStockRequests);
         mockedContext.Setup(c => c.OrderItems).Returns(mockedOrderItems.Object);
         mockedContext.Setup(c => c.OrderItemSources).Returns(mockedOrderItemSources.Object);

         var mockedPropertyProvider = new Mock<IPropertyProvider>();
         var mockedLogger = new Mock<ICustomLogger>();

         //SUT
         var stockUoW = new StockUnitOfWork(mockedContext.Object, mockedPropertyProvider.Object, mockedLogger.Object);

         //Act
         stockUoW.ReceiveOrderItem(orderItem, "Unit Test");

         // Check order item is Received
         Assert.AreEqual(OrderItemStatus.Received, orderItem.Status);
         // Check associated requests are closed
         Assert.AreEqual(RequestStatus.Closed, request.RequestStatus, "Close request");
      }

      [TestMethod]
      public void ReceiveDuplicateSerialNumberForProduct_AvailableStock()
      {
         var existingStock = new Stock
         {
            ProductId = 123,
            Quantity = 1,
            ReceivedQuantity = 1,
            SerialNumber = SerialNumber,
            StockStatus = StockStatus.Available,
            StoredAt = LocationId
         };

         var testData = new MockedInventoryStockData(new List<Product>(), new List<Stock> { existingStock });
         var uow = MockedInventoryUnitOfWorkFactory.GetStockUnitOfWork(testData);

         var adjustment = TestAdjustment;
         adjustment.SerialNumber = SerialNumber;

         ExceptionAssert.Throws<StockException>(() => uow.ReceiveNewStock(adjustment, null, Username));

         testData.MockedStock.Verify(c => c.Add(It.IsAny<Stock>()), Times.Never, "Not added stock");
      }

      [TestMethod]
      public void ReceiveDuplicateSerialNumberForProduct_ConsumedStock()
      {
         var deductedStock = new Stock
         {
            ProductId = Product.ProductId,
            Quantity = 0,
            ReceivedQuantity = 1,
            SerialNumber = SerialNumber,
            StockStatus = StockStatus.Deducted,
            StoredAt = LocationId
         };

         var testData = new MockedInventoryStockData(new List<Product> { Product }, new List<Stock> { deductedStock });
         var uow = MockedInventoryUnitOfWorkFactory.GetStockUnitOfWork(testData);

         var adjustment = TestAdjustment;
         adjustment.Quantity = 1;
         adjustment.SerialNumber = SerialNumber;

         uow.ReceiveNewStock(adjustment, null, Username);

         testData.MockedStock.Verify(c => c.Add(It.IsAny<Stock>()), Times.Once);
      }
   }
}