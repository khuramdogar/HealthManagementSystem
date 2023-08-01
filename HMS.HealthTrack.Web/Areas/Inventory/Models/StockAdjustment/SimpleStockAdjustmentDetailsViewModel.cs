using HMS.HealthTrack.Web.Infrastructure;
using HMS.HealthTrack.Web.Utils;
using System;
using System.ComponentModel.DataAnnotations;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.StockAdjustment
{
   [ModelMetaType(typeof(StockAdjustmentMeta))]
   public class SimpleStockAdjustmentDetailsViewModel : IMapFrom<Data.Model.Inventory.StockAdjustment>
   {
      public int StockAdjustmentId { get; set; }
      [Display(Name = "Adjusted on")]
      public DateTime AdjustedOn { get; set; }

      [Display(Name = "Adjusted by")]
      public string AdjustedBy { get; set; }

      [Display(Name = "Adjustment source")]
      public AdjustmentSource Source { get; set; }

      [Display(Name = "Quantity")]
      public int Quantity { get; set; }

      [Display(Name = "Clinical record")]
      public long? ClinicalRecordId { get; set; }
   }
}