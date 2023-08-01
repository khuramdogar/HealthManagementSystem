using AutoMapper;
using HMS.HealthTrack.Web.Areas.Inventory.Models.Stock;
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
using HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers.API
{
   [HealthTrackAuthorize(HealthTrackPermission = ("Inventory"))]
   public class StockController : ApiController
   {
      private readonly ICustomLogger _logger;
      private readonly IStockUnitOfWork _unitOfWork;

      public StockController(IStockUnitOfWork unitOfWork, ICustomLogger logger)
      {
         _logger = logger;
         _unitOfWork = unitOfWork;
      }

      [HttpGet, Route("api/inventory/stock/GetStockByProductPerLocation")]
      public DataSourceResult GetStockByProductPerLocation(
         [ModelBinder(typeof(WebApiDataSourceRequestModelBinder))] DataSourceRequest request, int productId)
      {
         var productStock =
            _unitOfWork.StockRepository.FindAll()
               .Where(s => s.ProductId == productId && s.Quantity > 0)
               .GroupBy(s => s.StorageLocation)
               .Select(gs => new StockAtLocationModel
               {
                  Location = gs.Key.Name,
                  LocationId = gs.Key.LocationId,
                  Quantity = gs.Sum(s => s.Quantity)
               });
         return productStock.ToDataSourceResult(request);
      }

      [HttpPost, Route("api/inventory/stock/ReallocateStock")]
      public HttpResponseMessage ReallocateStock(ReallocateStockModel reallocateStockModel)
      {
         _logger.Information("Reallocating {Quantity} stock for {StockId} to {TargetLocation}",
               reallocateStockModel.Quantity, reallocateStockModel.StockId, reallocateStockModel.CurrentLocation,
               reallocateStockModel.TargetLocation);

         try
         {
            var targetLocation = _unitOfWork.StockLocationRepository.Find(reallocateStockModel.TargetLocation);
            if (targetLocation == null)
            {
               _logger.Information("Cannot reallocate stock to location {Location} that does not exist");
               return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Location does not exist");
            }

            var existingStock = _unitOfWork.StockRepository.Find(reallocateStockModel.StockId);
            if (existingStock == null)
            {
               _logger.Information("Stock for {StockId} does not exist", reallocateStockModel.StockId);
               return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Stock does not exist.");
            }

            if (existingStock.StockStatus != StockStatus.Available && existingStock.StockStatus != StockStatus.Reserved)
            {
               _logger.Information("Stock {StockId} not reallocatable - stock not available or reserved", existingStock.StockId);
               return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                  "Can only reallocate Available or Reserved stock");
            }
            if (existingStock.DeletedOn.HasValue)
            {
               _logger.Information("Cannot reallocate deleted stock {StockId}", reallocateStockModel.StockId);
               return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Cannot reallocate deleted stock");
            }

            if (existingStock.Quantity < 1)
            {
               _logger.Information("Cannot reallocate stock {StockId} with quantity {Quantity}", reallocateStockModel.StockId, existingStock.Quantity);
               return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                  "Unable to reallocate stock with quantity below 1.");
            }

            // Reallocate the stock
            var result = _unitOfWork.ReallocateStock(existingStock, targetLocation,
               reallocateStockModel.Quantity, User.Identity.Name);

            if (!result)
            {
               return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Unable to reallocate stock");
            }

            _unitOfWork.Commit();
            return Request.CreateResponse(HttpStatusCode.OK);

         }
         catch (Exception exception)
         {
            _logger.Error(exception, "Could not reallocate stock for {StockId}");
            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "The server encountered an error while reallocating the specified stock");
         }
      }

      [HttpPost, Route("api/Inventory/Stock/ReallocateAllStock")]
      public HttpResponseMessage ReallocateAllStock(ReallocateStockModel reallocateStockModel)
      {
         _logger.Information("Reallocating all stock from {CurrentLocation} to {TargetLocation}", reallocateStockModel.CurrentLocation, reallocateStockModel.TargetLocation);

         try
         {
            var location = _unitOfWork.StockLocationRepository.Find(reallocateStockModel.TargetLocation);
            if (location == null)
            {
               _logger.Information("Cannot reallocate stock to location {Location} that does not exist", reallocateStockModel.TargetLocation);
               return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Location does not exist");
            }

            var success = _unitOfWork.ReallocateAllStock(reallocateStockModel.CurrentLocation, location, User.Identity.Name);
            if (!success)
               return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                  string.Format("Could not reallocate stock for location {0}", location.Name));

            _unitOfWork.Commit();
            return Request.CreateResponse(HttpStatusCode.OK);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, "Could not reallocate all stock from {CurrentLocation} to {TargetLocation}", reallocateStockModel.CurrentLocation, reallocateStockModel.TargetLocation);
            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
               "The server encountered an error while reallocationg stock");
         }
      }

      [HttpPost, Route("api/Inventory/Stock/ReallocateMultipleStock")]
      public HttpResponseMessage ReallocateMultipleStock(ReallocateMultipleStocksModel model)
      {
         _logger.Information("Reallocating {NumberToReallocate} stock items to {TargetLocation}", model.StockIds.Length, model.TargetLocation);

         try
         {
            if (model.StockIds.Length < 1)
            {
               _logger.Information("No stock items specified for reallocation");
               return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No stock items specified for reallocation");
            }

            var targetLocation = _unitOfWork.StockLocationRepository.Find(model.TargetLocation);
            if (targetLocation == null)
            {
               _logger.Information("Cannot reallocate stock to location {Location} that does not exist");
               return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Location does not exist");
            }

            var success = true;
            foreach (var stockId in model.StockIds)
            {
               var existingStock = _unitOfWork.StockRepository.Find(stockId);
               if (existingStock == null)
               {
                  _logger.Information("Stock for {StockId} does not exist", stockId);
                  return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Stock does not exist.");
               }
               if (!_unitOfWork.ReallocateStock(existingStock, targetLocation, null, User.Identity.Name))
               {
                  success = false;
               }
            }

            if (!success)
               return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Failed to reallocate stocks");

            _unitOfWork.Commit();
            return Request.CreateResponse(HttpStatusCode.OK);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, "Could not reallocate stocks to {TargetLocation}", model.TargetLocation);
            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "The server encountered an error while reallocationg stock");
         }
      }
   }

   public class ReallocateMultipleStocksModel
   {
      public int[] StockIds { get; set; }
      public int TargetLocation { get; set; }
   }

   public class ReallocateStockModel
   {
      public int StockId { get; set; }
      public int CurrentLocation { get; set; }
      public int TargetLocation { get; set; }
      public int Quantity { get; set; }
   }
}