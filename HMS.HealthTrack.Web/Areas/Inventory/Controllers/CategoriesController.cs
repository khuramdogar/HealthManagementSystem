using AutoMapper;
using HMS.HealthTrack.Web.Areas.Inventory.Models.Categories;
using HMS.HealthTrack.Web.Areas.Inventory.Models.StockSettings;
using Kendo.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers
{
   [HealthTrackAuthorize(HealthTrackPermission = ("Inventory"))]
   public class CategoriesController : Controller
   {
      private readonly ICategoryRepository _categoryRepository;
      private readonly ICustomLogger _logger;

      public CategoriesController(ICategoryRepository categoryRepository, ICustomLogger logger)
      {
         _categoryRepository = categoryRepository;
         _logger = logger;
      }

      public ActionResult Index()
      {
         return View();
      }

      public ActionResult Get([DataSourceRequest] DataSourceRequest request)
      {
         var categories = ConstructCategoryTree();

         var categoryName = Nameof<CategoryViewModel>.Property(p => p.CategoryName);

         var filters = request.Filters;
         foreach (var filterDescriptor in filters)
         {
            var filter = (FilterDescriptor)filterDescriptor;
            if (filter == null) continue;

            if (filter.Member.IsCaseInsensitiveEqual(categoryName))
            {
               var filteredCategoryIds = _categoryRepository.Search(filter.Value.ToString());
               categories = categories.Where(c => filteredCategoryIds.Contains(c.CategoryId.Value));
            }
         }

         request.Filters.Clear();
         var result = categories.ToTreeDataSourceResult(request, e => e.CategoryId, e => e.ParentId);
         return Json(result, JsonRequestBehavior.AllowGet);
      }

      private IEnumerable<CategoryViewModel> ConstructCategoryTree()
      {
         var roots = _categoryRepository.FindRoots().ToList();

         var categories = new List<CategoryViewModel>();

         foreach (var category in roots)
         {
            categories.Add(new CategoryViewModel
            {
               CategoryId = category.CategoryId,
               CategoryName = category.CategoryName,
               CreationDate = category.CreationDate,
               Disinherit = category.Disinherit,
               HasChildren = category.CategoryChildren.Any(cc => cc.Depth == 1),
               LastModifiedDate = category.LastModifiedDate,
               LastModifiedUser = category.LastModifiedUser,
               ParentId = null,
               StockSettings = category.StockSettings.Select(Mapper.Map<StockSetting, StockSettingViewModel>),
               UserCreated = category.UserCreated,
            });
            BuildCategories(category, categories);
         }
         return categories;
      }

      private void BuildCategories(Category category, IList<CategoryViewModel> categories)
      {
         if (category.CategoryChildren.All(cc => cc.Depth != 1)) return;

         foreach (var child in category.CategoryChildren.Where(cc => cc.Depth == 1).Select(cc => cc.CategoryChildren).Where(c => !c.DeletedOn.HasValue))
         {
            categories.Add(new CategoryViewModel
            {
               CategoryId = child.CategoryId,
               CategoryName = child.CategoryName,
               CreationDate = child.CreationDate,
               Disinherit = child.Disinherit,
               HasChildren = child.CategoryChildren.Any(cc => cc.Depth == 1),
               LastModifiedDate = child.LastModifiedDate,
               LastModifiedUser = child.LastModifiedUser,
               ParentId = category.CategoryId,
               StockSettings = child.StockSettings.Select(Mapper.Map<StockSetting, StockSettingViewModel>),
               UserCreated = child.UserCreated,
            });
            BuildCategories(child, categories);
         }
      }

      public ActionResult Create([DataSourceRequest] DataSourceRequest request, CategoryViewModel model)
      {
         if (!ModelState.IsValid)
         {
            _logger.Information("Unable to create category {CategoryName} due to invalid values", model.CategoryName);
            return Json(new[] { model }.ToTreeDataSourceResult(request, ModelState));
         }

         var category = Mapper.Map<CategoryViewModel, Category>(model);
         _categoryRepository.Add(category, model.ParentId, User.Identity.Name);
         _categoryRepository.Commit();

         var createdCategory = Mapper.Map<Category, CategoryViewModel>(category);
         return Json(new[] { createdCategory }.ToDataSourceResult(request, ModelState));
      }

      public ActionResult Update([DataSourceRequest] DataSourceRequest request, CategoryViewModel model)
      {
         if (!ModelState.IsValid)
         {
            _logger.Information("Unable to update category {CategoryId} due to invalid values", model.CategoryId);
            return Json(new[] { model }.ToTreeDataSourceResult(request, ModelState));
         }

         var category = Mapper.Map<CategoryViewModel, Category>(model);
         _categoryRepository.Update(category, model.ParentId, User.Identity.Name);
         _categoryRepository.Commit();

         var updatedCategory = Mapper.Map<Category, CategoryViewModel>(category);

         return Json(new[] { updatedCategory }.ToTreeDataSourceResult(request, ModelState));
      }

      public JsonResult Destroy([DataSourceRequest] DataSourceRequest request, CategoryViewModel model)
      {
         if (!ModelState.IsValid || !model.CategoryId.HasValue)
         {
            _logger.Information("Unable to delete category {CategoryId} due to invalid values", model.CategoryId);
            return Json(new[] { model }.ToTreeDataSourceResult(request, ModelState));
         }
         var category = _categoryRepository.Find(model.CategoryId.Value);
         if (category == null)
         {
            _logger.Information("Unable to delete category {CategoryId}, does not exist", model.CategoryId);
            return Json(new[] { model }.ToTreeDataSourceResult(request, ModelState));
         }

         _categoryRepository.Remove(model.CategoryId.Value, User.Identity.Name);
         _categoryRepository.Commit();
         var deletedCategory = Mapper.Map<Category, CategoryViewModel>(_categoryRepository.Find(model.CategoryId.Value));

         return Json(new[] { deletedCategory }.ToTreeDataSourceResult(request, ModelState));
      }

      public JsonResult GetParents(int? id)
      {
         var categories =
            _categoryRepository.FindParents(id)
               .Select(c => new { Value = c.CategoryId, Text = c.CategoryName }).OrderBy(c => c.Text).ToList();
         return Json(categories, JsonRequestBehavior.AllowGet);
      }

      public JsonResult Categories(int? id)
      {
         var roots = _categoryRepository.FindRoots().ToList();

         var categories = new List<CategoryViewModel>();

         foreach (var category in roots)
         {
            categories.Add(new CategoryViewModel
            {
               CategoryId = category.CategoryId,
               CategoryName = category.CategoryName,
               CreationDate = category.CreationDate,
               Disinherit = category.Disinherit,
               HasChildren = category.CategoryChildren.Any(cc => cc.Depth == 1),
               LastModifiedDate = category.LastModifiedDate,
               LastModifiedUser = category.LastModifiedUser,
               ParentId = null,
               StockSettings = category.StockSettings.Select(Mapper.Map<StockSetting, StockSettingViewModel>),
               UserCreated = category.UserCreated,
            });
            BuildCategories(category, categories);
         }

         var toReturn = (from c in categories
                         where (id.HasValue ? c.ParentId == id : c.ParentId == null)
                         select new
                         {
                            id = c.CategoryId,
                            c.CategoryName,
                            hasChildren = c.HasChildren
                         });
         return Json(toReturn, JsonRequestBehavior.AllowGet);
      }

      public ActionResult CategorySelector()
      {
         return PartialView("_CategorySelector");
      }
   }
}