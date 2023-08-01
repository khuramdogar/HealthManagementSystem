using System.Data.Entity;
using System.Linq;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork
{
   public class ProductImportUnitOfWork : IProductImportUnitOfWork
   {
      private readonly IDbContextInventoryContext _context;
      private readonly ICustomLogger _logger;

      public ProductImportUnitOfWork(IDbContextInventoryContext context, IPropertyProvider propertyProvider,
         ICustomLogger logger)
      {
         _context = context;
         _logger = logger;
         PropertyProvider = propertyProvider;
         ProductImportRepository = new ProductImportRepository(context);
         ProductRepository = new ProductRepository(context, propertyProvider);
         ProductPriceRepository = new ProductPriceRepository(context);
         CompanyRepository = new CompanyRepository(context);
         CategoryRepository = new CategoryRepository(context);
         SupplierRepository = new SupplierRepository(context);
         ProductImportDataRepository = new ProductImportDataRepository(context);
         GeneralLedgerRepository = new GeneralLedgerRepository(context);
         GeneralLedgerTierRepository = new GeneralLedgerTierRepository(context);
         StockSettingRepository = new StockSettingRepository(context);
         ScanCodeRepository = new ScanCodeRepository(context);
      }

      public IProductRepository ProductRepository { get; }

      public ICompanyRepository CompanyRepository { get; }

      public ICategoryRepository CategoryRepository { get; }

      public IProductPriceRepository ProductPriceRepository { get; }

      public IProductImportRepository ProductImportRepository { get; }

      public IPropertyProvider PropertyProvider { get; }

      public ISupplierRepository SupplierRepository { get; }

      public IProductImportDataRepository ProductImportDataRepository { get; }

      public IGeneralLedgerRepository GeneralLedgerRepository { get; }

      public IGeneralLedgerTierRepository GeneralLedgerTierRepository { get; }

      public IStockSettingRepository StockSettingRepository { get; }

      public IScanCodeRepository ScanCodeRepository { get; }

      public void Commit()
      {
         _context.ObjectContext.SaveChanges();
      }

      public void Detach()
      {
         var objectStateEntries = _context.ObjectContext.ObjectStateManager.GetObjectStateEntries(EntityState.Added).AsEnumerable()
            .Union(_context.ObjectContext.ObjectStateManager.GetObjectStateEntries(EntityState.Modified));
         foreach (var objectStateEntry in objectStateEntries) _context.ObjectContext.Detach(objectStateEntry.Entity);
      }
   }

   public interface IProductImportUnitOfWork
   {
      IProductRepository ProductRepository { get; }
      ICompanyRepository CompanyRepository { get; }
      ICategoryRepository CategoryRepository { get; }
      IProductPriceRepository ProductPriceRepository { get; }
      IProductImportRepository ProductImportRepository { get; }
      IPropertyProvider PropertyProvider { get; }
      ISupplierRepository SupplierRepository { get; }
      IProductImportDataRepository ProductImportDataRepository { get; }
      IGeneralLedgerRepository GeneralLedgerRepository { get; }
      IGeneralLedgerTierRepository GeneralLedgerTierRepository { get; }
      IStockSettingRepository StockSettingRepository { get; }
      IScanCodeRepository ScanCodeRepository { get; }
      void Commit();
      void Detach();
   }
}