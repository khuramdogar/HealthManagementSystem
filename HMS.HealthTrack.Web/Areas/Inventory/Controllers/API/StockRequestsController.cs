using AutoMapper;
using HMS.HealthTrack.Web.Areas.Inventory.Models.StockRequests;
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
using HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers.API
{
   [HealthTrackAuthorize(HealthTrackPermission = ("Inventory"))]
   public class StockRequestsController : ApiController
   {
      private readonly IStockUnitOfWork _unitOfWork;
      private readonly IStockRequestRepository _stockRequestRepo;
      private readonly ICustomLogger _logger;

      public StockRequestsController(IStockUnitOfWork unitOfWork, ICustomLogger logger)
      {
         _stockRequestRepo = unitOfWork.StockRequestRepository;
         _logger = logger;
         _unitOfWork = unitOfWork;
      }

      [HttpGet]
      public DataSourceResult GetStockRequests([ModelBinder(typeof(WebApiDataSourceRequestModelBinder))] DataSourceRequest request, bool onlyOpen = false)
      {
         try
         {
            var stockRequests = onlyOpen ? _stockRequestRepo.FindOpenRequests() : _stockRequestRepo.FindCurrentRequests();
            return
               stockRequests.Select(Mapper.Map<ProductStockRequest, IndexProductStockRequestViewModel>)
                  .ToDataSourceResult(request);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, "There was a problem retrieving Stock Requests.");
            return new DataSourceResult();
         }
      }

      [HttpPost]
      public IHttpActionResult ApproveRequest(ApproveRequestDTO approveRequestDto)
      {
         try
         {
            var existingRequest = _stockRequestRepo.Find(approveRequestDto.StockRequestId);
            if (existingRequest == null)
            {
               return BadRequest();
            }
            existingRequest.LastModifiedBy = User.Identity.Name;
            existingRequest.LastModifiedOn = DateTime.Now;
            existingRequest.ApprovedQuantity = approveRequestDto.ApprovedQuantity;
            existingRequest.RequestStatus = RequestStatus.Approved;
            _stockRequestRepo.Commit();
            return Ok();
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem approving a Stock Request with a Stock Request ID '{0}' and an Approved Quantity of '{1}'.", approveRequestDto.StockRequestId, approveRequestDto.ApprovedQuantity));
            return InternalServerError(exception);
         }
      }

      [HttpPost, Route("api/StockRequests/CreateStockRequestFromStockSet")]
      public IHttpActionResult CreateStockRequestFromStockSet(CreateStockRequestFromStockSetDTO createRequestFromStockSetDto)
      {
         try
         {
            var stockSet = _unitOfWork.StockSetRepository.Find(createRequestFromStockSetDto.StockSetId);
            if (stockSet == null)
            {
               _logger.Warning("Attempting to create request from non-existant stock set");
               return BadRequest("Cannot create request. Stock set no longer exists.");
            }

            if (stockSet.Items.Count == 0)
            {
               _logger.Warning("Attempting to request empty stock set {StockSetId}", createRequestFromStockSetDto.StockSetId);
               return BadRequest("Cannot create request from an empty stock set.");
            }

            if (_unitOfWork.ProductRepository.ContainsUnorderableProducts(stockSet.Items.Select(ssi => ssi.ProductId)))
            {
               _logger.Warning("Attempting to request stock set containing unorderable products {StockSetId}", createRequestFromStockSetDto.StockSetId);
               return BadRequest("Cannot create request for stock set containing unorderable products");
            }

            _unitOfWork.CreateRequestsForStockSet(stockSet, createRequestFromStockSetDto.Location, createRequestFromStockSetDto.Urgent, User.Identity.Name);
            _unitOfWork.Commit();
            return Ok(stockSet.Name);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem creating a '{0}' Stock Request with a Stock Set ID of '{1}' for the Location '{2}'.", createRequestFromStockSetDto.Urgent ? "urgent" : "non-urgent", createRequestFromStockSetDto.StockSetId, createRequestFromStockSetDto.Location));
            return InternalServerError(exception);
         }
      }

      [HttpPost, Route("api/StockRequests/CreateStockRequest")]
      public HttpResponseMessage CreateStockRequest(CreateStockRequestDTO createRequestDto)
      {
         try
         {
            _logger.Information("Attempting to create request for {Quantity} of {ProductId} at {Location}", createRequestDto.RequestedQuantity, createRequestDto.ProductId, createRequestDto.LocationId);
            var product = _unitOfWork.ProductRepository.Find(createRequestDto.ProductId);
            if (product == null)
            {
               _logger.Information("Cannot find product {ProductId}", createRequestDto.ProductId);
               return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not find product for request.");
            }

            if (product.Unorderable)
            {
               _logger.Information("Cannot create request for {ProductId} marked unorderable", createRequestDto.ProductId);
               return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                  "Cannot create request for unorderable product");
            }

            if (createRequestDto.RequestedQuantity < 1)
            {
               _logger.Information("Cannot create request for {ProductId} with invalid quantity {Quantity}", createRequestDto.ProductId, createRequestDto.RequestedQuantity);
               return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                  "Cannot create request with invalid quantity");
            }

            var request = _stockRequestRepo.CreateRequest(product.ProductId, createRequestDto.RequestedQuantity, createRequestDto.LocationId, createRequestDto.Urgent, User.Identity.Name);
            _unitOfWork.Commit();
            return Request.CreateResponse(HttpStatusCode.OK, request.StockRequestId);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem creating a '{0}' Stock Request for {1} of Product ID '{2}' for the Location '{3}'.", createRequestDto.Urgent ? "urgent" : "non-urgent", createRequestDto.RequestedQuantity, createRequestDto.ProductId, createRequestDto.LocationId));
            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exception);
         }
      }

      [HttpDelete]
      public HttpResponseMessage Delete(int id)
      {
         try
         {
            _logger.Information("Attempting to delete stock request {StockRequestId}", id);
            var stockRequest = _stockRequestRepo.Find(id);
            if (stockRequest == null)
            {
               _logger.Warning("Could not find stock request {StockRequestId} for deletion", id);
               return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Could not find stock request for deletion");
            }
            _stockRequestRepo.Remove(stockRequest);
            _stockRequestRepo.Commit();
            return Request.CreateResponse(HttpStatusCode.OK, "Stock request successfully deleted.");
         }
         catch (Exception exception)
         {
            _logger.Error(exception, "There was an error deleting stock request {StockRequestId}", id);
            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
               "There was an error attempting to delete the stock request");
         }
      }
   }

   public class CreateStockRequestDTO
   {
      public int ProductId { get; set; }
      public int RequestedQuantity { get; set; }
      public int LocationId { get; set; }
      public bool Urgent { get; set; }
   }

   public class ApproveRequestDTO
   {
      public int StockRequestId { get; set; }
      public int ApprovedQuantity { get; set; }
   }
}