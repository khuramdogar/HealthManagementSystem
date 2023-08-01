
namespace HMS.HealthTrack.Inventory.OrderingIntegration.Oracle
{
    internal sealed class OracleOutboundOrderDistributionLine
    {
        private readonly char _lineItemIndicator;
        private readonly int _orderItemId;
        private readonly int _orderItemLineId;
        private readonly int _quantity;
        private readonly string _fmisDistributionChargeAccount;

        public OracleOutboundOrderDistributionLine(char lineItemIndicator, int orderItemId, int orderItemLineId, int quantity, string fmisDistributionChargeAccount)
        {
            _lineItemIndicator = lineItemIndicator;
            _orderItemId = orderItemId;
            _orderItemLineId = orderItemLineId;
            _quantity = quantity;
            _fmisDistributionChargeAccount = fmisDistributionChargeAccount;
        }

        public string ToPipeSeperated()
        {
            return string.Join("|",
                _lineItemIndicator,
                _orderItemId,
                _orderItemLineId,
                _quantity,
                _fmisDistributionChargeAccount) + "|";
        }
    }
}
