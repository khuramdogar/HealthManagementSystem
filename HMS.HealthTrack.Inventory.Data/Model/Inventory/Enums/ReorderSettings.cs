using System.ComponentModel.DataAnnotations;

namespace HMS.HealthTrack.Web.Data.Model.Inventory
{
   public enum ReorderSettings
   {
      [Display(Name = "Specify levels")] SpecifyLevels = 0,
      [Display(Name = "Do not auto-order")] DoNotReorder = 1,
      [Display(Name = "Replace on use")] OneForOneReplace = 2
   }
}