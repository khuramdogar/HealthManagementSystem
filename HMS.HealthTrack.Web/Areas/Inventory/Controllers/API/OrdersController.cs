using AutoMapper;
using HMS.HealthTrack.Web.Areas.Inventory.Models.Orders;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers.API
{
   [HealthTrackAuthorize(HealthTrackPermission = ("Inventory"))]
   public class OrdersController : ApiController
   {
      private readonly IOrderUnitOfWork _unitOfWork;
      private readonly ICustomLogger _logger;

      public OrdersController(IOrderUnitOfWork unitOfWork, ICustomLogger logger)
      {
         _unitOfWork = unitOfWork;
         _logger = logger;
      }
      
      [HttpGet]
      public DataSourceResult GetOrders([ModelBinder(typeof(WebApiDataSourceRequestModelBinder))] DataSourceRequest request, bool receivable = false)
      {
         try
         {
            var orders = _unitOfWork.OrderRepository.FindAll();
            if (receivable)
            {
               orders = orders.Where(xx => xx.Status == OrderStatus.Ordered || xx.Status == OrderStatus.PartiallyReceived);
            }
            return orders.ToList().Select(Mapper.Map<Order, IndexOrdersViewModel>).ToDataSourceResult(request);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, $"There was a problem retrieving Orders with receiveable set to '{receivable}'.");
            return new DataSourceResult();
         }
      }
   }
}