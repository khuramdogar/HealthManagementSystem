using System.Linq;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Model.Clinical;
using HMS.HealthTrack.Web.Data.Model.HealthTrackInventory;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Clinical;
using HMS.HealthTrack.Web.Data.Repositories.HealthTrackInventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;

namespace HMS.HealthTrack.Web.Data.Repositories
{
   public class InventoryUnitOfWork : IInventoryUnitOfWork
   {
      private readonly IDbContextClinicalContext _clinicalContext;

      // Inventory repositories
      private readonly IHealthTrackInventoryContext _healthTrackInventoryContext;
      private readonly IDbContextInventoryContext _inventoryContext;


      // HealthTrack repositories
      private readonly ICustomLogger _logger;


      private readonly IPropertyProvider _propertyProvider;

      public InventoryUnitOfWork(IHealthTrackInventoryContext healthTrackInventoryContext, IDbContextInventoryContext inventoryContext, IDbContextClinicalContext clinicalContext,
         IPropertyProvider propertyProvider, ICustomLogger logger)
      {
         _healthTrackInventoryContext = healthTrackInventoryContext;
         _inventoryContext = inventoryContext;
         _clinicalContext = clinicalContext;
         _propertyProvider = propertyProvider;
         _logger = logger;

         InventoryMasterRepository = new InventoryMasterRepository(healthTrackInventoryContext);
         InventoryUsedRepository = new InventoryUsedRepository(healthTrackInventoryContext);
         ProductRepository = new ProductRepository(inventoryContext, propertyProvider);
         ExternalProductMappingRepository = new ExternalProductMappingRepository(inventoryContext, logger);
         StockLocationRepository = new StockLocationRepository(inventoryContext);
         StockRepository = new StockRepository(inventoryContext);
         HealthTrackConsumptionRepository = new HealthTrackConsumptionRepository(inventoryContext);

         MedicalRecordRepository = new MedicalRecordRepository(clinicalContext);
      }

      // HealthTrack repositories
      public IInventoryMasterRepository InventoryMasterRepository { get; }

      public IInventoryUsedRepository InventoryUsedRepository { get; }


      // Inventory repositories
      public IExternalProductMappingRepository ExternalProductMappingRepository { get; }

      public IHealthTrackConsumptionRepository HealthTrackConsumptionRepository { get; }

      public IMedicalRecordRepository MedicalRecordRepository { get; }

      public IStockRepository StockRepository { get; }

      public IStockLocationRepository StockLocationRepository { get; }

      public IProductRepository ProductRepository { get; }

      public void Commit()
      {
         _inventoryContext.ObjectContext.SaveChanges();
         _healthTrackInventoryContext.ObjectContext.SaveChanges();
      }

      public Inventory_Master FindHealthTrackProduct(int inventoryProductId)
      {
         var mappings = ExternalProductMappingRepository.GetInventoryProductMapping(inventoryProductId);

         int inventoryMasterId;
         if (mappings.Count() > 1)
            inventoryMasterId = mappings.OrderByDescending(m => m.LastModifiedOn).First().ExternalProductId;
         else if (mappings.Count() == 1)
            inventoryMasterId = mappings.Single().ExternalProductId;
         else
            return null;

         return InventoryMasterRepository.Find(inventoryMasterId);
      }

      public Inventory_Master FindUnmappedProductBySpc(string spc)
      {
         var inventoryProductsWithSpc = InventoryMasterRepository.FindAll().Where(im => im.Inv_SPC == spc).OrderByDescending(im => im.dateLastModified).ToList();
         var unmappedIds = inventoryProductsWithSpc.Select(im => im.Inv_ID).Except(ExternalProductMappingRepository.FindAll().Select(epm => epm.ExternalProductId));

         return !unmappedIds.Any() ? null : inventoryProductsWithSpc.FirstOrDefault(p => p.Inv_ID == unmappedIds.First());
      }
   }

   public interface IInventoryUnitOfWork
   {
      IInventoryMasterRepository InventoryMasterRepository { get; }
      IInventoryUsedRepository InventoryUsedRepository { get; }
      IProductRepository ProductRepository { get; }
      IMedicalRecordRepository MedicalRecordRepository { get; }
      IStockLocationRepository StockLocationRepository { get; }
      IStockRepository StockRepository { get; }
      IHealthTrackConsumptionRepository HealthTrackConsumptionRepository { get; }
      IExternalProductMappingRepository ExternalProductMappingRepository { get; }
      void Commit();
      Inventory_Master FindHealthTrackProduct(int inventoryProductId);
      Inventory_Master FindUnmappedProductBySpc(string spc);
   }
}