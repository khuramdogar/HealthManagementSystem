
using HMS.HealthTrack.Web.Infrastructure;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.GeneralLedger
{
   public class GeneralLedgerModel : IMapFrom<Web.Data.Model.Inventory.GeneralLedger>
   {
      public int? LedgerId { get; set; }
      public string Name { get; set; }
      public string Code { get; set; }
      public string AlternateCode { get; set; }
      public int? ParentId { get; set; }
      public bool HasChildren { get; set; }
      public int? ParentLedger { get; set; }
      public int LedgerType { get; set; }
      public string Tier { get; set; }
   }
}