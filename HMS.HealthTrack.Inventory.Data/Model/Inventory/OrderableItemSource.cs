using System.ComponentModel.DataAnnotations;

namespace HMS.HealthTrack.Web.Data.Model.Inventory
{
   public enum OrderableItemSource
   {
      [Display(Name = "Top-up")] Topup = 1,
      Request = 2,
      [Display(Name = "Consumption Notice")] Invoice = 5,
      Replacement = 6
   }
}