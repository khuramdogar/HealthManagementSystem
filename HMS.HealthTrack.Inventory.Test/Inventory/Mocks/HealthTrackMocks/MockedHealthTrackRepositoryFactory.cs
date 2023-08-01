using HMS.HealthTrack.Web.Data.Repositories.HealthTrackInventory;

namespace HMS.HealthTrack.Inventory.Test.Inventory.Mocks.HealthTrackMocks
{
   internal static class MockedHealthTrackRepositoryFactory
   {
      public static InventoryUsedRepository GetInventoryUsedRepository(MockedHealthTrackData data)
      {
         return new InventoryUsedRepository(data.MockedHealthTrackInventoryContext.Object);
      }
   }
}
