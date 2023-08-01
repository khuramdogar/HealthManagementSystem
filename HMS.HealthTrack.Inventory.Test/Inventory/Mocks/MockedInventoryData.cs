using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Model.Clinical;
using HMS.HealthTrack.Web.Data.Model.HealthTrackInventory;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;
using Moq;

namespace HMS.HealthTrack.Inventory.Test.Inventory.Mocks
{
   internal abstract class MockedInventoryData
   {
      public Mock<InventoryContext> MockedContext { get; set; }
      public Mock<ClinicalContext> MockedClinicalContext { get; set; }
      public Mock<HealthTrackInventoryContext> MockedHealthTrackInventoryContext { get; set; }
      public Mock<ICustomLogger> MockedLogger { get; set; }
      public Mock<IPropertyProvider> MockedPropertyProvider { get; set; }

      protected MockedInventoryData()
      {
         MockedContext = new Mock<InventoryContext>();
         MockedPropertyProvider = new Mock<IPropertyProvider>();
         MockedLogger = new Mock<ICustomLogger>();
         MockedClinicalContext = new Mock<ClinicalContext>();
         MockedHealthTrackInventoryContext = new Mock<HealthTrackInventoryContext>();
      }
   }
}