using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI;
using HMS.HealthTrack.Web.App_Start;

namespace HMS.HealthTrack.Web.Controllers
{
   public class EnumsController : Controller
   {
      [OutputCache(Duration = Int32.MaxValue, Location = OutputCacheLocation.Any, VaryByParam = "group")]
      public ContentResult GetEnums(string group = null)
      {
         var contentParts = JavascriptEnabledEnums.GetTypes(group).Select(ConvertEnumToJson).ToList();
         var javascript = string.Format("var Enums = {{ {0} }};", string.Join(",", contentParts));
         return Content(javascript, "text/javascript");
      }

      private static string ConvertEnumToJson(Type enumType)
      {
         var ret = enumType.Name + ": {";
         foreach (var val in Enum.GetValues(enumType))
         {
            ret += Enum.GetName(enumType, val) + ":" + ((int)val) + ",";
         }
         ret += "}";
         return ret;
      }
   }
}