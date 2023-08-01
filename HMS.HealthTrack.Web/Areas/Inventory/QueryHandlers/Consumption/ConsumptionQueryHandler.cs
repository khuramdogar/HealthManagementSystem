using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HMS.HealthTrack.Web.Areas.Inventory.Models.ExternalApi.Consumption;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using MediatR;

namespace HMS.HealthTrack.Web.Areas.Inventory.QueryHandlers.Consumption
{
   public class ConsumptionQueryHandler : IRequestHandler<ConsumptionQuery, ConsumptionDto>
   {
      private readonly IDbContextInventoryContext _context;

      public ConsumptionQueryHandler(IDbContextInventoryContext context)
      {
         _context = context;
      }

      public async Task<ConsumptionDto> Handle(ConsumptionQuery request, CancellationToken cancellationToken)
      {
         if(request.ConsumptionId != 0)
            return await _context.HealthTrackConsumptions.Where(c => c.UsedId == request.ConsumptionId)
            .Select(HealthTrackConsumptionExtension.Projection)
            .SingleOrDefaultAsync(cancellationToken);

         var query = from c in _context.HealthTrackConsumptions select c;

         return await query
            .Select(HealthTrackConsumptionExtension.Projection)
            .SingleOrDefaultAsync(cancellationToken);
      }
   }
}