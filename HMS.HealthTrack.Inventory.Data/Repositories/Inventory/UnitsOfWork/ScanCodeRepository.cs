using System.Collections.Generic;
using System.Linq;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork
{
   public interface IScanCodeRepository
   {
      IQueryable<ScanCode> FindAll();
      IQueryable<string> FindDistinct();
      ScanCode Create(int productId, string value);
      void Commit();
      void Remove(int scanCodeId);
      void UpdateCode(int scanCodeId, string value);
   }

   public class ScanCodeRepository : IScanCodeRepository
   {
      private readonly IDbContextInventoryContext _context;

      public ScanCodeRepository(IDbContextInventoryContext context)
      {
         _context = context;
      }

      public IQueryable<ScanCode> FindAll()
      {
         return _context.ScanCodes;
      }

      public IQueryable<string> FindDistinct()
      {
         if (_context.ScanCodes == null)
            return new List<string>().AsQueryable();
         var uniqueCodes = (from sc in _context.ScanCodes select sc).GroupBy(sc => sc.Value);
         return uniqueCodes.Select(c => c.Key);
      }

      public ScanCode Create(int productId, string value)
      {
         var newCode = new ScanCode {ProductId = productId, Value = value};
         _context.ScanCodes.Add(newCode);

         return newCode;
      }

      public void Commit()
      {
         _context.ObjectContext.SaveChanges();
      }

      public void Remove(int scanCodeId)
      {
         var scanCode = _context.ScanCodes.Find(scanCodeId);

         if (scanCode != null)
            _context.ScanCodes.Remove(scanCode);
      }

      public void UpdateCode(int scanCodeId, string value)
      {
         var code = _context.ScanCodes.Find(scanCodeId);

         if (code != null)
            code.Value = value;
      }
   }
}