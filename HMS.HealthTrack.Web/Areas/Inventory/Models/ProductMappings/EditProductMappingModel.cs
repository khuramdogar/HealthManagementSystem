using HMS.HealthTrack.Web.Infrastructure;
using System.ComponentModel.DataAnnotations;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.ProductMappings
{
   public class EditProductMappingModel : IMapFrom<ExternalProductMapping>
   {
      public int ProductMappingId { get; set; }

      [Display(Name = "External ID")]
      public int ExternalProductId { get; set; }

      [Display(Name = "Map to")]
      public int InventoryProductId { get; set; }

      [Display(Name = "Product source")]
      public ProductMappingSource ProductSource { get; set; }
   }
}