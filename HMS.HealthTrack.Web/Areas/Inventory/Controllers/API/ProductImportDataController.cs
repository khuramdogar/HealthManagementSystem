using AutoMapper;
using Hangfire;
using HMS.HealthTrack.Web.Areas.Inventory.Models.ProductImport;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers.API
{
   [HealthTrackAuthorize(HealthTrackPermission = ("Inventory"))]
   public class ProductImportDataController : ApiController
   {
      private readonly IProductImportDataRepository _repository;
      private readonly ICustomLogger _logger;

      public ProductImportDataController(IProductImportDataRepository repository, ICustomLogger logger)
      {
         _repository = repository;
         _logger = logger;
      }

      [HttpGet]
      public DataSourceResult GetProductImportData(
         [ModelBinder(typeof(WebApiDataSourceRequestModelBinder))] DataSourceRequest request)
      {
         try
         {
            var sets = _repository.GetProductImportData();
            return
               sets.AsEnumerable().Select(Mapper.Map<ProductImportData, ProductImportDataViewModel>)
                  .ToDataSourceResult(request);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, "An error occurred while fetching product import sets");
            return new DataSourceResult();
         }
      }

      [HttpPost, Route("api/Inventory/ProductImportData/StoreProductsData")]
      public async Task<HttpResponseMessage> StoreProductsData()
      {
         try
         {
            _logger.Information("Attempting to store products data for import");
            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            var fileContents = provider.Contents.FirstOrDefault();
            if (fileContents == null)
            {
               _logger.Warning("No file uploaded for import of products.");
               return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No file uploaded");
            }

            var file = provider.Contents.First();
            var filename = file.Headers.ContentDisposition.FileName.Trim('\"');
            var productImportData = new ProductImportData { Name = filename, CreatedBy = User.Identity.Name, LastModifiedBy = User.Identity.Name };

            var buffer = await file.ReadAsByteArrayAsync();
            productImportData.ProductsData = buffer;
            _repository.AddProductImportData(productImportData);

            _repository.Commit();
            return Request.CreateResponse(HttpStatusCode.OK);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, "There was a problem importing products");
            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "There was an error processing the request");
         }
      }

      [HttpGet, Route("api/Inventory/ProductImportData/ProcessOutstanding")]
      public HttpResponseMessage ProcessOutstanding()
      {
         _logger.Information("Processing outstanding imports for product import data files");
         try
         {
            var jobToken =
               BackgroundJob.Enqueue<ProductImportProcessor>(x => x.ProcessAllUnprocessed(User.Identity.Name));
            return Request.CreateResponse(HttpStatusCode.OK, jobToken);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, "There was a problem processing product import data files");
            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "There was an error processing the request");
         }
      }

      [HttpPost, Route("api/Inventory/ProductImportData/ProcessProductImport")]
      public HttpResponseMessage ProcessProductImport(ImportData importData)
      {
         try
         {
            _logger.Information("Processing product import data files for {ProductImportDataId}", importData.ProductImportDataId);
            var productData = _repository.Find(importData.ProductImportDataId);
            if (productData == null)
            {
               _logger.Warning("Cannot find Product Import Data to process for {ProductImportDataId}",
                  importData.ProductImportDataId);
               return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Data to process does not exist");
            }

            if (productData.Status != ProductImportStatus.Uploaded)
            {
               _logger.Warning("Product Import Data {ProductImportDataId} has incorrect status of {ProductImportStatus} and cannot be processed", importData.ProductImportDataId, productData.Status);
            }

            productData.Status = ProductImportStatus.Processing;
            _repository.Commit();
            var jobToken = BackgroundJob.Enqueue<ProductImportProcessor>(x => x.Process(productData.ProductImportDataId, User.Identity.Name));
            return Request.CreateResponse(HttpStatusCode.OK, jobToken);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, "There was a problem processing product import data {ProductImportDataId}", importData.ProductImportDataId);
            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "There was an error processing the request");
         }
      }

      [HttpDelete]
      public HttpResponseMessage Delete(int id)
      {
         try
         {
            _logger.Information("Attemping to delete product import data {ProductImportDataId}", id);
            var import = _repository.Find(id);
            if (import == null)
            {
               _logger.Warning("Could not find product import data {ProductImportDataId} for deletion", id);
               return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                  "Could not find product import data for deletion");
            }
            import.DeletedBy = User.Identity.Name;
            _repository.Remove(import);
            _repository.Commit();
            return Request.CreateResponse(HttpStatusCode.OK, "Product import data successfully removed");

         }
         catch (Exception exception)
         {
            _logger.Error(exception, "There was a problem deleting the product import data for {ProductImportDataId}",
               id);
            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
               "There was an error deleting the product import data");
         }
      }

      public class ImportData
      {
         public int ProductImportDataId { get; set; }
      }
   }
}