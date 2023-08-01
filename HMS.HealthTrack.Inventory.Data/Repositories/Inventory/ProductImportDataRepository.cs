using System;
using System.Data.Entity;
using System.Linq;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Data.Repositories.Inventory
{
   public class ProductImportDataRepository : IProductImportDataRepository
   {
      private readonly IDbContextInventoryContext _context;

      public ProductImportDataRepository(IDbContextInventoryContext context)
      {
         _context = context;
      }

      public IQueryable<ProductImportData> GetProductImportData()
      {
         return _context.ProductImportDatas.Include(pis => pis.ProductImports).Where(pid => !pid.DeletedOn.HasValue);
      }

      public ProductImportData Find(int id)
      {
         return _context.ProductImportDatas.SingleOrDefault(pis => pis.ProductImportDataId == id);
      }

      public void UpdateStatus(int id)
      {
         var importData =
            _context.ProductImportDatas.Include(pis => pis.ProductImports)
               .SingleOrDefault(pis => pis.ProductImportDataId == id);

         if (importData != null)
         {
            if (importData.ProductImports.Any(pi => pi.Invalid))
               importData.Status = ProductImportStatus.Error;
            else if (importData.ProductImports.Any(pi => !pi.Processed))
               importData.Status = ProductImportStatus.Pending;
            else
               importData.Status = ProductImportStatus.Complete;
         }
      }

      public void AddProductImportData(ProductImportData productImport)
      {
         productImport.Status = ProductImportStatus.Uploaded;
         productImport.CreatedOn = DateTime.Now;
         productImport.LastModifiedOn = DateTime.Now;
         _context.ProductImportDatas.Add(productImport);
      }

      public IQueryable<ProductImportData> GetUnprocessedSets()
      {
         return
            _context.ProductImportDatas.Where(
               pis => pis.ProductsData != null && pis.Status == ProductImportStatus.Pending);
      }

      public void Remove(ProductImportData import)
      {
         import.DeletedOn = DateTime.Now;
         import.ProductsData = null;
      }

      public void Commit()
      {
         _context.ObjectContext.SaveChanges();
      }
   }

   public interface IProductImportDataRepository
   {
      IQueryable<ProductImportData> GetProductImportData();
      ProductImportData Find(int id);
      void UpdateStatus(int importData);
      void AddProductImportData(ProductImportData productImport);
      void Commit();
      IQueryable<ProductImportData> GetUnprocessedSets();
      void Remove(ProductImportData import);
   }
}