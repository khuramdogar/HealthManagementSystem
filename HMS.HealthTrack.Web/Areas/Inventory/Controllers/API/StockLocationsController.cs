

using AutoMapper;
using HMS.HealthTrack.Web.Areas.Inventory.Models.StockLocations;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers.API
{
   [HealthTrackAuthorize(HealthTrackPermission = ("Inventory"))]
   public class StockLocationsController : ApiController
   {
      private readonly IStockLocationRepository _stockLocationRepository;
      private readonly ICustomLogger _logger;

      public StockLocationsController(IStockLocationRepository stockLocationRepository, ICustomLogger logger)
      {
         _stockLocationRepository = stockLocationRepository;
         _logger = logger;
      }

      [HttpGet]
      public DataSourceResult GetLocations([ModelBinder(typeof(WebApiDataSourceRequestModelBinder))] DataSourceRequest request)
      {
         try
         {
            var locations = _stockLocationRepository.FindAllWithDeleted().Select(Mapper.Map<StockLocation, IndexLocationsDisplayModel>);
            return locations.ToDataSourceResult(request);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, "There was a problem retrieving Locations");
            return new DataSourceResult();
         }
      }

      [HttpDelete]
      public HttpResponseMessage Delete(int id)
      {
         try
         {
            _logger.Information("Attempting to delete location {LocationId}", id);
            var location = _stockLocationRepository.Find(id);
            if (location == null)
            {
               _logger.Warning("Could not find location {LocationId} for deletion", id);
               return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not find location for deletion");
            }
            location.DeletedBy = User.Identity.Name;
            _stockLocationRepository.Remove(location);
            _stockLocationRepository.Commit();
            return Request.CreateResponse(HttpStatusCode.OK, "Location successfully deleted.");
         }
         catch (Exception exception)
         {
            _logger.Error(exception, "There was an error deleting location {LocationId}", id);
            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
               "There was an error attempting to delete the location");
         }
      }
   }
}