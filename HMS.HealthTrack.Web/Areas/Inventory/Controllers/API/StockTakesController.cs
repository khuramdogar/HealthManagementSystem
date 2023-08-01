using HMS.HealthTrack.Web.Areas.Inventory.Models.StockTakes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers.API
{
   [HealthTrackAuthorize(HealthTrackPermission = ("Inventory"))]
   public class StockTakesController : ApiController
   {
      private readonly IStockTakeUnitOfWork _unitOfWork;
      private readonly IPropertyProvider _propertyProvider;
      private readonly ICustomLogger _logger;

      public StockTakesController(IStockTakeUnitOfWork unitOfWork, IPropertyProvider propertyProvider, ICustomLogger logger)
      {
         _unitOfWork = unitOfWork;
         _propertyProvider = propertyProvider;
         _logger = logger;
      }

      [HttpPost]
      public HttpResponseMessage PostSubmit(StockTakeViewModel viewModel)
      {
         //Process stock take
         try
         {
            _logger.Information("Attempting to adjust stock for {StockTakeId}", viewModel.StockTakeId);
            var result = _unitOfWork.ProcessStockTake(viewModel.StockTakeId, User.Identity.Name);
            return result ? Request.CreateResponse(HttpStatusCode.OK, "Stock successfully adjusted.") : Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Unable to process stock adjustment");
         }
         catch (Exception exception)
         {
            _logger.Error(exception, "Failed to process stock take {StockTakeId}", viewModel.StockTakeId);
            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
               "There was an error attempting to process stock take.");
         }
      }


      [HttpPost, Route("api/Inventory/StockTakes/CreateAndSubmit")]
      public HttpResponseMessage CreateAndSubmit(IncomingStockTake dto)
      {
         // Create and process stock take
         var stockTakeItems = new List<StockTakeItem>();

         try
         {
            foreach (var incomingStockTakeItem in dto.Items)
         {
            var item = new StockTakeItem
            {
               CreatedBy = dto.CreatedBy,
               Message = incomingStockTakeItem.Message,
               Status = StockTakeItemStatus.Pending,
               CreatedOn = DateTime.Now,
               ModifiedOn = DateTime.Now,
               StockLevel = incomingStockTakeItem.StockLevel,
            };

            if (incomingStockTakeItem.ProductId.HasValue)
               item.ProductId = incomingStockTakeItem.ProductId.Value;
            else
            {
               var products = _unitOfWork.ProductRepository.FindByCode(new ProductSearchCriteria { SPC = incomingStockTakeItem.SPC });
               int productCount = products.Count();
               switch (productCount)
               {
                  case 1:
                     item.ProductId = products.Single().ProductId;
                     break;
                  case 0:
                     throw new ArgumentException("Product not found");
                  default:
                     throw new ArgumentException("Multiple products matched");
               }
            }
            stockTakeItems.Add(item);
         }

         var stockTake = new StockTake
         {
            LocationId = dto.LocationId,
            StockTakeDate = dto.StockTakeDate,
            CreatedBy = dto.CreatedBy,
            StockTakeItems = stockTakeItems,
         };
         
            _unitOfWork.StockTakeRepository.Add(stockTake);
            _unitOfWork.ProcessStockTake(stockTake);
            _unitOfWork.Commit();

            var result = new StockTakeSubmissionResult { Message = "Stock take complete" };
            return Request.CreateResponse(HttpStatusCode.OK, result);

         }
         catch (Exception exception)
         {
            var error = new StockTakeSubmissionResult {Message = exception.Message};
            return Request.CreateResponse(HttpStatusCode.BadRequest, error, new System.Net.Http.Headers.MediaTypeHeaderValue("application/json"));
         }
      }


      [HttpPost, Route("api/Inventory/StockTakes/SubmitStockTakeForProduct")]
      public HttpResponseMessage SubmitStockTakeForProduct(SingleProductStockTakeDTO dto)
      {
         if (dto.StockTakeProductId < 1)
         {
            _logger.Warning("Attempted to submit new stock take for a product without specifying the ProductId");
            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No product selected.");
         }

         if (dto.StockTakeLocationId < 1)
         {
            _logger.Warning("Attempted to submit new stock take for a product without specifying the LocationId");
            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No location selected.");
         }

         if (dto.StockLevel < 0)
         {
            _logger.Warning("Attempted to submit new stock take for a product with an invalid stock level {StockLevel}", dto.StockLevel);
            return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
               "Stock level must be zero or greater.");
         }

         var negativeStock =
            _unitOfWork.StockRepository.GetNegativeStock().SingleOrDefault(ns => ns.ProductId == dto.StockTakeProductId && ns.StoredAt == dto.StockTakeLocationId);
         if (negativeStock == null)
         {
            _logger.Warning(
               "Attempted to create stock take for Product {ProductId} at Location {LocationId} for which the stock level is no longer negative. No stock take performed.");
            return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
               "Unable to create stock take as product's stock level at the specified location is no longer negative. Please refresh and try again.");
         }

         // Create and process stock take
         var stockTake = _unitOfWork.StockTakeRepository.CreateSingleProductStockTake(dto.StockTakeProductId, dto.StockTakeLocationId, dto.StockLevel,
            DateTime.Now, User.Identity.Name,
            string.Format("Correcting stock level of -{0}", negativeStock.NegativeQuantity));
         _unitOfWork.ProcessStockTake(stockTake);
         _unitOfWork.Commit();

         return Request.CreateResponse(HttpStatusCode.OK, stockTake.StockTakeId);
      }
   }
}