using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Data.Repositories.Inventory
{
   public interface IReportingLevelRepository
   {
      int CountOrganisationLevels();
      int CountProductLevels();
   }

   public class ReportingLevelRepository : IReportingLevelRepository
   {
      private readonly IDbContextInventoryContext _context;
      private readonly ICustomLogger _logger;

      public ReportingLevelRepository(IDbContextInventoryContext context, ICustomLogger logger)
      {
         _context = context;
         _logger = logger;
      }

      public int CountOrganisationLevels()
      {
         //return _context.ReportingLevels.Count(rl => rl.ReportingType == ReportingLevelType.Organisational);
         return -1;
      }

      public int CountProductLevels()
      {
         //return _context.ReportingLevels.Count(rl => rl.ReportingType == ReportingLevelType.Product);
         return -1;
      }
   }
}