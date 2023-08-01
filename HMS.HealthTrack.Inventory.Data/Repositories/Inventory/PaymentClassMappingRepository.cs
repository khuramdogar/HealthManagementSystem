using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Data.Repositories.Inventory
{
   public class PaymentClassMappingRepository : IPaymentClassMappingRepository
   {
      private readonly IDbContextInventoryContext _context;

      public PaymentClassMappingRepository(IDbContextInventoryContext context)
      {
         _context = context;
      }

      public IQueryable<PaymentClassMapping> FindAll()
      {
         return _context.PaymentClassMappings.Include(m => m.PriceType);
      }

      public PaymentClassMapping Find(string paymentClass)
      {
         return _context.PaymentClassMappings.Include(m => m.PriceType).SingleOrDefault(m => m.PaymentClass == paymentClass);
      }

      public void Create(string paymentClass, int priceTypeId, string username)
      {
         var mapping = new PaymentClassMapping
         {
            CreatedBy = username,
            CreatedOn = DateTime.Now,
            ModifiedBy = username,
            ModifiedOn = DateTime.Now,
            PaymentClass = paymentClass,
            PriceTypeId = priceTypeId
         };
         _context.PaymentClassMappings.Add(mapping);
      }

      public void Update(string paymentClass, int priceTypeId, string username)
      {
         var existing = Find(paymentClass);
         if (existing == null) return;

         existing.PriceTypeId = priceTypeId;
         existing.ModifiedBy = username;
         existing.ModifiedOn = DateTime.Now;
      }

      public void Remove(string paymentClass, string username)
      {
         var existing = Find(paymentClass);
         if (existing == null) return;

         _context.PaymentClassMappings.Remove(existing);
      }

      public void Commit()
      {
         _context.ObjectContext.SaveChanges();
      }

      public int UnmappedPaymentClasses()
      {
         var mappedPaymentClasses =
            FindAll().Where(pc => !pc.PriceTypeId.HasValue).Select(pc => pc.PaymentClass).Distinct().Count();
         return mappedPaymentClasses;
      }

      public void AddNewlyUsedPaymentClasses(IList<string> usedPaymentClasses)
      {
         var newPaymentClasses = usedPaymentClasses.Except(FindAll().Select(pc => pc.PaymentClass)).ToList();
         if (newPaymentClasses.Any())
            foreach (var newPaymentClass in newPaymentClasses)
               _context.PaymentClassMappings.Add(new PaymentClassMapping
               {
                  CreatedBy = "Consumption Processor",
                  CreatedOn = DateTime.Now,
                  ModifiedBy = "Consumption Processor",
                  ModifiedOn = DateTime.Now,
                  PaymentClass = newPaymentClass
               });
      }
   }

   public interface IPaymentClassMappingRepository
   {
      IQueryable<PaymentClassMapping> FindAll();
      PaymentClassMapping Find(string paymentClass);
      void Create(string paymentClass, int priceTypeId, string username);
      void Update(string paymentClass, int priceTypeId, string username);
      void Remove(string paymentClass, string username);
      void Commit();
      int UnmappedPaymentClasses();
      void AddNewlyUsedPaymentClasses(IList<string> usedPaymentClasses);
   }
}