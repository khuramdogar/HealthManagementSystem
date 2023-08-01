using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Data.Repositories.Inventory
{
   public class ExternalProductMappingRepository : IExternalProductMappingRepository
   {
      private readonly IDbContextInventoryContext _context;
      private readonly ICustomLogger _logger;

      public ExternalProductMappingRepository(IDbContextInventoryContext context, ICustomLogger logger)
      {
         _context = context;
         _logger = logger;
      }

      /// <summary>
      ///    Warning: If there are no present mappings this will return a DELETED mapping if one exists
      /// </summary>
      /// <param name="invItemId"></param>
      /// <returns></returns>
      public ExternalProductMapping GetHealthTrackProductMapping(int invItemId)
      {
         //Get mappings
         var existingMappings = from m in _context.ExternalProductMappings.Include(epm => epm.Product)
            where m.ExternalProductId == invItemId
                  && m.ProductSource == ProductMappingSource.HealthTrack
                  && m.DeletedOn == null
            select m;

         if (existingMappings.Count() == 1) return existingMappings.Single();

         //Get local mappings
         var newMapping = (from m in _context.ExternalProductMappings.Local
            where m.ExternalProductId == invItemId
                  && m.ProductSource == ProductMappingSource.HealthTrack
                  && m.DeletedOn == null
            select m).SingleOrDefault();

         if (newMapping != null) return newMapping;

         //There should never be multiple mappings for the same product
         if (existingMappings.Count() > 1)
         {
            var message =
               string.Format("Found multiple external product mappings for product " + invItemId);
            _logger.Error(message);
            throw new Exception(message);
         }

         // fall back to deleted mappings before adding a new one, raise this as in error
         var deletedMappings = from m in _context.ExternalProductMappings.Include(epm => epm.Product)
            where m.ExternalProductId == invItemId
                  && m.ProductSource == ProductMappingSource.HealthTrack
                  && m.DeletedOn != null
            orderby m.DeletedOn descending
            select m;

         if (deletedMappings.Any()) return deletedMappings.First();

         return null;
      }

      public IQueryable<ExternalProductMapping> GetInventoryProductMapping(int inventoryProductId)
      {
         return _context.ExternalProductMappings.Where(epm => epm.InventoryProductId == inventoryProductId
                                                              && epm.ProductSource == ProductMappingSource.HealthTrack
                                                              && epm.DeletedOn == null);
      }

      public IQueryable<MappingOverview> GetMappingOverviews()
      {
         var overviews = from htpm in _context.HealthTrackProductMappings
            join p in _context.Products on htpm.InventoryProductId equals p.ProductId
            select new MappingOverview
            {
               CreatedBy = htpm.CreatedBy,
               CreatedOn = htpm.CreatedOn,
               ExternalDescription = htpm.Inv_Description,
               ExternalId = htpm.ExternalProductId,
               InternalDescription = p.Description,
               InternalId = htpm.InventoryProductId,
               MappingId = htpm.ProductMappingId,
               SPC = p.SPC
            };
         return overviews;
      }

      public void Commit()
      {
         _context.ObjectContext.SaveChanges();
      }

      public ExternalProductMapping Find(int id)
      {
         return _context.ExternalProductMappings.SingleOrDefault(pm => pm.ProductMappingId == id);
      }

      public IQueryable<ExternalProductMapping> FindAll()
      {
         return from m in _context.ExternalProductMappings where !m.DeletedOn.HasValue select m;
      }

      public void Add(ExternalProductMapping externalProductMapping)
      {
         externalProductMapping.LastModifiedOn = DateTime.Now;
         _context.ExternalProductMappings.Add(externalProductMapping);
      }

      public void Update(ExternalProductMapping externalProductMapping)
      {
         externalProductMapping.LastModifiedOn = DateTime.Now;
         _context.Entry(externalProductMapping).State = EntityState.Modified;
      }

      public void Remove(ExternalProductMapping externalProductMapping)
      {
         externalProductMapping.DeletedOn = DateTime.Now;
      }

      public void Remove(int productId, string username)
      {
         var mappings = FindAll().Where(m => m.InventoryProductId == productId);
         foreach (var externalProductMapping in mappings)
         {
            externalProductMapping.DeletedBy = username;
            externalProductMapping.DeletedOn = DateTime.Now;
         }
      }

      public void Restore(ICollection<ExternalProductMapping> externalProductMapping, string username)
      {
         foreach (var productMapping in externalProductMapping)
         {
            productMapping.DeletedOn = null;
            productMapping.DeletedBy = null;
            productMapping.LastModifiedBy = username;
            productMapping.LastModifiedOn = DateTime.Now;
         }
      }
   }

   public interface IExternalProductMappingRepository
   {
      void Commit();
      IQueryable<ExternalProductMapping> FindAll();
      ExternalProductMapping Find(int id);
      void Add(ExternalProductMapping externalProductMapping);
      void Update(ExternalProductMapping externalProductMapping);
      void Remove(ExternalProductMapping externalProductMapping);
      ExternalProductMapping GetHealthTrackProductMapping(int invItemId);
      IQueryable<ExternalProductMapping> GetInventoryProductMapping(int inventoryProductId);
      IQueryable<MappingOverview> GetMappingOverviews();
      void Remove(int productId, string username);
      void Restore(ICollection<ExternalProductMapping> externalProductMapping, string username);
   }
}