using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.IO;
using System.Web;

namespace HMS.HealthTrack.Web.Infrastructure
{
   public static class JavaScriptConvert
   {
      public static IHtmlString SerializeObject(object value)
      {
         using (var stringWriter = new StringWriter())
         using (var jsonWriter = new JsonTextWriter(stringWriter))
         {
            var serializer = new JsonSerializer
            {
               // Let's use camelCasing as is common practice in JavaScript
               ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            // We don't want quotes around object names
            jsonWriter.QuoteName = true;
            serializer.Serialize(jsonWriter, value);

            return new HtmlString(stringWriter.ToString());
         }
      }
   }

   public static class StringExtensions
   {
      public static int? GetNullableInt(this string value)
      {
         int ret;
         if (int.TryParse(value, out ret))
         {
            return ret;
         }
         return null;
      }

      public static string TrimStart(this string source, string toTrim)
      {
         string s = source;
         while (s.StartsWith(toTrim))
         {
            s = s.Substring(toTrim.Length - 1);
         }
         return s;
      }

      public static string UsernameToShortName(this string name)
      {
         if (string.IsNullOrWhiteSpace(name)) return string.Empty;
         return name.Contains(@"\") ? name.Split(new[] { '\\' })[1] : name;
      }
   }

   public static class ObjectExtensions
   {
      public static string ToNullOrString(this object o)
      {
         return string.IsNullOrWhiteSpace(o.ToString()) ? null : o.ToString().Trim();
      }
   }

   public static class BoolExtensions
   {
      public static string ToDisplayString(this bool boolean)
      {
         return boolean ? "Yes" : "No";
      }

      public static string ToDisplayString(this bool? boolean)
      {
         if (!boolean.HasValue)
         {
            return "Not set";
         }

         return boolean.Value ? "Yes" : "No";
      }
   }
}