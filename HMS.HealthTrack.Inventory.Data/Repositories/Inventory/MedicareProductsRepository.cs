using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Data.Repositories.Inventory
{
   public class MedicareProductsRepository : IMedicareProductsRepository
   {
      private readonly IDbContextInventoryContext _context;

      public MedicareProductsRepository(IDbContextInventoryContext context)
      {
         _context = context;
      }

      public MedicareProduct Find(int id)
      {
         return
            _context.MedicareProducts
               .Include(mp => mp.MedicareGroup)
               .Include(mp => mp.MedicareSubGroup)
               .Include(mp => mp.MedicareProductSponsor)
               .SingleOrDefault(mp => mp.MedicareProductId == id);
      }

      public MedicareProduct FindByRebateCode(string rebateCode)
      {
         return _context.MedicareProducts
            .Include(mp => mp.MedicareGroup)
            .Include(mp => mp.MedicareSubGroup)
            .Include(mp => mp.MedicareProductSponsor)
            .SingleOrDefault(mp => mp.Code.Equals(rebateCode));
      }

      public IQueryable<MedicareProduct> FindAll()
      {
         return _context.MedicareProducts;
      }

      public List<KeyValuePair<string, string>> Search(string text)
      {
         var terms = text.Split(' ');

         var nameSearch = _context.MedicareProducts.AsQueryable();
         var rebateCodeSearch = _context.MedicareProducts.AsQueryable();

         foreach (var term in terms)
         {
            var keyword = term.Trim();
            nameSearch = nameSearch.Where(product => product.Name.Contains(keyword));
            rebateCodeSearch = rebateCodeSearch.Where(product => product.Code.Contains(keyword));
         }

         var nameKvp =
            nameSearch.AsEnumerable()
               .Select(mp => new KeyValuePair<string, string>(mp.Code, string.Format("{0} - {1}", mp.Code, mp.Name)));
         var rebateCodeKvp =
            rebateCodeSearch.AsEnumerable()
               .Select(mp => new KeyValuePair<string, string>(mp.Code, string.Format("{0} - {1}", mp.Code, mp.Name)));
         return nameKvp.Union(rebateCodeKvp).ToList();
      }
   }

   public interface IMedicareProductsRepository
   {
      MedicareProduct Find(int id);
      MedicareProduct FindByRebateCode(string rebateCode);
      IQueryable<MedicareProduct> FindAll();
      List<KeyValuePair<string, string>> Search(string text);
   }
}