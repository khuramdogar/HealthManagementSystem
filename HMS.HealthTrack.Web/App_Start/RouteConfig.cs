using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace HMS.HealthTrack.Web
{
   public class RouteConfig
   {
      public static void RegisterWebRoutes(RouteCollection routes)
      {
         routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
         routes.IgnoreRoute("{resource}.ashx/{*pathInfo}");

         routes.MapRoute(
            name: "DefaultWeb",
            url: "{controller}/{action}/{id}",
            defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
         namespaces: new[] { "HMS.HealthTrack.Web.Controllers" });

         //API routes that are used by the web
         routes.MapHttpRoute(
             name: "DefaultApi",
             routeTemplate: "api/{controller}/{id}",
             defaults: new { id = RouteParameter.Optional }
         );
      }

      public static void RegisterApiRoutes(HttpConfiguration config)
      {
         // Web API routes
         config.MapHttpAttributeRoutes();

         config.Routes.MapHttpRoute(
             name: "DefaultApi",
             routeTemplate: "api/{controller}/{id}",
             defaults: new { id = RouteParameter.Optional }
         );

         config.Routes.MapHttpRoute(
         name: "InventoryApi",
         routeTemplate: "api/Inventory/{controller}/{id}",
         defaults: new { id = RouteParameter.Optional }
         );
      }
   }
}
