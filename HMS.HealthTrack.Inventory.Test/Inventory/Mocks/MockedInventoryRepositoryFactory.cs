using HMS.HealthTrack.Web.Data.Repositories.Inventory;

namespace HMS.HealthTrack.Inventory.Test.Inventory.Mocks
{
   internal static class MockedInventoryRepositoryFactory
   {
      public static StockRepository GetStockRepository(MockedInventoryData testData)
      {
         return new StockRepository(testData.MockedContext.Object);
      }

      public static StockAdjustmentRepository GetStockAdjustmentRepository(MockedInventoryData testData)
      {
         return new StockAdjustmentRepository(testData.MockedContext.Object);
      }

      public static StockLocationRepository GetStockLocationRepository(MockedInventoryData testData)
      {
         return new StockLocationRepository(testData.MockedContext.Object);
      }

      public static ProductRepository GetProductRepository(MockedInventoryData testData)
      {
         return new ProductRepository(testData.MockedContext.Object, testData.MockedPropertyProvider.Object);
      }
   }
}