using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using HMS.HealthTrack.Inventory.Common.Constants;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using Moq;

namespace HMS.HealthTrack.Inventory.Test.Inventory.Mocks
{
   internal sealed class MockedInventoryProductData : MockedInventoryData
   {
      public Mock<IDbSet<Product>> MockedProducts { get; set; }
      public Mock<IDbSet<ProductCategory>> MockedProductCategories{ get; set; }
      public Mock<IDbSet<PriceType>> MockedPriceTypes { get; set; }

      public Mock<IDbSet<HealthTrackConsumption>> MockedHealthTrackConsumptions { get; set; }
      public Mock<IDbSet<ExternalProductMapping>> MockedExternalProductMappings { get; set; }
      public Mock<IDbSet<ConsumptionNotificationManagement>> MockedConsumptionNotificationManagements { get; set; }

      public Mock<IDbSet<StockAdjustment>> MockedStockAdjustments { get; set; }
      public Mock<IDbSet<Stock>> MockedStock { get; set; }
      public Mock<IDbSet<StockAdjustmentReason>> MockedStockAdjustmentReasons { get; set; }

      public MockedInventoryProductData(IEnumerable<Product> products, IEnumerable<PriceType> priceTypes,
         IEnumerable<HealthTrackConsumption> healthTrackConsumptions,
         IEnumerable<ExternalProductMapping> externalProductMappings,
         IEnumerable<ConsumptionNotificationManagement> consumptionNotificationManagements, IEnumerable<StockAdjustment> stockAdjustments, IEnumerable<Stock> stock,
         IEnumerable<ProductCategory> productCategories)
      {
         MockedProducts = MockingHelper.GetMockedDbSet(products.AsQueryable());
         MockedProductCategories = MockingHelper.GetMockedDbSet(productCategories.AsQueryable());
         MockedPriceTypes = MockingHelper.GetMockedDbSet(priceTypes.AsQueryable());
         MockedHealthTrackConsumptions = MockingHelper.GetMockedDbSet(healthTrackConsumptions.AsQueryable());
         MockedExternalProductMappings = MockingHelper.GetMockedDbSet(externalProductMappings.AsQueryable());
         MockedConsumptionNotificationManagements = MockingHelper.GetMockedDbSet(consumptionNotificationManagements.AsQueryable());
         MockedStockAdjustments = MockingHelper.GetMockedDbSet(stockAdjustments.AsQueryable());
         MockedStock = MockingHelper.GetMockedDbSet(stock.AsQueryable());
         MockedStockAdjustmentReasons = MockingHelper.GetMockedDbSet(GetSystemReasons());

         MockedContext.Setup(c => c.Products).Returns(MockedProducts.Object);
         MockedContext.Setup(c => c.ProductCategories).Returns(MockedProductCategories.Object);
         MockedContext.Setup(c => c.PriceTypes).Returns(MockedPriceTypes.Object);
         MockedContext.Setup(c => c.HealthTrackConsumptions).Returns(MockedHealthTrackConsumptions.Object);
         MockedContext.Setup(c => c.ExternalProductMappings).Returns(MockedExternalProductMappings.Object);
         MockedContext.Setup(c => c.ConsumptionNotificationManagements).Returns(MockedConsumptionNotificationManagements.Object);
         MockedContext.Setup(c => c.StockAdjustments).Returns(MockedStockAdjustments.Object);
         MockedContext.Setup(c => c.Stocks).Returns(MockedStock.Object);
         MockedContext.Setup(c => c.StockAdjustmentReasons).Returns(MockedStockAdjustmentReasons.Object);
      }

      public MockedInventoryProductData()
         : this(
            new List<Product>(), new List<PriceType>(), new List<HealthTrackConsumption>(),
            new List<ExternalProductMapping>(), new List<ConsumptionNotificationManagement>(), new List<StockAdjustment>(), new List<Stock>(),
            new List<ProductCategory>())
      {
      }

      private IQueryable<StockAdjustmentReason> GetSystemReasons()
      {
         return new List<StockAdjustmentReason>
         {
            new StockAdjustmentReason
            {

               IsSystemReason = true,
               Name = InventoryConstants.StockAdjustmentReasons.InitialStock,
               StockAdjustmentReasonId = 1
            },
            new StockAdjustmentReason
            {

               IsSystemReason = true,
               Name = InventoryConstants.StockAdjustmentReasons.StockManagementWriteOff,
               StockAdjustmentReasonId = 2
            },
         }.AsQueryable();

      }
   }
}