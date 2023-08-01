using System.ComponentModel.DataAnnotations;
using HMS.HealthTrack.Web.Data.Helpers;

namespace HMS.HealthTrack.Web.Data.Model.Inventory
{
   [JavascriptEnum("Inventory")]
   public enum OrderStatus
   {
      Created = 0,
      [Display(Name = "Pending Approval")] PendingApproval = 1,
      Approved = 2,
      Ordered = 3,
      [Display(Name = "Partially Received")] PartiallyReceived = 4,
      Complete = 5,
      Failed = 6,
      Rejected = 7,
      Cancelled = 8,
      Reversed = 9,
      [Display(Name = "Notified")] Invoiced = 10
   }
}