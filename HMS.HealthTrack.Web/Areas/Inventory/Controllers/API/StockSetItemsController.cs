using AutoMapper;
using HMS.HealthTrack.Web.Areas.Inventory.Models.StockSets;
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
   public class StockSetItemsController : ApiController
   {
      private readonly IStockSetItemRepository _stockSetItemRepository;
      private readonly IProductRepository _productRepository;
      private readonly ICustomLogger _logger;

      public StockSetItemsController(IStockSetItemRepository stockSetItemRepository, IProductRepository productRepository, ICustomLogger logger)
      {
         _stockSetItemRepository = stockSetItemRepository;
         _logger = logger;
         _productRepository = productRepository;
      }

      [HttpGet]
      public DataSourceResult GetStockSetItems(
         [ModelBinder(typeof(WebApiDataSourceRequestModelBinder))] DataSourceRequest request, int stockSetId)
      {
         try
         {
            var stockSetItems = _stockSetItemRepository.FindAll(stockSetId).ToList().Select(Mapper.Map<StockSetItem, StockSetItemModel>);
            return stockSetItems.ToDataSourceResult(request);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem retrieving Stock Set Items with Stock Set ID '{0}'.", stockSetId));
            return new DataSourceResult();
         }
      }

      // POST api/Products
      public HttpResponseMessage Post(StockSetItemModel model)
      {
         try
         {
            if (ModelState.IsValid)
            {
               var item = new StockSetItem
               {
                  ProductId = model.ProductId.Value,
                  Quantity = model.Quantity,
                  StockSetId = model.StockSetId
               };

               var product = _productRepository.Find(model.ProductId.Value);
               if (product.Unorderable)
                  return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                     "Cannot add unorderable product to a stock set.");

               _stockSetItemRepository.Add(item);

               _stockSetItemRepository.Commit();

               model.StockSetItemId = item.StockSetItemId;

               var result = new DataSourceResult
               {
                  Data = new[] { model },
                  Total = 1
               };
               var response = Request.CreateResponse(HttpStatusCode.Created, result);
               response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = item.StockSetItemId }));
               return response;
            }
            else
            {
               return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem creating a Stock Set Item for Product ID '{0}' with a Quantity of '{1}'.", model.ProductId, model.Quantity));
            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exception);
         }
      }

      // PUT
      public HttpResponseMessage PutStockSetItem(int id, StockSetItemModel model)
      {
         try
         {
            if (!ModelState.IsValid)
            {
               return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            if (id != model.StockSetItemId || !model.ProductId.HasValue)
            {
               return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            var item = _stockSetItemRepository.Find(id);
            if (item == null)
            {
               return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            _stockSetItemRepository.Update(item, model.ProductId.Value, model.Quantity);
            _stockSetItemRepository.Commit();

            return Request.CreateResponse(HttpStatusCode.OK);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem updating the Stock Set Item with ID '{0}'.", id));
            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exception);
         }
      }

      // DELETE api/Products/5
      public HttpResponseMessage Delete(int id)
      {
         try
         {
            var item = _stockSetItemRepository.Find(id);
            if (item == null)
            {
               return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            _stockSetItemRepository.Remove(item);
            _stockSetItemRepository.Commit();

            return Request.CreateResponse(HttpStatusCode.OK);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem deleting the Stock Set Item with ID '{0}'.", id));
            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exception);
         }
      }

   }
}
