using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using HMS.HealthTrack.Web.Data.Model.HealthTrackInventory;
using Moq;

namespace HMS.HealthTrack.Inventory.Test.Inventory.Mocks.HealthTrackMocks
{
   internal class MockedHealthTrackConsumptionProcessorData : MockedHealthTrackData
   {
      public Mock<IDbSet<Inventory_Used>> MockedInventoryUsed { get; set; }

      public MockedHealthTrackConsumptionProcessorData(IEnumerable<Inventory_Used> inventoryUsed)
      {
         MockedInventoryUsed = MockingHelper.GetMockedDbSet(inventoryUsed.AsQueryable());

         MockedHealthTrackInventoryContext.Setup(c => c.Inventory_Used).Returns(MockedInventoryUsed.Object);
      }
   }
}
