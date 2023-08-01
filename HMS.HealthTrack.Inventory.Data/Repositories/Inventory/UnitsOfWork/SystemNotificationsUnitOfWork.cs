using System.Collections.Generic;
using System.Linq;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Inventory.Common.Constants;
using HMS.HealthTrack.Web.Data.Model;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork
{
   public interface ISystemNotificationsUnitOfWork
   {
      IEnumerable<DashboardNotification> GetAvailableDashboardNotifications();
      DashboardNotification GetDashboardNotification(string notificationId);
   }

   public class SystemNotificationsUnitOfWork : ISystemNotificationsUnitOfWork
   {
      private readonly IDashboardNotificationRepository _dashboardNotificationRepository;
      private readonly ICustomLogger _logger;
      private readonly IPropertyProvider _propertyProvider;

      public SystemNotificationsUnitOfWork(IDbContextInventoryContext context, IPropertyProvider propertyProvider, ICustomLogger logger)
      {
         _propertyProvider = propertyProvider;
         _logger = logger;
         _dashboardNotificationRepository = new DashboardNotificationRepository(context);
      }

      public IEnumerable<DashboardNotification> GetAvailableDashboardNotifications()
      {
         var notifications = _dashboardNotificationRepository.GetActiveNotifications().ToList();
         return notifications;
      }

      public DashboardNotification GetDashboardNotification(string notificationId)
      {
         var notification = _dashboardNotificationRepository.GetActiveNotification(notificationId);
         PopulateNotificationData(notification);
         return notification;
      }

      private void PopulateNotificationData(DashboardNotification dashboardNotification)
      {
         using (var ct = new CodeTimer(dashboardNotification.DashboardNotificationId))
         {
            switch (dashboardNotification.DashboardNotificationId)
            {
               case InventoryConstants.OrderItems:
                  dashboardNotification.ItemCount = new OrderableItemsUnitOfWork(new InventoryContext(), _propertyProvider, _logger).GetOrderableItems().Count;
                  break;
               case InventoryConstants.ConsumptionNotificationProcessingErrors:
                  dashboardNotification.ItemCount = new HealthTrackConsumptionRepository(new InventoryContext()).FindHealthTrackConsumptions(ConsumptionProcessingStatus.Error).Count();
                  break;
               case InventoryConstants.UnclassifiedProducts:
                  dashboardNotification.ItemCount = new ProductUnitOfWork(new InventoryContext(), _propertyProvider, _logger).FindUnclassified().Count();
                  break;
               case InventoryConstants.MissingPaymentClass:
                  dashboardNotification.ItemCount = new StockAdjustmentRepository(new InventoryContext()).FindDeductionsRequiringPaymentClass().Count(c => !c.PaymentClass.HasValue);
                  break;
               case InventoryConstants.UnmappedPaymentClasses:
                  dashboardNotification.ItemCount = new PaymentClassMappingRepository(new InventoryContext()).UnmappedPaymentClasses();
                  break;
               case InventoryConstants.NegativeStock:
                  dashboardNotification.ItemCount = new StockRepository(new InventoryContext()).GetNegativeStockCount();
                  break;
               case InventoryConstants.PendingConsumedProducts:
                  dashboardNotification.ItemCount = new ProductUnitOfWork(new InventoryContext(), _propertyProvider, _logger).ProductRepository.FindPendingProducts().Count();
                  break;
               case InventoryConstants.ProductsInError:
                  dashboardNotification.ItemCount = new ProductUnitOfWork(new InventoryContext(), _propertyProvider, _logger).ProductRepository.GetCurrentErroredProducts().Count();
                  break;
               default:
                  dashboardNotification.ItemCount = 0;
                  break;
            }
         }
      }
   }
}