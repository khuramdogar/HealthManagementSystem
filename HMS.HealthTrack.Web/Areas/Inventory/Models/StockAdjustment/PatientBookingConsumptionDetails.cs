
using System;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.StockAdjustment
{
   public class PatientBookingConsumptionDetails
   {
      public string PatientNumber { get; set; }
      public string FirstName { get; set; }
      public string Surname { get; set; }
      public DateTime? AppointmentDate { get; set; }
      public long AdjustmentId { get; set; }
   }
}
