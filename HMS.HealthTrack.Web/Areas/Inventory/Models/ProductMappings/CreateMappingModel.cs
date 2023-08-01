using HMS.HealthTrack.Web.Areas.Inventory.Models.Products;
using System.ComponentModel.DataAnnotations;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.ProductMappings
{
   public class CreateMappingModel
   {
      [Required(ErrorMessage = "Please select a product to map to")]
      public int? MapToProductId { get; set; }
      public CreateProductsViewModel CreateProductsViewModel { get; set; }
      public int MapFromExternalId { get; set; }
      public int? ConsumptionId { get; set; }

      public string Description { get; set; }
      public string UPN { get; set; }
      public string SPC { get; set; }
   }
}
