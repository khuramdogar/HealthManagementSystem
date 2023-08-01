using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using Moq;

namespace HMS.HealthTrack.Inventory.Test.Inventory.Mocks
{
   internal sealed class MockedInventoryOrderItemData : MockedInventoryData
   {
      public Mock<IDbSet<OrderItem>> MockedOrderItems { get; set; }
      public Mock<IDbSet<OrderItemSource>> MockedOrderItemSources { get; set; }
      public Mock<IDbSet<Product>> MockedProducts { get; set; }
      public Mock<IDbSet<Stock>> MockedStock { get; set; }
      public Mock<IDbSet<StockAdjustment>> MockedStockAdjustments { get; set; }
      public Mock<IDbSet<StockAdjustmentStock>> MockedStockAdjustmentStock { get; set; }

      public MockedInventoryOrderItemData(IEnumerable<OrderItem> orderItems,
         IEnumerable<OrderItemSource> orderItemSources, IEnumerable<Product> products, IEnumerable<Stock> stocks,
         IEnumerable<StockAdjustment> stockAdjustments, IEnumerable<StockAdjustmentStock> stockAdjustmentStocks)
      {
         MockedOrderItems = MockingHelper.GetMockedDbSet(orderItems.AsQueryable());
         MockedOrderItemSources = MockingHelper.GetMockedDbSet(orderItemSources.AsQueryable());
         MockedProducts = MockingHelper.GetMockedDbSet(products.AsQueryable());
         MockedStock = MockingHelper.GetMockedDbSet(stocks.AsQueryable());
         MockedStockAdjustments = MockingHelper.GetMockedDbSet(stockAdjustments.AsQueryable());
         MockedStockAdjustmentStock = MockingHelper.GetMockedDbSet(stockAdjustmentStocks.AsQueryable());

         MockedContext.Setup(c => c.OrderItems).Returns(MockedOrderItems.Object);
         MockedContext.Setup(c => c.OrderItemSources).Returns(MockedOrderItemSources.Object);
         MockedContext.Setup(c => c.Products).Returns(MockedProducts.Object);
         MockedContext.Setup(c => c.Stocks).Returns(MockedStock.Object);
         MockedContext.Setup(c => c.StockAdjustments).Returns(MockedStockAdjustments.Object);
         MockedContext.Setup(c => c.StockAdjustmentStocks).Returns(MockedStockAdjustmentStock.Object);
      }

      public MockedInventoryOrderItemData()
         : this(
            new List<OrderItem>(), new List<OrderItemSource>(), new List<Product>(), new List<Stock>(),
            new List<StockAdjustment>(), new List<StockAdjustmentStock>())
      {
      }
   }
}