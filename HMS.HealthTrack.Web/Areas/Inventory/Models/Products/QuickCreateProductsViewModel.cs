using HMS.HealthTrack.Web.Areas.Inventory.Models.Stock;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.Products
{
   public class QuickCreateProductsViewModel
   {
      [Required, Display(Name = "Description")]
      public string QuickCreateDescription { get; set; }
      [Required, Display(Name = "SPC")]
      public string QuickCreateSpc { get; set; }
      [Display(Name = "UPN")]
      public string QuickCreateUpn { get; set; }

      public IEnumerable<StockAtLocationModel> InitialStock { get; set; }
   }
}