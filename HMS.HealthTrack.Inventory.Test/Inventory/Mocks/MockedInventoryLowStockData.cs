using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using Moq;

namespace HMS.HealthTrack.Inventory.Test.Inventory.Mocks
{
   internal sealed class MockedInventoryLowStockData : MockedInventoryData
   {

      public Mock<IDbSet<OrderItemSource>> MockedOrderItemSources { get; set; }

      public Mock<IDbSet<Product>> MockedProducts { get; set; }

      public Mock<IDbSet<Stock>> MockedStock { get; set; }

      public MockedInventoryLowStockData(IEnumerable<Product> products, IEnumerable<Stock> stock, IEnumerable<OrderItemSource> orderItemSources)
      {
         //Setup
         MockedStock = MockingHelper.GetMockedDbSet(stock.AsQueryable());
         MockedProducts = MockingHelper.GetMockedDbSet(products.AsQueryable());
         MockedOrderItemSources = MockingHelper.GetMockedDbSet(orderItemSources.AsQueryable());

         //Mocked data context
         MockedContext.Setup(c => c.Stocks).Returns(MockedStock.Object);
         MockedContext.Setup(c => c.Products).Returns(MockedProducts.Object); //Products
         MockedContext.Setup(c => c.OrderItemSources).Returns(MockedOrderItemSources.Object);
      }
      public MockedInventoryLowStockData()
         : this(new List<Product>(), new List<Stock>(), new List<OrderItemSource>())
      {
      }
   }
}