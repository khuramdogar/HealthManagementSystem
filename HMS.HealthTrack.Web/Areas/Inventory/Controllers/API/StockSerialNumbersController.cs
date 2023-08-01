using HMS.HealthTrack.Web.Areas.Inventory.Models.Stock;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using AutoMapper;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers.API
{
   [HealthTrackAuthorize(HealthTrackPermission = ("Inventory"))]
   public class StockSerialNumbersController : ApiController
   {
      private readonly IStockRepository _stockRepository;
      private readonly IStockUnitOfWork _unitOfWork;
      private readonly ICustomLogger _logger;

      public StockSerialNumbersController(IStockRepository stockRepository, IStockUnitOfWork unitOfWork, ICustomLogger logger)
      {
         _stockRepository = stockRepository;
         _unitOfWork = unitOfWork;
         _logger = logger;
      }

      public DataSourceResult GetSerialNumberStock(
         [ModelBinder(typeof(WebApiDataSourceRequestModelBinder))] DataSourceRequest request, int productId,
         int locationId)
      {
         var stock =
            _unitOfWork.StockRepository.GetSerialNumberStock(productId, locationId)
               .Select(Mapper.Map<Stock, SerialNumberStock>);
         return stock.ToDataSourceResult(request);
      }

      [HttpPut]
      public HttpResponseMessage PutProduct(SerialNumberStock model)
      {
         try
         {
            _logger.Information("Attempting to update Stock {StockId} with {SerialNumber}", model.StockId, model.SerialNumber);

            if (!ModelState.IsValid)
            {
               _logger.Warning("Serial number update failed. Invalid model state for {StockId} with {SerialNumber}", model.StockId, model.SerialNumber);
               return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            var stockItem = _stockRepository.Find(model.StockId);
            if (stockItem == null)
            {
               _logger.Warning("Serial number update failed. Stock {StockId} does not exist", model.StockId);
               return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Unable to find stock to update.");
            }

            if (stockItem.StockStatus != StockStatus.Available || stockItem.DeletedOn != null || stockItem.Quantity != 1)
            {
               _logger.Warning("Serial nubmer update failed. Stock {StockId} is not in a valid state for updating", model.StockId);
               return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Stock item cannot be updated.");
            }

            var success = _stockRepository.UpdateSerialNumber(stockItem, model.SerialNumber, User.Identity.Name);
            if (!success)
            {
               _logger.Warning("Cannot update serial number for {ProductId} to {SerialNumber}", stockItem.ProductId, stockItem.SerialNumber);
               return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Stock with this serial number already exists.");
            }
            _stockRepository.Commit();

            return Request.CreateResponse(HttpStatusCode.OK);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, "There was a problem updating the serial number of {StockId} to {NewSerialNumber}'.", model.StockId, model.SerialNumber);
            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exception);
         }

      }
   }
}
