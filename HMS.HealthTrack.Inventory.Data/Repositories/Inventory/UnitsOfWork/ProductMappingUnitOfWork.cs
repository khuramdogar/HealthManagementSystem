using System;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Model.HealthTrackInventory;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.HealthTrackInventory;

namespace HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork
{
   public class ProductMappingUnitOfWork : IProductMappingUnitOfWork
   {
      private readonly IDbContextInventoryContext _context;
      private readonly IHealthTrackInventoryContext _healthTrackInventoryContext;

      // HealthTrack context repositories

      // Inventory context repositories
      private readonly IPropertyProvider _propertyProvider;

      public ProductMappingUnitOfWork(IDbContextInventoryContext context, IHealthTrackInventoryContext healthTrackInventoryContext, ICustomLogger logger, IPropertyProvider propertyProvider)
      {
         _context = context;
         _healthTrackInventoryContext = healthTrackInventoryContext;
         _propertyProvider = propertyProvider;
         MappingRepository = new ExternalProductMappingRepository(context, logger);
         CompanyRepository = new CompanyRepository(context);
         ProductPriceRepository = new ProductPriceRepository(context);
         CategoryRepository = new CategoryRepository(context);
         StockLocationRepository = new StockLocationRepository(context);
         ProductRepository = new ProductRepository(context, propertyProvider);
         HealthTrackConsumptionRepository = new HealthTrackConsumptionRepository(context);

         InventoryMasterRepository = new InventoryMasterRepository(healthTrackInventoryContext);
      }

      public void Commit()
      {
         _context.ObjectContext.SaveChanges();
         _healthTrackInventoryContext.ObjectContext.SaveChanges();
      }

      public void CreateMapping(MappingOverview mappingDetails)
      {
         MappingRepository.Add(new ExternalProductMapping
         {
            CreatedOn = DateTime.Now,
            CreatedBy = mappingDetails.CreatedBy,
            ExternalProductId = mappingDetails.ExternalId,
            InventoryProductId = mappingDetails.InternalId,
            ProductSource = ProductMappingSource.HealthTrack
         });
      }

      public IExternalProductMappingRepository MappingRepository { get; }

      public ICompanyRepository CompanyRepository { get; }

      public IProductPriceRepository ProductPriceRepository { get; }

      public ICategoryRepository CategoryRepository { get; }

      public IStockLocationRepository StockLocationRepository { get; }

      public IInventoryMasterRepository InventoryMasterRepository { get; }

      public IProductRepository ProductRepository { get; }

      public IHealthTrackConsumptionRepository HealthTrackConsumptionRepository { get; }
   }

   public interface IProductMappingUnitOfWork
   {
      IExternalProductMappingRepository MappingRepository { get; }
      ICompanyRepository CompanyRepository { get; }
      IProductPriceRepository ProductPriceRepository { get; }
      ICategoryRepository CategoryRepository { get; }
      IStockLocationRepository StockLocationRepository { get; }
      IInventoryMasterRepository InventoryMasterRepository { get; }
      IProductRepository ProductRepository { get; }
      IHealthTrackConsumptionRepository HealthTrackConsumptionRepository { get; }
      void Commit();
      void CreateMapping(MappingOverview mappingDetails);
   }
}