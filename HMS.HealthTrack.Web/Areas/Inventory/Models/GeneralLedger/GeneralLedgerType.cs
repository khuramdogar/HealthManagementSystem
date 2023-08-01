using HMS.HealthTrack.Web.Infrastructure;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.GeneralLedger
{
   public class GeneralLedgerType : IMapFrom<Web.Data.Model.Inventory.GeneralLedgerType>
   {
      public int LedgerTypeId { get; set; }
      public string Name { get; set; }
      public int DisplayOrder { get; set; }
   }
}