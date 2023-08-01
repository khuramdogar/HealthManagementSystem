using System.Linq;
using System.Web.Mvc;
using HMS.HealthTrack.Web.Data.Helpers;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers
{
   [HealthTrackAuthorize(HealthTrackPermission = ("Inventory"))]
   public class ProductSettingsController : Controller
   {
      private readonly IStockSettingRepository _repository;
      private readonly ICategoryRepository _categoryRepository;

      public ProductSettingsController(IStockSettingRepository repository, ICategoryRepository categoryRepository)
      {
         _repository = repository;
         _categoryRepository = categoryRepository;
      }

      // GET: ProductSettings/GetAll
      public JsonResult GetAll(string text)
      {
         var settings = _repository.FindAll();
         if (!string.IsNullOrWhiteSpace(text))
            settings = settings.Where(s => s.Name.Contains(text));

         var items = settings.Select(s => new
         {
            Text = s.Name,
            SettingId = s.SettingId
         });

         return Json(items, JsonRequestBehavior.AllowGet);
      }

      public JsonResult GetAllUserReadable()
      {
         var stockSettings = _repository.FindAll().ToList();
         var stockSettingsNames = ProductImportStaticValidation.StockSettings;

         var items = stockSettingsNames.Select(s => new SelectListItem
         {
            Text = stockSettings.Single(ss => ss.SettingId == s.Value).Name,
            Value = s.Key
         });
         return Json(items, JsonRequestBehavior.AllowGet);
      }

      //public JsonResult()

      // GET: ProductSettings/CategoriesSettings/1
      public JsonResult CategoriesSettings(int[] ids)
      {
         var settings =
            _categoryRepository.FindAll()
               .Where(c => ids.Contains(c.CategoryId))
               .SelectMany(c => c.StockSettings.Select(ss => ss.SettingId));

         return Json(settings.Distinct(), JsonRequestBehavior.AllowGet);
      }
   }
}
