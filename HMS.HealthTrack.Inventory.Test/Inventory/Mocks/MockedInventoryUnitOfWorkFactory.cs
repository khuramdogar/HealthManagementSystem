using HMS.HealthTrack.Web.Data.Repositories;
using HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork;

namespace HMS.HealthTrack.Inventory.Test.Inventory.Mocks
{
   internal static class MockedInventoryUnitOfWorkFactory
   {
      internal static OrderableItemsUnitOfWork GetOrderableItemsUnitOfWork(MockedInventoryData data)
      {
         return new OrderableItemsUnitOfWork(data.MockedContext.Object, data.MockedPropertyProvider.Object, data.MockedLogger.Object);
      }

      internal static StockUnitOfWork GetStockUnitOfWork(MockedInventoryData data)
      {
         return new StockUnitOfWork(data.MockedContext.Object, data.MockedPropertyProvider.Object, data.MockedLogger.Object);
      }

      internal static InventoryUnitOfWork GetInventoryUnitOfWork(MockedInventoryData data)
      {
         return new InventoryUnitOfWork(data.MockedHealthTrackInventoryContext.Object, data.MockedContext.Object,
            data.MockedClinicalContext.Object, data.MockedPropertyProvider.Object, data.MockedLogger.Object);
      }

      internal static ConsumptionUnitOfWork GetConsumptionUnitOfWork(MockedInventoryData data)
      {
         return new ConsumptionUnitOfWork(data.MockedContext.Object, data.MockedPropertyProvider.Object, data.MockedLogger.Object);
      }

      internal static ProductImportUnitOfWork GetProductImportUnitOfWork(MockedInventoryData data)
      {
         return new ProductImportUnitOfWork(data.MockedContext.Object, data.MockedPropertyProvider.Object,
            data.MockedLogger.Object);
      }

      internal static ProductUnitOfWork GetProductUnitOfWork(MockedInventoryData data)
      {
         return new ProductUnitOfWork(data.MockedContext.Object, data.MockedPropertyProvider.Object,
            data.MockedLogger.Object);
      }

      internal static StockAdjustmentUnitOfWork GetStockAdjustmentUnitOfWork(MockedInventoryData data)
      {
         return new StockAdjustmentUnitOfWork(data.MockedContext.Object, data.MockedPropertyProvider.Object, data.MockedLogger.Object);
      }
   }
}