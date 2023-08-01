using System;
using AutoMapper;
using HMS.HealthTrack.Web.Areas.Inventory.Models.Orders;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System.Linq;
using System.Web.Mvc;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Inventory.Common.Constants;
using HMS.HealthTrack.Web.Areas.Inventory.Utils;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers
{
   public class OrderItemsController : Controller
   {
      private ICustomLogger _logger;
      private IOrderUnitOfWork _unitOfWork;

      private readonly IPropertyProvider _propertyProvider;
      public OrderItemsController(IOrderUnitOfWork unitOfWork, ICustomLogger logger, IPropertyProvider propertyProvider)
      {
         _unitOfWork = unitOfWork;
         _logger = logger;
         _propertyProvider = propertyProvider;
      }

      public ActionResult Get([DataSourceRequest] DataSourceRequest request, int orderId)
      {
         var orderItems = _unitOfWork.OrderItemRepository.FindItemsForOrder(orderId);
         var models = orderItems.Select(Mapper.Map<OrderItem, OrderItemsViewModel>);
         return Json(models.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
      }

      [HttpPost]
      public JsonResult GetOpenOrderItems(DataSourceRequest request, int id, bool all = false)
      {
         try
         {
            var productLedgerType = _propertyProvider.LedgerTypes.Single(lt => lt.Name == InventoryConstants.ProductLedgerType).LedgerTypeId;
            var orderItems = _unitOfWork.OrderItemRepository.FindItemsForOrder(id);
            var orderItemModels = orderItems.Select(Mapper.Map<OrderItem, OrderItemsViewModel>).ToList();
            foreach (var model in orderItemModels.Where(model => model.LedgerId.HasValue))
            {
               model.GLC = GeneralLedgerHelper.GetGeneralLedgerCode(_unitOfWork.GeneralLedgerRepository,
                  _unitOfWork.GeneralLedgerTierRepository, _propertyProvider, model.LedgerId.Value, productLedgerType);
            }

            return Json(orderItemModels.ToDataSourceResult(request));
         }
         catch (Exception exception)
         {
            var message = $"There was a problem retrieving Order Items for the Order with ID '{id}'.";
            _logger.Error(exception, message);
            return Json(message);
         }
      }
   }
}