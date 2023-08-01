using System;
using System.Linq;
using System.Threading.Tasks;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;

namespace HMS.HealthTrack.Inventory.OrderingIntegration.Oracle
{
    internal class OraclePurchaseOrderService : IOracleIncomingFileService
    {
        private readonly IOrderStatusManagementService _orderStatusManagementService;
        private readonly IOraclePurchaseOrderReceiptRepository _purchaseOrderReceiptRepository;

        public OraclePurchaseOrderService(
            IOrderStatusManagementService orderStatusManagementService,
            IOraclePurchaseOrderReceiptRepository purchaseOrderReceiptRepository)
        {
            _orderStatusManagementService = orderStatusManagementService;
            _purchaseOrderReceiptRepository = purchaseOrderReceiptRepository;
        }

        public void ProcessFile(OracleInboundFile inboundFile)
        {
            var orders = inboundFile.FileContents.SplitLines().Select(line => new OraclePurchaseOrderReceipt(line));

            Parallel.ForEach(orders, o =>
            {
                Archive(o);
                ProcessPurchaseOrder(o);
            });
        }

        private void Archive(OraclePurchaseOrderReceipt purchaseOrderReceipt)
        {
            _purchaseOrderReceiptRepository.Add(new Web.Data.Model.Inventory.OraclePurchaseOrderReceipt
            {
                InventoryOrderId = purchaseOrderReceipt.OrderId,
                InventoryOrderItemId = purchaseOrderReceipt.OrderLineId,
                OracleRequisitionNumber = purchaseOrderReceipt.FmisRequisitionNumber,
                OracleRequisitionDistributionNumber = purchaseOrderReceipt.FmisRequisitionDistributionNumber,
                OracleDistribution = purchaseOrderReceipt.FmisDistribution,
                OracleVendorCode = purchaseOrderReceipt.FmisVendorCode,
                OracleSupplierProductCode = purchaseOrderReceipt.FmisSupplierProductCode,
                OracleHospitalProductCode = purchaseOrderReceipt.FmisHospitalProductCode,
                OraclePricePerUnit = purchaseOrderReceipt.FmisPricePerUnit,
                OracleUnitOfMeasure = purchaseOrderReceipt.FmisUnitOfMeasure,
                OracleOrgId = purchaseOrderReceipt.FmisOrgId
            });

            _purchaseOrderReceiptRepository.Commit();
        }

        private void ProcessPurchaseOrder(OraclePurchaseOrderReceipt purchaseOrderReceipt)
        {
            _orderStatusManagementService.UpdateOrderToOrdered(Convert.ToInt32(purchaseOrderReceipt.OrderId));
        }
    }
}
