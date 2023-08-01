using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.ProductMappings
{
   public class MappingDetails
   {
      public MappingOverview Overview { get; set; }
      public ExternalProductMapping ExternalProductMapping { get; set; }
   }
}
