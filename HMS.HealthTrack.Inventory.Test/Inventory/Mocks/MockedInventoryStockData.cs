using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using Moq;

namespace HMS.HealthTrack.Inventory.Test.Inventory.Mocks
{
   internal sealed class MockedInventoryStockData : MockedInventoryData
   {

      public Mock<IDbSet<StockAdjustment>> MockedAdjustments { get; set; }

      public Mock<IDbSet<Product>> MockedProducts { get; set; }

      public Mock<IDbSet<Stock>> MockedStock { get; set; }

      public Mock<IDbSet<StockTakeItem>> MockedStockTakeItems { get; set; }

      public Mock<IDbSet<StockTake>> MockedStockTakes { get; set; }

      public Mock<IDbSet<StockSet>> MockedStockSets { get; set; }

      public MockedInventoryStockData(IEnumerable<Product> products, IEnumerable<Stock> stock, List<StockAdjustment> stockDeductions)
      {
         //Setup
         MockedStock = MockingHelper.GetMockedDbSet(stock.AsQueryable());
         MockedProducts = MockingHelper.GetMockedDbSet(products.AsQueryable());
         MockedAdjustments = MockingHelper.GetMockedDbSet(stockDeductions.AsQueryable());
         MockedStockTakeItems = MockingHelper.GetMockedDbSet(new List<StockTakeItem>().AsQueryable());
         MockedStockTakes = MockingHelper.GetMockedDbSet(new List<StockTake>().AsQueryable());
         MockedStockSets = MockingHelper.GetMockedDbSet(new List<StockSet>().AsQueryable());

         //Mocked data context
         MockedContext.Setup(c => c.Stocks).Returns(MockedStock.Object);
         MockedContext.Setup(c => c.Products).Returns(MockedProducts.Object); //Products
         MockedContext.Setup(c => c.StockAdjustments).Returns(MockedAdjustments.Object); //Deductions
         MockedContext.Setup(c => c.StockTakeItems).Returns(MockedStockTakeItems.Object); //Stock Adjustments
         MockedContext.Setup(c => c.StockTakes).Returns(MockedStockTakes.Object);
         MockedContext.Setup(c => c.StockSets).Returns(MockedStockSets.Object);
      }

      public MockedInventoryStockData(IEnumerable<Product> products, IEnumerable<Stock> stock)
      {
         //Setup
         MockedStock = MockingHelper.GetMockedDbSet(stock.AsQueryable());
         MockedProducts = MockingHelper.GetMockedDbSet(products.AsQueryable());
         MockedAdjustments = MockingHelper.GetMockedDbSet(new List<StockAdjustment>().AsQueryable());
         MockedStockTakeItems = MockingHelper.GetMockedDbSet(new List<StockTakeItem>().AsQueryable());
         MockedStockTakes = MockingHelper.GetMockedDbSet(new List<StockTake>().AsQueryable());
         MockedStockSets = MockingHelper.GetMockedDbSet(new List<StockSet>().AsQueryable());

         //Mocked data context
         MockedContext.Setup(c => c.Stocks).Returns(MockedStock.Object);
         MockedContext.Setup(c => c.Products).Returns(MockedProducts.Object); //Products
         MockedContext.Setup(c => c.StockAdjustments).Returns(MockedAdjustments.Object); //Deductions
         MockedContext.Setup(c => c.StockTakeItems).Returns(MockedStockTakeItems.Object); //Stock Adjustments
         MockedContext.Setup(c => c.StockTakes).Returns(MockedStockTakes.Object);
         MockedContext.Setup(c => c.StockSets).Returns(MockedStockSets.Object);
         MockedPropertyProvider.Setup(c => c.PrimaryBuyPriceTypeId).Returns(1);
      }



      public MockedInventoryStockData()
         : this(new List<Product>(), new List<Stock>())
      {
      }
   }
}
