using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;
using Moq;

namespace HMS.HealthTrack.Inventory.Test
{
   public static class MockedInventoryItems
   {
      public static Mock<ICustomLogger> Logger => new Mock<ICustomLogger>();
      public static Mock<IPropertyProvider> Properties => new Mock<IPropertyProvider>();
   }
}
