using System;
using System.Linq;
using System.Threading.Tasks;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;

namespace HMS.HealthTrack.Inventory.OrderingIntegration.Oracle
{
    internal class OracleErrorReportService : IOracleIncomingFileService
    {
        private readonly IOrderStatusManagementService _orderStatusManagementService;
        private readonly IOraclePurchaseOrderErrorRepository _purchaseOrderErrorRepository;

        public OracleErrorReportService(
            IOrderStatusManagementService orderStatusManagementService,
            IOraclePurchaseOrderErrorRepository purchaseOrderErrorRepository)
        {
            _orderStatusManagementService = orderStatusManagementService;
            _purchaseOrderErrorRepository = purchaseOrderErrorRepository;
        }

        public void ProcessFile(OracleInboundFile inboundFile)
        {
            var orders = inboundFile.FileContents.SplitLines().Select(line => new OraclePurchaseOrderErrorReport(line));

            Parallel.ForEach(orders, o =>
            {
                Archive(o);
                ProcessErrorReport(o);
            });
        }

        private void Archive(OraclePurchaseOrderErrorReport errorReport)
        {
            _purchaseOrderErrorRepository.Add(new OraclePurchaseOrderError
            {
                InventoryOrderId = errorReport.OrderId,
                InventoryOrderItemId = errorReport.OrderLineId,
                ItemDescription = errorReport.ItemDescription,
                RequestId = errorReport.RequestId,
                CreationDate = errorReport.CreationDate,
                ProcessFlag = errorReport.ProcessFlag,
                InterfaceSourceCode = errorReport.InterfaceSourceCode
            });

            _purchaseOrderErrorRepository.Commit();
        }

        private void ProcessErrorReport(OraclePurchaseOrderErrorReport errorReport)
        {
            _orderStatusManagementService.UpdateOrderToFailed(Convert.ToInt32(errorReport.OrderId));
        }
    }
}
