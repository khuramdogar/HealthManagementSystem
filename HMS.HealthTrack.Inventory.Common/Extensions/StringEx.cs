using System;
using System.ComponentModel;
using System.Text;

namespace HMS.HealthTrack.Inventory.Common
{
   public static class StringEx
   {
      /// <summary>
      /// Convert string to T or throw <see cref="ArgumentException"/>
      /// </summary>
      public static T ConvertArgument<T>(this string value, string fieldName)
      {
         try
         {
            return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(value);
         }
         catch (Exception)
         {
            throw new ArgumentException(string.Format("exception=Unable to convert to specified type, field={0}, value=\"{1}\", type={2}", fieldName, value, typeof(T)));
         }
      }

      /// <summary>
      /// Convert string to T or throw <see cref="ArgumentException"/>
      /// </summary>
      public static string[] SplitLines(this string s)
      {
         return s.Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
      }

      public static String ToBase64String(this String source)
      {
         return Convert.ToBase64String(Encoding.Unicode.GetBytes(source));
      }
   }
}
