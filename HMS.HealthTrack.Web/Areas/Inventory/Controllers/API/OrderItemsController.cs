using AutoMapper;
using HMS.HealthTrack.Web.Areas.Inventory.Models.Orders;
using HMS.HealthTrack.Web.Areas.Inventory.Utils;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers.API
{
   [HealthTrackAuthorize(HealthTrackPermission = ("Inventory"))]
   public class OrderItemsController : ApiController
   {
      private readonly IOrderUnitOfWork _unitOfWork;
      private readonly ICustomLogger _logger;

      public OrderItemsController(IOrderUnitOfWork unitOfWork, ICustomLogger logger, IPropertyProvider propertyProvider)
      {
         _unitOfWork = unitOfWork;
         _logger = logger;
      }
      
     

      [HttpGet, Route("api/inventory/OrderItems/CancelOrderItem")]
      public HttpResponseMessage CancelOrderItem(int id)
      {
         try
         {
            var orderItem = _unitOfWork.OrderItemRepository.Find(id);
            if (orderItem == null)
               return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Could not find Order Item.");

            _unitOfWork.OrderItemRepository.Cancel(orderItem);
            OrderRepository.UpdateOrderStatus(orderItem.Order, User.Identity.Name);

            _unitOfWork.Commit();
            return Request.CreateResponse(HttpStatusCode.OK, orderItem.Order.Status);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem retrieving Order Items for the Order with ID '{0}'.", id));
            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exception);
         }
      }
   }
}