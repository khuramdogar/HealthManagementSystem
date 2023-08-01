using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Data.Repositories.Inventory
{
   public sealed class CompanyRepository : ICompanyRepository
   {
      private readonly IDbContextInventoryContext _context;

      public CompanyRepository(IDbContextInventoryContext context)
      {
         _context = context;
      }

      public List<Company> GetPaginatedCompanies(string searchTerm, int pageSize, int pageNum)
      {
         var suppliers = _context.Companies.Where(xx => !xx.deleted).OrderBy(xx => xx.companyName);
         if (!string.IsNullOrWhiteSpace(searchTerm))
         {
            var lowerTerm = searchTerm.ToLower();
            suppliers = suppliers.Where(xx => xx.companyName.Contains(lowerTerm)).OrderBy(xx => xx.companyName);
         }

         return suppliers.Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();
      }

      public IList<Company> Find(IList<int> ids)
      {
         return _context.Companies.Where(xx => ids.Contains(xx.company_ID)).ToList();
      }

      public IQueryable<Company> FindAll()
      {
         return Find(s => s.deleted == false).OrderBy(s => s.companyName);
      }

      public void Commit()
      {
         _context.ObjectContext.SaveChanges();
      }

      public IQueryable<Company> Find(Expression<Func<Company, bool>> predicate)
      {
         return _context.Companies.Where(predicate);
      }
   }

   public interface ICompanyRepository
   {
      void Commit();
      List<Company> GetPaginatedCompanies(string searchTerm, int pageSize, int pageNum);
      IList<Company> Find(IList<int> ids);
      IQueryable<Company> FindAll();
   }
}