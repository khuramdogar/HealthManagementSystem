using System;
using System.Collections.Generic;

namespace HMS.HealthTrack.Web.Data.Helpers
{
   public static class EnumHelper
   {
      public static HashSet<string> EnumToHashSet<T>()
      {
         var names = Enum.GetNames(typeof(T));
         var hsNames = new HashSet<string>();
         foreach (var name in names) hsNames.Add(name);
         return hsNames;
      }
   }
}