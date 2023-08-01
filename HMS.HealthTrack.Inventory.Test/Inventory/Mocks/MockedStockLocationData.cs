using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using Moq;

namespace HMS.HealthTrack.Inventory.Test.Inventory.Mocks
{
   internal sealed class MockedStockLocationData : MockedInventoryData
   {
      public Mock<IDbSet<StockLocation>> MockedStockLocations { get; set; }

      public MockedStockLocationData(IEnumerable<StockLocation> stockLocations)
      {
         // Setup
         MockedStockLocations = MockingHelper.GetMockedDbSet(stockLocations.AsQueryable());
      }
   }
}
