using HMS.HealthTrack.Web.Areas.Inventory.Models.Products;
using System.Collections.Specialized;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory
{
   /// <summary>
   /// This class defines the order, column names and related product fields for importing/exporting products.
   /// </summary>
   public static class ProductImportColumnNames
   {
      public static OrderedDictionary ProductExportColumnsNames = new OrderedDictionary()
      {
         {Consumed, Nameof<ExportProductsViewModel>.Property(m => m.ConsumptionCount)},
         {Spc, Nameof<ExportProductsViewModel>.Property(m => m.SPC)},
         {Description, Nameof<ExportProductsViewModel>.Property(m => m.Description)},
         {Notes, Nameof<ExportProductsViewModel>.Property(m => m.ProductNotes)},
         {Supplier, Nameof<ExportProductsViewModel>.Property(m => m.PrimarySupplier)},
         {MinOrder, Nameof<ExportProductsViewModel>.Property(m => m.ProductMinimumOrder)},
         {OrderMultiple, Nameof<ExportProductsViewModel>.Property(m => m.ProductOrderMultiple)},
         {ReorderThreshold, Nameof<ExportProductsViewModel>.Property(m => m.ProductReorderThreshold)},
         {TargetStockLevel, Nameof<ExportProductsViewModel>.Property(m => m.ProductTargetStockLevel)},
         {PublicBuyPrice, Nameof<ExportProductsViewModel>.Property(m => m.PublicBuyPrice)},
         {PrivateBuyPrice, Nameof<ExportProductsViewModel>.Property(m => m.PrivateBuyPrice)},
         {Consignment, Nameof<ExportProductsViewModel>.Property(m => m.Consignment)},
         {Lpc, Nameof<ExportProductsViewModel>.Property(m => m.ProductLPC)},
         {Upn, Nameof<ExportProductsViewModel>.Property(m => m.UPN)},
         {Manufacturer, Nameof<ExportProductsViewModel>.Property(m => m.Manufacturer)},
         {Sterile, Nameof<ExportProductsViewModel>.Property(m => m.ProductUseSterile)},
         {RebateCode, Nameof<ExportProductsViewModel>.Property(m => m.RebateCode)},
         {ProductId, Nameof<ExportProductsViewModel>.Property(m => m.ProductProductId)},
         {InternalProductId, Nameof<ExportProductsViewModel>.Property(m => m.ProductProductId)},
         {Category, Nameof<ExportProductsViewModel>.Property(m => m.Categories)},
         {AutoReorderSetting, Nameof<ExportProductsViewModel>.Property(m => m.AutoReorderSetting)},
         {ProductSettings, Nameof<ExportProductsViewModel>.Property(m => m.Settings)},
         {UseCategorySettings, Nameof<ExportProductsViewModel>.Property(m => m.ProductUseCategorySettings)},
      };

      public static OrderedDictionary ProductImportColumnsNames = new OrderedDictionary()
      {
         {Spc, Nameof<ProductImport>.Property(m => m.SPC)},
         {Description, Nameof<ProductImport>.Property(m => m.Description)},
         {Notes, Nameof<ProductImport>.Property(m => m.Notes)},
         {Supplier, Nameof<ProductImport>.Property(m => m.Supplier)},
         {MinOrder, Nameof<ProductImport>.Property(m => m.MinimumOrder)},
         {OrderMultiple, Nameof<ProductImport>.Property(m => m.OrderMultiple)},
         {ReorderThreshold, Nameof<ProductImport>.Property(m => m.ReorderThreshold)},
         {TargetStockLevel, Nameof<ProductImport>.Property(m => m.TargetStockLevel)},
         {PublicBuyPrice, Nameof<ProductImport>.Property(m => m.PublicUnitPrice)},
         {PrivateBuyPrice, Nameof<ProductImport>.Property(m => m.PrivateUnitPrice)},
         {Consignment, Nameof<ProductImport>.Property(m => m.Consignment)},
         {Lpc, Nameof<ProductImport>.Property(m => m.LPC)},
         {Upn, Nameof<ProductImport>.Property(m => m.UPN)},
         {Manufacturer, Nameof<ProductImport>.Property(m => m.Manufacturer)},
         {Sterile, Nameof<ProductImport>.Property(m => m.Sterile)},
         {RebateCode, Nameof<ProductImport>.Property(m => m.RebateCode)},
         {ProductId, Nameof<ProductImport>.Property(m => m.ProductId)},
         {InternalProductId, Nameof<ProductImport>.Property(m => m.ProductId)},
         //{Category, Nameof<ProductImport>.Property(m => m.Categories)}, //TODO: categories
         {AutoReorderSetting, Nameof<ProductImport>.Property(m => m.ReorderSetting)},
         {ProductSettings, Nameof<ProductImport>.Property(m => m.ProductSettings)},
         {UseCategorySettings, Nameof<ProductImport>.Property(m => m.UseCategorySettings)},
      };


      public const string Consumed = "Consumed";
      public const string ProductId = "ProductId";
      public const string InternalProductId = "HTP";
      public const string Spc = "SPC";
      public const string Description = "Name";
      public const string Notes = "Description";
      public const string Supplier = "Supplier";
      public const string Category = "Category";
      public const string MinOrder = "Min order";
      public const string OrderMultiple = "Order in multiples";
      public const string ReorderThreshold = "Reorder threshold";
      public const string TargetStockLevel = "Target stock level";
      public const string PublicBuyPrice = "Public unit price";
      public const string PrivateBuyPrice = "Private unit price";
      public const string Consignment = "Consignment";
      public const string Glc = "GLC";
      public const string Lpc = "LPC";
      public const string Upn = "UPN";
      public const string Manufacturer = "Manufacturer";
      public const string Sterile = "Sterile";
      public const string RebateCode = "Rebate code";
      public const string AutoReorderSetting = "Automatic reorder";
      public const string ProductSettings = "Product settings";
      public const string UseCategorySettings = "Use category settings";
   }
}