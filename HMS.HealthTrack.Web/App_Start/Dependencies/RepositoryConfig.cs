using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories;
using HMS.HealthTrack.Web.Data.Repositories.Clinical;
using HMS.HealthTrack.Web.Data.Repositories.Configuration;
using HMS.HealthTrack.Web.Data.Repositories.HealthTrackInventory;
using HMS.HealthTrack.Web.Data.Repositories.Infrastructure;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork;
using HMS.HealthTrack.Web.Data.Repositories.Security;
using Microsoft.Practices.Unity;

namespace HMS.HealthTrack.Web.Dependencies
{
   /// <summary>
   /// Register repositories with the IoC container
   /// </summary>
   internal static class RepositoryConfig
   {
      internal static void RegisterRepositories(this IUnityContainer container)
      {
         // Generic repositories
         container.RegisterType<IBaseRepository<OrderItem>, BaseRepository<OrderItem>>();
         container.RegisterType<IStockSettingRepository, StockSettingRepository>();

         // Specific repositories
         container.RegisterType<IConfigurationRepository, ConfigurationRepository>();
         container.RegisterType<IOrderRepository, OrderRepository>();
         container.RegisterType<IOrderSubmissionRepository, OrderSubmissionRepository>();
         container.RegisterType<IOrderItemRepository, OrderItemRepository>();
         container.RegisterType<IProductRepository, ProductRepository>();
         container.RegisterType<IUserRepository, UserRepository>();
         container.RegisterType<ICompanyRepository, CompanyRepository>();
         container.RegisterType<ICategoryRepository, CategoryRepository>();
         container.RegisterType<IStockRepository, StockRepository>();
         container.RegisterType<IStockRequestRepository, StockRequestRepository>();
         container.RegisterType<IStockAdjustmentRepository, StockAdjustmentRepository>();
         container.RegisterType<IStockSetRepository, StockSetRepository>();
         container.RegisterType<IOraclePurchaseOrderReceiptRepository, OraclePurchaseOrderReceiptRepository>();
         container.RegisterType<IOraclePurchaseOrderErrorRepository, OraclePurchaseOrderErrorRepository>();
         container.RegisterType<IStockLocationRepository, StockLocationRepository>();
         container.RegisterType<IExternalProductMappingRepository, ExternalProductMappingRepository>();
         container.RegisterType<IProductPriceRepository, ProductPriceRepository>();
         container.RegisterType<IPropertyProvider, InventorySettingsProvider>();
         container.RegisterType<IMedicalRecordRepository, MedicalRecordRepository>();
         container.RegisterType<IStockSetItemRepository, StockSetItemRepository>();
         container.RegisterType<ISupplierRepository, SupplierRepository>();
         container.RegisterType<IMedicareProductsRepository, MedicareProductsRepository>();
         container.RegisterType<IPreferenceRepository, PreferenceRepository>();
         container.RegisterType<IStockTakeRepository, StockTakeRepository>();
         container.RegisterType<IProductImportRepository, ProductImportRepository>();
         container.RegisterType<IDashboardNotificationRepository, DashboardNotificationRepository>();
         container.RegisterType<IProductImportDataRepository, ProductImportDataRepository>();
         container.RegisterType<IReportingLevelRepository, ReportingLevelRepository>();
         container.RegisterType<IGeneralLedgerTierRepository, GeneralLedgerTierRepository>();
         container.RegisterType<IGeneralLedgerRepository, GeneralLedgerRepository>();
         container.RegisterType<IHealthTrackConsumptionRepository, HealthTrackConsumptionRepository>();
         container.RegisterType<IHealthTrackLocationRepository, HealthTrackLocationRepository>();
         container.RegisterType<IInventoryMasterRepository, InventoryMasterRepository>();
         container.RegisterType<IInventoryUsedRepository, InventoryUsedRepository>();
         container.RegisterType<IPaymentClassMappingRepository, PaymentClassMappingRepository>();
         container.RegisterType<IScanCodeRepository, ScanCodeRepository>();
         container.RegisterType<IOrderChannelRepository, OrderChannelRepository>();
         container.RegisterType<IConsumptionRepository, ConsumptionRepository>();

      }

      /// <summary>
      /// Register unit of work classes with the IoC container
      /// </summary>
      internal static void RegisterUnitsOfWork(this IUnityContainer container)
      {
         // Units of work
         container.RegisterType<IProductUnitOfWork, ProductUnitOfWork>();
         container.RegisterType<IOrderUnitOfWork, OrderUnitOfWork>();
         container.RegisterType<IStockUnitOfWork, StockUnitOfWork>();
         container.RegisterType<ISecurityUnitOfWork, SecurityUnitOfWork>();
         container.RegisterType<IProductMappingUnitOfWork, ProductMappingUnitOfWork>();
         container.RegisterType<IOrderableItemsUnitOfWork, OrderableItemsUnitOfWork>();
         container.RegisterType<IProductImportUnitOfWork, ProductImportUnitOfWork>();
         container.RegisterType<IStockTakeUnitOfWork, StockTakeUnitOfWork>();
         container.RegisterType<ISystemNotificationsUnitOfWork, SystemNotificationsUnitOfWork>();
         container.RegisterType<IGeneralLedgerUnitOfWork, GeneralLedgerUnitOfWork>();
         container.RegisterType<ILocationUnitOfWork, LocationUnitOfWork>();
         container.RegisterType<IStockTakeUnitOfWork, StockTakeUnitOfWork>();
         container.RegisterType<IInventoryUnitOfWork, InventoryUnitOfWork>();
         container.RegisterType<IConsumptionUnitOfWork, ConsumptionUnitOfWork>();
         container.RegisterType<IStockAdjustmentUnitOfWork, StockAdjustmentUnitOfWork>();
         container.RegisterType<IOrderSubmissionUnitOfWork, OrderSubmissionUnitOfWork>();
      }
   }
}