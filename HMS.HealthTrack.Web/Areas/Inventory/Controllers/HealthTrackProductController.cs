using AutoMapper;
using HMS.HealthTrack.Web.Areas.Inventory.Models.HealthTrackProducts;
using System.Web.Mvc;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Model.HealthTrackInventory;
using HMS.HealthTrack.Web.Data.Repositories.HealthTrackInventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers
{
   public class HealthTrackProductController : Controller
   {
      private readonly IInventoryMasterRepository _repository;
      private readonly ICustomLogger _logger;

      public HealthTrackProductController(IInventoryMasterRepository repository, ICustomLogger logger)
      {
         _repository = repository;
         _logger = logger;
      }

      public ActionResult Details(int id)
      {
         var product = _repository.Find(id);
         if (product == null)
         {
            _logger.Information("Unable to find details for HealthTrack product with ID {Inv_ID}", id);
            return Content("Cannot find details for product");
         }

         return PartialView("_ProductDetails", Mapper.Map<Inventory_Master, DetailsHealthTrackProductModel>(product));
      }
   }
}