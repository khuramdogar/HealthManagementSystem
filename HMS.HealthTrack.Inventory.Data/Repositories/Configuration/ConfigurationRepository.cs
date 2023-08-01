using System;
using System.ComponentModel;
using System.Linq;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Data.Repositories.Configuration
{
   public class ConfigurationRepository : IConfigurationRepository
   {
      private readonly IDbContextInventoryContext _context;
      private readonly ICustomLogger _customLogger;

      public ConfigurationRepository(
         IDbContextInventoryContext context,
         ICustomLogger customLogger)
      {
         _context = context;
         _customLogger = customLogger;
      }

      public T GetConfigurationValue<T>(string propertyIdentifier)
      {
         var config = _context.Properties.SingleOrDefault(p => p.PropertyName == propertyIdentifier);

         if (config == null)
         {
            _customLogger.Error($"exception=Unable to locate configuration in database, name={propertyIdentifier}");
            return default(T);
         }

         try
         {
            return (T) TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(config.PropertyValue);
         }
         catch (Exception)
         {
            _customLogger.Error($"exception=Unable to convert config value, name={propertyIdentifier}, type={typeof(T)}, value={config.PropertyValue}");
            return default(T);
         }
      }
   }

   public interface IConfigurationRepository
   {
      T GetConfigurationValue<T>(string propertyIdentifier);
   }
}