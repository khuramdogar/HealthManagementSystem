using System.Web.Mvc;

namespace HMS.HealthTrack.Web.Areas.Inventory
{
   public class InventoryAreaRegistration : AreaRegistration
   {
      public override string AreaName
      {
         get
         {
            return "Inventory";
         }
      }

      public override void RegisterArea(AreaRegistrationContext context)
      {
         context.MapRoute(
             "Inventory_default",
             "Inventory/{controller}/{action}/{id}",
             defaults: new { controller = "Inventory", action = "Index", id = UrlParameter.Optional },
             namespaces: new[] { "HMS.HealthTrack.Web.Areas.Inventory.Controllers" }
         );
      }
   }
}