using HMS.HealthTrack.Web.Areas.Inventory.Models.Dashboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Inventory.Common.Constants;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers
{
   [HealthTrackAuthorize(HealthTrackPermission = ("Inventory"))]
   public class InventoryController : Controller
   {
      private readonly ISystemNotificationsUnitOfWork _notificationsUnitOfWork;
      private readonly ICustomLogger _logger;

      public InventoryController(ISystemNotificationsUnitOfWork notificationsUnitOfWork, ICustomLogger logger)
      {
         _notificationsUnitOfWork = notificationsUnitOfWork;
         _logger = logger;
      }

      public ActionResult Index()
      {
         return View();
      }

      //Gets the notificaiton including the item count for a specific notification
      [HttpGet]
      public JsonResult GetNotification(string id)
      {
         var notification = _notificationsUnitOfWork.GetDashboardNotification(id);
         var model = AutoMapper.Mapper.Map<DashboardNotification, DashboardNotificationView>(notification);
         AddNotificationWebElements(model);
         return Json(model,JsonRequestBehavior.AllowGet);
      }

      //Gets available notifications without their item counts
      [HttpPost]
      public JsonResult GetNotifications([DataSourceRequest] DataSourceRequest request)
      {
         try
         {
            var notifications = GetAvailableDashboardNotifications();
            return Json(notifications.ToDataSourceResult(request));
         }
         catch (Exception exception)
         {
            _logger.Error(exception, "There was a problem retrieving Inventory information.");
            return Json("There was a problem retrieving Inventory information.");
         }
      }
      
      private IEnumerable<DashboardNotificationView> GetAvailableDashboardNotifications()
      {
         var notifications = _notificationsUnitOfWork.GetAvailableDashboardNotifications().Select(AutoMapper.Mapper.Map<DashboardNotification, DashboardNotificationView>).ToList();

         foreach (var notification in notifications)
         {
            AddNotificationWebElements(notification);
         }

         return notifications;
      }

      private void AddNotificationWebElements(DashboardNotificationView notification)
      {
         notification.Icon = Url.Content("~/content/images/Icons/72x72/" + notification.Icon);
         switch (notification.DashboardNotificationId)
         {
            case InventoryConstants.OrderItems:
               notification.Link = Url.Action("Approve", "Orders");
               break;
            case InventoryConstants.ConsumptionNotificationProcessingErrors:
               notification.Link = Url.Action("Errors", "Consumptions");
               break;
            case InventoryConstants.UnclassifiedProducts:
               notification.Link = Url.Action("Unclassified", "Products");
               break;
            case InventoryConstants.MissingPaymentClass:
               notification.Link = Url.Action("RequirePaymentClass", "StockAdjustments");
               break;
            case InventoryConstants.UnmappedPaymentClasses:
               notification.Link = Url.Action("Index", "PaymentClassMapping");
               break;
            case InventoryConstants.NegativeStock:
               notification.Link = Url.Action("NegativeStock", "Stock");
               break;
            case InventoryConstants.PendingConsumedProducts:
               notification.Link = Url.Action("PendingConsumedProducts", "Products");
               break;
            case InventoryConstants.ProductsInError:
               notification.Link = Url.Action("InError", "Products");
               break;
         }
      }
   }
}