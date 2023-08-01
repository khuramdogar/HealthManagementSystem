using System;
using System.Collections.Generic;
using System.Linq;
using HMS.HealthTrack.Web.Data.Model;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Data.Repositories.Inventory
{
   public class HealthTrackConsumptionRepository : IHealthTrackConsumptionRepository
   {
      private readonly IDbContextInventoryContext _context;

      public HealthTrackConsumptionRepository(IDbContextInventoryContext context)
      {
         _context = context;
      }

      /// <summary>
      ///    Gets consumption details with doctor, patient and procedure information
      /// </summary>
      /// <returns>Items used in procedures</returns>
      public IQueryable<ClinicalConsumption> FindClinicalConsumptions()
      {
         return from c in _context.ClinicalConsumptions select c;
      }

      /// <summary>
      ///    Gets all consumption records from HealthTrack
      /// </summary>
      /// <param name="categoryId"></param>
      /// <returns>Consumption with product details</returns>
      public IQueryable<HealthTrackConsumption> FindHealthTrackConsumptions()
      {
         return from c in _context.HealthTrackConsumptions where c.deleted == false select c;
      }

      /// <summary>
      ///    Gets all consumption records from HealthTrack with a specific status
      /// </summary>
      /// <param name="status">Processing status (StockStatus)</param>
      /// <returns>Consumption with product details</returns>
      public IQueryable<HealthTrackConsumption> FindHealthTrackConsumptions(ConsumptionProcessingStatus status)
      {
         return from c in FindHealthTrackConsumptions() where c.ProcessingStatus == status select c;
      }

      public void Commit()
      {
         _context.ObjectContext.SaveChanges();
      }

      public IQueryable<HealthTrackConsumption> FindHealthTrackConsumptions(IEnumerable<int> categoryId)
      {
         return from con in FindHealthTrackConsumptions()
            join cat in _context.ExternalMappedCategories on con.ProductId equals cat.ExternalProductId
            where cat.CategoryId.HasValue && categoryId.Contains(cat.CategoryId.Value)
            select con;
      }

      /// <summary>
      ///    Gets a specific consumption record from HealthTrack
      /// </summary>
      /// <param name="consumptionId">InvUsed_ID</param>
      /// <returns>A consumption record with product details</returns>
      public HealthTrackConsumption Find(int consumptionId)
      {
         return _context.HealthTrackConsumptions.SingleOrDefault(c => c.UsedId == consumptionId);
      }

      public IQueryable<HealthTrackConsumption> FindNonInvoicedConsumptions()
      {
         var results = from cn in _context.HealthTrackConsumptions
            join cnm in _context.ConsumptionNotificationManagements on cn.UsedId equals cnm.invUsed_ID into
               gcnm
            from cnm in gcnm.DefaultIfEmpty()
            where cnm == null || !cnm.Invoiced
            select cn;

         return results;
      }

      public IQueryable<HealthTrackConsumption> FindNonArchivedConsumptions()
      {
         var results = from cn in _context.HealthTrackConsumptions
            where cn.deleted == false && cn.ArchivedOn == null
            select cn;
         return results;
      }

      public IQueryable<ConsumptionNotificationManagement> FindNonArchivedConsumptionNotifications()
      {
         var results = _context.ConsumptionNotificationManagements.Where(cnm => cnm.ArchivedOn == null);
         return results;
      }

      public void ArchiveConsumptionNotification(ConsumptionNotificationManagement notificationToIgnore, string username)
      {
         notificationToIgnore.ArchivedBy = username;
         notificationToIgnore.ArchivedOn = DateTime.Now;
         notificationToIgnore.LastModifiedBy = username;
         notificationToIgnore.LastModifiedOn = DateTime.Now;
      }

      public ConsumptionNotificationManagement FindConsumptionNotificationManagement(int invUsedId)
      {
         return _context.ConsumptionNotificationManagements.SingleOrDefault(cnm => cnm.invUsed_ID == invUsedId);
      }

      public void AddConsumptionNotificationManagement(ConsumptionNotificationManagement management)
      {
         _context.ConsumptionNotificationManagements.Add(management);
      }

      public void MarkAsReported(int consumptionId, string user)
      {
         var consumptionNotification = _context.ConsumptionNotificationManagements.SingleOrDefault(c => c.invUsed_ID == consumptionId);
         if (consumptionNotification == null)
         {
            consumptionNotification = new ConsumptionNotificationManagement {invUsed_ID = consumptionId};
            _context.ConsumptionNotificationManagements.Add(consumptionNotification);
         }

         consumptionNotification.Reported = true;
         consumptionNotification.ReportedBy = user;
         consumptionNotification.ReportedOn = DateTime.Now;
         consumptionNotification.LastModifiedBy = user;
         consumptionNotification.LastModifiedOn = DateTime.Now;
      }

      public ConsumptionNotificationManagement FindConsumptionNotification(int consumptionId)
      {
         return _context.ConsumptionNotificationManagements.SingleOrDefault(cnm => cnm.invUsed_ID == consumptionId);
      }

      public IQueryable<ConsumptionNotificationManagement> FindAllConsumptionNotificationManagements()
      {
         return _context.ConsumptionNotificationManagements;
      }

      public IQueryable<ConsumptionNotificationManagement> GetUnprocessed()
      {
         return
            _context.ConsumptionNotificationManagements.Where(
               cnm => cnm.ProcessingStatus == ConsumptionProcessingStatus.Unprocessed);
      }

      public IQueryable<ConsumptionNotificationManagement> GetUnprocessedConsumptionNotifications(int externalProductId)
      {
         var consumptions = from cnm in _context.ConsumptionNotificationManagements
            join htc in FindHealthTrackConsumptions() on cnm.invUsed_ID equals htc.UsedId
            where cnm.ProcessingStatus == ConsumptionProcessingStatus.Unprocessed
                  && htc.ProductId == externalProductId
            select cnm;
         return consumptions;
      }

      public IQueryable<ConsumptionNotificationManagement> GetAllNotifications()
      {
         return _context.ConsumptionNotificationManagements;
      }

      public void ManageNewConsumptions(string username)
      {
         var unmanagedConsumptions = _context.UnmanagedConsumptions;
         foreach (var unmanagedConsumption in unmanagedConsumptions)
            AddConsumptionNotificationManagement(new ConsumptionNotificationManagement
            {
               invUsed_ID = unmanagedConsumption.invUsed_ID,
               LastModifiedBy = username,
               LastModifiedOn = DateTime.Now
            });
      }

      public ConsumptionNotificationManagement ManageConsumption(int id)
      {
         var unmanagedConsumption = _context.UnmanagedConsumptions.SingleOrDefault(uc => uc.invUsed_ID == id);
         if (unmanagedConsumption == null) return null;

         var consumptionManagement = new ConsumptionNotificationManagement
         {
            invUsed_ID = unmanagedConsumption.invUsed_ID
         };
         AddConsumptionNotificationManagement(consumptionManagement);
         return consumptionManagement;
      }

      /// <summary>
      ///    Mark as processed but ignore the consumption notification
      /// </summary>
      /// <param name="consumptionId"></param>
      /// <param name="username"></param>
      public void IgnoreUnprocessed(int consumptionId, string username)
      {
         var consumptionNotification = FindConsumptionNotification(consumptionId);

         if (consumptionNotification == null) return;

         consumptionNotification.ArchivedBy = username;
         consumptionNotification.ArchivedOn = DateTime.Now;
         consumptionNotification.LastModifiedBy = username;
         consumptionNotification.LastModifiedOn = DateTime.Now;
         consumptionNotification.ProcessingStatus = ConsumptionProcessingStatus.Processed;
         consumptionNotification.ProcessingStatusMessage = string.Empty;
      }

      /// <summary>
      ///    Set the consumption as Ignored so that it is not taken into account when retreiving processed consumptions,
      ///    specifically for Approve Items
      /// </summary>
      /// <param name="consumptionId"></param>
      /// <param name="username"></param>
      public void ArchiveProcessed(int consumptionId, string username)
      {
         var consumptionNotification = FindConsumptionNotification(consumptionId);

         if (consumptionNotification == null) return;

         consumptionNotification.ArchivedBy = username;
         consumptionNotification.ArchivedOn = DateTime.Now;
         consumptionNotification.LastModifiedBy = username;
         consumptionNotification.LastModifiedOn = DateTime.Now;
      }

      public void ClearAllErrors(string username)
      {
         var records = from c in _context.ConsumptionNotificationManagements
            where c.ProcessingStatus == ConsumptionProcessingStatus.Error
            select c;

         foreach (var consumptionNotification in records)
         {
            consumptionNotification.ProcessingStatus = ConsumptionProcessingStatus.Unprocessed;
            consumptionNotification.ProcessingStatusMessage = "";
            consumptionNotification.LastModifiedBy = username;
            consumptionNotification.LastModifiedOn = DateTime.Now;
         }
      }

      public List<string> FindUsedPaymentClasses()
      {
         return
            _context.HealthTrackConsumptions.Where(c => c.PaymentClass != null)
               .Select(c => c.PaymentClass)
               .Distinct()
               .ToList();
      }

      public bool ArchiveConsumptions(List<int> replacementConsumptionIds, out string errorMessage, string username)
      {
         var consumptionNotifications =
            FindAllConsumptionNotificationManagements()
               .Where(cnm => replacementConsumptionIds.Contains(cnm.invUsed_ID));

         if (replacementConsumptionIds.Count() != consumptionNotifications.Count())
         {
            errorMessage = "Unable to find one or more Replacement Items for Approval. Please refresh and try again.";
            return false;
         }

         foreach (var notification in consumptionNotifications) ArchiveConsumptionNotification(notification, username);
         errorMessage = string.Empty;
         return true;
      }
   }

   public interface IHealthTrackConsumptionRepository
   {
      // HealthTrackConsumption methods
      IQueryable<ClinicalConsumption> FindClinicalConsumptions();
      IQueryable<HealthTrackConsumption> FindHealthTrackConsumptions();
      HealthTrackConsumption Find(int consumptionId);
      IQueryable<HealthTrackConsumption> FindHealthTrackConsumptions(ConsumptionProcessingStatus status);
      void Commit();
      IQueryable<HealthTrackConsumption> FindHealthTrackConsumptions(IEnumerable<int> categoryId);
      IQueryable<HealthTrackConsumption> FindNonInvoicedConsumptions();

      // Consumption Notification Management methods
      ConsumptionNotificationManagement FindConsumptionNotificationManagement(int invUsedId);
      void AddConsumptionNotificationManagement(ConsumptionNotificationManagement management);
      void MarkAsReported(int consumptionId, string username);
      ConsumptionNotificationManagement FindConsumptionNotification(int consumptionId);
      IQueryable<ConsumptionNotificationManagement> GetUnprocessed();
      IQueryable<ConsumptionNotificationManagement> GetUnprocessedConsumptionNotifications(int externalProductId);
      IQueryable<ConsumptionNotificationManagement> GetAllNotifications();
      void ManageNewConsumptions(string username);
      ConsumptionNotificationManagement ManageConsumption(int id);
      void IgnoreUnprocessed(int consumptionId, string username);
      void ClearAllErrors(string name);
      List<string> FindUsedPaymentClasses();
      void ArchiveProcessed(int consumptionId, string username);
      IQueryable<HealthTrackConsumption> FindNonArchivedConsumptions();
      IQueryable<ConsumptionNotificationManagement> FindNonArchivedConsumptionNotifications();
      void ArchiveConsumptionNotification(ConsumptionNotificationManagement notificationToIgnore, string username);
      IQueryable<ConsumptionNotificationManagement> FindAllConsumptionNotificationManagements();
      bool ArchiveConsumptions(List<int> replacementConsumptionIds, out string errorMessage, string username);
   }
}