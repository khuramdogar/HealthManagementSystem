using AutoMapper;
using HMS.HealthTrack.Web.Areas.Inventory.Models.StockTakes;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers
{
   [HealthTrackAuthorize(HealthTrackPermission = ("Inventory"))]
   public class StockTakeItemsController : Controller
   {
      private readonly IStockTakeRepository _stockTakeRepository;
      private readonly IProductRepository _productRepository;
      private readonly ICustomLogger _logger;

      public StockTakeItemsController(IStockTakeRepository stockTakeRepository, IProductRepository productRepository,
         ICustomLogger logger)
      {
         _stockTakeRepository = stockTakeRepository;
         _productRepository = productRepository;
         _logger = logger;
      }

      [HttpPost]
      public ActionResult GetStockTakeItems([DataSourceRequest] DataSourceRequest request, int? stockTakeId)
      {
         _logger.Information("Attempting to get stock take items from {StockTakeId}", stockTakeId);
         try
         {
            var stockTake = stockTakeId.HasValue
               ? _stockTakeRepository.Fetch(stockTakeId.Value)
               : new StockTake { StockTakeItems = new Collection<StockTakeItem>() };
            var stockTakeItems = stockTake.StockTakeItems.Where(sti => !sti.DeletedOn.HasValue).Select(Mapper.Map<StockTakeItem, StockTakeItemViewModel>);
            return Json(stockTakeItems.ToDataSourceResult(request));
         }
         catch (Exception exception)
         {
            _logger.Error(exception, "There was an error returning Stock Take Items for {StockTakeId}", stockTakeId);
            return Json(new Collection<StockTakeItemViewModel>().ToDataSourceResult(request));
         }
      }

      [HttpPost]
      public ActionResult CreateStockTakeItem([DataSourceRequest] DataSourceRequest request, StockTakeItemViewModel model)
      {
         _logger.Information("Creating stock take item for {ProductId} as part of {StockTakeId}", model.ProductId, model.StockTakeId);
         if (model == null || !ModelState.IsValid) return Json(model);
         var stockTake = _stockTakeRepository.Fetch(model.StockTakeId);
         var stockTakeItem = Mapper.Map<StockTakeItemViewModel, StockTakeItem>(model);
         var product = _productRepository.Find(model.ProductId);

         StockTakeItemViewModel returnItem;
         if (product == null)
         {
            ModelState.AddModelError("UPN", "Product does not exist");
            returnItem = model;
            _logger.Information("Unable to create stock take item for {ProductId}", model.ProductId);
         }
         else
         {
            stockTakeItem.CreatedBy = User.Identity.Name;
            stockTakeItem.ProductId = product.ProductId;
            stockTakeItem.StockTake = stockTake;

            _stockTakeRepository.AddItem(stockTakeItem);
            _stockTakeRepository.Commit();
            var item = _stockTakeRepository.FetchItem(stockTakeItem.StockTakeItemId).SingleOrDefault();
            returnItem = Mapper.Map<StockTakeItem, StockTakeItemViewModel>(item);
         }

         return Json(new[] { returnItem }.ToDataSourceResult(request, ModelState));
      }

      [HttpPost]
      public ActionResult Update([DataSourceRequest] DataSourceRequest request, StockTakeItemViewModel stockTakeItem)
      {
         _logger.Information("Updating stock take item with {StockTakeItemId}", stockTakeItem.StockTakeItemId);

         if (stockTakeItem != null && ModelState.IsValid)
         {
            _stockTakeRepository.UpdateItem(Mapper.Map<StockTakeItemViewModel, StockTakeItem>(stockTakeItem), User.Identity.Name);
            _stockTakeRepository.Commit();
         }

         var item = _stockTakeRepository.FetchItem(stockTakeItem.StockTakeItemId).SingleOrDefault();
         var toReturn = Mapper.Map<StockTakeItem, StockTakeItemViewModel>(item);

         return Json(new[] { toReturn }.ToDataSourceResult(request, ModelState));
      }

      [HttpPost]
      public ActionResult Remove([DataSourceRequest] DataSourceRequest request, StockTakeItemViewModel stockTakeItem)
      {
         _logger.Information("Removing stock take item with {StockTakeItemId}", stockTakeItem.StockTakeItemId);
         if (stockTakeItem.StockTakeItemId > 0)
         {
            _stockTakeRepository.RemoveItem(stockTakeItem.StockTakeItemId, User.Identity.Name);
            _stockTakeRepository.Commit();
         }
         return Json(new[] { stockTakeItem }.ToDataSourceResult(request, ModelState));
      }

      [HttpPost]
      public ActionResult AddStockTakeItem(int stockTakeId, int productId, int stockLevel)
      {
         _logger.Information("Adding stock take item for {ProductId} as part of {StockTakeId}", productId, stockTakeId);
         string message;

         var stockTake = _stockTakeRepository.Fetch(stockTakeId);
         if (stockTake == null)
         {
            _logger.Warning("Unable to find stock take with ID {StockTakeId}", stockTakeId);
            message = "Unable to find stock take. Please refresh and try again.";
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return Json(message, JsonRequestBehavior.AllowGet);
         }

         if (stockTake.Status != StockTakeStatus.Created && stockTake.Status != StockTakeStatus.Failed)
         {
            _logger.Warning("Cannot add item to stock take with ID {StockTakeId} that is not in progress.", stockTakeId);
            message = "Unable to add item to stock take which is not in progress. Please create a new stock take.";
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return Json(message, JsonRequestBehavior.AllowGet);
         }

         var product = _productRepository.Find(productId);
         if (product == null)
         {
            _logger.Warning("Unable to find product with ID {ProductId}", productId);
            message = "Unable to find product. Please ensure the product exists.";
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return Json(message, JsonRequestBehavior.AllowGet);
         }

         if (stockLevel < 0)
         {
            _logger.Warning("Unable to add stock take item with stock level below zero");
            message = "Invalid stock level provided. Stock level must be 0 or greater.";
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return Json(message, JsonRequestBehavior.AllowGet);
         }

         var stockTakeItem = stockTake.StockTakeItems.SingleOrDefault(sti => sti.ProductId == productId && sti.DeletedOn == null);
         if (stockTakeItem != null)
         {
            stockTakeItem.StockLevel = stockLevel;
            _logger.Debug("Found existing stock take item within stock take for product with ID {ProductId}", productId);
            message = string.Format("Stock level for product <strong>{0}</strong> updated to <strong>{1}</strong>.",
               product.Description, stockLevel);
         }
         else
         {
            _stockTakeRepository.AddItem(stockTakeId, productId, stockLevel, User.Identity.Name);
            message = string.Format("Stock level for product <strong>{0}</strong> recorded as <strong>{1}</strong>.",
               product.Description, stockLevel);
         }

         _stockTakeRepository.Commit();
         return Json(message, JsonRequestBehavior.AllowGet);
      }
   }
}