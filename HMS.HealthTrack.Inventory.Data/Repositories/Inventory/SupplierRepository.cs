using System;
using System.Data.Entity;
using System.Linq;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Data.Repositories.Inventory
{
   public class SupplierRepository : ISupplierRepository
   {
      private readonly IDbContextInventoryContext _context;

      public SupplierRepository(IDbContextInventoryContext context)
      {
         _context = context;
      }

      public IQueryable<Supplier> FindAll()
      {
         return
            _context.Suppliers.Include(s => s.Company)
               .Include(s => s.Company.AddressCorporates)
               .Where(s => !s.Company.deleted);
      }

      public void Add(SupplierModel model, string user)
      {
         var company = new Company
         {
            billing_HealthFund = false,
            billing_Medicare = false,
            billing_Other = false,
            billing_Practice = false,
            billing_Scheduled = false,
            companyName = model.Name,
            DateCreated = DateTime.Now,
            DateLastModified = DateTime.Now,
            healthFundType = "N",
            UserCreated = user,
            UserLastModified = user
         };

         var address = new AddressCorporate
         {
            address1 = model.Address1,
            address2 = model.Address2,
            suburb = model.Suburb,
            state = model.State,
            postcode = model.PostCode,
            country = model.Country,
            primaryAddress = true,
            addressType = 0
         };

         company.AddressCorporates.Add(address);

         var supplier = new Supplier
         {
            CreatedBy = user,
            LastModifiedBy = user,
            LastModifiedOn = DateTime.Now
         };
         supplier.Company = company;

         _context.Suppliers.Add(supplier);
      }

      public void Commit()
      {
         _context.ObjectContext.SaveChanges();
      }

      public Supplier Find(int id)
      {
         return _context.Suppliers.Include(s => s.Company.AddressCorporates).SingleOrDefault(s => s.company_ID == id);
      }

      public void Update(SupplierModel model)
      {
         if (!model.company_ID.HasValue)
            return;

         var existingSupplier = Find(model.company_ID.Value);
         existingSupplier.LastModifiedBy = model.LastModifiedBy;
         existingSupplier.LastModifiedOn = DateTime.Now;
         existingSupplier.Company.companyName = model.Name;
         existingSupplier.Company.DateLastModified = DateTime.Now;
         existingSupplier.Company.UserLastModified = model.LastModifiedBy;

         var existingAddress = existingSupplier.Company.AddressCorporates.First(ac => ac.primaryAddress);
         existingAddress.address1 = model.Address1;
         existingAddress.address2 = model.Address2;
         existingAddress.department = model.Department;
         existingAddress.suburb = model.Suburb;
         existingAddress.state = model.State;
         existingAddress.postcode = model.PostCode;
         existingAddress.country = model.Country;

         existingAddress.contactTitle = model.ContactTitle;
         existingAddress.contactFirstname = model.ContactFirstname;
         existingAddress.contactSurname = model.ContactSurname;
         existingAddress.phoneNumber = model.PhoneNumber;
         existingAddress.faxNumber = model.FaxNumber;
         existingAddress.email = model.Email;
         existingAddress.webSite = model.WebSite;
      }

      public void Remove(int id, string user)
      {
         var supplier = Find(id);
         supplier.Company.deleted = true;
         supplier.Company.deletionDate = DateTime.Now;
         supplier.Company.deletionUser = user;
      }

      public Supplier FindByName(string name)
      {
         var supplier = _context.Suppliers.FirstOrDefault(s => s.Company.companyName.Equals(name));
         return supplier ?? _context.Suppliers.Local.FirstOrDefault(s => s.Company.companyName.Equals(name));
      }

      public Supplier FindLocalByName(string name)
      {
         return _context.Suppliers.Local.FirstOrDefault(s => s.Company.companyName.Equals(name));
      }
   }

   public interface ISupplierRepository
   {
      IQueryable<Supplier> FindAll();
      void Add(SupplierModel model, string user);
      void Commit();
      Supplier Find(int id);
      void Update(SupplierModel model);
      void Remove(int id, string user);
      Supplier FindByName(string name);
      Supplier FindLocalByName(string name);
   }
}