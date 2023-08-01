
namespace HMS.HealthTrack.Web.Areas.Inventory.Models.ProductImport
{
   public class GeneralLedgerTierCodeModel
   {
      public int TierId { get; set; }
      public int Tier { get; set; }
      private string _name;
      public string Name
      {
         get { return _name + " ledger code"; }
         set { _name = value; }
      }
      public string Code { get; set; }
      public string LedgerId { get; set; }
   }
}