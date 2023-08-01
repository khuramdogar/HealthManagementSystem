using System.ComponentModel.DataAnnotations;

namespace HMS.HealthTrack.Web.Data.Model.Inventory
{
   public enum OrderItemAction
   {
      Complete = 0,
      [Display(Name = "Keep open")] KeepOpen = 1,
      Cancel = 2
   }
}