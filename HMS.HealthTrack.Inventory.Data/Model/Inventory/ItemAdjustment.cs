using System;
using System.ComponentModel.DataAnnotations;

namespace HMS.HealthTrack.Web.Data.Model.Inventory
{
   public class ItemAdjustment
   {
      [Required]
      [Range(1, int.MaxValue, ErrorMessage = "Product ID is an invalid number")]

      public int ProductId { get; set; }


      public string SerialNumber { get; set; }

      public string BatchNumber { get; set; }

      public string AdjustedBy { get; set; }

      public DateTime AdjustedOn { get; set; }

      [Required]
      public int? StockLocationId { get; set; }

      public int Quantity { get; set; }
      public bool HasSerial => !string.IsNullOrWhiteSpace(SerialNumber);
      public bool HasBatchNumber => !string.IsNullOrWhiteSpace(BatchNumber);
      public int? PatientId { get; set; }
      public int? ClinicalRecordId { get; set; }
      public AdjustmentSource Source { get; set; }
      public int? ReasonId { get; set; }
      public int? LedgerId { get; set; }
      public string Note { get; set; }
      public bool IsPositive { get; set; }
   }
}