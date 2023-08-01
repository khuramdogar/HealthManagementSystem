using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Infrastructure;
using HMS.HealthTrack.Web.Utils;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.Products
{
   [ModelMetaType(typeof(ProductMeta))]
   public class OrderProductsViewModel : IMapFrom<Product>
   {
      public int ProductId { get; set; }
      public string LPC { get; set; }
      public string UPN { get; set; }
      public string Description { get; set; }
      public string RebateCode { get; set; }
   }
}