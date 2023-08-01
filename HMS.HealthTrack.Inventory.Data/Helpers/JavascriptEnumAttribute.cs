using System;

namespace HMS.HealthTrack.Web.Data.Helpers
{
   public class JavascriptEnumAttribute : Attribute
   {
      public JavascriptEnumAttribute(params string[] groups)
      {
         Groups = groups;
      }

      public string[] Groups { get; set; }
   }
}