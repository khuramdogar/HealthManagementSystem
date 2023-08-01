using HMS.HealthTrack.Web.Utils;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Utils
{
   public static class PropertyHelper
   {
      public static string GetPropertyDisplayName(PropertyInfo propertyInfo)
      {
         var attributes = Attribute.GetCustomAttributes(propertyInfo);
         if (attributes.Any(a => a is IgnoreAttribute))
         {
            return null;
         }

         var displayAttributes = attributes.FirstOrDefault(a => a is DisplayAttribute);
         if (displayAttributes != null)
         {
            return ((DisplayAttribute)displayAttributes).Name;
         }
         return propertyInfo.Name;
      }

      public static PropertyInfo GetProductPricePropertyInfo(string propertyName)
      {
         PropertyInfo propertyInfo;

         try
         {
            propertyInfo = new ProductPrice().GetType().GetProperty(propertyName);
            if (propertyInfo == null)
            {
               throw new Exception(String.Format("No property exists for ProductPrice with name {0}", propertyName));
            }
         }
         catch (Exception exception)
         {
            throw;
         }
         return propertyInfo;
      }
   }
}
