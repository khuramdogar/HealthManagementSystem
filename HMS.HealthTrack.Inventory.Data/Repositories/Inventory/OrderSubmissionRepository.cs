using System.Linq;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Data.Repositories.Inventory
{
   public class OrderSubmissionRepository : IOrderSubmissionRepository
   {
      private readonly IDbContextInventoryContext _context;

      public OrderSubmissionRepository(IDbContextInventoryContext context)
      {
         _context = context;
      }

      public IQueryable<Submission> GetOpenSubmissions()
      {
         return _context.Submissions.Where(s => s.SubmissionStatus == SubmissionStatus.Sent);
      }

      public void Add(Submission result)
      {
         _context.Submissions.Add(result);
      }
   }

   public interface IOrderSubmissionRepository
   {
      IQueryable<Submission> GetOpenSubmissions();
      void Add(Submission result);
   }
}