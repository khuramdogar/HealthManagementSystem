using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HMS.HealthTrack.Web.Data.Helpers;

namespace HMS.HealthTrack.Web.App_Start
{
   /// <summary>
   /// Using solution as in http://fairwaytech.com/2014/03/making-c-enums-available-within-javascript/
   /// </summary>
   public static class JavascriptEnabledEnums
   {
      private static IList<JavascriptEnumTypeInfo> Types = null;

      public static void LoadTypes()
      {
         if (Types != null) return;

         Types = new List<JavascriptEnumTypeInfo>();

         var typesWithAttribute =
            from t in Assembly.GetAssembly(typeof(JavascriptEnumAttribute)).GetTypes().Where(xx => xx.IsEnum)
            let attributes = t.GetCustomAttributes(typeof(JavascriptEnumAttribute), true)
            where attributes != null && attributes.Length > 0
            select new { Type = t, Attributes = attributes.Cast<JavascriptEnumAttribute>() };

         foreach (var info in typesWithAttribute.Select(x => new { x.Type, x.Attributes.First().Groups }))
         {
            // if there are no groups then it defaults to everywhere
            if (info.Groups == null || info.Groups.Length == 0)
            {
               Types.Add(new JavascriptEnumTypeInfo
               {
                  Type = info.Type,
                  Group = ""
               });
            }
            else
            {
               foreach (var group in info.Groups)
               {
                  Types.Add(new JavascriptEnumTypeInfo
                  {
                     Type = info.Type,
                     Group = String.IsNullOrEmpty(group) ? "" : group.ToLower()
                  });
               }
            }
         }
      }

      public static Type[] GetTypes(string group)
      {
         LoadTypes();
         return
            Types.Where(xx => string.IsNullOrWhiteSpace(group) || xx.Group.Equals(group))
               .Select(xx => xx.Type)
               .Distinct()
               .ToArray();
      }

   }

   internal class JavascriptEnumTypeInfo
   {
      public string Group { get; set; }
      public Type Type { get; set; }
   }
}