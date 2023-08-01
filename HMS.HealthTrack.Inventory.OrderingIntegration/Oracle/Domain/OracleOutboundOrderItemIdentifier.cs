using System;

namespace HMS.HealthTrack.Inventory.OrderingIntegration.Oracle
{
    internal sealed class OracleOutboundOrderItemIdentifier
    {
        private readonly string _fmisSupplierItemCode;
        private readonly string _fmisItemCode;

        public OracleOutboundOrderItemIdentifier(string fmisSupplierItemCode, string fmisItemCode)
        {
            if (string.IsNullOrEmpty(fmisSupplierItemCode) && string.IsNullOrEmpty(fmisItemCode))
                throw new ArgumentException("exception=Unable to construct OracleOutboundOrderItemIdentifier as arguments are null.");

            _fmisSupplierItemCode = fmisSupplierItemCode;
            _fmisItemCode = fmisItemCode;
        }

        public string FmisSupplierItemCode
        {
            get { return _fmisSupplierItemCode; }
        }

        public string FmisItemCode
        {
            get { return _fmisItemCode; }
        }
    }
}
