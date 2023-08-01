using HMS.HealthTrack.Web.Infrastructure;
using HMS.HealthTrack.Web.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HMS.HealthTrack.Web.Data.Model.HealthTrackInventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.HealthTrackConsumptions
{
   public class CreateHealthTrackConsumptionModel : IMapFrom<ConsumptionDetails>, IValidatableObject
   {
      [Required(ErrorMessage = "Please select a product above")]
      public int? ProductId { get; set; }

      [Display(Name = "Container ID")]
      public long? ContainerId { get; set; }

      [RequiredIf("ContainerId", null, ErrorMessage = "An MRN is required")]
      public string MRN { get; set; }

      [Display(Name = "Serial number")]
      public string SerialNumber { get; set; }

      [Display(Name = "Lot/Batch number")]
      public string LotNumber { get; set; }

      [Required(ErrorMessage = "A quantity is required")]
      public int Quantity { get; set; }

      [Required(ErrorMessage = "A date of consumption is required")]
      [Display(Name = "Consumed on")]
      public DateTime ConsumedOn { get; set; }

      [Display(Name = "Consumed by")]
      public string ConsumedBy { get; set; }

      [Required(ErrorMessage = "A location is required")]
      [Display(Name = "HealthTrack location")]
      public int? HTLocationId { get; set; }

      public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
      {
         if (!string.IsNullOrWhiteSpace(SerialNumber) && Quantity > 1)
         {
            yield return new ValidationResult("Cannot consume multiple items with for a single serial number", new[] { "Quantity" });
         }
      }
   }
}