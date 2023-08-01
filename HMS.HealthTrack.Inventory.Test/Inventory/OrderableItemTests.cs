using System;
using System.Collections.Generic;
using System.Linq;
using HMS.HealthTrack.Inventory.Common.Constants;
using HMS.HealthTrack.Inventory.Test.Inventory.Mocks;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HMS.HealthTrack.Inventory.Test.Inventory
{
   [TestClass]
   public class OrderableItemTests
   {
      private const string Username = "UnitTest";

      private Order Order
      {
         get
         {
            return new Order
            {
               InventoryOrderId = 1,
               Items = new List<OrderItem>
               {
                  new OrderItem
                  {
                     InventoryOrderId = 1,
                     ProductId = 123,
                     Status = OrderItemStatus.Ordered,
                     Quantity = 1
                  }
               },
               Status = OrderStatus.Ordered,
            };
         }
      }

      private Product Product
      {
         get
         {
            return new Product
            {
               AutoReorderSetting = ReorderSettings.SpecifyLevels,
               Description = "Test Product",
               ManageStock = true,
               SPC = "SPC",
               Stocks = new List<Stock>
               {
                  new Stock
                  {
                     StockId = 1,
                     ProductId = 123,
                     Quantity = 4,
                     StockStatus = StockStatus.Available,
                  },
               },
               PrimarySupplier = 1,
               PrimarySupplierCompany = new Company
               {
                  companyName = "TestCompany",
                  company_ID = 1,
               },
               ProductId = 123,
               ReorderThreshold = 5,
               TargetStockLevel = 10,
            };
         }
      }

      private ProductStockRequest StockRequest
      {
         get
         {
            return new ProductStockRequest
            {
               ApprovedQuantity = 1,
               Product = Product,
               RequestedQuantity = 1,
               RequestStatus = RequestStatus.Approved,
               StockRequestId = 1,
            };
         }
      }

      private ConsumptionNotificationManagement Management
      {
         get
         {
            return new ConsumptionNotificationManagement
            {
               invUsed_ID = 1,
            };
         }
      }

      private HealthTrackConsumption HealthTrackConsumption
      {
         get
         {
            return new HealthTrackConsumption
            {
               deleted = false,
               Quantity = 1,
               UsedId = 1,
            };
         }
      }

      #region GetOrderItemTests

      [TestMethod]
      public void GetOrderItemForProduct_ReturnsExistingOrderItem()
      {
         var order = Order;
         var orderItem = order.Items.First();
         var dto = new OrderableItemDTO
         {
            ProductId = 123
         };

         var testData = new MockedInventoryOrderableItemsData();

         var uow = MockedInventoryUnitOfWorkFactory.GetOrderableItemsUnitOfWork(testData);

         var existingOrderItem = uow.GetOrderItemForProduct(dto.ProductId, order);

         Assert.AreSame(orderItem, existingOrderItem);
      }

      [TestMethod]
      public void GetOrderItemForProduct_ReturnsNewItemWhenExistingNotPending()
      {
         var order = Order;
         order.Items.First().Status = OrderItemStatus.Cancelled;

         var dto = new OrderableItemDTO
         {
            ProductId = 123
         };

         var testData = new MockedInventoryOrderableItemsData();
         var uow = MockedInventoryUnitOfWorkFactory.GetOrderableItemsUnitOfWork(testData);

         var newOrderItem = uow.GetOrderItemForProduct(dto.ProductId, order);

         Assert.AreEqual(dto.ProductId, newOrderItem.ProductId);
         Assert.AreEqual(OrderItemStatus.Ordered, newOrderItem.Status);
         Assert.AreEqual(2, order.Items.Count());
      }

      [TestMethod]
      public void GetOrderItemForProduct_ReturnsNewOrderItem()
      {
         var order = Order;
         order.Items.Clear();
         var dto = new OrderableItemDTO
         {
            ProductId = 123
         };

         var testData = new MockedInventoryOrderableItemsData();
         var uow = MockedInventoryUnitOfWorkFactory.GetOrderableItemsUnitOfWork(testData);
         var newOrderItem = uow.GetOrderItemForProduct(dto.ProductId, order);

         Assert.AreEqual(dto.ProductId, newOrderItem.ProductId);
         Assert.AreEqual(OrderItemStatus.Ordered, newOrderItem.Status);
         Assert.AreEqual(1, order.Items.Count());
      }

      #endregion

      #region StockReplenishmentTests

      [TestMethod]
      public void AddStockReplenishment_StockBelowThreshold_ReturnsItemWithQuantityToMeetTargetStockLevel()
      {
         var lowStock = new LowStock
         {
            Product = Product,
            StockCount = 4,
            TargetStockLevel = 10,
            ReorderThreshold = 5
         };

         var lowStockList = new List<LowStock> { lowStock };
         var orderableItems = new List<OrderableItem>();

         var testData = new MockedInventoryOrderableItemsData();

         var uow = MockedInventoryUnitOfWorkFactory.GetOrderableItemsUnitOfWork(testData);

         uow.AddStockReplenishment(orderableItems, 1, lowStockList);

         Assert.AreEqual(1, orderableItems.Count());
         Assert.AreEqual(123, orderableItems.First().ProductId);
         Assert.AreEqual(OrderableItemSource.Topup, orderableItems.First().Source);
         Assert.AreEqual(6, orderableItems.First().Quantity);
      }

      [TestMethod]
      public void AddStockReplenishment_StockBelowThreshold_UnmanagedStock_ReturnsNothing()
      {
         var product = Product;
         product.AutoReorderSetting = ReorderSettings.OneForOneReplace;
         product.ManageStock = false;
         var lowStock = new LowStock
         {
            Product = product,
            StockCount = 4,
            TargetStockLevel = 10,
            ReorderThreshold = 5
         };

         var lowStockList = new List<LowStock> { lowStock };
         var orderableItems = new List<OrderableItem>();

         var testData = new MockedInventoryOrderableItemsData();

         var uow = MockedInventoryUnitOfWorkFactory.GetOrderableItemsUnitOfWork(testData);

         uow.AddStockReplenishment(orderableItems, 1, lowStockList);

         Assert.AreEqual(0, orderableItems.Count());
      }

      [TestMethod]
      public void AddStockReplenishment_StockBelowThreshold_ManagedStock_DoNotReorder_ReturnsNothing()
      {
         var product = Product;
         product.AutoReorderSetting = ReorderSettings.DoNotReorder;

         var lowStock = new LowStock
         {
            Product = product,
            StockCount = 4,
            TargetStockLevel = 10,
            ReorderThreshold = 5
         };

         var lowStockList = new List<LowStock> { lowStock };
         var orderableItems = new List<OrderableItem>();

         var testData = new MockedInventoryOrderableItemsData();

         var uow = MockedInventoryUnitOfWorkFactory.GetOrderableItemsUnitOfWork(testData);

         uow.AddStockReplenishment(orderableItems, 1, lowStockList);

         Assert.AreEqual(0, orderableItems.Count());
      }

      [TestMethod]
      public void AddStockReplenishment_StockBelowThreshold_ManagedStock_OneForOneReplace_ReturnsNothing()
      {
         var product = Product;
         product.AutoReorderSetting = ReorderSettings.OneForOneReplace;

         var lowStock = new LowStock
         {
            Product = product,
            StockCount = 4,
            TargetStockLevel = 10,
            ReorderThreshold = 5
         };

         var lowStockList = new List<LowStock> { lowStock };
         var orderableItems = new List<OrderableItem>();
         var testData = new MockedInventoryOrderableItemsData();

         var uow = MockedInventoryUnitOfWorkFactory.GetOrderableItemsUnitOfWork(testData);

         uow.AddStockReplenishment(orderableItems, 1, lowStockList);

         Assert.AreEqual(0, orderableItems.Count());
      }

      [TestMethod]
      public void
         AddStockReplenishment_StockBelowThreshold_ReturnsItemWithQuantityToMeetTargetStockLevel_IgnoringNegativeStock()
      {
         var product = Product;

         var lowStock = new LowStock
         {
            Product = product,
            StockCount = 4,
            TargetStockLevel = 10,
            ReorderThreshold = 5,
         };

         var negativeStock = new NegativeStock
         {
            NegativeQuantity = 3,
            ProductId = product.ProductId,
            StoredAt = product.Stocks.First().StoredAt,
         };

         var lowStockList = new List<LowStock> { lowStock };
         var orderableItems = new List<OrderableItem>();


         var testData = new MockedInventoryOrderableItemsData(new List<StockAdjustment>(),
            new List<DeductionRequiringPaymentClass>(), new List<Product>(),
            new List<OrderItemSource>(), new List<Order>(), new List<NegativeStock> { negativeStock }, new List<ProductStockRequest>(), new List<ConsumptionNotificationManagement>(), new List<HealthTrackConsumption>());


         var uow = MockedInventoryUnitOfWorkFactory.GetOrderableItemsUnitOfWork(testData);

         uow.AddStockReplenishment(orderableItems, 1, lowStockList);

         Assert.AreEqual(1, orderableItems.Count());
         Assert.AreEqual(123, orderableItems.First().ProductId);
         Assert.AreEqual(OrderableItemSource.Topup, orderableItems.First().Source);
         Assert.AreEqual(6, orderableItems.First().Quantity);
      }

      [TestMethod]
      public void AddStockReplenishment_StockAtThreshold_ReturnsItemWithQuantityToMeetTargetStockLevel()
      {
         var lowStock = new LowStock
         {
            Product = Product,
            StockCount = 5,
            TargetStockLevel = 10,
            ReorderThreshold = 5
         };

         var lowStockList = new List<LowStock> { lowStock };
         var orderableItems = new List<OrderableItem>();

         var testData = new MockedInventoryOrderableItemsData();

         var uow = MockedInventoryUnitOfWorkFactory.GetOrderableItemsUnitOfWork(testData);

         uow.AddStockReplenishment(orderableItems, 1, lowStockList);

         Assert.AreEqual(1, orderableItems.Count());
         Assert.AreEqual(123, orderableItems.First().ProductId);
         Assert.AreEqual(OrderableItemSource.Topup, orderableItems.First().Source);
         Assert.AreEqual(5, orderableItems.First().Quantity);
      }

      [TestMethod]
      public void AddStockReplenishment_EqualThresholdAndTarget_StockAtTarget_NoItemReturned()
      {
         var product = Product;
         product.ReorderThreshold = 10;
         product.TargetStockLevel = 10;

         var lowStockList = new List<LowStock> { };
         var orderableItems = new List<OrderableItem>();

         var testData = new MockedInventoryOrderableItemsData();

         var uow = MockedInventoryUnitOfWorkFactory.GetOrderableItemsUnitOfWork(testData);

         uow.AddStockReplenishment(orderableItems, 1, lowStockList);

         Assert.AreEqual(0, orderableItems.Count());
      }

      [TestMethod]
      public void AddStockReplenishment_AccountsForOrderedConsumptions()
      {
         var product = Product;

         var lowStock = new LowStock
         {
            Product = product,
            StockCount = 4,
            TargetStockLevel = 10,
            ReorderThreshold = 5
         };
         var lowStockList = new List<LowStock> { lowStock };

         var orderItem = new OrderItem
         {
            ProductId = 123,
            Product = product,
            Quantity = 1,
            Status = OrderItemStatus.Ordered
         };
         var orderItemSource = new OrderItemSource
         {
            Quantity = 1,
            OrderItem = orderItem
         };
         product.OrderItems.Add(orderItem);


         var testData = new MockedInventoryOrderableItemsData(new List<StockAdjustment>(),
            new List<DeductionRequiringPaymentClass>(), new List<Product>(),
            new List<OrderItemSource> { orderItemSource }, new List<Order>(), new List<NegativeStock>(), new List<ProductStockRequest>(), new List<ConsumptionNotificationManagement>(), new List<HealthTrackConsumption>());


         var orderableItems = new List<OrderableItem>();
         var uow = MockedInventoryUnitOfWorkFactory.GetOrderableItemsUnitOfWork(testData);

         uow.AddStockReplenishment(orderableItems, 1, lowStockList);

         Assert.AreEqual(1, orderableItems.Count());
         Assert.AreEqual(123, orderableItems.First().ProductId);
         Assert.AreEqual(OrderableItemSource.Topup, orderableItems.First().Source);
         Assert.AreEqual(5, orderableItems.First().Quantity);
      }

      [TestMethod]
      public void AddStockReplenishment_IncludesConsumptionMissingRequiredPaymentClassInThresholdCheck()
      {
         var product = Product;

         product.ProductSettings.Add(new StockSetting
         {
            SettingId = InventoryConstants.StockSettings.RequiresPaymentClass
         });

         product.Stocks.First().Quantity = 0;

         var consumptionRequiringPaymentClass = new DeductionRequiringPaymentClass
         {
            ProductId = product.ProductId,
            Quantity = 1
         };

         var testData = new MockedInventoryOrderableItemsData(new List<StockAdjustment>(),
            new List<DeductionRequiringPaymentClass> { consumptionRequiringPaymentClass }, new List<Product>(),
            new List<OrderItemSource>(), new List<Order>(), new List<NegativeStock>(), new List<ProductStockRequest>(), new List<ConsumptionNotificationManagement>(), new List<HealthTrackConsumption>());


         var uow = MockedInventoryUnitOfWorkFactory.GetOrderableItemsUnitOfWork(testData);

         var orderableItems = new List<OrderableItem>();
         var lowStock = new List<LowStock>
         {
            new LowStock
            {
               Product = product,
               ReorderThreshold = product.ReorderThreshold.Value,
               StockCount = product.Stocks.Sum(xx => xx.Quantity),
               TargetStockLevel = product.TargetStockLevel.Value
            }
         };

         uow.AddStockReplenishment(orderableItems, 1, lowStock);

         Assert.AreEqual(1, orderableItems.Count());
      }

      [TestMethod]
      public void AddStockReplenishment_CheckOrderQuantityRoundsToMultiplesOf()
      {
         var product = Product;
         product.ReorderThreshold = 50;
         product.TargetStockLevel = 75;
         product.OrderMultiple = 5;

         product.Stocks.First().Quantity = 49;

         var testData = new MockedInventoryOrderableItemsData(new List<StockAdjustment>(),
            new List<DeductionRequiringPaymentClass>(), new List<Product>(),
            new List<OrderItemSource>(), new List<Order>(), new List<NegativeStock>(), new List<ProductStockRequest>(), new List<ConsumptionNotificationManagement>(), new List<HealthTrackConsumption>());


         var uow = MockedInventoryUnitOfWorkFactory.GetOrderableItemsUnitOfWork(testData);

         var orderableItems = new List<OrderableItem>();
         var lowStock = new List<LowStock>
         {
            new LowStock
            {
               Product = product,
               ReorderThreshold = product.ReorderThreshold.Value,
               StockCount = product.Stocks.Sum(xx => xx.Quantity),
               TargetStockLevel = product.TargetStockLevel.Value
            }
         };

         uow.AddStockReplenishment(orderableItems, 1, lowStock);

         Assert.AreEqual(1, orderableItems.Count, "Should all be in one order");
         Assert.AreEqual(30, orderableItems.First().Quantity, "Should reach the target threshold");
      }

      [TestMethod]
      public void AddStockReplenishment_CheckOrderQuantityWithNoMultiplesOf()
      {
         var product = Product;
         product.ReorderThreshold = 50;
         product.TargetStockLevel = 75;
         product.OrderMultiple = 1;
         product.Stocks.First().Quantity = 49;

         var testData = new MockedInventoryOrderableItemsData(new List<StockAdjustment>(),
            new List<DeductionRequiringPaymentClass>(), new List<Product>(),
            new List<OrderItemSource>(), new List<Order>(), new List<NegativeStock>(), new List<ProductStockRequest>(), new List<ConsumptionNotificationManagement>(), new List<HealthTrackConsumption>());

         var uow = MockedInventoryUnitOfWorkFactory.GetOrderableItemsUnitOfWork(testData);

         var orderableItems = new List<OrderableItem>();
         var lowStock = new List<LowStock>
         {
            new LowStock
            {
               Product = product,
               ReorderThreshold = product.ReorderThreshold.Value,
               StockCount = product.Stocks.Sum(xx => xx.Quantity),
               TargetStockLevel = product.TargetStockLevel.Value
            }
         };

         uow.AddStockReplenishment(orderableItems, 1, lowStock);

         Assert.AreEqual(1, orderableItems.Count, "Should all be in one order");
         Assert.AreEqual(26, orderableItems.First().Quantity, "Should reach the target threshold");
      }

      [TestMethod]
      public void AddStockReplenishment_CheckOrderQuantityWithMinimumSet()
      {
         var product = Product;
         product.ReorderThreshold = 50;
         product.TargetStockLevel = 60;
         product.MinimumOrder = 30;
         product.Stocks.First().Quantity = 49;

         var testData = new MockedInventoryOrderableItemsData(new List<StockAdjustment>(),
            new List<DeductionRequiringPaymentClass>(), new List<Product>(),
            new List<OrderItemSource>(), new List<Order>(), new List<NegativeStock>(), new List<ProductStockRequest>(), new List<ConsumptionNotificationManagement>(), new List<HealthTrackConsumption>());

         var uow = MockedInventoryUnitOfWorkFactory.GetOrderableItemsUnitOfWork(testData);

         var orderableItems = new List<OrderableItem>();
         var lowStock = new List<LowStock>
         {
            new LowStock
            {
               Product = product,
               ReorderThreshold = product.ReorderThreshold.Value,
               StockCount = product.Stocks.Sum(xx => xx.Quantity),
               TargetStockLevel = product.TargetStockLevel.Value
            }
         };

         uow.AddStockReplenishment(orderableItems, 1, lowStock);

         Assert.AreEqual(1, orderableItems.Count, "Should all be in one order");
         Assert.AreEqual(30, orderableItems.First().Quantity, "Should reach the target threshold");
      }

      [TestMethod]
      public void AddStockReplenishment_CheckOrderQuantityIsAlreadyMultipleOf()
      {
         var product = new Product
         {
            ManageStock = true,
            ProductId = 123,
            ReorderThreshold = 50,
            TargetStockLevel = 75,
         };

         var stock = new Stock
         {
            Product = product,
            Quantity = 40
         };

         product.Stocks.Add(stock);

         var testData = new MockedInventoryOrderableItemsData(new List<StockAdjustment>(),
            new List<DeductionRequiringPaymentClass>(), new List<Product>(),
            new List<OrderItemSource>(), new List<Order>(), new List<NegativeStock>(), new List<ProductStockRequest>(), new List<ConsumptionNotificationManagement>(), new List<HealthTrackConsumption>());

         var uow = MockedInventoryUnitOfWorkFactory.GetOrderableItemsUnitOfWork(testData);

         var orderableItems = new List<OrderableItem>();
         var lowStock = new List<LowStock>
         {
            new LowStock
            {
               Product = product,
               ReorderThreshold = product.ReorderThreshold.Value,
               StockCount = product.Stocks.Sum(xx => xx.Quantity),
               TargetStockLevel = product.TargetStockLevel.Value
            }
         };

         uow.AddStockReplenishment(orderableItems, 1, lowStock);

         Assert.AreEqual(1, orderableItems.Count, "Should all be in one order");
         Assert.AreEqual(35, orderableItems.First().Quantity, "Should reach the target threshold");
      }

      #endregion

      #region CreateOrderFromTests

      [TestMethod]
      public void CreateOrderFrom_RequestValid()
      {
         var order = Order;
         order.Items.Clear();
         string errorMessage;


         var orderableItem = new OrderableItem
         {
            ProductId = Product.ProductId,
            Quantity = 1,
            RequestIds = new List<int> { 1 },
            Source = OrderableItemSource.Request,
         };

         var testData = new MockedInventoryOrderableItemsData(new List<StockAdjustment>(),
            new List<DeductionRequiringPaymentClass>(), new List<Product> { Product }, new List<OrderItemSource>(),
            new List<Order> { order }, new List<NegativeStock>(), new List<ProductStockRequest> { StockRequest }, new List<ConsumptionNotificationManagement>(), new List<HealthTrackConsumption>());
         var uow = MockedInventoryUnitOfWorkFactory.GetOrderableItemsUnitOfWork(testData);

         uow.AddOrderableItemsToOrder(order, new List<OrderableItem> { orderableItem }, out errorMessage, Username);

         Assert.IsTrue(string.IsNullOrWhiteSpace(errorMessage), errorMessage);
         var orderItem = order.Items.Single();
         Assert.AreEqual(Product.ProductId, orderItem.ProductId, "Product Id");
         Assert.AreEqual(StockRequest.ApprovedQuantity.Value, orderItem.Quantity, "Quantity");
      }

      [TestMethod]
      public void CreateOrderFrom_RequestValid_UsesApprovedQuantity()
      {
         var order = Order;
         order.Items.Clear();
         string errorMessage;

         var request = StockRequest;
         request.RequestedQuantity = 10;
         request.ApprovedQuantity = 5;

         var orderableItem = new OrderableItem
         {
            ProductId = Product.ProductId,
            Quantity = 5,
            RequestIds = new List<int> { 1 },
            Source = OrderableItemSource.Request,
         };

         var testData = new MockedInventoryOrderableItemsData(new List<StockAdjustment>(),
            new List<DeductionRequiringPaymentClass>(), new List<Product> { Product }, new List<OrderItemSource>(),
            new List<Order> { order }, new List<NegativeStock>(), new List<ProductStockRequest> { request }, new List<ConsumptionNotificationManagement>(), new List<HealthTrackConsumption>());
         var uow = MockedInventoryUnitOfWorkFactory.GetOrderableItemsUnitOfWork(testData);

         uow.AddOrderableItemsToOrder(order, new List<OrderableItem> { orderableItem }, out errorMessage, Username);

         Assert.IsTrue(string.IsNullOrWhiteSpace(errorMessage), errorMessage);
         var orderItem = order.Items.Single();
         Assert.AreEqual(Product.ProductId, orderItem.ProductId, "Product Id");
         Assert.AreEqual(request.ApprovedQuantity.Value, orderItem.Quantity, "Quantity");
      }

      [TestMethod]
      public void CreateOrderFrom_RequestValid_RequestUpdated()
      {
         var order = Order;
         order.Items.Clear();
         string errorMessage;
         var request = StockRequest;

         var orderableItem = new OrderableItem
         {
            ProductId = Product.ProductId,
            Quantity = 1,
            RequestIds = new List<int> { 1 },
            Source = OrderableItemSource.Request,
         };

         var testData = new MockedInventoryOrderableItemsData(new List<StockAdjustment>(),
            new List<DeductionRequiringPaymentClass>(), new List<Product> { Product }, new List<OrderItemSource>(),
            new List<Order> { order }, new List<NegativeStock>(), new List<ProductStockRequest> { request }, new List<ConsumptionNotificationManagement>(), new List<HealthTrackConsumption>());
         var uow = MockedInventoryUnitOfWorkFactory.GetOrderableItemsUnitOfWork(testData);

         uow.AddOrderableItemsToOrder(order, new List<OrderableItem> { orderableItem }, out errorMessage, Username);

         Assert.IsTrue(string.IsNullOrWhiteSpace(errorMessage), errorMessage);
         Assert.AreEqual(RequestStatus.Ordered, request.RequestStatus);
      }

      [TestMethod]
      public void CreateOrderFrom_RequestValidUrgent_OrderMarkedUrgent()
      {
         var order = Order;
         order.Items.Clear();
         string errorMessage;

         var request = StockRequest;
         request.IsUrgent = true;

         var orderableItem = new OrderableItem
         {
            ProductId = Product.ProductId,
            Quantity = 1,
            RequestIds = new List<int> { 1 },
            Source = OrderableItemSource.Request,
         };

         var testData = new MockedInventoryOrderableItemsData(new List<StockAdjustment>(),
            new List<DeductionRequiringPaymentClass>(), new List<Product> { Product }, new List<OrderItemSource>(),
            new List<Order> { order }, new List<NegativeStock>(), new List<ProductStockRequest> { request }, new List<ConsumptionNotificationManagement>(), new List<HealthTrackConsumption>());
         var uow = MockedInventoryUnitOfWorkFactory.GetOrderableItemsUnitOfWork(testData);

         uow.AddOrderableItemsToOrder(order, new List<OrderableItem> { orderableItem }, out errorMessage, Username);

         Assert.IsTrue(string.IsNullOrWhiteSpace(errorMessage), errorMessage);
         var orderItem = order.Items.Single();
         Assert.AreEqual(Product.ProductId, orderItem.ProductId, "Product Id");
         Assert.AreEqual(request.ApprovedQuantity.Value, orderItem.Quantity, "Quantity");
         Assert.IsTrue(order.IsUrgent, "Urgent");
      }

      [TestMethod]
      public void CreateOrderFrom_RequestValid_TopsUpOrderItemQuantity()
      {
         var order = Order;
         string errorMessage;


         var orderableItem = new OrderableItem
         {
            ProductId = Product.ProductId,
            Quantity = 1,
            RequestIds = new List<int> { 1 },
            Source = OrderableItemSource.Request,
         };

         var testData = new MockedInventoryOrderableItemsData(new List<StockAdjustment>(),
            new List<DeductionRequiringPaymentClass>(), new List<Product> { Product }, new List<OrderItemSource>(),
            new List<Order> { order }, new List<NegativeStock>(), new List<ProductStockRequest> { StockRequest }, new List<ConsumptionNotificationManagement>(), new List<HealthTrackConsumption>());
         var uow = MockedInventoryUnitOfWorkFactory.GetOrderableItemsUnitOfWork(testData);

         uow.AddOrderableItemsToOrder(order, new List<OrderableItem> { orderableItem }, out errorMessage, Username);

         Assert.IsTrue(string.IsNullOrWhiteSpace(errorMessage), errorMessage);
         var orderItem = order.Items.Single();
         Assert.AreEqual(Product.ProductId, orderItem.ProductId, "Product Id");
         Assert.AreEqual(2, orderItem.Quantity, "Quantity");
      }

      [TestMethod]
      public void CreateOrderFrom_RequestValid_OrderItemSourcePopulated()
      {
         var order = Order;
         order.Items.Clear();
         string errorMessage;


         var orderableItem = new OrderableItem
         {
            ProductId = Product.ProductId,
            Quantity = 1,
            RequestIds = new List<int> { 1 },
            Source = OrderableItemSource.Request,
         };

         var testData = new MockedInventoryOrderableItemsData(new List<StockAdjustment>(),
            new List<DeductionRequiringPaymentClass>(), new List<Product> { Product }, new List<OrderItemSource>(),
            new List<Order> { order }, new List<NegativeStock>(), new List<ProductStockRequest> { StockRequest }, new List<ConsumptionNotificationManagement>(), new List<HealthTrackConsumption>());
         var uow = MockedInventoryUnitOfWorkFactory.GetOrderableItemsUnitOfWork(testData);

         uow.AddOrderableItemsToOrder(order, new List<OrderableItem> { orderableItem }, out errorMessage, Username);

         Assert.IsTrue(string.IsNullOrWhiteSpace(errorMessage), errorMessage);
         var orderItem = order.Items.Single();
         var orderItemSource = orderItem.OrderItemSources.Single();
         Assert.IsNotNull(orderItemSource.StockRequestId);
         Assert.AreEqual(StockRequest.StockRequestId, orderItemSource.StockRequestId.Value);
         Assert.AreEqual(StockRequest.ApprovedQuantity, orderItemSource.Quantity);
         Assert.IsNull(orderItemSource.invUsed_ID);
      }

      [TestMethod]
      public void CreateOrderFrom_RequestValid_NoProduct_NoOrderItemsAdded()
      {
         var order = Order;
         order.Items.Clear();
         string errorMessage;

         var orderableItem = new OrderableItem
         {
            ProductId = Product.ProductId,
            Quantity = 1,
            RequestIds = new List<int> { 1 },
            Source = OrderableItemSource.Request,
         };

         var testData = new MockedInventoryOrderableItemsData(new List<StockAdjustment>(),
            new List<DeductionRequiringPaymentClass>(), new List<Product>(), new List<OrderItemSource>(),
            new List<Order> { order }, new List<NegativeStock>(), new List<ProductStockRequest> { StockRequest }, new List<ConsumptionNotificationManagement>(), new List<HealthTrackConsumption>());
         var uow = MockedInventoryUnitOfWorkFactory.GetOrderableItemsUnitOfWork(testData);

         uow.AddOrderableItemsToOrder(order, new List<OrderableItem> { orderableItem }, out errorMessage, Username);

         Assert.IsFalse(string.IsNullOrWhiteSpace(errorMessage), errorMessage);
         Assert.AreEqual(0, order.Items.Count());
      }

      [TestMethod]
      public void CreateOrderFrom_RequestInvalid_IdDoesNotExist()
      {
         var order = Order;
         order.Items.Clear();
         string errorMessage;

         var orderableItem = new OrderableItem
         {
            ProductId = Product.ProductId,
            Quantity = 1,
            RequestIds = new List<int> { 9 },
            Source = OrderableItemSource.Request,
         };

         var testData = new MockedInventoryOrderableItemsData(new List<StockAdjustment>(),
            new List<DeductionRequiringPaymentClass>(), new List<Product>(), new List<OrderItemSource>(),
            new List<Order> { order }, new List<NegativeStock>(), new List<ProductStockRequest> { StockRequest }, new List<ConsumptionNotificationManagement>(), new List<HealthTrackConsumption>());
         var uow = MockedInventoryUnitOfWorkFactory.GetOrderableItemsUnitOfWork(testData);

         uow.AddOrderableItemsToOrder(order, new List<OrderableItem> { orderableItem }, out errorMessage, Username);

         Assert.IsFalse(string.IsNullOrWhiteSpace(errorMessage), errorMessage);
         Assert.AreEqual(0, order.Items.Count());
      }

      [TestMethod]
      public void CreateOrderFrom_RequestInvalid_NoApprovedQuantity()
      {
         var order = Order;
         order.Items.Clear();
         string errorMessage;

         var request = StockRequest;
         request.ApprovedQuantity = null;

         var orderableItem = new OrderableItem
         {
            ProductId = Product.ProductId,
            Quantity = 1,
            RequestIds = new List<int> { 9 },
            Source = OrderableItemSource.Request,
         };

         var testData = new MockedInventoryOrderableItemsData(new List<StockAdjustment>(),
            new List<DeductionRequiringPaymentClass>(), new List<Product>(), new List<OrderItemSource>(),
            new List<Order> { order }, new List<NegativeStock>(), new List<ProductStockRequest> { request }, new List<ConsumptionNotificationManagement>(), new List<HealthTrackConsumption>());
         var uow = MockedInventoryUnitOfWorkFactory.GetOrderableItemsUnitOfWork(testData);

         uow.AddOrderableItemsToOrder(order, new List<OrderableItem> { orderableItem }, out errorMessage, Username);

         Assert.IsFalse(string.IsNullOrWhiteSpace(errorMessage), errorMessage);
         Assert.AreEqual(0, order.Items.Count());
      }

      [TestMethod]
      public void CreateOrderFrom_RequestInvalid_RequestNotApproved()
      {
         var order = Order;
         order.Items.Clear();
         string errorMessage;

         var request = StockRequest;
         request.RequestStatus = RequestStatus.Open;

         var orderableItem = new OrderableItem
         {
            ProductId = Product.ProductId,
            Quantity = 1,
            RequestIds = new List<int> { 9 },
            Source = OrderableItemSource.Request,
         };

         var testData = new MockedInventoryOrderableItemsData(new List<StockAdjustment>(),
            new List<DeductionRequiringPaymentClass>(), new List<Product>(), new List<OrderItemSource>(),
            new List<Order> { order }, new List<NegativeStock>(), new List<ProductStockRequest> { request }, new List<ConsumptionNotificationManagement>(), new List<HealthTrackConsumption>());
         var uow = MockedInventoryUnitOfWorkFactory.GetOrderableItemsUnitOfWork(testData);

         uow.AddOrderableItemsToOrder(order, new List<OrderableItem> { orderableItem }, out errorMessage, Username);

         Assert.IsFalse(string.IsNullOrWhiteSpace(errorMessage), errorMessage);
         Assert.AreEqual(0, order.Items.Count());
      }

      [TestMethod]
      public void CreateOrderFrom_TopUp()
      {
         var order = Order;
         order.Items.Clear();
         string errorMessage;

         var orderableItem = new OrderableItem
         {
            ProductId = Product.ProductId,
            Quantity = 1,
            Source = OrderableItemSource.Topup,
         };

         var testData = new MockedInventoryOrderableItemsData(new List<StockAdjustment>(),
            new List<DeductionRequiringPaymentClass>(), new List<Product> { Product }, new List<OrderItemSource>(),
            new List<Order> { order }, new List<NegativeStock>(), new List<ProductStockRequest>(), new List<ConsumptionNotificationManagement>(), new List<HealthTrackConsumption>());
         var uow = MockedInventoryUnitOfWorkFactory.GetOrderableItemsUnitOfWork(testData);

         uow.AddOrderableItemsToOrder(order, new List<OrderableItem> { orderableItem }, out errorMessage, Username);

         Assert.IsTrue(string.IsNullOrWhiteSpace(errorMessage), errorMessage);
         var orderItem = order.Items.Single();
         Assert.AreEqual(Product.ProductId, orderItem.ProductId, "Product Id");
         Assert.AreEqual(orderableItem.Quantity, orderItem.Quantity, "Quantity");
      }

      [TestMethod]
      public void CreateOrderFrom_TopUp_TopsUpOrderItemQuantity()
      {
         var order = Order;
         string errorMessage;

         var orderableItem = new OrderableItem
         {
            ProductId = Product.ProductId,
            Quantity = 1,
            Source = OrderableItemSource.Topup,
         };

         var testData = new MockedInventoryOrderableItemsData(new List<StockAdjustment>(),
            new List<DeductionRequiringPaymentClass>(), new List<Product> { Product }, new List<OrderItemSource>(),
            new List<Order> { order }, new List<NegativeStock>(), new List<ProductStockRequest>(), new List<ConsumptionNotificationManagement>(), new List<HealthTrackConsumption>());
         var uow = MockedInventoryUnitOfWorkFactory.GetOrderableItemsUnitOfWork(testData);

         uow.AddOrderableItemsToOrder(order, new List<OrderableItem> { orderableItem }, out errorMessage, Username);

         Assert.IsTrue(string.IsNullOrWhiteSpace(errorMessage), errorMessage);
         var orderItem = order.Items.Single();
         Assert.AreEqual(Product.ProductId, orderItem.ProductId, "Product Id");
         Assert.AreEqual(2, orderItem.Quantity, "Quantity");
      }

      [TestMethod]
      public void CreateOrderFrom_TopUp_CreatesOrderItemSource()
      {
         var order = Order;
         string errorMessage;

         var orderableItem = new OrderableItem
         {
            ProductId = Product.ProductId,
            Quantity = 1,
            Source = OrderableItemSource.Topup,
         };

         var testData = new MockedInventoryOrderableItemsData(new List<StockAdjustment>(),
            new List<DeductionRequiringPaymentClass>(), new List<Product> { Product }, new List<OrderItemSource>(),
            new List<Order> { order }, new List<NegativeStock>(), new List<ProductStockRequest>(), new List<ConsumptionNotificationManagement>(), new List<HealthTrackConsumption>());
         var uow = MockedInventoryUnitOfWorkFactory.GetOrderableItemsUnitOfWork(testData);

         uow.AddOrderableItemsToOrder(order, new List<OrderableItem> { orderableItem }, out errorMessage, Username);

         Assert.IsTrue(string.IsNullOrWhiteSpace(errorMessage), errorMessage);
         var orderItem = order.Items.Single();
         Assert.AreEqual(1, orderItem.OrderItemSources.Count());
         var orderItemSource = orderItem.OrderItemSources.First();
         Assert.AreEqual(orderableItem.Quantity, orderItemSource.Quantity);
         Assert.IsNull(orderItemSource.invUsed_ID);
         Assert.IsNull(orderItemSource.StockRequestId);
      }


      [TestMethod]
      public void CreateOrderFrom_Topups_MultipleProducts_TwoOrderItems()
      {
         var order = Order;
         order.Items.Clear();
         string errorMessage;

         var orderableItem = new OrderableItem
         {
            ProductId = Product.ProductId,
            Quantity = 1,
            Source = OrderableItemSource.Topup,
         };
         var productv2 = Product;
         productv2.ProductId = 999;

         var orderableItem2 = new OrderableItem
         {
            ProductId = 999,
            Quantity = 1,
            Source = OrderableItemSource.Topup,
         };

         var testData = new MockedInventoryOrderableItemsData(new List<StockAdjustment>(),
            new List<DeductionRequiringPaymentClass>(), new List<Product> { Product, productv2 }, new List<OrderItemSource>(),
            new List<Order> { order }, new List<NegativeStock>(), new List<ProductStockRequest>(), new List<ConsumptionNotificationManagement>(), new List<HealthTrackConsumption>());
         var uow = MockedInventoryUnitOfWorkFactory.GetOrderableItemsUnitOfWork(testData);

         uow.AddOrderableItemsToOrder(order, new List<OrderableItem> { orderableItem, orderableItem2 }, out errorMessage, Username);

         Assert.IsTrue(string.IsNullOrWhiteSpace(errorMessage), errorMessage);
         Assert.AreEqual(2, order.Items.Count());
      }

      [TestMethod]
      public void CreateOrderFrom_Replacement()
      {
         var order = Order;
         order.Items.Clear();
         string errorMessage;

         var orderableItem = new OrderableItem
         {
            ConsumptionId = 1,
            ProductId = Product.ProductId,
            Quantity = 1,
            Source = OrderableItemSource.Replacement,
         };

         var testData = new MockedInventoryOrderableItemsData(new List<StockAdjustment>(),
            new List<DeductionRequiringPaymentClass>(), new List<Product> { Product }, new List<OrderItemSource>(),
            new List<Order> { order }, new List<NegativeStock>(), new List<ProductStockRequest>(), new List<ConsumptionNotificationManagement> { Management }, new List<HealthTrackConsumption> { HealthTrackConsumption });
         var uow = MockedInventoryUnitOfWorkFactory.GetOrderableItemsUnitOfWork(testData);

         uow.AddOrderableItemsToOrder(order, new List<OrderableItem> { orderableItem }, out errorMessage, Username);

         Assert.IsTrue(string.IsNullOrWhiteSpace(errorMessage), errorMessage);
         var orderItem = order.Items.Single();
         Assert.AreEqual(Product.ProductId, orderItem.ProductId, "Product Id");
         Assert.AreEqual(HealthTrackConsumption.Quantity.Value, orderItem.Quantity, "Quantity");
      }

      [TestMethod]
      public void CreateOrderFrom_Replacement_NoConsumption()
      {
         var order = Order;
         order.Items.Clear();
         string errorMessage;

         var orderableItem = new OrderableItem
         {
            ConsumptionId = 3,
            ProductId = Product.ProductId,
            Quantity = 1,
            Source = OrderableItemSource.Replacement,
         };

         var testData = new MockedInventoryOrderableItemsData(new List<StockAdjustment>(),
            new List<DeductionRequiringPaymentClass>(), new List<Product> { Product }, new List<OrderItemSource>(),
            new List<Order> { order }, new List<NegativeStock>(), new List<ProductStockRequest>(), new List<ConsumptionNotificationManagement> { Management }, new List<HealthTrackConsumption> { HealthTrackConsumption });
         var uow = MockedInventoryUnitOfWorkFactory.GetOrderableItemsUnitOfWork(testData);

         uow.AddOrderableItemsToOrder(order, new List<OrderableItem> { orderableItem }, out errorMessage, Username);

         Assert.IsFalse(string.IsNullOrWhiteSpace(errorMessage), errorMessage);
         Assert.AreEqual(0, order.Items.Count());
      }

      [TestMethod]
      public void CreateOrderFrom_Replacement_OrderItemSourcePopulated()
      {
         var order = Order;
         order.Items.Clear();
         string errorMessage;

         var orderableItem = new OrderableItem
         {
            ConsumptionId = 1,
            ProductId = Product.ProductId,
            Quantity = 1,
            Source = OrderableItemSource.Replacement,
         };

         var testData = new MockedInventoryOrderableItemsData(new List<StockAdjustment>(),
            new List<DeductionRequiringPaymentClass>(), new List<Product> { Product }, new List<OrderItemSource>(),
            new List<Order> { order }, new List<NegativeStock>(), new List<ProductStockRequest>(), new List<ConsumptionNotificationManagement> { Management }, new List<HealthTrackConsumption> { HealthTrackConsumption });
         var uow = MockedInventoryUnitOfWorkFactory.GetOrderableItemsUnitOfWork(testData);

         uow.AddOrderableItemsToOrder(order, new List<OrderableItem> { orderableItem }, out errorMessage, Username);

         Assert.IsTrue(string.IsNullOrWhiteSpace(errorMessage), errorMessage);
         var orderItem = order.Items.Single();
         Assert.AreEqual(1, orderItem.OrderItemSources.Count());
         var source = orderItem.OrderItemSources.Single();
         Assert.AreEqual(orderableItem.ConsumptionId, source.invUsed_ID);
         Assert.AreEqual((Int32)HealthTrackConsumption.Quantity, source.Quantity, "Quantity");
         Assert.IsNull(source.StockRequestId);
      }

      [TestMethod]
      public void CreateOrderFrom_Replacement_TopsUpOrderItem()
      {
         var order = Order;
         string errorMessage;

         var orderableItem = new OrderableItem
         {
            ConsumptionId = 1,
            ProductId = Product.ProductId,
            Quantity = 1,
            Source = OrderableItemSource.Replacement,
         };

         var testData = new MockedInventoryOrderableItemsData(new List<StockAdjustment>(),
            new List<DeductionRequiringPaymentClass>(), new List<Product> { Product }, new List<OrderItemSource>(),
            new List<Order> { order }, new List<NegativeStock>(), new List<ProductStockRequest>(), new List<ConsumptionNotificationManagement> { Management }, new List<HealthTrackConsumption> { HealthTrackConsumption });
         var uow = MockedInventoryUnitOfWorkFactory.GetOrderableItemsUnitOfWork(testData);

         uow.AddOrderableItemsToOrder(order, new List<OrderableItem> { orderableItem }, out errorMessage, Username);

         Assert.IsTrue(string.IsNullOrWhiteSpace(errorMessage), errorMessage);
         var orderItem = order.Items.Single();
         Assert.AreEqual(2, orderItem.Quantity);
      }

      [TestMethod]
      public void CreateOrderFrom_Replacement_TopsUpOrderItem_QuantityGreaterThanOne()
      {
         var order = Order;
         string errorMessage;

         var consumption = HealthTrackConsumption;
         consumption.Quantity = 2;
         var orderableItem = new OrderableItem
         {
            ConsumptionId = 1,
            ProductId = Product.ProductId,
            Quantity = 2,
            Source = OrderableItemSource.Replacement,
         };

         var testData = new MockedInventoryOrderableItemsData(new List<StockAdjustment>(),
            new List<DeductionRequiringPaymentClass>(), new List<Product> { Product }, new List<OrderItemSource>(),
            new List<Order> { order }, new List<NegativeStock>(), new List<ProductStockRequest>(),
            new List<ConsumptionNotificationManagement> { Management }, new List<HealthTrackConsumption> { consumption });
         var uow = MockedInventoryUnitOfWorkFactory.GetOrderableItemsUnitOfWork(testData);

         uow.AddOrderableItemsToOrder(order, new List<OrderableItem> { orderableItem }, out errorMessage, Username);

         Assert.IsTrue(string.IsNullOrWhiteSpace(errorMessage), errorMessage);
         var orderItem = order.Items.Single();
         Assert.AreEqual(3, orderItem.Quantity);
      }

      [TestMethod]
      public void CreateOrderFrom_Replacement_ConsumptionNotificationManagementArchived()
      {
         var order = Order;
         string errorMessage;
         var management = Management;

         var orderableItem = new OrderableItem
         {
            ConsumptionId = 1,
            ProductId = Product.ProductId,
            Quantity = 1,
            Source = OrderableItemSource.Replacement,
         };

         var testData = new MockedInventoryOrderableItemsData(new List<StockAdjustment>(),
            new List<DeductionRequiringPaymentClass>(), new List<Product> { Product }, new List<OrderItemSource>(),
            new List<Order> { order }, new List<NegativeStock>(), new List<ProductStockRequest>(),
            new List<ConsumptionNotificationManagement> { management },
            new List<HealthTrackConsumption> { HealthTrackConsumption });
         var uow = MockedInventoryUnitOfWorkFactory.GetOrderableItemsUnitOfWork(testData);

         uow.AddOrderableItemsToOrder(order, new List<OrderableItem> { orderableItem }, out errorMessage, Username);

         Assert.IsTrue(string.IsNullOrWhiteSpace(errorMessage), errorMessage);
         Assert.AreEqual(orderableItem.ConsumptionId, management.invUsed_ID);
         Assert.IsNotNull(management.ArchivedBy, "Archived By");
         Assert.IsNotNull(management.ArchivedOn, "Archived On");
      }

      [TestMethod]
      public void CreateOrderFrom_Invoice_NoOrderItemAdded()
      {
         var order = Order;
         order.Items.Clear();
         string errorMessage;

         var orderableItem = new OrderableItem
         {
            ConsumptionId = 1,
            ProductId = Product.ProductId,
            Quantity = 1,
            Source = OrderableItemSource.Invoice,
         };

         var testData = new MockedInventoryOrderableItemsData(new List<StockAdjustment>(),
            new List<DeductionRequiringPaymentClass>(), new List<Product> { Product }, new List<OrderItemSource>(),
            new List<Order> { order }, new List<NegativeStock>(), new List<ProductStockRequest>(), new List<ConsumptionNotificationManagement> { Management }, new List<HealthTrackConsumption> { HealthTrackConsumption });
         var uow = MockedInventoryUnitOfWorkFactory.GetOrderableItemsUnitOfWork(testData);

         uow.AddOrderableItemsToOrder(order, new List<OrderableItem> { orderableItem }, out errorMessage, Username);

         Assert.AreEqual(0, order.Items.Count());
      }

      #endregion
   }
}