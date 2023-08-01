using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HMS.HealthTrack.Web.Areas.Inventory.Models.ExternalApi.Consumption;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using MediatR;

namespace HMS.HealthTrack.Web.Areas.Inventory.QueryHandlers.Consumption
{
   public class ConsumptionReversalHandler : IRequestHandler<ConsumptionReversalRequest, ConsumptionDeletionResult>
   {
      private readonly IDbContextInventoryContext _context;

      public ConsumptionReversalHandler(IDbContextInventoryContext context)
      {
         _context = context;
      }

      public async Task<ConsumptionDeletionResult> Handle(ConsumptionReversalRequest request, CancellationToken cancellationToken)
      {
         //Check the consumption exists
         var consumption = _context.Consumptions.SingleOrDefault(c => c.ConsumptionReference == request.ConsumptionReference);
         if (consumption == null) return new ConsumptionDeletionResult {ErrorMessage = $"No consumption with a reference of {request.ConsumptionReference} has been received"};

         //Check the consumption hasn't already been reversed
         if (_context.ConsumptionReversals.Any(cr => cr.ConsumptionReference == request.ConsumptionReference))
            return new ConsumptionDeletionResult {ErrorMessage = $"A consumption reversal with a reference of {request.ConsumptionReference} has already been received"};

         //Create a new consumption reversal
         var consumptionReversal = new ConsumptionReversal
         {
            ConsumptionReference = request.ConsumptionReference,
            SubmittedBy = request.Requester,
            SubmittedOn = DateTime.Now,
         };

         //Save the reversal for processing
         _context.ConsumptionReversals.Add(consumptionReversal);
         _context.ObjectContext.SaveChanges();
         
         //Return the reversal details
         return await _context.ConsumptionReversals.Where(c => c.ConsumptionReversalId == consumptionReversal.ConsumptionReversalId)
            .Select(c => new ConsumptionDeletionResult { ConsumptionReversalId = c.ConsumptionReversalId }).SingleOrDefaultAsync(cancellationToken);
      }
   }
}