using AutoMapper;
using Hangfire;
using HMS.HealthTrack.Web.Areas.Inventory.Models.ProductImport;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Helpers;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers.API
{
   [HealthTrackAuthorize(HealthTrackPermission = ("Inventory"))]
   public class ProductImportController : ApiController
   {
      private readonly ICustomLogger _logger;
      private readonly IProductImportUnitOfWork _unitOfWork;
      private readonly IPropertyProvider _propertyProvider;

      public ProductImportController(IProductImportUnitOfWork unitOfWork, ICustomLogger logger, IPropertyProvider propertyProvider)
      {
         _logger = logger;
         _propertyProvider = propertyProvider;
         _unitOfWork = unitOfWork;
      }

      [HttpGet]
      public DataSourceResult GetProductsFromSet(
         [ModelBinder(typeof(WebApiDataSourceRequestModelBinder))] DataSourceRequest request, int id)
      {
         try
         {
            var productImports = _unitOfWork.ProductImportRepository.GetProductsForImport(id);
            return
               productImports.AsEnumerable()
                  .Select(Mapper.Map<ProductImport, ProductImportModel>)
                  .ToDataSourceResult(request);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, "An error occurred while fetching product imports for {ProductImportDataId}", id);
            return new DataSourceResult();
         }
      }

      [HttpPut]
      public HttpResponseMessage PutProductImport(int id, ProductImportModel model)
      {
         try
         {
            _logger.Information("Attemping to update product for import {ProductImportId}", id);
            if (!ModelState.IsValid)
            {
               _logger.Warning("Update product for import {ProductImportId} failed. Invalid model state.", id);
               return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            ProductImport existing = _unitOfWork.ProductImportRepository.Find(id);
            if (existing == null)
            {
               _logger.Warning("Update product for import {ProductImportId} failed. Product for import does not exist.", id);
               return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Product for import does not exist");
            }

            var product = new ProductImport();
            Mapper.Map(model, product);
            var productImportHelper = new ProductImportHelper(_unitOfWork, _propertyProvider);
            product.Invalid = ProductImport.IsInvalid(product, productImportHelper, _logger);

            _unitOfWork.ProductImportRepository.Update(existing, product);
            _unitOfWork.Commit();

            _unitOfWork.ProductImportDataRepository.UpdateStatus(model.ProductImportDataId);
            _unitOfWork.Commit();

            var result = new DataSourceResult
            {
               Data = new[] { product },
               Total = 1
            };

            var response = Request.CreateResponse(HttpStatusCode.OK, result);
            response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = id }));
            return response;
         }
         catch (Exception exception)
         {
            _logger.Error(exception, "An error occurred while trying to update the product for import {ProductImportId}",
               id);
            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exception);
         }
      }

      [HttpPost]
      public HttpResponseMessage PostImportProducts(Import import)
      {
         try
         {
            _logger.Information("Attempting to import products for {ProductImportDataId}", import.ImportId);
            var productData = _unitOfWork.ProductImportDataRepository.Find(import.ImportId);
            if (productData == null)
            {
               _logger.Warning("Product data does not exist {ProductImportDataId}", import.ImportId);
               return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Import specified does not exist");
            }

            if (productData.Status == ProductImportStatus.Error)
            {
               _logger.Information("Cannot import data for {ProductImportDataId} with products in and invalid state", import.ImportId);
               return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                  "Cannot import products with one or more in an invalid state. Please correct all invalid products.");
            }

            if (productData.Status == ProductImportStatus.Complete)
            {
               _logger.Information("Cannot import products from {ProductImportDataId}. Nothing to import.", import.ImportId);
               return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Import has already been run.");
            }

            if (productData.Status == ProductImportStatus.Processing)
            {
               _logger.Information("Cannot import products from {ProductImportDataId}. Product data has not yet been processed.", import.ImportId);
               return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Product data has not yet been processed or is still being processed.");
            }

            // set product import to Processing
            productData.Status = ProductImportStatus.Processing;
            _unitOfWork.Commit();

            var jobToken =
               BackgroundJob.Enqueue<ProductImportProcessor>(x => x.ImportProducts(import.ImportId, User.Identity.Name));
            return Request.CreateResponse(HttpStatusCode.OK, jobToken);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, "An error occurred while trying to import products from {ProductImportDataId}",
               import.ImportId);
            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exception);
         }
      }

      [HttpDelete]
      public HttpResponseMessage DeleteImportProduct(int id)
      {
         try
         {
            var productImport = _unitOfWork.ProductImportRepository.Find(id);
            if (productImport == null)
            {
               return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Product to import does not exist.");
            }
            _unitOfWork.ProductImportRepository.Remove(productImport);
            _unitOfWork.Commit();
            return Request.CreateResponse(HttpStatusCode.OK);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, "An error occurred while trying to delete the product to import {ProductImportId}", id);
            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exception);
         }
      }

      public class Import
      {
         public int ImportId { get; set; }
      }
   }
}