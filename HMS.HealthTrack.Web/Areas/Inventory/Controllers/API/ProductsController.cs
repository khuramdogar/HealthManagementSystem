using AutoMapper;
using HMS.HealthTrack.Web.Areas.Inventory.Models.Products;
using HMS.HealthTrack.Web.Areas.Inventory.Utils;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Areas.Inventory.Models.ExternalApi.Products;
using HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork;
using ScanCodeResult = HMS.HealthTrack.Web.Areas.Inventory.Models.ScanCode.ScanCodeResult;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers.API
{
   [HealthTrackAuthorize(HealthTrackPermission = ("Inventory"))]
   public class ProductsController : MediatedController
   {
      private readonly IProductUnitOfWork _unitOfWork;
      private readonly ICustomLogger _logger;

      public ProductsController(IProductUnitOfWork unitOfWork, ICustomLogger logger)
      {
         _unitOfWork = unitOfWork;
         _logger = logger;
      }

      [HttpGet]
      public async Task<JsonResult<ProductDto>> GetProduct([FromUri]ProductQuery query)
      {
         var result = await Mediator.Send(query);
         return Json(result);
      }

      [HttpDelete]
      public HttpResponseMessage DeleteProduct(int id)
      {
         try
         {
            _logger.Information("Attemping to delete product {ProductId}", id);
            var product = _unitOfWork.ProductRepository.Find(id);
            if (product == null)
            {
               _logger.Warning("Could not find product {ProductId} for deletion", id);
               return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Could not find product for deletion");
            }

            _unitOfWork.ProductRepository.Remove(product, User.Identity.Name);
            _unitOfWork.Commit();
            return Request.CreateResponse(HttpStatusCode.OK, "Product successfully deleted.");
         }
         catch (Exception exception)
         {
            _logger.Error(exception, "There was a problem deleting the Product {ProductId}", id);
            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
               "There was a problem deleting the product.");
         }
      }

      
   }
}