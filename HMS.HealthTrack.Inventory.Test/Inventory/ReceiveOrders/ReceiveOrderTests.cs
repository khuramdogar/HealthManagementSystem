using System.Collections.Generic;
using System.Linq;
using HMS.HealthTrack.Inventory.Test.Inventory.Mocks;
using HMS.HealthTrack.Web.Data.Exceptions;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MSTestExtensions;

namespace HMS.HealthTrack.Inventory.Test.Inventory.ReceiveOrders
{
   [TestClass]
   public class ReceiveOrderTests
   {
      #region Test Variables
      private IEnumerable<Product> TestProducts
      {
         get
         {
            return new List<Product>
            {
               TestProduct
            };
         }
      }

      private IEnumerable<OrderItemSource> TestOrderItemSources
      {
         get
         {
            return new List<OrderItemSource>
            {
               new OrderItemSource
               {
                  OrderItemId = 1,
                  StockRequestId = 1,
                  ProductStockRequest = TestRequest,
                  Quantity = 1
               }
            };
         }
      }

      private ProductStockRequest TestRequest
      {
         get
         {
            return new ProductStockRequest
            {
               ApprovedQuantity = 1,
               RequestedQuantity = 1,
               RequestStatus = RequestStatus.Ordered,
               RequestLocationId = DeliveryLocationId,
               StockRequestId = 1
            };
         }
      }

      private IEnumerable<Order> TestOrders
      {
         get
         {
            return new List<Order>
            {
               new Order
               {
                  Items = TestOrderItems.ToList()
               }
            };
         }
      }

      private IEnumerable<OrderItem> TestOrderItems
      {
         get
         {
            return new List<OrderItem>
            {
               TestOrderItem
            };
         }
      }

      private IEnumerable<Stock> TestStocks
      {
         get { return new List<Stock>(); }
      }

      private Product TestProduct
      {
         get
         {
            return new Product
            {
               ManageStock = true,
               ProductId = 123,
               Prices = new List<ProductPrice> { new ProductPrice { PriceType = new PriceType { PriceTypeId = 1 } } },
               PriceModelId = 4,
            };
         }
      }

      private OrderItem TestOrderItem
      {
         get
         {
            return new OrderItem
            {
               Order = new Order
               {
                  Status = OrderStatus.Ordered
               },
               OrderItemId = 1,
               OrderItemSources = TestOrderItemSources.ToList(),
               Product = TestProduct,
               ProductId = 123,
               Quantity = 500,
               Status = OrderItemStatus.Ordered
            };
         }
      }

      public const int DeliveryLocationId = 6;

      #endregion

      [TestMethod]
      public void ReceiveOrder_CreatesAdjustmentForOrder()
      {
         var testData = new MockedInventoryOrderItemData();
         var stockUoW = MockedInventoryUnitOfWorkFactory.GetStockUnitOfWork(testData);

         var dto = new ProcessOrderDTO()
         {
            OrderItemId = 1,
            Action = OrderItemAction.Complete,
            Quantity = 1,
         };

         var testOrderItem = TestOrderItem;

         //Act
         stockUoW.ProcessOrderItem(testOrderItem, dto, "Test", DeliveryLocationId);

         testData.MockedStock.Verify(s => s.Add(It.IsAny<Stock>()), Times.Once());
         testData.MockedStock.Verify(s => s.Add(It.Is<Stock>(ns => ns.Quantity == 1)));
         testData.MockedStock.Verify(s => s.Add(It.Is<Stock>(ns => ns.StockStatus == StockStatus.Available)));

         testData.MockedStock.Verify(s => s.Add(It.Is<Stock>(ns => ns.StockAdjustmentStocks.Count() == 1)));
         testData.MockedStock.Verify(s => s.Add(It.Is<Stock>(ns => ns.StockAdjustmentStocks.First().StockAdjustment.Source == AdjustmentSource.Order)));
         testData.MockedStock.Verify(s => s.Add(It.Is<Stock>(ns => ns.StockAdjustmentStocks.First().StockAdjustment.Quantity == dto.Quantity)));

         Assert.AreEqual(OrderItemStatus.Received, testOrderItem.Status);
      }

      [TestMethod]
      public void Test_ProcessOrderItem_Complete_ZeroQuantity()
      {
         var testData = new MockedInventoryOrderItemData();
         var stockUoW = MockedInventoryUnitOfWorkFactory.GetStockUnitOfWork(testData);

         var dto = new ProcessOrderDTO()
         {
            OrderItemId = 1,
            Action = OrderItemAction.Complete,
            Quantity = 0,
         };

         var testOrderItem = TestOrderItem;

         //Act
         stockUoW.ProcessOrderItem(testOrderItem, dto, "Test", DeliveryLocationId);
         testData.MockedStock.Verify(s => s.Add(It.IsAny<Stock>()), Times.Never());

         // Check order item is Received
         Assert.AreEqual(OrderItemStatus.Received, testOrderItem.Status, "Order item status");
      }

      [TestMethod]
      public void Test_ProcessOrderItem_Complete_NegativeQuantity()
      {
         var testData = new MockedInventoryOrderItemData();
         var stockUoW = MockedInventoryUnitOfWorkFactory.GetStockUnitOfWork(testData);

         var dto = new ProcessOrderDTO()
         {
            OrderItemId = 1,
            Action = OrderItemAction.Complete,
            Quantity = -1,
         };

         var testOrderItem = TestOrderItem;

         ExceptionAssert.Throws<StockException>(
            () => stockUoW.ProcessOrderItem(testOrderItem, dto, "Test", DeliveryLocationId));

         testData.MockedStock.Verify(s => s.Add(It.IsAny<Stock>()), Times.Never());
         Assert.AreEqual(OrderStatus.Ordered, testOrderItem.Order.Status);
      }

      [TestMethod]
      public void Test_ProcessOrderItem_Complete_PositiveQuantity()
      {
         var testData = new MockedInventoryOrderItemData();
         var stockUoW = MockedInventoryUnitOfWorkFactory.GetStockUnitOfWork(testData);

         var dto = new ProcessOrderDTO()
         {
            OrderItemId = 1,
            Action = OrderItemAction.Complete,
            Quantity = 1,
         };

         var testOrderItem = TestOrderItem;

         //Act
         stockUoW.ProcessOrderItem(testOrderItem, dto, "Test", DeliveryLocationId);

         testData.MockedStock.Verify(s => s.Add(It.IsAny<Stock>()), Times.Once());
         testData.MockedStock.Verify(s => s.Add(It.Is<Stock>(ns => ns.Quantity == 1)));
         testData.MockedStock.Verify(s => s.Add(It.Is<Stock>(ns => ns.StockStatus == StockStatus.Available)));

         Assert.AreEqual(OrderItemStatus.Received, testOrderItem.Status);
      }

      [TestMethod]
      public void Test_ProcessOrderItem_Complete_PositiveQuantity_FulfillsRequest()
      {
         var testData = new MockedInventoryOrderItemData();
         var stockUoW = MockedInventoryUnitOfWorkFactory.GetStockUnitOfWork(testData);

         var dto = new ProcessOrderDTO()
         {
            OrderItemId = 1,
            Action = OrderItemAction.Complete,
            Quantity = 1,
         };

         var testOrderItem = TestOrderItem;

         //Act
         stockUoW.ProcessOrderItem(testOrderItem, dto, "Test", DeliveryLocationId);

         testData.MockedStock.Verify(s => s.Add(It.IsAny<Stock>()), Times.Once());
         testData.MockedStock.Verify(s => s.Add(It.Is<Stock>(ns => ns.Quantity == 1)));
         testData.MockedStock.Verify(s => s.Add(It.Is<Stock>(ns => ns.StockStatus == StockStatus.Available)));

         Assert.AreEqual(OrderItemStatus.Received, testOrderItem.Status);

         var request = testOrderItem.OrderItemSources.First().ProductStockRequest;
         Assert.AreEqual(RequestStatus.Closed, request.RequestStatus);
         Assert.AreEqual(true, request.Fulfilled);
      }

      [TestMethod]
      public void Test_ProcessOrderItem_Complete_PositiveQuantity_UnmanagedStock()
      {
         var testData = new MockedInventoryOrderItemData();
         var stockUoW = MockedInventoryUnitOfWorkFactory.GetStockUnitOfWork(testData);

         var dto = new ProcessOrderDTO()
         {
            OrderItemId = 1,
            Action = OrderItemAction.Complete,
            Quantity = 1,
         };

         var testOrderItem = TestOrderItem;
         testOrderItem.Product.ManageStock = false;

         //Act
         stockUoW.ProcessOrderItem(testOrderItem, dto, "Test", DeliveryLocationId);

         testData.MockedStock.Verify(s => s.Add(It.IsAny<Stock>()), Times.Never());

         Assert.AreEqual(OrderItemStatus.Complete, testOrderItem.Status);
      }

      [TestMethod]
      public void Test_ProcessOrderItem_Complete_PositiveQuantity_AlreadyReceivedStock_ReceivesNewStock()
      {
         var testStock = new Stock
         {
            ProductId = 123,
            Quantity = 1,
            ReceivedQuantity = 1,
            StoredAt = DeliveryLocationId,
            StockId = 11,
         };

         var stockAdjustment = new StockAdjustment
         {
            IsPositive = true,
            OrderItemId = TestOrderItems.First().OrderItemId,
            Quantity = 1,
            StockAdjustmentId = 66,
         };

         var stockAdjustmentStock = new StockAdjustmentStock
         {
            StockAdjustment = stockAdjustment
         };

         testStock.StockAdjustmentStocks.Add(stockAdjustmentStock);

         var testOrderItem = TestOrderItem;
         testOrderItem.Status = OrderItemStatus.PartiallyReceived;

         var testData = new MockedInventoryOrderItemData(TestOrderItems, TestOrderItemSources, TestProducts,
            new List<Stock> { testStock }, new List<StockAdjustment> { stockAdjustment },
            new List<StockAdjustmentStock> { stockAdjustmentStock });
         var stockUoW = MockedInventoryUnitOfWorkFactory.GetStockUnitOfWork(testData);

         var dto = new ProcessOrderDTO()
         {
            OrderItemId = 1,
            Action = OrderItemAction.Complete,
            Quantity = 1,
         };

         //Act
         stockUoW.ProcessOrderItem(testOrderItem, dto, "Test", DeliveryLocationId);

         testData.MockedStock.Verify(ms => ms.Add(It.Is<Stock>(s => s.Quantity == 1 && s.StockStatus == StockStatus.Available && s.ReceivedQuantity == 1)), Times.Once());

         Assert.AreEqual(StockStatus.Available, testStock.StockStatus, "Stock is not available");
         Assert.AreEqual(1, testStock.Quantity, "Stock quantity is incorrect");
         Assert.AreEqual(1, testStock.ReceivedQuantity, "Stock received quantity is incorrect");
      }

      [TestMethod]
      public void Test_ProcessOrderItem_Complete_PositiveQuantity_AlreadyReceivedStock_CreatesNewAdjustment()
      {
         var testStock = new Stock
         {
            ProductId = 123,
            Quantity = 1,
            ReceivedQuantity = 1,
            StoredAt = DeliveryLocationId,
            StockId = 11,
         };

         var stockAdjustment = new StockAdjustment
         {
            IsPositive = true,
            OrderItemId = TestOrderItems.First().OrderItemId,
            Quantity = 1,
            StockAdjustmentId = 66,
         };

         var stockAdjustmentStock = new StockAdjustmentStock
         {
            StockAdjustment = stockAdjustment
         };

         testStock.StockAdjustmentStocks.Add(stockAdjustmentStock);

         var testOrderItem = TestOrderItem;
         testOrderItem.Status = OrderItemStatus.PartiallyReceived;

         var testData = new MockedInventoryOrderItemData(TestOrderItems, TestOrderItemSources, TestProducts,
            new List<Stock> { testStock }, new List<StockAdjustment> { stockAdjustment },
            new List<StockAdjustmentStock> { stockAdjustmentStock });
         var stockUoW = MockedInventoryUnitOfWorkFactory.GetStockUnitOfWork(testData);

         var dto = new ProcessOrderDTO()
         {
            OrderItemId = 1,
            Action = OrderItemAction.Complete,
            Quantity = 1,
         };

         //Act
         stockUoW.ProcessOrderItem(testOrderItem, dto, "Test", DeliveryLocationId);
         testData.MockedStock.Verify(
            ms =>
               ms.Add(
                  It.Is<Stock>(
                     s =>
                        s.StockAdjustmentStocks.SingleOrDefault(
                           sa =>
                              sa.StockAdjustment.StockAdjustmentId == 0 &&
                              sa.StockAdjustment.OrderItemId == testOrderItem.OrderItemId) != null)), Times.Once());

      }


      [TestMethod]
      public void Test_ProcessOrderItem_PartiallyReceive_ZeroQuantity()
      {
         var testData = new MockedInventoryOrderItemData(TestOrderItems, TestOrderItemSources, TestProducts, TestStocks,
            new List<StockAdjustment>(), new List<StockAdjustmentStock>());
         var stockUoW = MockedInventoryUnitOfWorkFactory.GetStockUnitOfWork(testData);

         var dto = new ProcessOrderDTO()
         {
            OrderItemId = 1,
            Action = OrderItemAction.KeepOpen,
            Quantity = 0,
         };

         var testOrderItem = TestOrderItem;

         //Act
         stockUoW.ProcessOrderItem(testOrderItem, dto, "Test", DeliveryLocationId);
         testData.MockedStock.Verify(s => s.Add(It.IsAny<Stock>()), Times.Never());

         // Check order item is Received
         Assert.AreEqual(OrderItemStatus.Ordered, testOrderItem.Status);
      }

      [TestMethod]
      public void Test_ProcessOrderItem_ParitallyReceive_NegativeQuantity()
      {
         var testData = new MockedInventoryOrderItemData();
         var stockUoW = MockedInventoryUnitOfWorkFactory.GetStockUnitOfWork(testData);

         var dto = new ProcessOrderDTO()
         {
            OrderItemId = 1,
            Action = OrderItemAction.KeepOpen,
            Quantity = -1,
         };

         var testOrderItem = TestOrderItem;

         ExceptionAssert.Throws<StockException>(
            () => stockUoW.ProcessOrderItem(testOrderItem, dto, "Test", DeliveryLocationId));


         testData.MockedStock.Verify(s => s.Add(It.IsAny<Stock>()), Times.Never());
         Assert.AreEqual(OrderStatus.Ordered, testOrderItem.Order.Status);
      }

      [TestMethod]
      public void Test_ProcessOrderItem_PartiallyReceive_PositiveQuantity()
      {
         var testData = new MockedInventoryOrderItemData();
         var stockUoW = MockedInventoryUnitOfWorkFactory.GetStockUnitOfWork(testData);

         var dto = new ProcessOrderDTO()
         {
            OrderItemId = 1,
            Action = OrderItemAction.KeepOpen,
            Quantity = 1,
         };

         var testOrderItem = TestOrderItem;

         //Act
         stockUoW.ProcessOrderItem(testOrderItem, dto, "Test", DeliveryLocationId);

         testData.MockedStock.Verify(s => s.Add(It.IsAny<Stock>()), Times.Once());
         testData.MockedStock.Verify(s => s.Add(It.Is<Stock>(ns => ns.Quantity == 1)));
         testData.MockedStock.Verify(s => s.Add(It.Is<Stock>(ns => ns.StockStatus == StockStatus.Available)));

         Assert.AreEqual(OrderItemStatus.PartiallyReceived, testOrderItem.Status);
      }

      [TestMethod]
      public void Test_ProcessOrderItem_PartiallyReceive_PositiveQuantity_AlreadyReceivedStock_CreatesNewStock()
      {
         var testStock = new Stock
         {
            ProductId = 123,
            Quantity = 1,
            ReceivedQuantity = 1,
            StoredAt = DeliveryLocationId,
            StockId = 11,
         };

         var stockAdjustment = new StockAdjustment
         {
            IsPositive = true,
            OrderItemId = TestOrderItems.First().OrderItemId,
            Quantity = 1,
            StockAdjustmentId = 66,
         };

         var stockAdjustmentStock = new StockAdjustmentStock
         {
            StockAdjustment = stockAdjustment
         };

         testStock.StockAdjustmentStocks.Add(stockAdjustmentStock);

         var testOrderItem = TestOrderItem;
         testOrderItem.Status = OrderItemStatus.PartiallyReceived;

         var testData = new MockedInventoryOrderItemData(TestOrderItems, TestOrderItemSources, TestProducts, new List<Stock> { testStock }, new List<StockAdjustment> { stockAdjustment }, new List<StockAdjustmentStock> { stockAdjustmentStock });
         var stockUoW = MockedInventoryUnitOfWorkFactory.GetStockUnitOfWork(testData);

         var dto = new ProcessOrderDTO()
         {
            OrderItemId = 1,
            Action = OrderItemAction.KeepOpen,
            Quantity = 1,
         };

         //Act
         stockUoW.ProcessOrderItem(testOrderItem, dto, "Test", DeliveryLocationId);

         testData.MockedStock.Verify(ms => ms.Add(It.Is<Stock>(s => s.Quantity == 1 && s.StockStatus == StockStatus.Available && s.ReceivedQuantity == 1)), Times.Once());
         var stock = testData.MockedStock.Object.First();
         Assert.AreEqual(StockStatus.Available, stock.StockStatus, "Stock is not available");
         Assert.AreEqual(1, stock.Quantity, "Stock quantity is incorrect");
         Assert.AreEqual(1, stock.ReceivedQuantity, "Stock received quantity is incorrect");

         Assert.AreEqual(OrderItemStatus.PartiallyReceived, testOrderItem.Status);
      }

      [TestMethod]
      public void Test_ProcessOrderItem_PartiallyReceive_PositiveQuantity_AlreadyExistingStock_CreatesNewAdjustment()
      {
         var testStock = new Stock
         {
            ProductId = 123,
            Quantity = 1,
            ReceivedQuantity = 1,
            StoredAt = DeliveryLocationId,
            StockId = 11,
         };

         var stockAdjustment = new StockAdjustment
         {
            IsPositive = true,
            OrderItemId = TestOrderItems.First().OrderItemId,
            Quantity = 1,
            StockAdjustmentId = 66,
         };

         var stockAdjustmentStock = new StockAdjustmentStock
         {
            StockAdjustment = stockAdjustment
         };

         testStock.StockAdjustmentStocks.Add(stockAdjustmentStock);

         var testOrderItem = TestOrderItem;
         testOrderItem.Status = OrderItemStatus.PartiallyReceived;

         var testData = new MockedInventoryOrderItemData(TestOrderItems, TestOrderItemSources, TestProducts, new List<Stock> { testStock }, new List<StockAdjustment> { stockAdjustment }, new List<StockAdjustmentStock> { stockAdjustmentStock });
         var stockUoW = MockedInventoryUnitOfWorkFactory.GetStockUnitOfWork(testData);

         var dto = new ProcessOrderDTO()
         {
            OrderItemId = 1,
            Action = OrderItemAction.KeepOpen,
            Quantity = 1,
         };

         //Act
         stockUoW.ProcessOrderItem(testOrderItem, dto, "Test", DeliveryLocationId);

         testData.MockedStock.Verify(
               ms =>
                  ms.Add(
                     It.Is<Stock>(
                        s =>
                           s.StockAdjustmentStocks.SingleOrDefault(
                              sa =>
                                 sa.StockAdjustment.StockAdjustmentId == 0 &&
                                 sa.StockAdjustment.OrderItemId == testOrderItem.OrderItemId) != null)), Times.Once());

      }

      [TestMethod]
      public void Test_ProcessOrderItem_Cancel_AllItems()
      {
         var testData = new MockedInventoryOrderItemData(TestOrderItems, TestOrderItemSources, TestProducts, TestStocks,
            new List<StockAdjustment>(), new List<StockAdjustmentStock>());
         var stockUoW = MockedInventoryUnitOfWorkFactory.GetStockUnitOfWork(testData);

         var dto = new ProcessOrderDTO()
         {
            OrderItemId = 1,
            Action = OrderItemAction.Cancel,
            Quantity = 1,
         };

         var testOrderItem = TestOrderItem;

         //Act
         stockUoW.ProcessOrderItem(testOrderItem, dto, "Test", DeliveryLocationId);

         testData.MockedStock.Verify(s => s.Add(It.IsAny<Stock>()), Times.Never());

         Assert.AreEqual(OrderItemStatus.Cancelled, testOrderItem.Status, "Order item status");
      }

      [TestMethod]
      public void Test_ProcessOrderItem_Cancel_OneItem()
      {
         var secondOrderItem = TestOrderItem;
         secondOrderItem.OrderItemId = 2;

         var testOrderItems = TestOrderItems.ToList();
         testOrderItems.Add(secondOrderItem);

         var testData = new MockedInventoryOrderItemData(testOrderItems, TestOrderItemSources, TestProducts, TestStocks,
            new List<StockAdjustment>(), new List<StockAdjustmentStock>());
         var stockUoW = MockedInventoryUnitOfWorkFactory.GetStockUnitOfWork(testData);

         var dto = new ProcessOrderDTO()
         {
            OrderItemId = 1,
            Action = OrderItemAction.Cancel,
            Quantity = 1,
         };

         var testOrderItem = TestOrderItem;

         //Act
         stockUoW.ProcessOrderItem(testOrderItem, dto, "Test", DeliveryLocationId);

         testData.MockedStock.Verify(s => s.Add(It.IsAny<Stock>()), Times.Never());

         Assert.AreEqual(OrderItemStatus.Cancelled, testOrderItem.Status, "Order item status");
      }

      [TestMethod]
      public void Test_ProcessOrderItem_Cancel_OneItem_PartiallyReceivedOrder()
      {
         var secondOrderItem = TestOrderItem;
         secondOrderItem.OrderItemId = 2;

         var testOrderItems = TestOrderItems.ToList();
         testOrderItems.Add(secondOrderItem);

         var testData = new MockedInventoryOrderItemData(testOrderItems, TestOrderItemSources, TestProducts, TestStocks,
            new List<StockAdjustment>(), new List<StockAdjustmentStock>());
         var stockUoW = MockedInventoryUnitOfWorkFactory.GetStockUnitOfWork(testData);

         var dto = new ProcessOrderDTO()
         {
            OrderItemId = 1,
            Action = OrderItemAction.Cancel,
            Quantity = 1,
         };

         var testOrderItem = TestOrderItem;

         //Act
         stockUoW.ProcessOrderItem(testOrderItem, dto, "Test", DeliveryLocationId);

         testData.MockedStock.Verify(s => s.Add(It.IsAny<Stock>()), Times.Never());

         Assert.AreEqual(OrderItemStatus.Cancelled, testOrderItem.Status, "Order item status");
      }

      [TestMethod]
      public void ProcessOrderItem_PartiallyReceive_ExistingHasSerialNumber_AddsNewStock()
      {
         var testOrderItem = TestOrderItem;

         var deductedStock = new Stock
         {
            ProductId = testOrderItem.ProductId,
            Quantity = 0,
            ReceivedQuantity = 1,
            SerialNumber = "SERIAL",
            StockId = 1,
            StockStatus = StockStatus.Deducted,
            StoredAt = DeliveryLocationId,
         };

         var adjustment = new StockAdjustment
         {
            OrderItemId = testOrderItem.OrderItemId,
            StockAdjustmentId = 1,
         };

         var stockAdjustmentStock = new StockAdjustmentStock
         {
            StockAdjustmentId = adjustment.StockAdjustmentId,
            StockId = deductedStock.StockId
         };

         var testData = new MockedInventoryOrderItemData(
            new List<OrderItem>(), new List<OrderItemSource>(), new List<Product>(), new List<Stock> { deductedStock },
            new List<StockAdjustment> { adjustment }, new List<StockAdjustmentStock> { stockAdjustmentStock });

         var stockUoW = MockedInventoryUnitOfWorkFactory.GetStockUnitOfWork(testData);
         var dto = new ProcessOrderDTO()
         {
            OrderItemId = 1,
            Action = OrderItemAction.KeepOpen,
            Quantity = 1,
         };

         //Act
         stockUoW.ProcessOrderItem(testOrderItem, dto, "Test", DeliveryLocationId);

         testData.MockedStock.Verify(s => s.Add(It.IsAny<Stock>()), Times.Once());
         testData.MockedStock.Verify(s => s.Add(It.Is<Stock>(ns => ns.Quantity == 1)));
         testData.MockedStock.Verify(s => s.Add(It.Is<Stock>(ns => ns.StockStatus == StockStatus.Available)));
         testData.MockedStock.Verify(ms => ms.Add(It.Is<Stock>(s => s.StockAdjustmentStocks.FirstOrDefault(sas => sas.StockAdjustment.OrderItemId.HasValue) != null)));

         Assert.AreEqual(OrderItemStatus.PartiallyReceived, testOrderItem.Status);
      }

      [TestMethod]
      public void ProcessOrderItem_PartiallyReceive_ExistingHasBatchNumber_AddsNewStock()
      {
         var testOrderItem = TestOrderItem;

         var deductedStock = new Stock
         {
            BatchNumber = "BatchNumber",
            ProductId = testOrderItem.ProductId,
            Quantity = 0,
            ReceivedQuantity = 1,
            StockId = 1,
            StockStatus = StockStatus.Deducted,
            StoredAt = DeliveryLocationId,
         };

         var adjustment = new StockAdjustment
         {
            OrderItemId = testOrderItem.OrderItemId,
            StockAdjustmentId = 1,
         };

         var stockAdjustmentStock = new StockAdjustmentStock
         {
            StockAdjustmentId = adjustment.StockAdjustmentId,
            StockId = deductedStock.StockId
         };

         var testData = new MockedInventoryOrderItemData(
            new List<OrderItem>(), new List<OrderItemSource>(), new List<Product>(), new List<Stock> { deductedStock },
            new List<StockAdjustment> { adjustment }, new List<StockAdjustmentStock> { stockAdjustmentStock });

         var stockUoW = MockedInventoryUnitOfWorkFactory.GetStockUnitOfWork(testData);
         var dto = new ProcessOrderDTO()
         {
            OrderItemId = 1,
            Action = OrderItemAction.KeepOpen,
            Quantity = 1,
         };

         //Act
         stockUoW.ProcessOrderItem(testOrderItem, dto, "Test", DeliveryLocationId);

         testData.MockedStock.Verify(s => s.Add(It.IsAny<Stock>()), Times.Once());
         testData.MockedStock.Verify(s => s.Add(It.Is<Stock>(ns => ns.Quantity == 1)));
         testData.MockedStock.Verify(s => s.Add(It.Is<Stock>(ns => ns.StockStatus == StockStatus.Available)));
         testData.MockedStock.Verify(ms => ms.Add(It.Is<Stock>(s => s.StockAdjustmentStocks.FirstOrDefault(sas => sas.StockAdjustment.OrderItemId.HasValue) != null)));

         Assert.AreEqual(OrderItemStatus.PartiallyReceived, testOrderItem.Status);
      }
   }
}
