using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HMS.HealthTrack.Web.Areas.Inventory.Models.ExternalApi.Consumption;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using MediatR;

namespace HMS.HealthTrack.Web.Areas.Inventory.QueryHandlers.Consumption
{
   public class ConsumptionSubmissionHandler : IRequestHandler<ConsumptionSubmission, ConsumptionSubmissionResult>
   {
      private readonly IDbContextInventoryContext _context;

      public ConsumptionSubmissionHandler(IDbContextInventoryContext context)
      {
         _context = context;
      }
      public async Task<ConsumptionSubmissionResult> Handle(ConsumptionSubmission submission, CancellationToken cancellationToken)
      {
         if (_context.Consumptions.Any(c => c.ConsumptionReference == submission.ConsumptionReference))
         {
            return new ConsumptionSubmissionResult {ErrorMessage = $"A consumption with a reference of {submission.ConsumptionReference} already exists"};
         }

         var newCon = new Web.Data.Model.Inventory.Consumption
         {
            SPC = submission.SPC,
            ProductId = submission.ProductId,
            Description = submission.Description,
            UPC = submission.UPC,
            RebateCode = submission.RebateCode,
            ApplicationId = submission.ApplicationId,
            BatchNumber = submission.BatchNumber,
            ConsumedOn = submission.ConsumedOn,
            Consumer = submission.Consumer,
            ConsumptionReference = submission.ConsumptionReference,
            LocationId = submission.LocationId,
            LocationName = submission.LocationName,
            ProductName = submission.ProductName,
            Quantity = submission.Quantity,
            SerialNumber = submission.Serial,
         };
         _context.Consumptions.Add(newCon);

         _context.ObjectContext.SaveChanges();

         return await _context.Consumptions.Where(c => c.ConsumptionId == newCon.ConsumptionId)
            .Select(c => new ConsumptionSubmissionResult {ConsumptionId = c.ConsumptionId}).SingleOrDefaultAsync(cancellationToken);
      }
   }
}