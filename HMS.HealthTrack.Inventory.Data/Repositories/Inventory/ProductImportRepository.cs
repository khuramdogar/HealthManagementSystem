using System.Linq;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Data.Repositories.Inventory
{
   public class ProductImportRepository : IProductImportRepository
   {
      private readonly IDbContextInventoryContext _context;

      public ProductImportRepository(IDbContextInventoryContext context)
      {
         _context = context;
      }

      public IQueryable<ProductImport> GetProductsForImport(int id)
      {
         return _context.ProductImports.Where(pi => pi.ProductImportDataId == id);
      }

      public void Update(ProductImport existing, ProductImport product)
      {
         product.Processed = false;
         _context.Entry(existing).CurrentValues.SetValues(product);
         _context.Entry(existing).Property(p => p.ProcessedOn).IsModified = false;
      }

      public int InvalidProducts(int productImportDataId)
      {
         return _context.ProductImports.Count(import => import.ProductImportDataId == productImportDataId && import.Invalid);
      }

      public void Remove(ProductImport productImport)
      {
         foreach (var item in productImport.ProductImportGeneralLedgerCodes.ToList()) _context.ProductImportGeneralLedgerCodes.Remove(item);
         _context.ProductImports.Remove(productImport);
      }

      public ProductImport Find(int id)
      {
         return _context.ProductImports.SingleOrDefault(pi => pi.ProductImportId == id);
      }

      public void AddProductImport(ProductImport productImport)
      {
         _context.ProductImports.Add(productImport);
      }

      public void Commit()
      {
         _context.ObjectContext.SaveChanges();
      }

      public IQueryable<ProductImportGeneralLedgerCode> GetProductImportGeneralLedgerCodes(int productImportId)
      {
         return _context.ProductImportGeneralLedgerCodes.Where(c => c.ProductImportId == productImportId);
      }
   }

   public interface IProductImportRepository
   {
      void AddProductImport(ProductImport productImport);
      void Commit();
      IQueryable<ProductImport> GetProductsForImport(int id);
      void Update(ProductImport existing, ProductImport product);
      ProductImport Find(int id);
      int InvalidProducts(int productImportDataId);
      void Remove(ProductImport productImport);
      IQueryable<ProductImportGeneralLedgerCode> GetProductImportGeneralLedgerCodes(int productImportId);
   }
}