using System;
using System.Linq;
using HMS.HealthTrack.Web.Data.Model;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Data.Repositories.Inventory
{
   public interface IConsumptionRepository
   {
      IQueryable<Consumption> UnprocessedConsumptions { get; }
      IQueryable<ConsumptionReversal> ConsumptionReversals { get; }
      ConsumptionReversal ReversedConsumption(int consumptionReference);
      ConsumptionManagement CreateConsumptionManagement(Consumption consumption);
      void Commit();
      ConsumptionManagement CreateConsumptionManagement(Consumption consumption, Exception exception);
      ConsumptionManagement CreateConsumptionManagement(Consumption consumption, string errorMessage);
      void ArchiveConsumptionNotification(ConsumptionManagement conManagement, string username);
   }

   public class ConsumptionRepository : IConsumptionRepository
   {
      private readonly IDbContextInventoryContext _context;

      public ConsumptionRepository(IDbContextInventoryContext context)
      {
         _context = context;
      }
      public IQueryable<Consumption> UnprocessedConsumptions => _context.Consumptions.Where(c => c.ConsumptionManagement == null);
      public IQueryable<ConsumptionReversal> ConsumptionReversals => _context.ConsumptionReversals.Where(c => !c.Processed);

      public ConsumptionReversal ReversedConsumption(int consumptionReference)
         => _context.ConsumptionReversals.SingleOrDefault(r => r.ConsumptionReference == consumptionReference && !r.Processed);

      public ConsumptionManagement CreateConsumptionManagement(Consumption consumption)
      {
         var managementRecord = new ConsumptionManagement
         {
            Consumption = consumption,
            ConsumptionId = consumption.ConsumptionId,
            LastModifiedBy = "Processor",
            LastModifiedOn = DateTime.Now
         };

         _context.ConsumptionManagements.Add(managementRecord);
         return managementRecord;
      }

      public void Commit()
      {
         _context.ObjectContext.SaveChanges();
      }

      public ConsumptionManagement CreateConsumptionManagement(Consumption consumption, Exception exception)
      {
         var record = CreateConsumptionManagement(consumption);
         record.ProcessingStatus = (int) ConsumptionProcessingStatus.Error;
         record.ProcessingStatusMessage = exception.Message;

         return record;
      }

      public ConsumptionManagement CreateConsumptionManagement(Consumption consumption, string errorMessage)
      {
         var record = CreateConsumptionManagement(consumption);
         record.ProcessingStatus = (int)ConsumptionProcessingStatus.Error;
         record.ProcessingStatusMessage = errorMessage;

         return record;
      }

      public void ArchiveConsumptionNotification(ConsumptionManagement conManagement, string username)
      {
         conManagement.ArchivedBy = username;
         conManagement.ArchivedOn = DateTime.Now;
         conManagement.LastModifiedBy = username;
         conManagement.LastModifiedOn = DateTime.Now;
      }
   }
}
