using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using HMS.HealthTrack.Web.Data.Model;
using HMS.HealthTrack.Web.Data.Model.HealthTrackInventory;

namespace HMS.HealthTrack.Web.Data.Repositories.HealthTrackInventory
{
   public class InventoryUsedRepository : IInventoryUsedRepository
   {
      private readonly IHealthTrackInventoryContext _context;

      public InventoryUsedRepository(IHealthTrackInventoryContext context)
      {
         _context = context;
      }

      public IList<string> CheckConsumptionForErrors(int consumptionId)
      {
         var validationErrors = new List<string>();

         var consumption = _context.Inventory_Used.SingleOrDefault(c => c.invUsed_ID == consumptionId);

         //Check it exists
         if (consumption == null)
         {
            validationErrors.Add("Consumption notification does not exist and cannot be processed");
            return validationErrors;
         }

         //Check it has a product id
         if (!consumption.invItem_ID.HasValue)
            validationErrors.Add("Missing product ID");

         //Serial numbers should only have a quantity of 1
         if (consumption.invUsed_Qty > 1 && !string.IsNullOrWhiteSpace(consumption.invUsed_SerialNo))
            validationErrors.Add("Cannot have both a serial number a quantity greater than 1");

         // Consumption must have a valid quantity
         if (consumption.invUsed_Qty <= 0)
            validationErrors.Add("Quantity must be greater than 0");

         // must have a location
         if (!consumption.invUsed_Location.HasValue)
            validationErrors.Add("A location must be selected");

         return validationErrors;
      }

      public Inventory_Used Find(int consumptionId)
      {
         return
            _context.Inventory_Used.Include(iu => iu.Inventory_Master).SingleOrDefault(
               iu => iu.invUsed_ID == consumptionId && iu.deleted.HasValue && !iu.deleted.Value);
      }


      /// <summary>
      ///    This creates an Inventory_Used item ONLY. It does not add the entity to the repository!
      /// </summary>
      /// <param name="product"></param>
      /// <param name="consumptionDetails"></param>
      /// <param name="patientId"></param>
      /// <returns></returns>
      public Inventory_Used CreateConsumption(Inventory_Master product, ConsumptionDetails consumptionDetails, int patientId)
      {
         var consumption = new Inventory_Used
         {
            container_ID = consumptionDetails.ContainerId,
            cversion = 0,
            dateCreated = consumptionDetails.ConsumedOn,
            dateLastModified = DateTime.Now,
            deleted = false,
            invDescription = product.Inv_Description,
            invUsed_Location = consumptionDetails.HTLocationId,
            invUsed_LPC = product.Inv_LPC,
            invUsed_Qty = (short) consumptionDetails.Quantity,
            invUsed_SerialNo = consumptionDetails.SerialNumber,
            invUsed_SPC = product.Inv_SPC,
            LOTNO = consumptionDetails.LotNumber,
            patient_ID = patientId,
            StockStatus = ConsumptionProcessingStatus.Unprocessed,
            userCreated = consumptionDetails.ConsumedBy,
            userLastModified = consumptionDetails.ConsumedBy
         };
         return consumption;
      }

      public void Commit()
      {
         _context.SaveChanges();
      }

      public void Update(HealthTrackConsumptionUpdate healthTrackConsumptionUpdate, string username)
      {
         var item = _context.Inventory_Used.SingleOrDefault(c => c.invUsed_ID == healthTrackConsumptionUpdate.UsedId);

         if (item == null) return;

         item.LOTNO = healthTrackConsumptionUpdate.LotNumber;
         item.invUsed_SerialNo = healthTrackConsumptionUpdate.SerialNumber;
         item.invItem_ID = healthTrackConsumptionUpdate.ProductId;
         item.invUsed_Qty = healthTrackConsumptionUpdate.Quantity;
         item.invUsed_Location = healthTrackConsumptionUpdate.Location;
         item.patient_ID = healthTrackConsumptionUpdate.PatientId;
         item.StatusMessage = string.Empty;
         item.dateLastModified = DateTime.Now;
         item.userLastModified = username;
      }
   }

   public interface IInventoryUsedRepository
   {
      void Commit();
      void Update(HealthTrackConsumptionUpdate healthTrackConsumptionUpdate, string username);
      IList<string> CheckConsumptionForErrors(int consumptionId);
      Inventory_Used Find(int consumptionId);
      Inventory_Used CreateConsumption(Inventory_Master product, ConsumptionDetails consumptionDetails, int patientId);
   }
}