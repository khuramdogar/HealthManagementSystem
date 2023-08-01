using AutoMapper;
using HMS.HealthTrack.Web.Areas.Inventory.Models.StockSets;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Data.Entity.Infrastructure;
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
   public class StockSetsController : ApiController
   {
      private readonly IStockSetRepository _stockSetRepository;
      private readonly ICustomLogger _logger;

      public StockSetsController(IStockSetRepository stockSetRepository, ICustomLogger logger)
      {
         _stockSetRepository = stockSetRepository;
         _logger = logger;
      }

      [HttpGet]
      public DataSourceResult GetStockSets(
         [ModelBinder(typeof(WebApiDataSourceRequestModelBinder))] DataSourceRequest request)
      {
         try
         {
            var stockSets = _stockSetRepository.FindAll().AsEnumerable().Select(Mapper.Map<StockSet, StockSetModel>);
            return stockSets.ToDataSourceResult(request);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem retrieving Stock Sets."));
            return new DataSourceResult();
         }
      }

      // DELETE api/StockSets/5
      public HttpResponseMessage DeleteStockSet(int id)
      {
         try
         {
            var stockConsumption = _stockSetRepository.Find(id);
            if (stockConsumption == null)
            {
               return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            _stockSetRepository.Remove(stockConsumption, User.Identity.Name);

            try
            {
               _stockSetRepository.Commit();
            }
            catch (DbUpdateConcurrencyException ex)
            {
               return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }
            return Request.CreateResponse(HttpStatusCode.OK);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem deleting Stock Set with ID '{0}'.", id));
            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exception);
         }
      }
   }
}
