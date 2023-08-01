using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Data.Repositories.Inventory
{
   public class OraclePurchaseOrderReceiptRepository : IOraclePurchaseOrderReceiptRepository
   {
      private readonly IDbContextInventoryContext _context;

      public OraclePurchaseOrderReceiptRepository(IDbContextInventoryContext context)
      {
         _context = context;
      }

      public void Add(OraclePurchaseOrderReceipt oraclePurchaseOrderReceipt)
      {
         _context.OraclePurchaseOrderReceipts.Add(oraclePurchaseOrderReceipt);
      }

      public void Commit()
      {
         _context.ObjectContext.SaveChanges();
      }
   }

   public interface IOraclePurchaseOrderReceiptRepository
   {
      void Add(OraclePurchaseOrderReceipt oraclePurchaseOrderReceipt);
      void Commit();
   }
}