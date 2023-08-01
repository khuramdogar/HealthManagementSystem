using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using Moq;

namespace HMS.HealthTrack.Inventory.Test.Inventory.Mocks
{
   internal sealed class MockedStockAdjustmentData : MockedInventoryData
   {

      public Mock<IDbSet<Stock>> MockedStock { get; set; }
      public Mock<IDbSet<StockAdjustment>> MockedStockAdjustments { get; set; }
      public Mock<IDbSet<StockAdjustmentStock>> MockedStockAdjustmentStocks { get; set; }

      public MockedStockAdjustmentData(IEnumerable<Stock> stock, IEnumerable<StockAdjustment> adjustments, IEnumerable<StockAdjustmentStock> sas)
      {
         MockedStock = MockingHelper.GetMockedDbSet(stock.AsQueryable());
         MockedStockAdjustments = MockingHelper.GetMockedDbSet(adjustments.AsQueryable());
         MockedStockAdjustmentStocks = MockingHelper.GetMockedDbSet(sas.AsQueryable());

         MockedContext.Setup(c => c.Stocks).Returns(MockedStock.Object);
         MockedContext.Setup(c => c.StockAdjustments).Returns(MockedStockAdjustments.Object);
         MockedContext.Setup(c => c.StockAdjustmentStocks).Returns(MockedStockAdjustmentStocks.Object);
      }

      public MockedStockAdjustmentData()
         : this(new List<Stock>(), new List<StockAdjustment>(), new List<StockAdjustmentStock>())
      {

      }

   }
}
