using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.Orders
{
   public class OrderQuickAdd : IValidatableObject
   {
      [Display(Name = "Product")]
      [Required(ErrorMessage = "Please select a valid product")]
      public int ProductId { get; set; }

      [Display(Name = "Quantity")]
      [Required]
      public int Quantity { get; set; }

      public string Name { get; set; }

      public string Notes { get; set; }

      [Display(Name = "Include Rebate Code")]
      public bool IncludeRebateCode { get; set; }

      [Display(Name = "Delivery Location")]
      public int? DeliveryLocationId { get; set; }

      [Display(Name = "Required By")]
      [DataType(DataType.Date)]
      public DateTime? NeedBy { get; set; }

      [Display(Name = "Urgent")]
      public bool IsUrgent { get; set; }

      [Display(Name = "Buy Price")]
      public int BuyPriceTypeId { get; set; }

      public IQueryable<StockSet> AvailableStockSets { get; set; }
      public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
      {
         if (NeedBy != null && NeedBy.Value.Date <= DateTime.Now.Date)
         {
            yield return new ValidationResult("This date must be in the future.", new[] { "NeedBy" });
         }
         if (Quantity < 1)
         {
            yield return new ValidationResult("A valid quantity above zero must be specified.");
         }
      }
   }
}