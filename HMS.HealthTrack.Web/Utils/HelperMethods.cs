using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HMS.HealthTrack.Web.Infrastructure
{
   public static class HelperMethods
   {
      public static List<T> CreateObjectsWithIntIds<T>(string ids, string idPropertyName)
      {
         if (string.IsNullOrWhiteSpace(ids)) return null;
         var objectType = typeof(T);
         var property = objectType.GetProperty(idPropertyName);
         var idArray = ids.Split(',');
         var idList = new List<T>();
         // creates an object and sets it's id value
         foreach (var id in idArray)
         {
            var instance = (T)Activator.CreateInstance(objectType);
            int value;
            if (int.TryParse(id, out value))
            {
               property.SetValue(instance, value);
               idList.Add(instance);
            }
            else
            {
               Log.Error(string.Format("Could not parse value {0} to int when creating a list of objects with type {1}", id,
                  objectType));
            }
         }
         return idList;
      }

      public static string GetEnumDisplayName<T>(Enum value)
      {
         var type = typeof(T);
         var name = Enum.GetName(type, value);
         var memberInfo = type.GetMember(name);
         var attributes = memberInfo[0].GetCustomAttributes(typeof(DisplayAttribute), true);
         if (attributes.Length == 0)
            return name;
         var description = ((DisplayAttribute)attributes[0]).Name;
         return description;
      }
   }
}