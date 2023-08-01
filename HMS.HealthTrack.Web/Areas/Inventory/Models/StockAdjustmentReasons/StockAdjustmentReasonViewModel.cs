using HMS.HealthTrack.Web.Infrastructure;
using System.ComponentModel.DataAnnotations;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.StockAdjustmentReasons
{
   public class StockAdjustmentReasonViewModel : IMapFrom<StockAdjustmentReason>
   {
      public int StockAdjustmentReasonId { get; set; }

      [Required]
      [MaxLength(50, ErrorMessage = "Name must be less than 50 charaters")]
      public string Name { get; set; }

      [MaxLength(1000, ErrorMessage = "Description must be under 1000 charaters")]
      public string Description { get; set; }
      public bool Disabled { get; set; }
      public bool IsSystemReason { get; set; }

   }
}