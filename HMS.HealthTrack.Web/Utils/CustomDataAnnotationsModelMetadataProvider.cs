using HMS.HealthTrack.Web.Areas.Inventory.Reporting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

namespace HMS.HealthTrack.Web.Utils
{
   /// <summary>
   /// Implementation taken from the following:
   /// http://stackoverflow.com/questions/20591244/mvc-passing-models-vs-viewmodels-how-to-retain-model-meta-information
   /// </summary>
   public class CustomDataAnnotationsModelMetadataProvider : DataAnnotationsModelMetadataProvider
   {
      private static readonly Type OrderReportType = typeof(OrderReport);

      protected override ModelMetadata CreateMetadata(IEnumerable<Attribute> attributes, Type containerType, Func<object> modelAccessor, Type modelType, string propertyName)
      {
         var allAttributes = IncludeMetaAttributes(containerType, propertyName, attributes);
         var meta = base.CreateMetadata(allAttributes, containerType, modelAccessor, modelType, propertyName);
         return meta;
      }

      private static IEnumerable<Attribute> IncludeMetaAttributes(Type containerType, string propertyName, IEnumerable<Attribute> attributes)
      {
         if (containerType == OrderReportType)
         {
            return new List<Attribute>();
         }

         var propertyInfo = !string.IsNullOrWhiteSpace(propertyName) ? containerType.GetProperty(propertyName) : null;
         if (propertyInfo == null)
         {
            return attributes;
         }
         var metaAttributes = propertyInfo.GetMetaAttributes();
         return attributes.Concat(metaAttributes);
      }
   }

   public sealed class ModelMetaTypeAttribute : Attribute
   {
      public Type ModelType { get; set; }
      public ModelMetaTypeAttribute(Type modelType)
      {
         ModelType = modelType;
      }
   }

   public static class ModelMetaTypeUtilities
   {
      private static readonly Dictionary<PropertyInfo, ICollection<Attribute>> MetaAttributesCache = new Dictionary<PropertyInfo, ICollection<Attribute>>();

      public static ICollection<Attribute> GetMetaAttributes(this PropertyInfo propertyInfo)
      {
         var key = propertyInfo;
         if (MetaAttributesCache.ContainsKey(key))
         {
            return MetaAttributesCache[key];
         }

         var attributes = new List<Attribute>();
         var classType = propertyInfo.DeclaringType ?? propertyInfo.ReflectedType;
         var metadataTypeAttribute = classType.GetCustomAttribute<ModelMetaTypeAttribute>();
         if (metadataTypeAttribute != null)
         {
            var metadataType = metadataTypeAttribute.ModelType;

            var metadataPropertyInfo = metadataType.GetProperty(propertyInfo.Name);
            if (metadataPropertyInfo != null)
            {
               attributes = metadataPropertyInfo.GetCustomAttributes().ToList();
            }
         }
         MetaAttributesCache.Add(key, attributes);
         return attributes;
      }
   }
}