
namespace HMS.HealthTrack.Inventory.OrderingIntegration.Oracle
{
    internal struct OracleOutboundOrderNonCatalogInfo
    {
        private readonly string _itemDescription;
        private readonly int _fmisCategory;
        private readonly double _unitPrice;
        private readonly string _fmisUnitOfMeasure;
        private readonly string _nonCatalogFreeTextItemsFlag;

        public OracleOutboundOrderNonCatalogInfo(string itemDescription, int fmisCategory, double unitPrice, string fmisUnitOfMeasure, string nonCatalogFreeTextItemsFlag)
        {
            _itemDescription = itemDescription;
            _fmisCategory = fmisCategory;
            _unitPrice = unitPrice;
            _fmisUnitOfMeasure = fmisUnitOfMeasure;
            _nonCatalogFreeTextItemsFlag = nonCatalogFreeTextItemsFlag;
        }

        public string ItemDescription
        {
            get { return _itemDescription; }
        }

        public int FmisCategory
        {
            get { return _fmisCategory; }
        }

        public double UnitPrice
        {
            get { return _unitPrice; }
        }

        public string FmisUnitOfMeasure
        {
            get { return _fmisUnitOfMeasure; }
        }

        public string NonCatalogFreeTextItemsFlag
        {
            get { return _nonCatalogFreeTextItemsFlag; }
        }
    }
}
