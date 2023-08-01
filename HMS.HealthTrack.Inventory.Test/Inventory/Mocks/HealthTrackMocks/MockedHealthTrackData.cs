using HMS.HealthTrack.Web.Data.Model.HealthTrackInventory;
using Moq;

namespace HMS.HealthTrack.Inventory.Test.Inventory.Mocks.HealthTrackMocks
{
   internal abstract class MockedHealthTrackData
   {
      public Mock<HealthTrackInventoryContext> MockedHealthTrackInventoryContext { get; set; }

      protected MockedHealthTrackData()
      {
         MockedHealthTrackInventoryContext = new Mock<HealthTrackInventoryContext>();
      }
   }
}
