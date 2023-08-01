using HMS.HealthTrack.Web.Areas.Inventory.Models.Products;
using HMS.HealthTrack.Web.Areas.Inventory.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers
{
   public class PriceTypesController : Controller
   {
      private readonly IProductPriceRepository _repository;
      private readonly ICustomLogger _logger;

      public PriceTypesController(IProductPriceRepository repository, ICustomLogger logger)
      {
         _repository = repository;
         _logger = logger;
      }

      [HttpGet]
      public ActionResult GetPriceTypes()
      {
         var priceTypes = _repository.FindAllPriceTypes().Select(pt => new SelectListItem
         {
            Text = pt.Name,
            Value = pt.PriceTypeId.ToString()
         });

         return Json(priceTypes, JsonRequestBehavior.AllowGet);
      }

      [HttpGet]
      public ActionResult GetPriceFields()
      {
         var properties = typeof(ProductPriceModels).GetProperties();
         var toReturn = new List<KeyValuePair<string, string>>();

         foreach (var propertyInfo in properties)
         {
            var name = PropertyHelper.GetPropertyDisplayName(propertyInfo);
            if (name != null)
               toReturn.Add(new KeyValuePair<string, string>(propertyInfo.Name, name));
         }

         return Json(toReturn.OrderBy(kvp => kvp.Key), JsonRequestBehavior.AllowGet);
      }
   }
}