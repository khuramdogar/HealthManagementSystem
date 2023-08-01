using System.ComponentModel.DataAnnotations;
using HMS.HealthTrack.Web.Data.Helpers;

namespace HMS.HealthTrack.Web.Data.Model.Inventory
{
   [JavascriptEnum("Inventory")]
   public enum OrderItemStatus : byte
   {
      Ordered = 0,
      Received = 1,
      Cancelled = 2,
      [Display(Name = "Partially Received")] PartiallyReceived = 3,
      Reversed = 4,
      [Display(Name = "Notified")] Invoiced = 5,
      Complete = 6
   }
}