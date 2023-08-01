using System.Collections.Generic;
using System.Linq;
using HMS.HealthTrack.Inventory.Test.Inventory.Mocks;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HMS.HealthTrack.Inventory.Test.Inventory
{
   [TestClass]
   public class LowStockTests
   {
      [TestMethod]
      public void GetLowStock_ReturnsLowStockWithoutPendingOrders()
      {
         var product = new Product
         {
            ManageStock = true,
            ProductId = 123,
            ReorderThreshold = 5,
            TargetStockLevel = 10
         };

         var stock = new Stock
         {
            ProductId = 123,
            Product = product,
            Quantity = 4
         };
         product.Stocks.Add(stock);

         var testData = new MockedInventoryLowStockData(new List<Product> { product }, new List<Stock> { stock }, new List<OrderItemSource>());

         var repo = MockedInventoryRepositoryFactory.GetStockRepository(testData);

         var lowStock = repo.GetLowStock();

         Assert.AreEqual(1, lowStock.Count());
      }

      [TestMethod]
      public void GetLowStock_ReturnsLowStockWithPendingOrderQuantitiesBelowThreshold()
      {
         var product = new Product
         {
            ManageStock = true,
            ProductId = 123,
            ReorderThreshold = 5,
            TargetStockLevel = 10
         };

         var stock = new Stock
         {
            ProductId = 123,
            Product = product,
            Quantity = 4
         };
         product.Stocks.Add(stock);

         var orderItem = new OrderItem
         {
            OrderItemId = 1,
            ProductId = 123,
            Product = product,
            Quantity = 1,
            Status = OrderItemStatus.Ordered,
         };

         var orderItemSource = new OrderItemSource
         {
            ItemSourceId = 1,
            Quantity = 1,
            OrderItem = orderItem,
            OrderItemId = 1
         };
         orderItem.OrderItemSources.Add(orderItemSource);

         var testData = new MockedInventoryLowStockData(new List<Product> { product }, new List<Stock> { stock }, new List<OrderItemSource> { orderItemSource });

         var repo = MockedInventoryRepositoryFactory.GetStockRepository(testData);

         var lowStock = repo.GetLowStock();

         Assert.AreEqual(1, lowStock.Count());
      }

      [TestMethod]
      public void GetLowStock_IgnoresProductsWithoutReorderThreshold()
      {
         var product = new Product
         {
            ProductId = 123,
            TargetStockLevel = 10
         };

         var stock = new Stock
         {
            ProductId = 123,
            Product = product,
            Quantity = 4
         };
         product.Stocks.Add(stock);

         var orderItem = new OrderItem
         {
            OrderItemId = 1,
            ProductId = 123,
            Product = product,
            Quantity = 1,
            Status = OrderItemStatus.Ordered,
         };

         var orderItemSource = new OrderItemSource
         {
            ItemSourceId = 1,
            Quantity = 1,
            OrderItem = orderItem,
            OrderItemId = 1
         };
         orderItem.OrderItemSources.Add(orderItemSource);

         var testData = new MockedInventoryLowStockData(new List<Product> { product }, new List<Stock> { stock }, new List<OrderItemSource> { orderItemSource });

         var repo = MockedInventoryRepositoryFactory.GetStockRepository(testData);

         var lowStock = repo.GetLowStock();

         Assert.AreEqual(0, lowStock.Count());
      }

      [TestMethod]
      public void GetLowStock_IgnoresProductsWithoutTargetLevel()
      {
         var product = new Product
         {
            ProductId = 123,
            ReorderThreshold = 5
         };

         var stock = new Stock
         {
            ProductId = 123,
            Product = product,
            Quantity = 4
         };
         product.Stocks.Add(stock);

         var orderItem = new OrderItem
         {
            OrderItemId = 1,
            ProductId = 123,
            Product = product,
            Quantity = 1,
            Status = OrderItemStatus.Ordered,
         };

         var orderItemSource = new OrderItemSource
         {
            ItemSourceId = 1,
            Quantity = 1,
            OrderItem = orderItem,
            OrderItemId = 1
         };
         orderItem.OrderItemSources.Add(orderItemSource);

         var testData = new MockedInventoryLowStockData(new List<Product> { product }, new List<Stock> { stock }, new List<OrderItemSource> { orderItemSource });

         var repo = MockedInventoryRepositoryFactory.GetStockRepository(testData);

         var lowStock = repo.GetLowStock();

         Assert.AreEqual(0, lowStock.Count());
      }

      [TestMethod]
      public void GetLowStock_IgnoresProductsWithoutReorderThresholdAndTargetLevel()
      {
         var product = new Product
         {
            ProductId = 123,
         };

         var stock = new Stock
         {
            ProductId = 123,
            Product = product,
            Quantity = 4
         };
         product.Stocks.Add(stock);

         var orderItem = new OrderItem
         {
            OrderItemId = 1,
            ProductId = 123,
            Product = product,
            Quantity = 1,
            Status = OrderItemStatus.Ordered,
         };

         var orderItemSource = new OrderItemSource
         {
            ItemSourceId = 1,
            Quantity = 1,
            OrderItem = orderItem,
            OrderItemId = 1
         };
         orderItem.OrderItemSources.Add(orderItemSource);

         var testData = new MockedInventoryLowStockData(new List<Product> { product }, new List<Stock> { stock }, new List<OrderItemSource> { orderItemSource });

         var repo = MockedInventoryRepositoryFactory.GetStockRepository(testData);

         var lowStock = repo.GetLowStock();

         Assert.AreEqual(0, lowStock.Count());
      }

      [TestMethod]
      public void GetLowStock_IgnoresProductsWithPendingOrderQuantitesAboveThreshold()
      {
         var product = new Product
         {
            ProductId = 123,
            ReorderThreshold = 5,
            TargetStockLevel = 10
         };

         var stock = new Stock
         {
            ProductId = 123,
            Product = product,
            Quantity = 4,
         };

         product.Stocks.Add(stock);

         var orderItem = new OrderItem
         {
            OrderItemId = 1,
            ProductId = 123,
            Product = product,
            Quantity = 2,
            Status = OrderItemStatus.Ordered,
         };
         product.OrderItems.Add(orderItem);

         var orderItemSource = new OrderItemSource
         {
            ItemSourceId = 1,
            Quantity = 2,
            OrderItem = orderItem,
            OrderItemId = 1
         };
         orderItem.OrderItemSources.Add(orderItemSource);

         var testData = new MockedInventoryLowStockData(new List<Product> { product }, new List<Stock> { stock }, new List<OrderItemSource> { orderItemSource });

         var repo = MockedInventoryRepositoryFactory.GetStockRepository(testData);

         var lowStock = repo.GetLowStock();

         Assert.AreEqual(0, lowStock.Count());
      }

      [TestMethod]
      public void GetLowStock_ReturnsProductWithNoStock()
      {
         var product = new Product
         {
            ManageStock = true,
            ProductId = 123,
            ReorderThreshold = 5,
            TargetStockLevel = 10
         };

         var testData = new MockedInventoryLowStockData(new List<Product> { product }, new List<Stock>(), new List<OrderItemSource>());

         var repo = MockedInventoryRepositoryFactory.GetStockRepository(testData);

         var lowStock = repo.GetLowStock();
         Assert.AreEqual(1, lowStock.Count());
      }

      [TestMethod]
      public void GetLowStock_IgnoresUnavailableStock()
      {
         var product = new Product
         {
            ManageStock = true,
            ProductId = 123,
            ReorderThreshold = 5,
            TargetStockLevel = 10
         };

         var stock = new Stock
         {
            ProductId = 123,
            Product = product,
            Quantity = 4,
            StockStatus = StockStatus.Available
         };
         product.Stocks.Add(stock);

         var unavailableStock = new Stock
         {
            ProductId = 123,
            Product = product,
            Quantity = 10,
            StockStatus = StockStatus.Removed
         };

         product.Stocks.Add(unavailableStock);

         var testData = new MockedInventoryLowStockData(new List<Product> { product }, new List<Stock> { stock }, new List<OrderItemSource>());

         var repo = MockedInventoryRepositoryFactory.GetStockRepository(testData);

         var lowStock = repo.GetLowStock();

         Assert.AreEqual(1, lowStock.Count());
      }

      [TestMethod]
      public void GetLowStock_EqualThresholdAndTarget_StockAboveTarget_ReturnsNoLowStock()
      {
         var product = new Product
         {
            ProductId = 123,
            ReorderThreshold = 5,
            TargetStockLevel = 5
         };

         var stock = new Stock
         {
            ProductId = 123,
            Product = product,
            Quantity = 6
         };
         product.Stocks.Add(stock);

         var testData = new MockedInventoryLowStockData(new List<Product> { product }, new List<Stock> { stock }, new List<OrderItemSource> { });

         var repo = MockedInventoryRepositoryFactory.GetStockRepository(testData);

         var lowStock = repo.GetLowStock();

         Assert.AreEqual(0, lowStock.Count());
      }

      [TestMethod]
      public void GetLowStock_EqualThresholdAndTarget_StockAtTarget_ReturnsNoLowStock()
      {
         var product = new Product
         {
            ProductId = 123,
            ReorderThreshold = 5,
            TargetStockLevel = 5
         };

         var stock = new Stock
         {
            ProductId = 123,
            Product = product,
            Quantity = 5
         };
         product.Stocks.Add(stock);

         var testData = new MockedInventoryLowStockData(new List<Product> { product }, new List<Stock> { stock }, new List<OrderItemSource> { });

         var repo = MockedInventoryRepositoryFactory.GetStockRepository(testData);

         var lowStock = repo.GetLowStock();

         Assert.AreEqual(0, lowStock.Count());
      }

      [TestMethod]
      public void GetLowStock_EqualThresholdAndTarget_StockBelowThreshold_ManagedStock_ReturnsLowStock()
      {
         var product = new Product
         {
            AutoReorderSetting = ReorderSettings.SpecifyLevels,
            ProductId = 123,
            ManageStock = true,
            ReorderThreshold = 5,
            TargetStockLevel = 5
         };

         var stock = new Stock
         {
            ProductId = 123,
            Product = product,
            Quantity = 4
         };
         product.Stocks.Add(stock);

         var testData = new MockedInventoryLowStockData(new List<Product> { product }, new List<Stock> { stock }, new List<OrderItemSource> { });

         var repo = MockedInventoryRepositoryFactory.GetStockRepository(testData);

         var lowStock = repo.GetLowStock();

         Assert.AreEqual(1, lowStock.Count());
         Assert.AreEqual(4, lowStock.First().StockCount, "Low stock level");
      }

      [TestMethod]
      public void GetLowStock_EqualThresholdAndTarget_StockBelowThreshold_UnmanagedStock_ReturnsNoStock()
      {
         var product = new Product
         {
            ManageStock = false,
            ProductId = 123,
            ReorderThreshold = 5,
            TargetStockLevel = 5
         };

         var stock = new Stock
         {
            ProductId = 123,
            Product = product,
            Quantity = 4
         };
         product.Stocks.Add(stock);

         var testData = new MockedInventoryLowStockData(new List<Product> { product }, new List<Stock> { stock }, new List<OrderItemSource> { });

         var repo = MockedInventoryRepositoryFactory.GetStockRepository(testData);

         var lowStock = repo.GetLowStock();

         Assert.AreEqual(0, lowStock.Count());
      }

      [TestMethod]
      public void GetLowStock_ThresholdBelowTarget_StockAboveThreshold_ReturnsNoLowStock() // mirrors one for one replace
      {
         var product = new Product
        {
           ProductId = 123,
           ReorderThreshold = 4,
           TargetStockLevel = 5
        };

         var stock = new Stock
         {
            ProductId = 123,
            Product = product,
            Quantity = 5
         };
         product.Stocks.Add(stock);

         var testData = new MockedInventoryLowStockData(new List<Product> { product }, new List<Stock> { stock }, new List<OrderItemSource> { });

         var repo = MockedInventoryRepositoryFactory.GetStockRepository(testData);

         var lowStock = repo.GetLowStock();

         Assert.AreEqual(0, lowStock.Count(), "Low stock level");
      }

      [TestMethod]
      public void GetLowStock_ThresholdBelowTarget_StockAtThreshold_ReturnsLowStock() // mirrors one for one replace
      {
         var product = new Product
         {
            ManageStock = true,
            ProductId = 123,
            ReorderThreshold = 4,
            TargetStockLevel = 5
         };

         var stock = new Stock
         {
            ProductId = 123,
            Product = product,
            Quantity = 4
         };
         product.Stocks.Add(stock);

         var testData = new MockedInventoryLowStockData(new List<Product> { product }, new List<Stock> { stock }, new List<OrderItemSource> { });

         var repo = MockedInventoryRepositoryFactory.GetStockRepository(testData);

         var lowStock = repo.GetLowStock();

         Assert.AreEqual(1, lowStock.Count());
         Assert.AreEqual(4, lowStock.First().StockCount, "Low stock level");
      }

      [TestMethod]
      public void GetLowStock_ThresholdBelowTarget_StockBelowThreshold_ReturnsLowStock() // mirrors one for one replace
      {
         var product = new Product
         {
            ManageStock = true,
            ProductId = 123,
            ReorderThreshold = 4,
            TargetStockLevel = 5
         };

         var stock = new Stock
         {
            ProductId = 123,
            Product = product,
            Quantity = 3
         };
         product.Stocks.Add(stock);

         var testData = new MockedInventoryLowStockData(new List<Product> { product }, new List<Stock> { stock }, new List<OrderItemSource> { });

         var repo = MockedInventoryRepositoryFactory.GetStockRepository(testData);

         var lowStock = repo.GetLowStock();

         Assert.AreEqual(1, lowStock.Count());
         Assert.AreEqual(3, lowStock.First().StockCount, "Low stock level");
      }
   }
}
