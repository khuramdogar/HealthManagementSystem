using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Configuration;

namespace HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork
{
   public class OrderSubmissionUnitOfWork : IOrderSubmissionUnitOfWork
   {
      private readonly IDbContextInventoryContext _context;
      private readonly ICustomLogger _logger;

      public OrderSubmissionUnitOfWork(IDbContextInventoryContext context, IPropertyProvider propertyProvider, ICustomLogger logger)
      {
         _context = context;
         _logger = logger;
         OrderRepo = new OrderRepository(context);
         SubmissionRepo = new OrderSubmissionRepository(context);
         PropertyProvider = propertyProvider;
         ProductRepo = new ProductRepository(context, PropertyProvider);
         OrderChannelRepo = new OrderChannelRepository(context);
         ConfigRepo = new ConfigurationRepository(context, _logger);
      }

      public IPropertyProvider PropertyProvider { get; }
      public IOrderSubmissionRepository SubmissionRepo { get; }
      public IOrderRepository OrderRepo { get; }
      public IOrderChannelRepository OrderChannelRepo { get; }
      public IProductRepository ProductRepo { get; }
      public IConfigurationRepository ConfigRepo { get; }

      public void Commit()
      {
         _context.ObjectContext.SaveChanges();
      }
   }

   public interface IOrderSubmissionUnitOfWork
   {
      IOrderRepository OrderRepo { get; }
      IOrderSubmissionRepository SubmissionRepo { get; }
      IProductRepository ProductRepo { get; }
      IOrderChannelRepository OrderChannelRepo { get; }
      IConfigurationRepository ConfigRepo { get; }
      void Commit();
   }
}