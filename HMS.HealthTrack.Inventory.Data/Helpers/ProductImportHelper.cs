using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;
using HMS.HealthTrack.Inventory.Common.Constants;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork;

namespace HMS.HealthTrack.Web.Data.Helpers
{
   public interface IProductImportHelper
   {
      int ProductLedgerType { get; }
      bool ValidGlc(string glc);
      int? GetLedgerId(string glc);
      bool ValidSupplier(string supplierName);
      int? GetSupplierId(string supplierName);
      bool InvalidReorderSetting(string settingName);
      bool InvalidProductSettings(string productSettings);
      IEnumerable<string> GetProductSettingIds(string productSettings);

      /// <summary>
      ///    Validate that all codes have a valid product tier and that the code specified corresponds to a code of that tier
      /// </summary>
      /// <param name="codes"></param>
      /// <returns></returns>
      bool ValidGeneralLedgerCodes(IEnumerable<ProductImportGeneralLedgerCode> codes);

      int? GetMaxStringLength(string fieldName);
      bool ValidProductToImport(Product product);
   }

   public class ProductImportHelper : IProductImportHelper
   {
      private static Dictionary<string, Facet> _productImportFieldFacets;
      private readonly ILookup<string, int> _productLedgers;
      private readonly IPropertyProvider _propertyProvider;
      private readonly Dictionary<string, string> _stockSettings;
      private readonly ILookup<string, int> _suppliers;
      private readonly IProductImportUnitOfWork _unitOfWork;
      private readonly Dictionary<int, List<string>> _generalLedgerTierCodes;
      private readonly HashSet<string> _spcs;
      private readonly List<string> _upns;

      public ProductImportHelper(IProductImportUnitOfWork unitOfWork, IPropertyProvider propertyProvider)
      {
         _unitOfWork = unitOfWork;
         _propertyProvider = propertyProvider;
         var ledgerType = _propertyProvider.LedgerTypes.SingleOrDefault(lt => lt.Name == InventoryConstants.ProductLedgerType);
         if (ledgerType == null) throw new Exception("System has not been configured to have the correct General Ledger Types");

         ProductLedgerType = ledgerType.LedgerTypeId;
         _productLedgers = _unitOfWork.GeneralLedgerRepository.FindAll().Where(g => g.GeneralLedgerTier.LedgerType == ProductLedgerType).ToLookup(g => g.Code, g => g.LedgerId);
         _suppliers = _unitOfWork.SupplierRepository.FindAll().ToLookup(s => s.Company.companyName.ToLower(), s => s.company_ID);
         _stockSettings = ProductImportStaticValidation.StockSettings;
         _generalLedgerTierCodes = _unitOfWork.GeneralLedgerTierRepository.FindAll()
            .Where(t => t.LedgerType == ProductLedgerType)
            .ToDictionary(t => t.TierId, t => t.GeneralLedgers.Select(gl => gl.Code).ToList());
         _productImportFieldFacets = GetEntityStringFacets(typeof(ProductImport));

         _spcs = new HashSet<string>(_unitOfWork.ProductRepository.FindAll().Select(p => p.SPC).Distinct().ToList());
         _upns = new List<string>(_unitOfWork.ScanCodeRepository.FindDistinct());
      }

      public int ProductLedgerType { get; }

      // Valid: Must exist and be the only general ledger with that code
      public bool ValidGlc(string glc)
      {
         var exists = _productLedgers.Contains(glc);
         if (!exists) return false;

         return _productLedgers[glc].Count() == 1;
      }

      public int? GetLedgerId(string glc)
      {
         return ValidGlc(glc) ? (int?) _productLedgers[glc].Single() : null;
      }

      public bool ValidSupplier(string supplierName)
      {
         var exists = _suppliers.Contains(supplierName.ToLower());
         if (!exists) return false;

         return _suppliers[supplierName.ToLower()].Count() == 1;
      }

      public int? GetSupplierId(string supplierName)
      {
         return ValidSupplier(supplierName.ToLower()) ? (int?) _suppliers[supplierName.ToLower()].Single() : null;
      }

      public bool InvalidReorderSetting(string settingName)
      {
         return ProductImportStaticValidation.InvalidReordersetting(settingName);
      }

      public bool InvalidProductSettings(string productSettings)
      {
         return ProductImportStaticValidation.InvalidProductSettings(productSettings);
      }

      public IEnumerable<string> GetProductSettingIds(string productSettings)
      {
         var settingStrings = productSettings.Split(',').Select(xx => xx.Trim()).Distinct();
         var settingIds = new List<string>();
         foreach (var setting in settingStrings)
         {
            string id;
            if (_stockSettings.TryGetValue(setting, out id))
               settingIds.Add(id);
            else
               return null;
         }

         return settingIds;
      }

      /// <summary>
      ///    Validate that all codes have a valid product tier and that the code specified corresponds to a code of that tier
      /// </summary>
      /// <param name="codes"></param>
      /// <returns></returns>
      public bool ValidGeneralLedgerCodes(IEnumerable<ProductImportGeneralLedgerCode> codes)
      {
         return codes.All(code => _generalLedgerTierCodes.ContainsKey(code.TierId) && _generalLedgerTierCodes[code.TierId].Contains(code.Code));
      }

      public int? GetMaxStringLength(string fieldName)
      {
         Facet facet = null;
         _productImportFieldFacets.TryGetValue(fieldName, out facet);
         int value;
         if (facet != null && int.TryParse(facet.Value.ToString(), out value)) return value;
         return null;
      }

      public bool ValidProductToImport(Product product)
      {
         var hasCode = _unitOfWork.ProductRepository.ProductCodesPresent(product);

         var uniqueSpc = true;
         if (!string.IsNullOrWhiteSpace(product.SPC))
         {
            uniqueSpc = !_spcs.Contains(product.SPC);
            if (uniqueSpc)
               _spcs.Add(product.SPC);
            else
               foreach (var existingProduct in _unitOfWork.ProductRepository.FindAll().Where(p => p.SPC == product.SPC))
                  existingProduct.InError = true;
         }

         var uniqueUpn = true;
         if (product.ScanCodes.Any())
         {
            uniqueUpn = !product.ScanCodes.Any(sc => _upns.Contains(sc.Value));
            if (uniqueUpn)
               _upns.AddRange(product.ScanCodes.Select(sc => sc.Value));
            else
               foreach (var existingScanCode in _unitOfWork.ScanCodeRepository.FindAll().Where(sc => product.ScanCodes.Any(psc => psc.Value == sc.Value)))
                  existingScanCode.Product.InError = true;
         }

         var validStockHandling = ProductRepository.ValidStockHandling(product);

         return hasCode && uniqueSpc && uniqueUpn && validStockHandling;
      }

      /// <summary>
      ///    Interrogates the EF5 context to find the max length of ProductImport entity's string properties
      ///    This will probably break when EF is updated.
      ///    http://stackoverflow.com/questions/748939/field-max-length-in-entity-framework/772556#772556
      /// </summary>
      /// <param name="entityType"></param>
      /// <param name="property"></param>
      /// <param name="context"></param>
      /// <returns></returns>
      private Dictionary<string, Facet> GetEntityStringFacets(Type entityType)
      {
         Dictionary<string, Facet> facets;
         using (var context = new InventoryContext())
         {
            facets = (from meta in
                  ((IObjectContextAdapter) context).ObjectContext.MetadataWorkspace.GetItems(DataSpace.CSpace)
                  .Where(mw => mw.BuiltInTypeKind == BuiltInTypeKind.EntityType) // find entities from metadata
               from p in (meta as EntityType).Properties // get properties of entity
                  .Where(p => p.DeclaringType.Name == entityType.Name // matching type
                              && p.TypeUsage.EdmType.Name == "String") // string fields
               select p).ToDictionary(p => p.Name, p => p.TypeUsage.Facets["MaxLength"]);
         }

         return facets;
      }
   }

   public static class ProductImportStaticValidation
   {
      public static readonly Dictionary<string, string> StockSettings = typeof(InventoryConstants.StockSettings).GetFields().ToDictionary(f => f.Name, f => f.GetValue(f).ToString());

      public static string GetProductSettingString(IEnumerable<StockSetting> stockSettings)
      {
         if (stockSettings == null || !stockSettings.Any()) return string.Empty;
         var dictSettings = StockSettings;
         return string.Join(",", stockSettings.Select(ss => dictSettings.FirstOrDefault(d => d.Value == ss.SettingId).Key).Distinct());
      }

      public static bool InvalidProductSettings(string productSettings)
      {
         var settings = productSettings.Split(',').Select(ps => ps.Trim()).Distinct();
         return !settings.All(StockSettings.Keys.Contains);
      }

      public static bool InvalidReordersetting(string settingName)
      {
         return !EnumHelper.EnumToHashSet<ReorderSettings>().Contains(settingName);
      }
   }
}