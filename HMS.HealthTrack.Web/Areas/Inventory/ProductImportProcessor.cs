using Excel;
using Hangfire;
using HMS.HealthTrack.Web.Areas.Inventory.Utils;
using HMS.HealthTrack.Web.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Inventory.Common.Constants;
using HMS.HealthTrack.Web.Data.Helpers;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork;

namespace HMS.HealthTrack.Web.Areas.Inventory
{
   [Queue(BackgroundQueues.Web)]
   public class ProductImportProcessor
   {
      private readonly IProductImportUnitOfWork _unitOfWork;
      private readonly ICustomLogger _logger;
      private readonly IPropertyProvider _propertyProvider;
      private const string ImportUser = "Import";
      private readonly IEnumerable<StockSetting> _stockSettings;

      public ProductImportProcessor(IProductImportUnitOfWork unitOfWork, ICustomLogger logger, IPropertyProvider propertyProvider)
      {
         _unitOfWork = unitOfWork;
         _logger = logger;
         _propertyProvider = propertyProvider;
         _stockSettings = unitOfWork.StockSettingRepository.FindAll();
      }

      public void ProcessAllUnprocessed(string username)
      {
         var unprocessed = _unitOfWork.ProductImportDataRepository.GetUnprocessedSets();
         var success = false;
         foreach (var productImport in unprocessed)
         {
            var data = productImport.ProductsData;
            if (data == null) continue;
            using (var ms = new MemoryStream(data))
            {
               success = ProcessSpreadsheet(ms, productImport.ProductImportDataId);
            }
            if (success)
            {
               productImport.ProductsData = null;
               productImport.Status = ProductImportStatus.Pending;
            }
            else
            {
               productImport.Status = ProductImportStatus.Error;
            }
         }
         _unitOfWork.Commit();
      }

      public void Process(int id, string name)
      {
         var productImport = _unitOfWork.ProductImportDataRepository.Find(id);
         if (productImport == null)
         {
            _logger.Warning("Unable to process product import data {ProductImportDataId} - no record found", id);
            return;
         }

         if (productImport.Status != ProductImportStatus.Processing)
         {
            _logger.Warning(
               "Unable to process product import data {ProductImportDataId} with status of {ProductImportStatus}",
               productImport.ProductImportDataId, productImport.Status);
            return;
         }

         var data = productImport.ProductsData;
         if (data == null)
         {
            _logger.Warning(
               "Unable to process product import data {ProductImportDataId} - no data present",
               productImport.ProductImportDataId);
            return;
         }
         var success = false;
         _logger.Information("Processing spreadsheet for product import {ProductImportDataId}", productImport.ProductImportDataId);
         using (var ms = new MemoryStream(data))
         {
            success = ProcessSpreadsheet(ms, productImport.ProductImportDataId);
         }
         if (success)
         {
            productImport.ProductsData = null;
            productImport.Status = productImport.ProductImports.Any(pi => pi.Invalid) ? ProductImportStatus.Invalid : ProductImportStatus.Pending;
         }
         else
         {
            _logger.Warning("Unable to process data of {ProductImportDataId}", productImport.ProductImportDataId);
            productImport.Status = ProductImportStatus.Error;
         }
         try
         {
            _unitOfWork.Commit();
         }
         catch (Exception exception)
         {
            _logger.Error(exception, "Unable to process data of product import {ProductImportDataId}",
               productImport.ProductImportDataId);
            _unitOfWork.Detach();
            var failedProductImport = _unitOfWork.ProductImportDataRepository.Find(id);
            failedProductImport.Status = ProductImportStatus.Error;
            failedProductImport.LastModifiedBy = name;
            failedProductImport.LastModifiedOn = DateTime.Now;
            failedProductImport.Message = "An error occurred while processing the product import. Please check that the file is in the correct format.";
            _unitOfWork.Commit();
         }
      }

      public bool ProcessSpreadsheet(Stream spreadsheet, int productImportDataId)
      {
         _logger.Information("Processing product data spreadsheet for {ProductImportDataId}", productImportDataId);
         var reader = ExcelReaderFactory.CreateOpenXmlReader(spreadsheet);
         reader.IsFirstRowAsColumnNames = true;

         var result = reader.AsDataSet();
         var table = result.Tables[0];
         var productImportData = _unitOfWork.ProductImportDataRepository.Find(productImportDataId);

         if (table.Rows[0].ItemArray.Length == 1) // test for unmodified kendo-exported spreadsheet
         {
            _logger.Warning("Cannot process spreadsheet due to known issue with spreadsheets exported from kendo. Open, save and try again.");
            productImportData.Message = "Unable to process spreadsheet. Please open in Excel, save and try again.";
            return false;
         }

         var productImportHelper = new ProductImportHelper(_unitOfWork, _propertyProvider);
         var productLedgerType = _propertyProvider.LedgerTypes.Single(lt => lt.Name == InventoryConstants.ProductLedgerType).LedgerTypeId;
         var segmentNames = _unitOfWork.GeneralLedgerTierRepository.GetSegmentNamesForExportImport(productLedgerType).ToList();
         var errorStringBuilder = new StringBuilder();
         var productImports = new List<ProductImport>();

         foreach (DataRow row in table.Rows)
         {
            var productImport = new ProductImport();
            productImports.Add(productImport);
            productImport.ProductImportDataId = productImportDataId;

            // product id that corresponds to Product ProductId
            if (table.Columns.Contains(ProductImportColumnNames.InternalProductId))
               productImport.InternalProductId = row[ProductImportColumnNames.InternalProductId].ToNullOrString();

            SetProductImportStringValues(table.Columns, row, productImportHelper, productImport, errorStringBuilder);

            var productImportGeneralLedgerCodes = new List<ProductImportGeneralLedgerCode>();
            // extract general ledger code segements
            foreach (var segment in segmentNames)
            {
               if (table.Columns.Contains(segment.Value) && !string.IsNullOrWhiteSpace(row[segment.Value].ToNullOrString()))
                  productImportGeneralLedgerCodes.Add(new ProductImportGeneralLedgerCode
                  {
                     Code = row[segment.Value].ToString(),
                     TierId = segment.Key
                  });
            }

            var validGlcCodes = productImportHelper.ValidGeneralLedgerCodes(productImportGeneralLedgerCodes);
            if (validGlcCodes)
            {
               // lookup the corresponding ledger id
               productImport.LedgerId = GeneralLedgerHelper.GetGeneralLedgerId(_unitOfWork.GeneralLedgerRepository,
                  _unitOfWork.GeneralLedgerTierRepository, _propertyProvider, productImportGeneralLedgerCodes,
                  productLedgerType);
            }

            // requires user intervention
            if (!validGlcCodes || productImport.LedgerId == null)
            {
               foreach (var entity in productImportGeneralLedgerCodes)
               {
                  productImport.ProductImportGeneralLedgerCodes.Add(entity);
               }
            }

            productImport.Invalid = ProductImport.IsInvalid(productImport, productImportHelper, _logger);

         }

         reader.Close();

         var errorString = errorStringBuilder.ToString();
         if (!string.IsNullOrWhiteSpace(errorString))
         {
            productImportData.Status = ProductImportStatus.Error;
            var message = "Unable to import the file as the following values would be truncated. Please correct the spreadsheet and reimport.";
            productImportData.Message = string.Concat(message, errorString);
            return false; // abort adding product imports, user to receive error message instead
         }

         foreach (var import in productImports)
         {
            _unitOfWork.ProductImportRepository.AddProductImport(import);
         }
         return true;
      }

      private void SetProductImportStringValues(DataColumnCollection columns, DataRow row, ProductImportHelper productImportHelper, ProductImport productImport, StringBuilder errorString)
      {
         var spreadsheetColumnNames = typeof(ProductImportColumnNames).GetFields().ToDictionary(f => f.Name, f => f.GetValue(f).ToString()); // spreadsheet column headings are the keys to the product import fields
         var productImportProperties = ProductImportColumnNames.ProductImportColumnsNames;
         var productImportType = typeof(ProductImport);

         foreach (var column in spreadsheetColumnNames)
         {
            if (!columns.Contains(column.Value)) continue;
            var value = row[column.Value].ToNullOrString(); // get the column's value from this row
            SetProductImportValueFromColumn(productImportHelper, productImport, errorString, column, productImportProperties, productImportType, value, _logger);
         }
      }

      public static void SetProductImportValueFromColumn(ProductImportHelper productImportHelper,
         ProductImport productImport, StringBuilder errorString, KeyValuePair<string, string> column,
         OrderedDictionary productImportProperties, Type productImportType, string value, ICustomLogger logger)
      {
         if (!string.IsNullOrWhiteSpace(value) && productImportProperties.Contains(column.Value))
         // check that the column is in the list of product import properties
         {
            var productImportPropertyName = productImportProperties[column.Value].ToString();
            var currentProperty = productImportType.GetProperty(productImportPropertyName);
            // the product import property that the column in the spreadsheet represents
            if (currentProperty.PropertyType != typeof(String)) return;
            var maxLength = productImportHelper.GetMaxStringLength(productImportPropertyName);
            // get max length of string property, null if varchar(max)
            if (maxLength.HasValue && value.Length > maxLength)
            {
               logger.Warning(
                  "Unable to set value of {PropertyName} as the value {Value} is greater than the max length {MaxLength}",
                  currentProperty.Name, value, maxLength);
               errorString.AppendLine(
                  string.Format("The value '{0}' was truncated as it is over {1} characters.", value,
                     maxLength));
            }
            else
            {
               currentProperty.SetValue(productImport, value); // set value of property
            }
         }
      }

      //Import
      public bool ImportProducts(int importId, string username)
      {
         _logger.Information("Importing products from {ProductImportDataId}", importId);
         var productData = _unitOfWork.ProductImportDataRepository.Find(importId);
         if (productData == null || productData.Status != ProductImportStatus.Processing)
         {
            _logger.Warning(
               "Could not import products from {ProductImportDataId}. Product import does not exist or is not in a state to be imported",
               importId);
            return false;
         }

         var productsToImport = _unitOfWork.ProductImportRepository.GetProductsForImport(importId);
         var productImportHelper = new ProductImportHelper(_unitOfWork, _propertyProvider);
         foreach (var product in productsToImport.Where(pi => !pi.Processed).ToList())
         {
            if (ImportProduct(product, username, productImportHelper))
            {
               continue;
            }
            _logger.Error("Product import for {ProductImportDataId} failed", importId);
            product.Processed = false;
            product.Invalid = true;
            productData.Status = ProductImportStatus.Error;
         }
         if (productData.Status != ProductImportStatus.Error)
         {
            productData.Status = ProductImportStatus.Complete;
            _logger.Information("Import of {ProductImportDataId} complete", importId);
         }
         else
         {
            _logger.Information("Import of {ProductImportDataId} completed with errors", importId);
         }
         productData.ImportedOn = DateTime.Now;
         _unitOfWork.Commit();

         return true;
      }

      private Product CreateNewProduct(ProductImport productImport, string username)
      {
         var product = _unitOfWork.ProductRepository.Create(ImportUser + username);

         var priceTypes = _unitOfWork.ProductPriceRepository.FindAllPriceTypes();
         product.Prices = priceTypes.Select(type => new ProductPrice { PriceType = type, }).ToList();

         _unitOfWork.ProductRepository.Add(product);
         _unitOfWork.ProductRepository.Commit();
         return product;
      }

      private bool ImportProduct(ProductImport productImport, string username, IProductImportHelper productImportHelper)
      {
         if (ProductImport.IsInvalid(productImport, productImportHelper, _logger))
         {
            _logger.Error("Attempting to import an invalid product {ProductImportId}", productImport.ProductImportId);
            productImport.Message = "Cannot import an invalid product.";
            return false;
         }

         if (productImport.Processed)
            return true;

         Product product;
         if (!string.IsNullOrWhiteSpace(productImport.InternalProductId))
         {
            int productId;
            if (!int.TryParse(productImport.InternalProductId, out productId))
            {
               UserParseError(productImport, Nameof<ProductImport>.Property(xx => xx.ProductId));
               return ParseError(productImport, Nameof<ProductImport>.Property(xx => xx.ProductId));
            }


            product = _unitOfWork.ProductRepository.Find(productId) ?? CreateNewProduct(productImport, username);
         }
         else
         {
            product = CreateNewProduct(productImport, username);
         }

         productImport.ProductId = product.ProductId;
         productImport.InternalProductId = product.ProductId.ToString();
         product.LastModifiedBy = ImportUser + username;

         product.SPC = productImport.SPC;
         product.LPC = productImport.LPC;
         product.ScanCodes.Clear();
         product.ScanCodes.Add(new ScanCode {Product = product,Value = productImport.UPN});
         product.Description = productImport.Description;
         product.Notes = productImport.Notes;
         product.Manufacturer = productImport.Manufacturer;

         if (productImport.LedgerId.HasValue)
         {
            product.LedgerId = productImport.LedgerId.Value;
         }
         else if (!string.IsNullOrWhiteSpace(productImport.GLC))
         {
            product.LedgerId = productImportHelper.GetLedgerId(productImport.GLC.Trim());
         }

         if (product.LedgerId != null)
         {
            // populate the glc field with the extended general ledger for that product
            product.GLC = GeneralLedgerHelper.GetGeneralLedgerCode(_unitOfWork.GeneralLedgerRepository, _unitOfWork.GeneralLedgerTierRepository, _propertyProvider, product.LedgerId.Value, productImportHelper.ProductLedgerType);
         }

         if (productImport.SupplierId.HasValue)
         {
            product.PrimarySupplier = productImport.SupplierId.Value;
         }
         else if (!string.IsNullOrWhiteSpace(productImport.Supplier))
         {
            product.PrimarySupplier = productImportHelper.GetSupplierId(productImport.Supplier.Trim());
         }

         //TODO: Categories

         int minOrder;
         if (productImport.MinimumOrder == null)
         {
            minOrder = 1;
         }
         else
         {
            if (!int.TryParse(productImport.MinimumOrder, out minOrder))
            {
               UserParseError(productImport, Nameof<ProductImport>.Property(xx => xx.MinimumOrder));
               return ParseError(productImport, Nameof<ProductImport>.Property(xx => xx.MinimumOrder));
            }

         }
         product.MinimumOrder = minOrder;

         int orderMultiple;
         if (productImport.OrderMultiple == null)
         {
            orderMultiple = minOrder;
         }
         else
         {
            if (!int.TryParse(productImport.OrderMultiple, out orderMultiple))
            {
               UserParseError(productImport, Nameof<ProductImport>.Property(xx => xx.OrderMultiple));
               return ParseError(productImport, Nameof<ProductImport>.Property(xx => xx.OrderMultiple));
            }

         }

         product.OrderMultiple = orderMultiple;

         if (productImport.ReorderThreshold == null)
         {
            product.ReorderThreshold = null;
         }
         else
         {
            int threshold;
            if (!int.TryParse(productImport.ReorderThreshold, out threshold))
            {
               UserParseError(productImport, Nameof<ProductImport>.Property(xx => xx.ReorderThreshold));
               return ParseError(productImport, Nameof<ProductImport>.Property(xx => xx.ReorderThreshold));
            }

            product.ReorderThreshold = threshold;
         }

         if (productImport.TargetStockLevel == null)
         {
            product.TargetStockLevel = null;
         }
         else
         {
            int target;
            if (!int.TryParse(productImport.TargetStockLevel, out target))
            {
               UserParseError(productImport, Nameof<ProductImport>.Property(xx => xx.TargetStockLevel));
               return ParseError(productImport, Nameof<ProductImport>.Property(xx => xx.TargetStockLevel));
            }

            product.TargetStockLevel = target;
         }

         var publicPrice = product.Prices.Single(p => p.PriceTypeId == _propertyProvider.PublicPriceTypeId);
         if (productImport.PublicUnitPrice == null)
         {
            publicPrice.BuyPrice = null;
         }
         else
         {
            decimal publicBuyPrice;
            if (!decimal.TryParse(productImport.PublicUnitPrice, out publicBuyPrice))
            {
               UserParseError(productImport, Nameof<ProductImport>.Property(xx => xx.PublicUnitPrice));
               return ParseError(productImport, Nameof<ProductImport>.Property(xx => xx.PublicUnitPrice));
            }

            publicPrice.BuyPrice = publicBuyPrice;
         }

         var privatePrice = product.Prices.Single(p => p.PriceTypeId == _propertyProvider.PrivatePriceTypeId);
         if (productImport.PrivateUnitPrice == null)
         {
            privatePrice.BuyPrice = null;
         }
         else
         {
            decimal privateBuyPrice;
            if (!decimal.TryParse(productImport.PrivateUnitPrice, out privateBuyPrice))
            {
               UserParseError(productImport, Nameof<ProductImport>.Property(xx => xx.PrivateUnitPrice));
               return ParseError(productImport, Nameof<ProductImport>.Property(xx => xx.PrivateUnitPrice));
            }

            privatePrice.BuyPrice = privateBuyPrice;
         }

         bool consignment;
         if (productImport.Consignment == null)
         {
            consignment = false;
         }
         else
         {
            if (!GetBoolean(productImport.Consignment, out consignment))
            {
               UserParseError(productImport, Nameof<ProductImport>.Property(xx => xx.Consignment));
               return ParseError(productImport, Nameof<ProductImport>.Property(xx => xx.Consignment));
            }

         }

         product.IsConsignment = consignment;

         bool sterile;
         if (productImport.Sterile == null)
         {
            sterile = false;
         }
         else
         {
            if (!GetBoolean(productImport.Sterile, out sterile))
            {
               UserParseError(productImport, Nameof<ProductImport>.Property(xx => xx.Sterile));
               return ParseError(productImport, Nameof<ProductImport>.Property(xx => xx.Sterile));
            }

         }
         product.UseSterile = sterile;

         // Reorder settings
         if (!string.IsNullOrWhiteSpace(productImport.ReorderSetting))
         {
            ReorderSettings reorderSetting;
            if (!Enum.TryParse(productImport.ReorderSetting, out reorderSetting))
            {
               UserParseError(productImport, Nameof<ProductImport>.Property(pi => pi.ReorderSetting));
               return ParseError(productImport, Nameof<ProductImport>.Property(pi => pi.ReorderSetting));
            }

            product.AutoReorderSetting = reorderSetting;
            SetStockHandlingSettings(product, reorderSetting); // must come after the setting of target stock level and reorder threshold

         }
         else
         {
            product.AutoReorderSetting = ReorderSettings.SpecifyLevels;
         }

         //use category settings
         bool useCategorySettings;
         if (productImport.UseCategorySettings == null)
         {
            useCategorySettings = false;
         }
         else
         {
            if (!GetBoolean(productImport.UseCategorySettings, out useCategorySettings))
            {
               UserParseError(productImport, Nameof<ProductImport>.Property(xx => xx.UseCategorySettings));
               return ParseError(productImport, Nameof<ProductImport>.Property(xx => xx.UseCategorySettings));
            }

         }
         product.UseCategorySettings = useCategorySettings;

         // only process product settings if not using category settings
         if (!useCategorySettings)
         {
            product.ProductSettings.Clear(); // clear settings and add new ones if present
            // Product settings
            if (!string.IsNullOrWhiteSpace(productImport.ProductSettings))
            {
               var productSettings = productImportHelper.GetProductSettingIds(productImport.ProductSettings);
               if (productSettings == null)
               {
                  UserParseError(productImport, Nameof<ProductImport>.Property(pi => pi.ProductSettings));
                  return ParseError(productImport, Nameof<ProductImport>.Property(pi => pi.ProductSettings));
               }

               foreach (var productSetting in productSettings)
               {
                  product.ProductSettings.Add(_stockSettings.Single(ss => ss.SettingId == productSetting));
               }
            }
         }

         product.RebateCode = productImport.RebateCode;

         product.InError = !productImportHelper.ValidProductToImport(product);

         productImport.Message = null;
         productImport.Processed = true;
         productImport.ProcessedOn = DateTime.Now;
         return true;
      }

      private void SetStockHandlingSettings(Product product, ReorderSettings reorderSetting)
      {
         switch (reorderSetting)
         {
            case ReorderSettings.DoNotReorder:
               product.TargetStockLevel = 0;
               product.ReorderThreshold = 0;
               break;
            case ReorderSettings.OneForOneReplace:
               product.ReorderThreshold = product.TargetStockLevel;
               break;
         }
      }

      private static bool GetBoolean(string stringToProcess, out bool result)
      {
         return ProductImport.ProductImportValidationRules.BooleanTextValues.TryGetValue(stringToProcess.ToLower(), out result);
      }

      private bool ParseError(ProductImport productImport, string fieldName)
      {
         _logger.Error("Could not parse {FieldName} for {ProductId} when importing {ProductImportId}", fieldName,
            productImport.ProductId,
            productImport.ProductImportId);
         return false;
      }

      private void UserParseError(ProductImport productImport, string fieldName)
      {
         if (productImport.Message == null)
         {
            productImport.Message = string.Format("Could not translate value for {0}", fieldName);
         }
         productImport.Message = string.Format("{0}{1}Could not translate value for {2}", productImport.Message, Environment.NewLine,
            fieldName);
      }
   }
}