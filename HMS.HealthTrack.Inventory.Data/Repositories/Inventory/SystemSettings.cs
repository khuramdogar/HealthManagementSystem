using System;
using System.Collections.Generic;
using System.Linq;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Data.Repositories.Inventory
{
   public class SystemSettings
   {
      private static readonly SystemSettings SettingsIntance = new SystemSettings(new InventoryContext());
      private static List<GeneralLedgerType> _ledgerTypes;

      private IDbContextInventoryContext _context;
      private IList<Property> _properties;

      private SystemSettings(IDbContextInventoryContext context)
      {
         _context = context;
         _properties = context.Properties.ToList();

         var publicPriceType = _context.PriceTypes.FirstOrDefault(pt => pt.Name.Equals("Public"));
         PublicPriceTypeId = publicPriceType == null ? (int?) null : publicPriceType.PriceTypeId;
         var privatePriceType = _context.PriceTypes.FirstOrDefault(pt => pt.Name.Equals("Private"));
         PrivatePriceTypeId = privatePriceType == null ? (int?) null : privatePriceType.PriceTypeId;

         _ledgerTypes = _context.GeneralLedgerTypes.OrderBy(glt => glt.DisplayOrder).ToList();
      }

      public static int TaxRate => GetSetting<int>(SettingName.TaxRate);

      public static int PrimaryBuyPriceTypeId => GetSetting<int>(SettingName.PrimaryBuyPrice);

      public static int? DefaultStockLocationId => GetSetting<int?>(SettingName.DefaultStockLocationId);

      public static int? PrimaryChargeAccountId => GetSetting<int?>(SettingName.PrimaryChargeAccountId);

      public static int ConsumptionProcessingSize
      {
         get
         {
            var setting = GetSetting<int>(SettingName.ConsumptionProcessingSize);
            return setting == 0 ? 1 : setting; //default it rather than 0
         }
      }

      public static int? PublicPriceTypeId { get; private set; }

      public static int? PrivatePriceTypeId { get; private set; }

      public static string GlcSectionDelimiter => GetSetting<string>(SettingName.GlcSectionDelimiter);

      public static string GlcDefaultFillChar => GetSetting<string>(SettingName.GlcDefaultFillChar);

      public static string GlcDefaultLength => GetSetting<string>(SettingName.GlcDefaultLength);

      public static T GetSetting<T>(string name)
      {
         var prop = SettingsIntance._properties.Where(p => p.PropertyName == name).ToList();
         if (!prop.Any())
            return default(T);

         var property = prop.SingleOrDefault();
         var requestedType = typeof(T);
         // account for settings of a nullable type
         var type = Nullable.GetUnderlyingType(requestedType) ?? requestedType;
         return
            (T)
            (property == null || property.PropertyValue == null
               ? null
               : Convert.ChangeType(property.PropertyValue, type));
      }

      public static void Refresh()
      {
         SettingsIntance.Reload();
      }

      public static List<GeneralLedgerType> GetLedgerTypes()
      {
         return _ledgerTypes;
      }

      private void Reload()
      {
         _context = new InventoryContext();
         _properties = _context.Properties.ToList();
      }
   }

   public class SettingName
   {
      public static string TaxRate => "TaxRate";

      public static string PrimaryBuyPrice => "PrimaryBuyPriceTypeId";

      public static string DefaultStockLocationId => "DefaultStockLocationId";

      public static string PrimaryChargeAccountId => "PrimaryChargeAccountId";

      public static string ConsumptionProcessingSize => "ConsumptionProcessingSize";

      public static string GlcSectionDelimiter => "GLCSectionDelimiter";

      public static string GlcDefaultFillChar => "GLCDefaultFillChar";

      public static string GlcDefaultLength => "GLCDefaultLength";
   }

   public class InventorySettingsProvider : IPropertyProvider
   {
      public T GetSetting<T>(string name)
      {
         return SystemSettings.GetSetting<T>(name);
      }

      public int TaxRate => SystemSettings.TaxRate;

      public int PrimaryBuyPriceTypeId => SystemSettings.PrimaryBuyPriceTypeId;

      public int? DefaultStockLocationId => SystemSettings.DefaultStockLocationId;

      public int? PrimaryChargeAccountId => SystemSettings.PrimaryChargeAccountId;

      public int? PublicPriceTypeId => SystemSettings.PublicPriceTypeId;

      public int? PrivatePriceTypeId => SystemSettings.PrivatePriceTypeId;

      public string GlcSectionDelimiter => SystemSettings.GlcSectionDelimiter;

      public string GlcDefaultFillChar => SystemSettings.GlcDefaultFillChar;

      public string GlcDefaultLength => SystemSettings.GlcDefaultLength;

      public List<GeneralLedgerType> LedgerTypes => SystemSettings.GetLedgerTypes();

      public int ConsumptionProcessingSize => SystemSettings.ConsumptionProcessingSize;
   }

   public interface IPropertyProvider
   {
      int TaxRate { get; }
      int PrimaryBuyPriceTypeId { get; }
      int? DefaultStockLocationId { get; }
      int? PrimaryChargeAccountId { get; }
      int? PublicPriceTypeId { get; }
      int? PrivatePriceTypeId { get; }
      string GlcSectionDelimiter { get; }
      string GlcDefaultFillChar { get; }
      string GlcDefaultLength { get; }
      List<GeneralLedgerType> LedgerTypes { get; }
      int ConsumptionProcessingSize { get; }
      T GetSetting<T>(string name);
   }
}