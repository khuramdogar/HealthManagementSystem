using AutoMapper;
using HMS.HealthTrack.Web.Areas.Inventory.Models.Products;
using HMS.HealthTrack.Web.Areas.Inventory.Models.ScanCode;
using HMS.HealthTrack.Web.Areas.Inventory.Utils;
using HMS.HealthTrack.Web.Infrastructure;
using HMS.HealthTrack.Web.Utils;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Inventory.Common.Constants;
using HMS.HealthTrack.Web.Areas.Inventory.Models.OrderChannels;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork;
using ScanCode = HMS.HealthTrack.Web.Data.Model.Inventory.ScanCode;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers
{
   [HealthTrackAuthorize(HealthTrackPermission = ("Inventory"))]
   public class ProductsController : Controller
   {
      private readonly IProductUnitOfWork _unitOfWork;
      private readonly IProductRepository _productRepository;
      private readonly ICustomLogger _logger;
      private readonly IPropertyProvider _propertyProvider;
      private readonly IPreferenceRepository _preferenceRepository;
      private readonly IGeneralLedgerUnitOfWork _generalLedgerUnitOfWork;
      private readonly IOrderChannelRepository _orderChannelRepository;

      private const string SuccessfulMerge = "SuccessfulMerge";
      private const string ExcelExportAllCharacter = "~";

      public ProductsController(IProductUnitOfWork unitOfWork, ICustomLogger logger, IPropertyProvider propertyProvider,
         IPreferenceRepository preferenceRepository, 
         IGeneralLedgerUnitOfWork generalLedgerUnitOfWork,
         IOrderChannelRepository orderChannelRepository)
      {
         _unitOfWork = unitOfWork;
         _logger = logger;
         _propertyProvider = propertyProvider;
         _preferenceRepository = preferenceRepository;
         _generalLedgerUnitOfWork = generalLedgerUnitOfWork;
         _orderChannelRepository = orderChannelRepository;
         _productRepository = unitOfWork.ProductRepository;
      }

      // GET: Inventory
      public ActionResult Index()
      {
         ViewBag.PresetFilterMessage = GetIndexPresetFilterMessage();
         // sets the variables for presetting a filter. Also need to update the javascript in the view.
         ViewBag.UserPreferredLocation = PreferenceHelper.GetPreferredStockLocation(_preferenceRepository,
            _propertyProvider, User.Identity);

         ViewBag.LedgerType =
            _propertyProvider.LedgerTypes.Single(lt => lt.Name == InventoryConstants.ProductLedgerType).LedgerTypeId;

         return View();
      }

      private string GetIndexPresetFilterMessage()
      {
         var filterSubject = string.Empty;
         if (TempData[InventoryConstants.Unclassified] != null)
         {
            filterSubject = "unclassified products";
            ViewBag.PresetFilter = TempData[InventoryConstants.Unclassified];
         }
         else if (TempData[InventoryConstants.PendingConsumedProducts] != null)
         {
            filterSubject = "consumed products that were created by the system and require more information";
            ViewBag.PresetFilter = TempData[InventoryConstants.PendingConsumedProducts];
         }
         else if (TempData[InventoryConstants.ProductsInError] != null)
         {
            filterSubject = "products that are in a state of error";
            ViewBag.PresetFilter = TempData[InventoryConstants.ProductsInError];
            ViewBag.IncludeDisabled = "IncludeDisabled";
         }

         return !string.IsNullOrWhiteSpace(filterSubject)
            ? string.Format("Displaying {0}. Click Adv. Search for more filtering options.", filterSubject)
            : filterSubject;
      }

      // GET: Inventory/Details/5
      public ActionResult Details(int? id)
      {
         try
         {
            if (id == null)
            {
               return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var product = _productRepository.FindIncludingDeleted(id.GetValueOrDefault());

            if (product == null)
            {
               return HttpNotFound();
            }

            var model = Mapper.Map<Product, DetailsProductsViewModel>(product);
            if (product.LedgerId.HasValue)
            {
               var productLedgerType =
                  _propertyProvider.LedgerTypes.Single(lt => lt.Name == InventoryConstants.ProductLedgerType)
                     .LedgerTypeId;
               model.GLC = GeneralLedgerHelper.GetGeneralLedgerCode(_generalLedgerUnitOfWork.GeneralLedgerRepository,
                  _generalLedgerUnitOfWork.GeneralLedgerTierRepository, _propertyProvider, product.LedgerId.Value,
                  productLedgerType);
            }

            model.SelectedCategories = GetSelectedCategoryNames(product);
            if (model.InError)
            {
               ViewBag.ErrorMessage = new HtmlString(GetErrorMessage(product));
            }

            if (TempData[SuccessfulMerge] != null)
            {
               ViewBag.SuccessMessage = "Successfully merged the product <strong>" + TempData[SuccessfulMerge] + "</strong> into this product.";
            }


            ViewData["channels"] = GetOrderChannels();

            return View(model);
         }
         catch (Exception exception)
         {
            _logger.Error(exception,
               string.Format("There was a problem retrieving the Product with ID '{0}'.",
                  id.HasValue ? id.Value.ToString() : string.Empty));
            return View("Error", new HandleErrorInfo(exception, "Products", "Details"));
         }
      }

      private IEnumerable<OrderChannelModel> GetOrderChannels()
      {
         var channels =  _orderChannelRepository.GetAvailableChannels();
         return channels.Select(Mapper.Map<OrderChannel, OrderChannelModel>);
      }

      public string GetErrorMessage(Product product)
      {
         var errors = _productRepository.GetProductErrors(product).ToList();
         var errorMessage = new StringBuilder();
         errorMessage.Append("This product is in a state of error due to: ");
         var duplicateSpcError = errors.SingleOrDefault(e => e.Reason == ProductErrorReason.DuplicateSpc);
         if (duplicateSpcError != null)
         {
            errorMessage.Append("<br>");
            errorMessage.Append(duplicateSpcError.Products.Aggregate("Same SPC as other product/s ",
               (current, p) =>
                  current +
                  string.Format("<a target=\'blank\' href=\'{0}\'>{1}</a>, ",
                     Url.Action("Details", "Products", new { id = p.Key }), p.Value)).Trim(' ').Trim(','));
         }

         var duplicateUpnError = errors.SingleOrDefault(e => e.Reason == ProductErrorReason.DuplicateUpn);
         if (duplicateUpnError != null)
         {
            errorMessage.Append("<br>");
            errorMessage.Append(duplicateUpnError.Products.Aggregate("Same UPN as other product/s ",
               (current, p) =>
                  current +
                  string.Format("<a target=\'blank\' href=\'{0}\'>{1}</a>, ",
                     Url.Action("Details", "Products", new { id = p.Key }), p.Value)));
         }

         var stockHandlingError = errors.SingleOrDefault(e => e.Reason == ProductErrorReason.StockHandling);
         if (stockHandlingError != null)
         {
            errorMessage.Append("<br>");
            if (product.ManageStock)
            {
               if (product.AutoReorderSetting == ReorderSettings.OneForOneReplace)
               {
                  errorMessage.Append("Invalid stock handling configuration. " +
                                      "A product cannot have combination of Manage Stock and One For One Replace. Please either uncheck Manage Stock or set Automatic Re-order to either Specify Levels or Do Not Auto-order.");
               }
               else
               {
                  errorMessage.Append("Invalid stock handling configuration. " +
                                      "The Reorder Threshold must be below or equal to the Target Stock Level when using the Automatic Re-Order setting of Specify Levels.");
               }
            }
            else
            {
               if (product.AutoReorderSetting == ReorderSettings.SpecifyLevels)
               {
                  errorMessage.Append("Invalid stock handling configuration. " +
                                      "A product cannot have combination of Manage Stock unchecked and Specify Levels. Please either check Manage Stock or set Automatic Re-order to either Do Not Auto-order or One For One Replace.");
               }
               else
               {
                  errorMessage.Append("Invalid stock handling configuration. ");
               }
            }
         }

         var missingSpcError = errors.SingleOrDefault(e => e.Reason == ProductErrorReason.MissingCode);
         if (missingSpcError != null)
         {
            errorMessage.Append("<br>");
            errorMessage.Append("Missing SPC or UPN. A product must have at least one identifying code.");
         }

         return errorMessage.ToString().Trim(' ').Trim(',');
      }

      // GET: Inventory/Create
      [System.Web.Mvc.HttpGet]
      public ActionResult Create()
      {
         try
         {
            var model = ProductFormHelper.ConstructProductModel(_unitOfWork.CompanyRepo,
               _unitOfWork.ProductPriceRepository, _unitOfWork.CategoryRepo, _unitOfWork.StockLocationRepository);
            // explicit defaults
            model.ManageStock = false;
            model.AutoReorderSetting = (int)ReorderSettings.DoNotReorder;

            ViewBag.LedgerType =
               _propertyProvider.LedgerTypes.Single(lt => lt.Name == InventoryConstants.ProductLedgerType).LedgerTypeId;

            if (TempData["ErrorMessage"] != null)
            {
               ViewBag.ErrorMessage = TempData["ErrorMessage"];
            }
            
            return View(model);
         }
         catch (Exception exception)
         {
            _logger.Error(exception,
               string.Format("There was a problem retrieving price types or companies before creating Product."));
            return View("Error", new HandleErrorInfo(exception, "Products", "Create"));
         }
      }

      // POST: Inventory/Create
      // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
      // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
      [System.Web.Mvc.HttpPost]
      public ActionResult Create(CreateProductsViewModel cpm)
      {
         try
         {
            if (!ModelState.IsValid)
            {
               return View(cpm);
            }

            var product = Mapper.Map<CreateProductsViewModel, Product>(cpm);
            product.CreatedBy = User.Identity.Name;
            product.LastModifiedBy = User.Identity.Name;
            if(!string.IsNullOrWhiteSpace(cpm.UPN))
               product.ScanCodes.Add(new ScanCode {Product = product,Value = cpm.UPN});

            _productRepository.Add(product, cpm.SelectedSettings, cpm.SelectedCategories);

            if (cpm.InitialStock.Any())
            {
               var stockForLocations = cpm.InitialStock.Where(s => s.Quantity.HasValue);
               foreach (var stock in stockForLocations)
               {
                  _unitOfWork.CreateInitialStock(product, stock.LocationId, stock.Quantity.Value,
                     User.Identity.Name);
               }
            }

            _unitOfWork.Commit();
            return RedirectToAction("Details", new { id = product.ProductId });
         }
         catch (Exception exception)
         {
            _logger.Error(exception,
               string.Format("There was a problem creating a Product with the description '{0}'.", cpm.Description));
            return View("Error", new HandleErrorInfo(exception, "Products", "Create"));
         }
      }

      [System.Web.Mvc.HttpGet]
      public ActionResult CreateFromExisting(int id)
      {
         try
         {
            var product = _productRepository.Find(id);
            if (product == null)
            {
               _logger.Warning("Unable to find product {ProductId} to prefill the create product model with", id);
               TempData["ErrorMessage"] = "Unable to find product. Please refresh and try again.";
               return RedirectToAction("Create");
            }

            var model = ProductFormHelper.ConstructProductModel(_unitOfWork.CompanyRepo,
               _unitOfWork.ProductPriceRepository, _unitOfWork.CategoryRepo, _unitOfWork.StockLocationRepository);
            ViewBag.LedgerType =
               _propertyProvider.LedgerTypes.Single(lt => lt.Name == InventoryConstants.ProductLedgerType).LedgerTypeId;

            Mapper.Map(product, model);

            model.LPC = string.Empty;
            model.SPC = string.Empty;
            model.ProductId = null;

            return View("Create", model);
         }
         catch (Exception exception)
         {
            _logger.Error(exception,
               string.Format("There was a problem retrieving price types or companies before creating Product."));
            return View("Error", new HandleErrorInfo(exception, "Products", "Create"));
         }
      }

      [System.Web.Mvc.HttpGet]
      public ActionResult CreateWindow()
      {
         var createModel = ProductFormHelper.ConstructProductModel(_unitOfWork.CompanyRepo,
            _unitOfWork.ProductPriceRepository, _unitOfWork.CategoryRepo, _unitOfWork.StockLocationRepository);
         var quickModel = new QuickCreateProductsViewModel
         {
            InitialStock = createModel.InitialStock
         };

         return PartialView("_QuickCreateWindow", quickModel);
      }
      

      [System.Web.Mvc.HttpPost]
      public ActionResult QuickCreate(QuickCreateProductsViewModel model)
      {
         if (!ModelState.IsValid)
         {
            return PartialView("_QuickCreate", model);
         }

         var product = _productRepository.Create(User.Identity.Name);
         product.Description = model.QuickCreateDescription;
         product.SPC = model.QuickCreateSpc;
         if(!string.IsNullOrWhiteSpace(model.QuickCreateUpn))
            product.ScanCodes.Add(new ScanCode {Product = product,Value = model.QuickCreateUpn});
         _productRepository.Add(product);

         if (model.InitialStock != null && model.InitialStock.Any())
         {
            var stockForLocations = model.InitialStock.Where(s => s.Quantity.HasValue);
            foreach (var stock in stockForLocations)
            {
               _unitOfWork.CreateInitialStock(product, stock.LocationId, stock.Quantity.Value, User.Identity.Name); // return the adjustments to add to the product
            }
         }

         _unitOfWork.Commit();

         return Json(new { product.ProductId, product.Description }); // return product id
      }

      // GET: Inventory/Edit/5
      [System.Web.Mvc.HttpGet]
      public ActionResult Edit(int? id, int? fillFromExisting)
      {
         try
         {
            if (id == null)
            {
               return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var product = _productRepository.Find(id.Value);
            if (product == null)
            {
               return HttpNotFound();
            }

            EditProductsViewModel model = null;

            if (fillFromExisting != null)
            {
               var otherProduct = _productRepository.Find(fillFromExisting.Value);
               if (otherProduct != null)
               {
                  model = Mapper.Map<Product, EditProductsViewModel>(otherProduct);
               }
               else
               {
                  _logger.Warning(
                     "Attempted to load details from existing product with id {OtherProductId} which cannot be found.",
                     fillFromExisting);
               }
            }
            else
            {
               model = Mapper.Map<Product, EditProductsViewModel>(product);
            }

            if (model == null)
               throw new Exception("No product model created by automapper");

            //Available channels
            ViewData["channels"] = GetOrderChannels();

            model.ProductId = product.ProductId; // ensure that model has the correct product id if using another product as a template
            model.SPC = product.SPC; // properties to keep from original product
            model.LPC = product.LPC;
            model.SelectedCategories = product.ProductCategories.Select(pc => pc.CategoryId);
            model.SelectedSettings = product.UseCategorySettings
               ? product.ProductCategories.SelectMany(pc => pc.Category.StockSettings)
                  .Select(ss => ss.SettingId)
                  .Distinct()
               : product.ProductSettings.Select(ss => ss.SettingId);

            if (product.LedgerId.HasValue)
            {
               var productLedgerType =
                  _propertyProvider.LedgerTypes.Single(lt => lt.Name == InventoryConstants.ProductLedgerType)
                     .LedgerTypeId;
               model.GLC = GeneralLedgerHelper.GetGeneralLedgerCode(_generalLedgerUnitOfWork.GeneralLedgerRepository,
                  _generalLedgerUnitOfWork.GeneralLedgerTierRepository, _propertyProvider, product.LedgerId.Value,
                  productLedgerType);
            }

            ViewBag.LedgerType =
               _propertyProvider.LedgerTypes.Single(lt => lt.Name == InventoryConstants.ProductLedgerType).LedgerTypeId;

            if (model.InError)
            {
               ViewBag.ErrorMessage = new HtmlString(GetErrorMessage(product));
            }

            return View(model);
         }
         catch (Exception exception)
         {
            _logger.Error(exception,
               string.Format("There was a problem retrieving the Product with ID '{0}'.",
                  id.HasValue ? id.Value.ToString() : string.Empty));
            return View("Error", new HandleErrorInfo(exception, "Products", "Edit"));
         }
      }

      private string GetSelectedCategoryNames(Product product)
      {
         return product.ProductCategories.Select(c => c.Category.CategoryName).Join(", ");
      }

      // POST: Inventory/Edit/5
      // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
      // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
      [System.Web.Mvc.HttpPost]
      public ActionResult Edit(EditProductsViewModel editModel)
      {
         //Available channels
         ViewData["channels"] = GetOrderChannels();

         try
         {
            if (editModel == null || editModel.ProductId == 0)
            {
               ModelState.AddModelError("", "Missing product details");
               return View(editModel);
            }

            if (!ModelState.IsValid)
            {
               return View(editModel);
            }

            var product = new Product();
            Mapper.Map(editModel, product);

            _unitOfWork.Update(product, editModel.SelectedSettings, editModel.SelectedCategories, User.Identity.Name);
            _unitOfWork.Commit();
            return RedirectToAction("Details", new { id = product.ProductId });
         }
         catch (Exception exception)
         {
            _logger.Error(exception,
               string.Format("There was a problem editing the Product with ID '{0}'.",
                  editModel != null ? editModel.ProductId.ToString() : string.Empty));
            return View("Error", new HandleErrorInfo(exception, "Products", "Edit"));
         }
      }

      // GET: Inventory/Delete/5
      [System.Web.Mvc.HttpGet]
      public ActionResult Delete(int? id)
      {
         try
         {
            if (id == null)
            {
               return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var product = _productRepository.Find(id.GetValueOrDefault());
            if (product == null)
            {
               return HttpNotFound();
            }
            var model = Mapper.Map<Product, DetailsProductsViewModel>(product);

            return View(model);
         }
         catch (Exception exception)
         {
            _logger.Error(exception,
               string.Format("There was a problem retrieving the Product with ID '{0}'.",
                  id.HasValue ? id.Value.ToString() : string.Empty));
            return View("Error", new HandleErrorInfo(exception, "Products", "Delete"));
         }
      }

      // POST: Inventory/Delete/5
      [System.Web.Mvc.HttpPost, System.Web.Mvc.ActionName("Delete")]
      [ValidateAntiForgeryToken]
      public ActionResult DeleteConfirmed(int id)
      {
         try
         {
            if (_unitOfWork.StockRepository.ItemInStock(id))
            {
               return RedirectToRoute("DefaultWeb", new { controller = "Error" });
            }
            var product = _productRepository.Find(id);
            _productRepository.Remove(product, User.Identity.Name);
            _unitOfWork.ExternalProductMappingRepository.Remove(product.ProductId, product.DeletedBy);
            _unitOfWork.Commit();
            return RedirectToAction("Index");
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem deleting the Product with ID '{0}'.", id));
            return View("Error", new HandleErrorInfo(exception, "Products", "Delete"));
         }
      }

      [System.Web.Mvc.HttpPost]
      [ValidateAntiForgeryToken]
      public ActionResult Restore(int id)
      {
         try
         {
            var product = _productRepository.FindIncludingDeleted(id);
            if (product == null)
            {
               return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            _productRepository.Restore(product, User.Identity.Name);
            _unitOfWork.ExternalProductMappingRepository.Restore(product.ExternalProductMappings, User.Identity.Name);
            _unitOfWork.Commit();
            return RedirectToAction("Details", new { id = product.ProductId });
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem restoring the Product with ID '{0}'.", id));
            return View("Error", new HandleErrorInfo(exception, "Products", "Restore"));
         }
      }

      [System.Web.Mvc.HttpGet]
      public ActionResult Search(string term)
      {
         try
         {
            var list =
               _productRepository.FindByDescription(term)
                  .ToList()
                  .Select(master => new KeyValuePair<int, string>(master.ProductId, master.Description));
            var result = Json(list, JsonRequestBehavior.AllowGet);
            return result;
         }
         catch (Exception exception)
         {
            _logger.Error(exception,
               string.Format("There was a problem searching for a Product using the term '{0}'.", term));
            return View("Error", new HandleErrorInfo(exception, "Products", "Search"));
         }
      }

      public JsonResult GetProducts(string text)
      {
         var results = from p in _unitOfWork.ProductRepository.FindAll() select new { p.Description, p.ProductId };
         if (!string.IsNullOrEmpty(text))
         {
            results = results.Where(p => p.Description.Contains(text));
         }

         return Json(results, JsonRequestBehavior.AllowGet);
      }

      public JsonResult ComplexSearch(string text)
      {
         var results =
            _productRepository.ComplexSearch(text)
               .Select(res => new SelectListItem { Value = res.Key.ToString(), Text = res.Value })
               .OrderBy(res => res.Text)
               .Take(100);
         return Json(results, JsonRequestBehavior.AllowGet);
      }

      public string GetDescription(int id)
      {
         var product = _productRepository.Find(id);
         return product == null ? string.Empty : product.Description;
      }

      public bool RequiresSerialNumber(int id)
      {
         var product = _productRepository.Find(id);
         return product != null && product.RequiresSerial;
      }

      public ActionResult Unclassified()
      {
         TempData[InventoryConstants.Unclassified] = InventoryConstants.Unclassified;
         return RedirectToAction("Index");
      }

      public ActionResult InError()
      {
         TempData[InventoryConstants.ProductsInError] = InventoryConstants.ProductsInError;
         ViewBag.IncludeDisabled = false;
         return RedirectToAction("Index");
      }

      public ActionResult PendingConsumedProducts()
      {
         TempData[InventoryConstants.PendingConsumedProducts] = InventoryConstants.PendingConsumedProducts;
         return RedirectToAction("Index");
      }

      public ActionResult Export()
      {
         return View();
      }

      [System.Web.Mvc.HttpPost]
      public ActionResult Excel_Export_Save(string contentType, string base64, string fileName)
      {
         try
         {
            var fileContents = Convert.FromBase64String(base64);

            return File(fileContents, contentType, fileName);
         }
         catch (Exception exception)
         {
            _logger.Error(exception,
               string.Format("There was a problem exporting an excel spreadsheet with the file name '{0}'.", fileName));
            return View("Error", new HandleErrorInfo(exception, "Products", "Excel_Export_Save"));
         }
      }

      public FileResult ExportToExcel([DataSourceRequest] DataSourceRequest request, FilterPeriod filterPeriod,
         string categoryIds, string statuses)
      {
         var products = _productRepository.GetProductsForExport(filterPeriod);
         if (!string.IsNullOrWhiteSpace(categoryIds) && !categoryIds.Equals(ExcelExportAllCharacter))
         {
            var categoryIdValues = categoryIds.Split(',').Select(int.Parse);
            products =
               products.Where(p => p.Product.ProductCategories.Any(pc => categoryIdValues.Contains(pc.CategoryId)));
         }

         if (!string.IsNullOrWhiteSpace(statuses) && !statuses.Equals(ExcelExportAllCharacter))
         {
            var statusesValues = statuses.Split(',').Select(s => (int)Enum.Parse(typeof(ProductStatus), s));
            products = products.Where(p => statusesValues.Contains((int)p.Product.ProductStatus));
         }

         var filteredProducts = products.Select(Mapper.Map<ProductForExport, ExportProductsViewModel>)
            .ToDataSourceResult(request)
            .Data;


         var productLedgerType =
            _propertyProvider.LedgerTypes.Single(lt => lt.Name == InventoryConstants.ProductLedgerType).LedgerTypeId;
         var productLedgerExportHelper = new GeneralLedgerExportHelper(
            _generalLedgerUnitOfWork.GeneralLedgerTierRepository, _generalLedgerUnitOfWork.GeneralLedgerRepository,
            productLedgerType);

         using (var stream = new MemoryStream())
         {
            var spreadsheet = Web.Utils.Excel.CreateWorkbook(stream);
            Web.Utils.Excel.AddBasicStyles(spreadsheet);
            Web.Utils.Excel.AddAdditionalStyles(spreadsheet);
            Web.Utils.Excel.AddWorksheet(spreadsheet, "Products");
            var worksheet = spreadsheet.WorkbookPart.WorksheetParts.First().Worksheet;

            var exportProperties = ProductImportColumnNames.ProductExportColumnsNames;
            var columnNameEnumerator = exportProperties.GetEnumerator();

            uint columnNumber = 1;
            uint rowNumber = 2;
            // add product import columns
            while (columnNameEnumerator.MoveNext())
            {
               Web.Utils.Excel.SetColumnHeadingValue(spreadsheet, worksheet, columnNumber,
                  columnNameEnumerator.Key.ToString(), false, false);
               columnNumber += 1;
            }

            // add glc segment columns
            var segmentNames =
               _generalLedgerUnitOfWork.GeneralLedgerTierRepository.GetSegmentNamesForExportImport(productLedgerType)
                  .Select(n => n.Value)
                  .ToList();
            foreach (var segmentName in segmentNames)
            {
               Web.Utils.Excel.SetColumnHeadingValue(spreadsheet, worksheet, columnNumber, segmentName, false, false);
               columnNumber += 1;
            }

            var modelType = typeof(ExportProductsViewModel);

            foreach (var product in filteredProducts)
            {
               columnNameEnumerator.Reset();
               columnNumber = 1;
               while (columnNameEnumerator.MoveNext())
               {
                  var value = modelType.GetProperty(columnNameEnumerator.Entry.Value.ToString()).GetValue(product);

                  Web.Utils.Excel.SetCellValue(spreadsheet, worksheet, columnNumber, rowNumber,
                     value != null ? value.ToString() : string.Empty, false,
                     false);
                  columnNumber += 1;
               }

               var model = product as ExportProductsViewModel;
               if (model == null)
               {
                  throw new Exception("Cast to ExportProductsViewModel failed. Cannot export product.");
               }
               // Populate general ledger code columns
               if (model.ProductLedgerId.HasValue)
               {
                  var glcCodes = productLedgerExportHelper.GetLedgerCodes(model.ProductLedgerId.Value);
                  if (glcCodes.Count > segmentNames.Count)
                  {
                     throw new Exception(
                        "Product has too many general ledger segments for the number defined. Cannot export product.");
                  }
                  foreach (var code in glcCodes)
                  {
                     Web.Utils.Excel.SetCellValue(spreadsheet, worksheet, columnNumber, rowNumber, code,
                        false, false);
                     columnNumber += 1;
                  }
               }
               rowNumber += 1;
            }

            worksheet.Save();
            spreadsheet.Close();

            return File(stream.ToArray(),
               "application/vnd.ms-excel",
               "ProductByConsumptionExport.xlsx");
         }
      }

      public ActionResult Get([DataSourceRequest] DataSourceRequest request, int[] categoryIds, string[] statuses, bool includeDeleted = false)
      {
         try
         {
            var products = _productRepository.FindAll(includeDeleted);
            if (categoryIds != null && !(categoryIds.Length == 1 && categoryIds[0] == 0))
            // when calling .read() on datasource it sends categoryIds = ['0']
            {
               products = products.Where(p => p.ProductCategories.Any(pc => categoryIds.Contains(pc.CategoryId)));
            }

            products = ApplyProductStatusFilter(products, statuses);

            var models = products.Select(Mapper.Map<Product, IndexProductsViewModel>);
            return Json(models.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem retrieving Products."));
            return Json(new DataSourceResult(), JsonRequestBehavior.AllowGet);
         }
      }

      private IQueryable<Product> ApplyProductStatusFilter(IQueryable<Product> products, string[] statuses)
      {
         if (statuses != null && statuses.Length > 0 &&
             !(statuses.Length == 1 && string.IsNullOrWhiteSpace(statuses[0])))
         {
            var statusEnums = statuses.Select(s => (int)Enum.Parse(typeof(ProductStatus), s));
            return products.Where(p => statusEnums.Contains((int)p.ProductStatus));
         }
         return products.Where(p => p.ProductStatus != ProductStatus.Disabled);
      }

      private IQueryable<ProductForExport> ApplyProductStatusFilter(IQueryable<ProductForExport> products,
         string[] statuses)
      {
         if (statuses != null && statuses.Length > 0 &&
             !(statuses.Length == 1 && string.IsNullOrWhiteSpace(statuses[0])))
         {
            var statusEnums = statuses.Select(s => (int)Enum.Parse(typeof(ProductStatus), s));
            return products.Where(p => statusEnums.Contains((int)p.Product.ProductStatus));
         }
         return products.Where(p => p.Product.ProductStatus != ProductStatus.Disabled);
      }

      [System.Web.Mvc.HttpPost]
      public ActionResult StockTakeProducts([DataSourceRequest] DataSourceRequest request,
         StockTakeProductFilter productFilter, int? filterLocationId)
      {
         var products = _productRepository.GetStockTakeProducts(productFilter, filterLocationId);
         var models = products.Select(Mapper.Map<Product, IndexProductsViewModel>);
         return Json(models.ToDataSourceResult(request));
      }

      [System.Web.Mvc.HttpGet]
      public ActionResult GetDetails(int id)
      {
         var product = _productRepository.Find(id);
         if (product == null)
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Cannot find product.");
         var details = Mapper.Map<Product, DetailsProductsViewModel>(product);
         return Json(details, JsonRequestBehavior.AllowGet);
      }

      [System.Web.Mvc.HttpPost]
      public ActionResult UpdateInformation(int productId, string upn, string spc)
      {
         if (string.IsNullOrWhiteSpace(upn) && string.IsNullOrWhiteSpace(spc))
         {
            _logger.Debug("Request to update information for product with ID {ProductID} but no UPN or SPC supplied",
               productId);
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "No information to update.");
         }

         var product = _productRepository.Find(productId);
         if (product == null)
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Cannot find product.");

         product.LastModifiedBy = User.Identity.Name;
         product.LastModifiedOn = DateTime.Now;
         //if (!string.IsNullOrWhiteSpace(upn))
         //{
         //   _logger.Debug("Updating UPN of product with ID {ProductId} from {CurrentUPN} to {NewUPN}", productId,
         //      product.UPN, upn);
         //   product.UPN = upn;
         //}

         if (!string.IsNullOrWhiteSpace(spc))
         {
            _logger.Debug("Updating SPC of product with ID {ProductId} from {CurrentSPC} to {NewSPC}", productId,
               product.SPC, spc);
            product.SPC = spc;
         }

         _productRepository.Commit();
         return new HttpStatusCodeResult(HttpStatusCode.OK);
      }

      [System.Web.Mvc.HttpGet]
      public ActionResult InStock(int id)
      {
         var productInStock = _unitOfWork.StockRepository.ItemInStock(id);
         return Json(productInStock, JsonRequestBehavior.AllowGet);
      }

      public ActionResult GetBulkUpdateFields()
      {
         var properties = typeof(BulkUpdateProductModel).GetProperties();
         var toReturn = new List<KeyValuePair<string, string>>();
         foreach (var propertyInfo in properties)
         {
            var name = PropertyHelper.GetPropertyDisplayName(propertyInfo);
            if (name != null)
               toReturn.Add(new KeyValuePair<string, string>(propertyInfo.Name, name));
         }

         var priceTypes = _unitOfWork.ProductPriceRepository.FindAllPriceTypes();
         foreach (var priceType in priceTypes)
         {
            toReturn.Add(
               new KeyValuePair<string, string>(
                  string.Format("{0}{1}", InventoryConstants.BulkUpdatePriceTypePrefix, priceType.PriceTypeId),
                  string.Format("{0} price", priceType.Name)));
         }

         return Json(toReturn.OrderBy(kvp => kvp.Value), JsonRequestBehavior.AllowGet);
      }

      public ActionResult GetHtmlControlForBulkUpdateField(string updateFieldName)
      {
         var properties = typeof(BulkUpdateProductModel).GetProperties();
         var property = properties.Where(p => p.Name == updateFieldName).ToList();
         dynamic model = new ExpandoObject();

         if (!property.Any())
         {
            // could be a price property
            var priceProperties = typeof(ProductPriceModels).GetProperties();
            var priceProperty = priceProperties.Where(p => p.Name == updateFieldName).ToList();
            if (!priceProperty.Any())
               return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Could not find field to update");
         }

         if (property.Count() > 1)
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Could not identify the field to update");
         }

         var type = property.Single().PropertyType;
         model.PropertyName = updateFieldName;

         if (updateFieldName == Nameof<BulkUpdateProductModel>.Property(p => p.ReorderThreshold)
             || updateFieldName == Nameof<BulkUpdateProductModel>.Property(p => p.TargetStockLevel)
             || updateFieldName == Nameof<BulkUpdateProductModel>.Property(p => p.MinimumOrder)
             || updateFieldName == Nameof<BulkUpdateProductModel>.Property(p => p.OrderMultiple))
         {
            model.Warning =
               "This detail will be updated regardless of other product details which could result products having invalid data.";
         }
         else if (updateFieldName == Nameof<BulkUpdateProductModel>.Property(p => p.AutoReorderSetting))
         {
            model.Warning =
               "Updating this detail will not affect the Reorder Threshold or Target Stock Level. Please update these seperately.";
            return PartialView("~/Areas/Inventory/Views/Products/PropertyControls/_AutoReorderSetting.cshtml", model);
         }
         else if (updateFieldName == Nameof<BulkUpdateProductModel>.Property(p => p.ManageStock))
         {
            model.Warning =
               "Updating products to not have their stock managed will result in the removal of all stock and the stock level being reset to 0.";
         }
         else
         {
            model.Warning = "";
         }

         if (updateFieldName == Nameof<BulkUpdateProductModel>.Property(p => p.SelectedCategories))
            return PartialView("~/Areas/Inventory/Views/Products/PropertyControls/_SelectedCategories.cshtml", model);

         if (updateFieldName == Nameof<BulkUpdateProductModel>.Property(p => p.SelectedSettings))
            return PartialView("~/Areas/Inventory/Views/Products/PropertyControls/_SelectedSettings.cshtml", model);

         if (updateFieldName == Nameof<BulkUpdateProductModel>.Property(p => p.PrimarySupplier))
            return PartialView("~/Areas/Inventory/Views/Products/PropertyControls/_PrimarySupplier.cshtml", model);

         if (updateFieldName == Nameof<BulkUpdateProductModel>.Property(p => p.RebateCode))
            return PartialView("~/Areas/Inventory/Views/Products/PropertyControls/_RebateCode.cshtml", model);

         if (updateFieldName == Nameof<BulkUpdateProductModel>.Property(p => p.ProductStatus))
            return PartialView("~/Areas/Inventory/Views/Products/PropertyControls/_Status.cshtml", model);

         if (type == (typeof(string)))
         {
            return PartialView("~/Areas/Inventory/Views/Products/PropertyControls/_TextBox.cshtml", model);
         }

         if (type == (typeof(int)) || type == (typeof(decimal)))
         {
            model.Value = 0;
            return PartialView("~/Areas/Inventory/Views/Products/PropertyControls/_IntegerProperty.cshtml", model);
         }

         if (type == typeof(int?) || type == (typeof(decimal?)))
         {
            model.Value = null;
            if (updateFieldName == Nameof<BulkUpdateProductModel>.Property(p => p.LedgerId))
            {
               model.LedgerType =
                  _propertyProvider.LedgerTypes.Single(lt => lt.Name == InventoryConstants.ProductLedgerType)
                     .LedgerTypeId;
               return PartialView("~/Areas/Inventory/Views/Products/PropertyControls/_GeneralLedgerCode.cshtml", model);
            }

            return PartialView("~/Areas/Inventory/Views/Products/PropertyControls/_IntegerProperty.cshtml", model);
         }

         if (type == typeof(bool))
         {
            model.Nullable = false;
            return PartialView("~/Areas/Inventory/Views/Products/PropertyControls/_BoolProperty.cshtml", model);
         }

         if (type == typeof(bool?))
         {
            model.Nullable = true;
            return PartialView("~/Areas/Inventory/Views/Products/PropertyControls/_BoolProperty.cshtml", model);
         }

         return PartialView("~/Areas/Inventory/Views/Products/PropertyControls/_TextBox.cshtml", model);
      }


      public ActionResult BulkUpdate([DataSourceRequest] DataSourceRequest request, string propertyName,
         string subDetail, string value, int[] categoryIds, string[] statuses)
      {
         _logger.Debug("Attempting to bulk update products. {PropertyName}, {SubDetail}, {Value}, {CategoryIds}, {Statuses}", propertyName, subDetail, value, categoryIds, statuses);

         if (string.IsNullOrWhiteSpace(propertyName))
         {
            _logger.Debug("Attempting to bulk update products with no field name provided");
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "No product detail provided.");
         }

         var updateProductPrice = propertyName.StartsWith(InventoryConstants.BulkUpdatePriceTypePrefix);
         // validate field exists and retrieve info if it is a list of values (i.e. settings)
         PropertyInfo propertyInfo = null;

         try
         {
            propertyInfo = updateProductPrice
               ? BulkUpdateHelper.GetPricePropertyInfo(subDetail)
               : BulkUpdateHelper.GetProductPropertyInfo(propertyName);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, "No Product property exists for field name {PropertyName} provided", propertyName);
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Could not find detail to update.");
         }

         object convertedValue = null;

         // validate and extract value as correct type
         if (!string.IsNullOrWhiteSpace(value))
         {
            try
            {
               convertedValue = BulkUpdateHelper.GetConvertedValue(propertyInfo, propertyName, value);
            }
            catch (Exception exception)
            {
               _logger.Error(exception, "Unable to convert value {Value} to the type of the field name {PropertyName}",
                  value,
                  propertyName);
               return new HttpStatusCodeResult(HttpStatusCode.BadRequest,
                  "Submitted value not able to be assigned to detail specified.");
            }
         }

         var productsToUpdate = _productRepository.FindAll();

         // filter by categories
         if (categoryIds != null && !(categoryIds.Length == 1 && categoryIds[0] == 0))
         {
            productsToUpdate =
               productsToUpdate.Where(p => p.ProductCategories.Any(pc => categoryIds.Contains(pc.CategoryId)));
         }

         productsToUpdate = ApplyProductStatusFilter(productsToUpdate, statuses);

         // filter products using kendo magic and extract ids
         if (request.Filters.Any())
         {
            try
            {
               productsToUpdate = BulkUpdateHelper.GetFilteredProductsFromIndexViewModelFilters(productsToUpdate,
                  request, _productRepository);
            }
            catch (Exception exception)
            {
               _logger.Error(exception, "Unable to filter products as part of bulk update");
               return new HttpStatusCodeResult(HttpStatusCode.InternalServerError,
                  "Cannot interpret products to update.");
            }
         }

         if (!productsToUpdate.Any())
         {
            _logger.Debug("No products in the filter to update");
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "No products to update.");
         }

         // update the products with the new value
         try
         {
            if (updateProductPrice)
            {
               BulkUpdateHelper.UpdateProductPriceProperties(productsToUpdate, propertyInfo, propertyName,
                  convertedValue,
                  _productRepository, User.Identity.Name);
            }
            else
            {
               BulkUpdateHelper.UpdateProductsProperties(productsToUpdate, propertyInfo, propertyName, convertedValue,
                  _unitOfWork, User.Identity.Name);
            }
         }
         catch (Exception exception)
         {
            _logger.Error(exception, "Unable to assign value {Value} to product property {PropertyName}", value,
               propertyName);
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Unable to assign value to product detail.");
         }

         _productRepository.Commit();
         return new HttpStatusCodeResult(HttpStatusCode.OK);
      }

      public ActionResult GetProductsForExport([DataSourceRequest] DataSourceRequest request, FilterPeriod filterPeriod,
         int[] categoryIds, string[] statuses)
      {
         try
         {
            var products = _unitOfWork.ProductRepository.GetProductsForExport(filterPeriod);
            if (categoryIds != null && !(categoryIds.Length == 1 && categoryIds[0] == 0))
            // when calling .read() on datasource it sends categoryIds = ['0']
            {
               products = products.Where(p => p.Product.ProductCategories.Any(pc => categoryIds.Contains(pc.CategoryId)));
            }

            products = ApplyProductStatusFilter(products, statuses);

            return
               Json(products.Select(Mapper.Map<ProductForExport, ExportProductsViewModel>).ToDataSourceResult(request),
                  JsonRequestBehavior.AllowGet);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem retrieving Products."));
            return Json(new DataSourceResult(), JsonRequestBehavior.AllowGet);
         }
      }


      /// <summary>
      /// Identifies products based on an input that could either be in the following formats:
      ///   - raw UPN
      ///   - raw SPC
      ///   - EAN barcode
      ///   - HIBC barcode
      ///   - GS1 barcode
      /// </summary>
      /// <param name="text"></param>
      /// <returns>
      ///   A list of products matched to the provided input in key value format.
      ///      Key:     Description of the product
      ///      Value:   ScanCodeProductResult which holds the ProductId, codes and any info extracted from the barcode
      /// </returns>
      [System.Web.Mvc.HttpGet]
      public ActionResult GetProductByCode(string text)
      {
         if (string.IsNullOrWhiteSpace(text))
            return Json(new List<KeyValuePair<string, ScanCodeProductResult>>(), JsonRequestBehavior.AllowGet);

         ScanCodeResult codeResult;
         ProductSearchCriteria criteria;

         try
         {
            codeResult = ScanCodeHelper.TryParseHibc(text, _logger);
         }
         catch
         {
            throw new HttpResponseException(HttpStatusCode.InternalServerError);
         }

         if (codeResult == null)
         {
            try
            {
               codeResult = ScanCodeHelper.TryParseGs1(text, _logger);
            }
            catch
            {
               throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
         }

         if (codeResult == null ||
             (string.IsNullOrWhiteSpace(codeResult.UPN) && string.IsNullOrWhiteSpace(codeResult.SPC)))
         {
            _logger.Debug("Searching products for UPN or SPC matching {code}", text);
            criteria = new ProductSearchCriteria { UPC = text, SPC = text };
         }
         else
         {
            criteria = new ProductSearchCriteria { UPC = codeResult.UPN, SPC = codeResult.SPC };
         }

         // Search for products by code with exact match
         var products = _unitOfWork.ProductRepository.FindByCode(criteria);

         // Search for products by code using contains
         if (!products.Any())
         {
            products = _unitOfWork.ProductRepository.FindByCodeUsingContains(criteria);
            _logger.Debug("{ProductCount} products found with codes containing the user input {Input}", products.Count(),
               text);
         }

         // Search for products by description using contains
         if (!products.Any())
         {
            products = _unitOfWork.ProductRepository.FindAll().Where(p => p.Description.Contains(text));
            _logger.Debug("{ProductCount} products found with a description containing the user input {Input}",
               products.Count(), text);
         }

         // Prepare list for return. If no products, return single list item based on scan result
         var toReturn = GetProductsWithScanCodeProductResults(products.Any() ? products : null, text, codeResult);

         return Json(toReturn, JsonRequestBehavior.AllowGet);
      }

      private List<KeyValuePair<string, ScanCodeProductResult>> GetProductsWithScanCodeProductResults(
         IQueryable<Product> products, string text, ScanCodeResult codeResult)
      {
         if (products != null)
            return (from p in products.AsEnumerable()
                    select
                       new KeyValuePair<string, ScanCodeProductResult>(p.Description,
                          MapToScanCodeProductResult(p, codeResult))).ToList();

         if (codeResult == null)
         {
            codeResult = new ScanCodeResult();
         }
         return new List<KeyValuePair<string, ScanCodeProductResult>>
         {
            new KeyValuePair<string, ScanCodeProductResult>(text,
               Mapper.Map<ScanCodeResult, ScanCodeProductResult>(codeResult))
         };
      }

      private ScanCodeProductResult MapToScanCodeProductResult(Product product, ScanCodeResult scanCodeResult)
      {
         var result = new ScanCodeProductResult();
         if (scanCodeResult != null)
         {
            result = Mapper.Map<ScanCodeResult, ScanCodeProductResult>(scanCodeResult);
         }
         Mapper.Map(product, result); // (source, dest)
         return result;
      }

      public ActionResult Merge(int? id)
      {
         ViewBag.PriceTypes = _unitOfWork.ProductPriceRepository.FindAllPriceTypes();

         return View(id);
      }

      public ActionResult DisplayMergeProducts(int productIdToKeep, int? productIdToDelete)
      {
         var products =
            _productRepository.FindAll().Where(p => p.ProductId == productIdToKeep || p.ProductId == productIdToDelete);
         if (products.Count() != 2 && productIdToDelete != null)
         {
            _logger.Error(
               "Unable to find one or more of the following products to display for merge. Product to keep {ProductIdToKeep}, product to delete {ProductIdToDelete}",
               productIdToKeep, productIdToDelete);
         }

         var models = products.Select(Mapper.Map<Product, MergeProductsViewModel>).ToList();

         var toReturn = new MergeProductsModel
         {
            ToKeep = models.Single(m => m.ProductId == productIdToKeep),
            ToDelete = models.SingleOrDefault(m => m.ProductId == productIdToDelete)
         };

         if (toReturn.ToDelete == null)
         {
            toReturn.ToDelete = new MergeProductsViewModel
            {
               Prices = new List<ProductPriceViewModel>()
            };
         }

         return PartialView("_MergeFields", toReturn);
      }

      [System.Web.Mvc.HttpPost]
      public ActionResult Merge(int productIdToKeep, int productIdToDelete, string[] propertiesToMerge)
      {
         var products =
            _productRepository.FindAll().Where(p => p.ProductId == productIdToKeep || p.ProductId == productIdToDelete);
         if (products.Count() != 2)
         {
            _logger.Error(
               "Unable to find one or more of the following products to merge. Product to keep {ProductIdToKeep}, product to delete {ProductIdToDelete}",
               productIdToKeep, productIdToDelete);
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Unable to find products for merge.");
         }

         var toKeep = products.Single(p => p.ProductId == productIdToKeep);
         var toDelete = products.Single(p => p.ProductId == productIdToDelete);
         var mergeUser = User.Identity.Name;
         try
         {
            var mergeResult = ProductMergeHelper.MergeProperties(toKeep, toDelete, propertiesToMerge, mergeUser, _unitOfWork, _logger);
            if (!mergeResult.Success)
            {
               _logger.Error("Unable to merge products. The following properties have failed to be merged: {FailedMergedProperties}", string.Join(", ", mergeResult.FailedProperties));
               return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Unable to merge products.");
            }

            toKeep.LastModifiedBy = mergeUser;
            toKeep.LastModifiedOn = DateTime.Now;
            _productRepository.ValidateProduct(toKeep);
            _productRepository.MergeRelatedItems(toKeep, toDelete, mergeUser);
            _productRepository.Remove(toDelete, mergeUser);
            _unitOfWork.Commit();
            TempData[SuccessfulMerge] = toDelete.Description;
            return new HttpStatusCodeResult(HttpStatusCode.OK);
         }
         catch (Exception exception)
         {
            var correlationId = Guid.NewGuid();
            _logger.Error(exception, "An error occurred attempting to merge the product {ProductIdToDelete} into {ProductIdToKeep}: {CorrelationId}", productIdToDelete, productIdToKeep,correlationId);
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, $"Unable to merge product {productIdToDelete} into {productIdToKeep}, please contact support. Ref:{correlationId}");
         }
      }

      public ActionResult GetStockEventTypes()
      {
         try
         {
            var stockEventTypes = (StockEvent[])Enum.GetValues(typeof(StockEvent));

            var stockEventList = stockEventTypes.Select(stockEvent => new SelectListItem
            {
               Text = HelperMethods.GetEnumDisplayName<StockEvent>(stockEvent),
               Value = ((int)stockEvent).ToString()
            }).OrderBy(o => o.Text).ToList();


            return Json(stockEventList, JsonRequestBehavior.AllowGet);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem retrieving the stock event types."));
            return View("Error", new HandleErrorInfo(exception, "Products", "GetStockEventTypes"));
         }
      }

      public ActionResult GetStockEvents([DataSourceRequest] DataSourceRequest request, int productId)
      {
         var stockDeductions =
            _unitOfWork.StockAdjustmentRepository.FindAllDeductions().Where(sd => sd.StockAdjustmentStocks.Any(sas => sas.Stock.ProductId == productId));

         var events = new List<ProductStockEventViewModel>();

         events.AddRange(stockDeductions.Select(sd => new ProductStockEventViewModel
         {
            Creator = sd.CreatedBy,
            EventDate = sd.CreatedOn,
            EventId = sd.StockAdjustmentId,
            Location = sd.StockAdjustmentStocks.FirstOrDefault().Stock.StorageLocation != null ? sd.StockAdjustmentStocks.FirstOrDefault().Stock.StorageLocation.Name : string.Empty,
            ProductId = productId,
            Quantity = sd.Quantity,
            Source = sd.Source.ToString(),
            StockEvent = StockEvent.Deduction
         }));

         var orders = _unitOfWork.OrderRepository.OrdersForProduct(productId);

         events.AddRange(orders.Select(order => new ProductStockEventViewModel
         {
            Creator = order.CreatedBy,
            EventDate = order.DateCreated,
            EventId = order.InventoryOrderId,
            Location = order.DeliveryLocation != null ? order.DeliveryLocation.Name : string.Empty,
            ProductId = productId,
            Quantity = order.Items.FirstOrDefault(i => i.ProductId == productId).Quantity,
            Status = order.Status.ToString(),
            StockEvent = StockEvent.Order
         }));

         return Json(events.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
      }
   }
}