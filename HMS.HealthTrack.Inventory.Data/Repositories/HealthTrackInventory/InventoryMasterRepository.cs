using System;
using System.Linq;
using HMS.HealthTrack.Web.Data.Model.HealthTrackInventory;

namespace HMS.HealthTrack.Web.Data.Repositories.HealthTrackInventory
{
   public interface IInventoryMasterRepository
   {
      IQueryable<Inventory_Master> FindAll();
      Inventory_Master Find(int htProductId);
      void Commit();
      Inventory_Master CreateNewProduct(string description, string spc, string upn, string username);
   }

   public class InventoryMasterRepository : IInventoryMasterRepository
   {
      private readonly IHealthTrackInventoryContext _context;

      public InventoryMasterRepository(IHealthTrackInventoryContext context)
      {
         _context = context;
      }

      public IQueryable<Inventory_Master> FindAll()
      {
         return _context.Inventory_Master.Where(im => im.deleted.HasValue && !im.deleted.Value);
      }

      public Inventory_Master Find(int htProductId)
      {
         return _context.Inventory_Master.SingleOrDefault(im => im.Inv_ID == htProductId && im.deleted != null && !im.deleted.Value);
      }

      public void Commit()
      {
         _context.SaveChanges();
      }

      public Inventory_Master CreateNewProduct(string description, string spc, string upn, string username)
      {
         var product = new Inventory_Master
         {
            dateCreated = DateTime.Now,
            dateLastModified = DateTime.Now,
            deleted = false,
            Inv_Description = description,
            Inv_SPC = spc,
            userCreated = username,
            userLastModified = username,
            Inv_UPN = upn
         };

         _context.Inventory_Master.Add(product);
         return product;
      }
   }
}