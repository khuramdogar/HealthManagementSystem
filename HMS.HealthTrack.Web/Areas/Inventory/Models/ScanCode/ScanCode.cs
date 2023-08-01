using HMS.HealthTrack.Web.Infrastructure;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.ScanCode
{
   public class ScanCodeModel : IMapFrom<Web.Data.Model.Inventory.ScanCode>
   {
      public int ScanCodeId { get; set; }
      public int ProductId { get; set; }
      public string Value { get; set; }
   }
}