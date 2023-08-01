using AutoMapper;
using HMS.HealthTrack.Web.Areas.Inventory.Models.Stock;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System.Linq;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers.API
{
   [HealthTrackAuthorize(HealthTrackPermission = ("Inventory"))]
   public class ReceiveStockController : ApiController
   {
      private readonly IStockUnitOfWork _unitOfWork;
      private readonly ICustomLogger _logger;

      public ReceiveStockController(IStockUnitOfWork unitOfWork, ICustomLogger logger)
      {
         _unitOfWork = unitOfWork;
         _logger = logger;
      }

      [HttpGet]
      public DataSourceResult GetReceivedStock(
         [ModelBinder(typeof(WebApiDataSourceRequestModelBinder))] DataSourceRequest request, int orderItemId)
      {
         var stockAdjustments = _unitOfWork.StockAdjustmentRepository.FindAllPositiveAdjustments().Where(sa => sa.OrderItemId == orderItemId);
         var receiptModels = stockAdjustments.Select(Mapper.Map<StockAdjustment, ReceivedStockModel>);
         return receiptModels.ToDataSourceResult(request);
      }

   }
}