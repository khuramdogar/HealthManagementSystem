using System;
using System.Collections.Generic;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Helpers;

namespace HMS.HealthTrack.Web.Data.Model.Inventory
{
   public partial class ProductImport
   {
      public static bool IsInvalid(ProductImport productImport, IProductImportHelper productImportHelper, ICustomLogger logger)
      {
         if (ProductImportValidationRules.InvalidRequired(productImport))
         {
            logger.Information("Import {ProductImportId} invalid - violates required rule", productImport.ProductImportId);
            return true;
         }

         if (ProductImportValidationRules.InvalidNumeric(productImport))
         {
            logger.Information("Import {ProductImportId} invalid - violates numeric rule", productImport.ProductImportId);
            return true;
         }

         if (ProductImportValidationRules.InvalidDecimal(productImport))
         {
            logger.Information("Import {ProductImportId} invalid - violates decimal rule", productImport.ProductImportId);
            return true;
         }

         if (ProductImportValidationRules.InvalidGLC(productImport, productImportHelper))
         {
            logger.Information("Import {ProductImportId} invalid GLC", productImport.ProductImportId);
            return true;
         }

         if (ProductImportValidationRules.InvalidSupplier(productImport, productImportHelper))
         {
            logger.Information("Import {ProductImportId} invalid Supplier", productImport.ProductImportId);
            return true;
         }

         if (!productImport.LedgerId.HasValue && ProductImportValidationRules.InvalidGeneralLedgerCodes(productImport, productImportHelper))
         {
            logger.Information("Import {ProductImportId} invalid glc code", productImport.ProductImportId);
            return true;
         }

         if (ProductImportValidationRules.InvalidProductSettings(productImport, productImportHelper))
         {
            logger.Information("Import {ProductImportId} invalid product settings", productImport.ProductImportId);
            return true;
         }

         if (ProductImportValidationRules.InvalidReorderSetting(productImport, productImportHelper))
         {
            logger.Information("Import {ProductImportId} invalid auto reorder settings", productImport.ProductImportId);
            return true;
         }

         return false;
      }

      public class ProductImportValidationRules
      {
         public static Dictionary<string, bool> BooleanTextValues = new Dictionary<string, bool>
         {
            {"true", true},
            {"yes", true},
            {"y", true},
            {"false", false},
            {"no", false},
            {"n", false}
         };

         internal static readonly Func<ProductImport, bool> InvalidDecimal = pi =>
         {
            decimal value;

            if (pi.PublicUnitPrice != null && !decimal.TryParse(pi.PublicUnitPrice, out value)) return true;

            if (pi.PrivateUnitPrice != null && !decimal.TryParse(pi.PrivateUnitPrice, out value)) return true;

            return false;
         };

         internal static readonly Func<ProductImport, bool> InvalidNumeric = pi =>
         {
            int value;
            if (pi.MinimumOrder != null && !int.TryParse(pi.MinimumOrder, out value)) return true;

            if (pi.OrderMultiple != null && !int.TryParse(pi.OrderMultiple, out value)) return true;

            if (pi.ReorderThreshold != null && !int.TryParse(pi.ReorderThreshold, out value)) return true;

            if (pi.TargetStockLevel != null && !int.TryParse(pi.TargetStockLevel, out value)) return true;

            return false;
         };

         internal static readonly Func<ProductImport, bool> InvalidBool = pi =>
         {
            if (pi.Consignment != null && BooleanTextValues.ContainsKey(pi.Consignment.ToLower().Trim())) return true;

            if (pi.Sterile != null && BooleanTextValues.ContainsKey(pi.Sterile.ToLower().Trim())) return true;

            if (pi.UseCategorySettings != null && BooleanTextValues.ContainsKey(pi.UseCategorySettings.ToLower().Trim())) return true;

            return false;
         };

         internal static readonly Func<ProductImport, bool> InvalidRequired =
            pi => pi.SPC == null || pi.Description == null;

         internal static readonly Func<ProductImport, IProductImportHelper, bool> InvalidGLC =
            (productImport, importHelper) => !productImport.LedgerId.HasValue && !string.IsNullOrWhiteSpace(productImport.GLC) && !importHelper.ValidGlc(productImport.GLC);

         internal static readonly Func<ProductImport, IProductImportHelper, bool> InvalidSupplier =
            (productImport, importHelper) => !productImport.SupplierId.HasValue && !string.IsNullOrWhiteSpace(productImport.Supplier) && !importHelper.ValidSupplier(productImport.Supplier);

         internal static readonly Func<ProductImport, IProductImportHelper, bool> InvalidReorderSetting =
            (productImport, importHelper) => !string.IsNullOrWhiteSpace(productImport.ReorderSetting) && importHelper.InvalidReorderSetting(productImport.ReorderSetting);

         internal static readonly Func<ProductImport, IProductImportHelper, bool> InvalidProductSettings =
            (productImport, importHelper) =>
               !string.IsNullOrWhiteSpace(productImport.ProductSettings) &&
               importHelper.InvalidProductSettings(productImport.ProductSettings);

         internal static readonly Func<ProductImport, IProductImportHelper, bool> InvalidGeneralLedgerCodes =
            (productImport, importHelper) => !importHelper.ValidGeneralLedgerCodes(productImport.ProductImportGeneralLedgerCodes);
      }
   }
}