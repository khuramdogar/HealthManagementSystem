using System.Linq;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Data.Repositories.Inventory
{
   public interface IDashboardNotificationRepository
   {
      IQueryable<DashboardNotification> GetActiveNotifications();
      DashboardNotification GetActiveNotification(string notificationId);
   }

   public class DashboardNotificationRepository : IDashboardNotificationRepository
   {
      private readonly IDbContextInventoryContext _dataContext;

      public DashboardNotificationRepository(IDbContextInventoryContext dataContext)
      {
         _dataContext = dataContext;
      }

      public IQueryable<DashboardNotification> GetActiveNotifications()
      {
         return _dataContext.DashboardNotifications.Where(n => !n.Disabled);
      }

      public DashboardNotification GetActiveNotification(string notificationId)
      {
         return _dataContext.DashboardNotifications.SingleOrDefault(n => n.DashboardNotificationId == notificationId);
      }
   }
}