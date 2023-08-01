using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using Moq;

namespace HMS.HealthTrack.Inventory.Test.Inventory.Mocks
{
   internal class MockedInventoryConsumptionProcessorData : MockedInventoryData
   {
      public Mock<IDbSet<StockAdjustment>> MockedAdjustments { get; set; }

      public Mock<IDbSet<Product>> MockedProducts { get; set; }

      public Mock<IDbSet<Stock>> MockedStock { get; set; }

      public Mock<IDbSet<StockTakeItem>> MockedStockTakeItems { get; set; }

      public Mock<IDbSet<ExternalProductMapping>> MockedExternalProductMappings { get; set; }

      public Mock<IDbSet<StockLocationMapping>> MockedStockLocationMappings { get; set; }

      public Mock<IDbSet<StockLocation>> MockedStockLocations { get; set; }

      public Mock<IDbSet<HealthTrackConsumption>> MockedHealthTrackConsumption { get; set; }

      public Mock<IDbSet<ConsumptionNotificationManagement>> MockedConsumptionNotificationManagement { get; set; }

      public Mock<IDbSet<PriceType>> MockedPriceTypes { get; set; }

      public Mock<IDbSet<ClinicalConsumption>> MockedClinicalConsumptions { get; set; }

      public MockedInventoryConsumptionProcessorData(IEnumerable<ExternalProductMapping> productMappings,
         IEnumerable<StockLocation> stockLocations, IEnumerable<StockLocationMapping> locationMappings,
         IEnumerable<Product> products, IEnumerable<Stock> stock, IEnumerable<HealthTrackConsumption> htConsumptions,
         IEnumerable<ConsumptionNotificationManagement> notifications,
         IEnumerable<ClinicalConsumption> clinicalConsumptions)
      {
         MockedStock = MockingHelper.GetMockedDbSet(stock.AsQueryable());
         MockedProducts = MockingHelper.GetMockedDbSet(products.AsQueryable());
         MockedAdjustments = MockingHelper.GetMockedDbSet(new List<StockAdjustment>().AsQueryable());
         MockedExternalProductMappings = MockingHelper.GetMockedDbSet(productMappings.AsQueryable());
         MockedStockLocations = MockingHelper.GetMockedDbSet(stockLocations.AsQueryable());
         MockedStockLocationMappings = MockingHelper.GetMockedDbSet(locationMappings.AsQueryable());
         MockedHealthTrackConsumption = MockingHelper.GetMockedDbSet(htConsumptions.AsQueryable());
         MockedConsumptionNotificationManagement = MockingHelper.GetMockedDbSet(notifications.AsQueryable());
         MockedPriceTypes = MockingHelper.GetMockedDbSet(new List<PriceType>().AsQueryable());
         MockedClinicalConsumptions = MockingHelper.GetMockedDbSet(clinicalConsumptions.AsQueryable());

         //Mocked data context
         MockedContext.Setup(c => c.Stocks).Returns(MockedStock.Object);
         MockedContext.Setup(c => c.Products).Returns(MockedProducts.Object);
         MockedContext.Setup(c => c.StockAdjustments).Returns(MockedAdjustments.Object); //Consumptions
         MockedContext.Setup(c => c.StockLocations).Returns(MockedStockLocations.Object); //Consumptions
         MockedContext.Setup(c => c.ExternalProductMappings).Returns(MockedExternalProductMappings.Object);
         MockedContext.Setup(c => c.StockLocationMappings).Returns(MockedStockLocationMappings.Object);
         MockedContext.Setup(c => c.HealthTrackConsumptions).Returns(MockedHealthTrackConsumption.Object);
         MockedContext.Setup(c => c.ConsumptionNotificationManagements).Returns(MockedConsumptionNotificationManagement.Object);
         MockedContext.Setup(c => c.PriceTypes).Returns(MockedPriceTypes.Object);
         MockedContext.Setup(c => c.ClinicalConsumptions).Returns(MockedClinicalConsumptions.Object);

      }

      public MockedInventoryConsumptionProcessorData()
         : this(
            new List<ExternalProductMapping>(), new List<StockLocation>(), new List<StockLocationMapping>(),
            new List<Product>(), new List<Stock>(), new List<HealthTrackConsumption>(),
            new List<ConsumptionNotificationManagement>(), new List<ClinicalConsumption>())
      {
      }

   }
}
