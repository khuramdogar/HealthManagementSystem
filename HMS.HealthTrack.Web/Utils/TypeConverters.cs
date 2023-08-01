using AutoMapper;

namespace HMS.HealthTrack.Web.Utils
{
   public class NullIntTypeConverter : TypeConverter<string, int?>
   {
      protected override int? ConvertCore(string source)
      {
         if (string.IsNullOrWhiteSpace(source))
            return null;

         int ret;
         if (int.TryParse(source, out ret))
            return ret;
         return null;
      }
   }

   public class NullIntStringTypeConverter : TypeConverter<int?, string>
   {
      protected override string ConvertCore(int? source)
      {
         return source == null ? string.Empty : source.ToString();
      }
   }

   public class BoolTypeConverter : TypeConverter<bool, string>
   {
      protected override string ConvertCore(bool source)
      {
         return source ? "Yes" : "No";
      }
   }

   public class NullBoolTypeConverter : TypeConverter<bool?, string>
   {
      protected override string ConvertCore(bool? source)
      {
         if (source == null)
            return "Not set";

         return source.Value ? "Yes" : "No";
      }
   }
}