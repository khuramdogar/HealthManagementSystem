using AutoMapper;
using HMS.HealthTrack.Web.Areas.Inventory.Models.PaymentClassMappings;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System.Linq;
using System.Web.Mvc;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers
{
   public class PaymentClassMappingController : Controller
   {
      private readonly IPaymentClassMappingRepository _repository;
      private readonly ICustomLogger _logger;

      public PaymentClassMappingController(IPaymentClassMappingRepository repository, ICustomLogger logger)
      {
         _repository = repository;
         _logger = logger;
      }

      [HttpGet]
      public ActionResult Index()
      {
         return View();
      }

      public ActionResult Read([DataSourceRequest] DataSourceRequest request)
      {
         var mappings = _repository.FindAll().Select(Mapper.Map<PaymentClassMapping, PaymentClassMappingModel>).ToList();
         return Json(mappings.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
      }

      [HttpPost]
      public ActionResult Create([DataSourceRequest] DataSourceRequest request, PaymentClassMappingModel model)
      {
         var toReturn = model;
         if (ModelState.IsValid && model != null)
         {
            int priceTypeId;
            if (!string.IsNullOrWhiteSpace(model.PriceTypeId) && int.TryParse(model.PriceTypeId, out priceTypeId) && priceTypeId > 0)
            {
               _logger.Warning("Attempting to create Payment Class Mapping for {PaymentClass}", model.PaymentClass);
               _repository.Create(model.PaymentClass, priceTypeId, User.Identity.Name);
               _repository.Commit();
               var created = _repository.Find(model.PaymentClass);
               if (created == null)
               {
                  _logger.Error("Failed to find created Payment Class Mapping for {PaymentClass}", model.PaymentClass);
               }
               else
               {
                  toReturn = Mapper.Map<PaymentClassMapping, PaymentClassMappingModel>(created);
               }
            }
            else
            {
               ModelState.AddModelError(Nameof<PaymentClassMappingModel>.Property(p => p.PriceTypeId),
                  "No price type specified");
            }
         }
         else
         {
            _logger.Warning("Attempting to create Payment Class Mapping with invalid data");
         }

         return Json(new[] { toReturn }.ToDataSourceResult(request, ModelState));
      }

      [HttpPost]
      public ActionResult Update([DataSourceRequest] DataSourceRequest request, PaymentClassMappingModel model)
      {
         var toReturn = model;
         if (ModelState.IsValid && model != null)
         {
            int priceTypeId;
            if (!string.IsNullOrWhiteSpace(model.PriceTypeId) && int.TryParse(model.PriceTypeId, out priceTypeId) && priceTypeId > 0)
            {
               _logger.Warning("Attempting to update Payment Class Mapping for {PaymentClass}", model.PaymentClass);
               _repository.Update(model.PaymentClass, priceTypeId, User.Identity.Name);
               _repository.Commit();
               var updated = _repository.Find(model.PaymentClass);
               if (updated == null)
               {
                  _logger.Error("Failed to find updated Payment Class Mapping for {PaymentClass}", model.PaymentClass);
               }
               else
               {
                  toReturn = Mapper.Map<PaymentClassMapping, PaymentClassMappingModel>(updated);
               }
            }
            else
            {
               ModelState.AddModelError(Nameof<PaymentClassMappingModel>.Property(p => p.PriceTypeId), "No price type specified");
            }
         }
         else
         {
            _logger.Warning("Attempting to update Payment Class Mapping with invalid data");
         }
         return Json(new[] { toReturn }.ToDataSourceResult(request, ModelState));
      }

      [HttpPost]
      public ActionResult Remove([DataSourceRequest] DataSourceRequest request, PaymentClassMappingModel model)
      {
         var toReturn = model;
         if (model != null)
         {
            _repository.Remove(model.PaymentClass, User.Identity.Name);
            _repository.Commit();
            var deleted = _repository.Find(model.PaymentClass);
            if (deleted == null)
            {
               _logger.Error("Failed to find deleted Payment Class Mapping for {PaymentClass}", model.PaymentClass);
            }
            else
            {
               toReturn = Mapper.Map<PaymentClassMapping, PaymentClassMappingModel>(deleted);
            }
         }
         else
         {
            _logger.Warning("Attempting to update Payment Class Mapping with invalid data");
         }
         return Json(new[] { toReturn }.ToDataSourceResult(request, ModelState));
      }
   }
}