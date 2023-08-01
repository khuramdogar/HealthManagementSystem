using AutoMapper;
using HMS.HealthTrack.Web.Infrastructure;
using HMS.HealthTrack.Web.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HMS.HealthTrack.Web.App_Start
{
   public class AutoMapperConfig
   {
      public static void Configure()
      {
         var types = Assembly.GetExecutingAssembly().GetExportedTypes();
         LoadCustomTypeConverters();
         LoadStandardMappings(types);
         LoadCustomMappings(types);
      }

      private static void LoadCustomTypeConverters()
      {
         Mapper.CreateMap<string, int?>().ConvertUsing<NullIntTypeConverter>();
         Mapper.CreateMap<int?, string>().ConvertUsing<NullIntStringTypeConverter>();

         Mapper.CreateMap<bool, string>().ConvertUsing<BoolTypeConverter>();
         Mapper.CreateMap<bool?, string>().ConvertUsing<NullBoolTypeConverter>();
      }

      private static void LoadCustomMappings(Type[] types)
      {
         var maps = (from t in types
                     from i in t.GetInterfaces()
                     where typeof(IHaveCustomMappings).IsAssignableFrom(t) &&
                           !t.IsAbstract &&
                           !t.IsInterface
                     select (IHaveCustomMappings)Activator.CreateInstance(t)).ToArray();

         foreach (var map in maps)
         {
            map.CreateMappings(Mapper.Configuration);
         }
      }

      private static void LoadStandardMappings(IEnumerable<Type> types)
      {
         var maps = (from t in types
                     from i in t.GetInterfaces()
                     where i.IsGenericType &&
                           i.GetGenericTypeDefinition() == typeof(IMapFrom<>) && // process any type that implements IMapFrom
                           !t.IsAbstract && // skip abstract types and interfaces
                           !t.IsInterface
                     select new
                     {
                        Source = i.GetGenericArguments()[0],
                        Destination = t
                     }
            ).ToArray();
         foreach (var map in maps)
         {
            Mapper.CreateMap(map.Source, map.Destination);
            Mapper.CreateMap(map.Destination, map.Source);
         }
      }
   }
}