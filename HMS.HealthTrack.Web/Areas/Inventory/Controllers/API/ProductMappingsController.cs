using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Data.Entity.Infrastructure;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers.API
{
   [HealthTrackAuthorize(HealthTrackPermission = ("Inventory"))]
   public class ProductMappingsController : ApiController
   {
      private readonly IProductMappingUnitOfWork _unitOfWork;
      private readonly ICustomLogger _logger;

      public ProductMappingsController(IProductMappingUnitOfWork productMappingUnitOfWork, ICustomLogger logger)
      {
         _unitOfWork = productMappingUnitOfWork;
         _logger = logger;
      }

      // GET api/ProductMappings
      [HttpGet]
      public DataSourceResult ProductMappings(
         [System.Web.Http.ModelBinding.ModelBinder(typeof(WebApiDataSourceRequestModelBinder))] DataSourceRequest
            request)
      {
         try
         {
            var mappings = _unitOfWork.MappingRepository.GetMappingOverviews();
            return mappings.ToDataSourceResult(request);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem retrieving Product Mappings."));
            return new DataSourceResult();
         }
      }

      // Post api/Products/5
      [HttpPost]
      public HttpResponseMessage PostProductMapping(MappingOverview mappingDetails)
      {
         try
         {
            if (!ModelState.IsValid)
            {
               return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            if (mappingDetails == null || mappingDetails.InternalId == 0)
            {
               return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            _unitOfWork.CreateMapping(mappingDetails);

            try
            {
               _unitOfWork.Commit();
            }
            catch (DbUpdateConcurrencyException ex)
            {
               return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
         }
         catch (Exception exception)
         {
            _logger.Error(exception,
               string.Format(
                  "There was a problem creating a Product Mapping for the External ID '{0}' and the Internal ID '{1}'.",
                  mappingDetails != null ? mappingDetails.ExternalId.ToString() : string.Empty,
                  mappingDetails != null ? mappingDetails.InternalId.ToString() : string.Empty));
            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exception);
         }
      }

      [HttpDelete]
      public HttpResponseMessage Delete(int id)
      {
         try
         {
            _logger.Information("Attempting to delete product mapping {MappingId}", id);
            var mapping = _unitOfWork.MappingRepository.Find(id);
            if (mapping == null)
            {
               _logger.Warning("Could not find product mapping {MappingId} for deletion", id);
               return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Could not find product mapping for deletion");
            }
            mapping.DeletedBy = User.Identity.Name;
            _unitOfWork.MappingRepository.Remove(mapping);
            _unitOfWork.MappingRepository.Commit();
            return Request.CreateResponse(HttpStatusCode.OK, "Product mapping successfully deleted.");
         }
         catch (Exception exception)
         {
            _logger.Error(exception, "There was an error deleting product mapping {MappingId}", id);
            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
               "There was an error attempting to delete the product mapping");
         }
      }
   }
}