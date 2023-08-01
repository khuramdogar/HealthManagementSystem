using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Data.Repositories.Inventory
{
   public class OraclePurchaseOrderErrorRepository : IOraclePurchaseOrderErrorRepository
   {
      private readonly IDbContextInventoryContext _context;

      public OraclePurchaseOrderErrorRepository(IDbContextInventoryContext context)
      {
         _context = context;
      }

      public void Add(OraclePurchaseOrderError error)
      {
         _context.OraclePurchaseOrderErrors.Add(error);
      }

      public void Commit()
      {
         _context.ObjectContext.SaveChanges();
      }
   }

   public interface IOraclePurchaseOrderErrorRepository
   {
      void Add(OraclePurchaseOrderError error);
      void Commit();
   }
}