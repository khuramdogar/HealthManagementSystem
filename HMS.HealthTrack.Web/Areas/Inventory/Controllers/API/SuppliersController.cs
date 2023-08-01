using AutoMapper;
using HMS.HealthTrack.Web.Areas.Inventory.Models.Suppliers;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers.API
{
   [HealthTrackAuthorize(HealthTrackPermission = ("Inventory"))]
   public class SuppliersController : ApiController
   {
      private readonly ISupplierRepository _supplierRepository;
      private readonly ICustomLogger _logger;

      public SuppliersController(ISupplierRepository supplierRepository, ICustomLogger logger)
      {
         _supplierRepository = supplierRepository;
         _logger = logger;
      }

      [HttpGet]
      public DataSourceResult GetSuppliers(
         [ModelBinder(typeof(WebApiDataSourceRequestModelBinder))] DataSourceRequest request)
      {
         try
         {
            return
               _supplierRepository.FindAll().Select(Mapper.Map<Supplier, SuppliersViewModel>).OrderBy(svm => svm.Name).ToDataSourceResult(request);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem retrieving Suppliers."));
            return new DataSourceResult();
         }
      }
   }
}
