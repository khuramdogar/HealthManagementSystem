using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Infrastructure;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.StockSets
{
   public class StockSetItemModel : IMapFrom<StockSetItem>, IValidatableObject
   {
      public int StockSetItemId { get; set; }
      public int StockSetId { get; set; }
      [Required]
      public int? ProductId { get; set; }
      public string ProductSPC { get; set; }
      public string ProductDescription { get; set; }
      public int Quantity { get; set; }

      public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
      {
         if (Quantity < 1)
         {
            yield return new ValidationResult("Quantity must be a number greater than zero.", new[] { "Quantity" });

         }
      }
   }
}