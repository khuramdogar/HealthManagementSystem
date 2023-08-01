using System.Collections.Generic;
using System.Linq;

namespace HMS.HealthTrack.Inventory.OrderingIntegration.Oracle
{
    internal struct OracleOutboundOrderItem
    {
       public OracleOutboundOrderItem(int orderId, int orderLineId, char headerItemIndicator, int fmisSupplierNumber, string fmisVendorSiteCode, 
                                   OracleOutboundOrderItemIdentifier itemIdentifier, int quantity, string fmisRequesterName, string fmisDistributionChargeAccount, 
                                   string fmisDeliveryLocationCode, string fmisPreparerName, string fmisDestinationOrderCode, char fmisMultiDistributionFlag, 
                                   int fmisOrgId, string fmisInterfaceType, ICollection<OracleOutboundOrderDistributionLine> distributionLines)
        {
            HeaderItemIndicator = headerItemIndicator;
            FmisSupplierNumber = fmisSupplierNumber;
            FmisVendorSiteCode = fmisVendorSiteCode;
            ItemIdentifier = itemIdentifier;
            Quantity = quantity;
            OrderId = orderId;
            OrderLineId = orderLineId;
            FmisRequesterName = fmisRequesterName;
            FmisDistributionChargeAccount = fmisDistributionChargeAccount;
            FmisDeliveryLocationCode = fmisDeliveryLocationCode;
            FmisPreparerName = fmisPreparerName;
            FmisDestinationOrderCode = fmisDestinationOrderCode;
            FmisMultiDistributionFlag = fmisMultiDistributionFlag;
            FmisOrgId = fmisOrgId;
            FmisInterfaceType = fmisInterfaceType;
            DistributionLines = distributionLines;
            OptionalInfo = null;
            NonCatalogInfo = null;
        }

        public OracleOutboundOrderItem(int orderId, int orderLineId, char headerItemIndicator, int fmisSupplierNumber, string fmisVendorSiteCode, 
                                   OracleOutboundOrderItemIdentifier itemIdentifier, int quantity, string fmisRequesterName, string fmisDistributionChargeAccount, 
                                   string fmisDeliveryLocationCode, string fmisPreparerName, string fmisDestinationOrderCode, char fmisMultiDistributionFlag, 
                                   int fmisOrgId, string fmisInterfaceType, ICollection<OracleOutboundOrderDistributionLine> distributionLines, 
                                   OracleOutboundOrderOptionalInfo optionalInfo)
            : this(orderId, orderLineId, headerItemIndicator, fmisSupplierNumber, fmisVendorSiteCode, itemIdentifier, quantity, fmisRequesterName,
                   fmisDistributionChargeAccount, fmisDeliveryLocationCode, fmisPreparerName, fmisDestinationOrderCode, fmisMultiDistributionFlag,
                   fmisOrgId, fmisInterfaceType, distributionLines)
        {
            OptionalInfo = optionalInfo;
        }

        public OracleOutboundOrderItem(int orderId, int orderLineId, char headerItemIndicator, int fmisSupplierNumber, string fmisVendorSiteCode,
                                  OracleOutboundOrderItemIdentifier itemIdentifier, int quantity, string fmisRequesterName, string fmisDistributionChargeAccount,
                                  string fmisDeliveryLocationCode, string fmisPreparerName, string fmisDestinationOrderCode, char fmisMultiDistributionFlag,
                                  int fmisOrgId, string fmisInterfaceType, ICollection<OracleOutboundOrderDistributionLine> distributionLines,
                                  OracleOutboundOrderNonCatalogInfo nonCatalogInfo)
            : this(orderId, orderLineId, headerItemIndicator, fmisSupplierNumber, fmisVendorSiteCode, itemIdentifier, quantity, fmisRequesterName,
                   fmisDistributionChargeAccount, fmisDeliveryLocationCode, fmisPreparerName, fmisDestinationOrderCode, fmisMultiDistributionFlag,
                   fmisOrgId, fmisInterfaceType, distributionLines)
        {
            NonCatalogInfo = nonCatalogInfo;
        }

        public OracleOutboundOrderItem(int orderId, int orderLineId, char headerItemIndicator, int fmisSupplierNumber, string fmisVendorSiteCode,
                                  OracleOutboundOrderItemIdentifier itemIdentifier, int quantity, string fmisRequesterName, string fmisDistributionChargeAccount,
                                  string fmisDeliveryLocationCode, string fmisPreparerName, string fmisDestinationOrderCode, char fmisMultiDistributionFlag,
                                  int fmisOrgId, string fmisInterfaceType, ICollection<OracleOutboundOrderDistributionLine> distributionLines,
                                  OracleOutboundOrderOptionalInfo optionalInfo, OracleOutboundOrderNonCatalogInfo nonCatalogInfo)
            : this(orderId, orderLineId, headerItemIndicator, fmisSupplierNumber, fmisVendorSiteCode, itemIdentifier, quantity, fmisRequesterName,
                   fmisDistributionChargeAccount, fmisDeliveryLocationCode, fmisPreparerName, fmisDestinationOrderCode, fmisMultiDistributionFlag,
                   fmisOrgId, fmisInterfaceType, distributionLines)
        {
            OptionalInfo = optionalInfo;
            NonCatalogInfo = nonCatalogInfo;
        }

        public char HeaderItemIndicator { get; set; }

       public int FmisSupplierNumber { get; set; }

       public string FmisVendorSiteCode { get; set; }

       public OracleOutboundOrderItemIdentifier ItemIdentifier { get; set; }

       public int Quantity { get; set; }

       public int OrderId { get; set; }

       public int OrderLineId { get; set; }

       public string FmisRequesterName { get; set; }

       public string FmisDistributionChargeAccount { get; set; }

       public string FmisDeliveryLocationCode { get; set; }

       public string FmisPreparerName { get; set; }

       public string FmisDestinationOrderCode { get; set; }

       public char FmisMultiDistributionFlag { get; set; }

       public int FmisOrgId { get; set; }

       public string FmisInterfaceType { get; set; }

       public ICollection<OracleOutboundOrderDistributionLine> DistributionLines { get; set; }

       public OracleOutboundOrderOptionalInfo? OptionalInfo { get; set; }

       public OracleOutboundOrderNonCatalogInfo? NonCatalogInfo { get; set; }

       public IList<string> ToPipeSeperatedLines()
        {
            var lines = new List<string>
            {
                string.Join("|",
                    HeaderItemIndicator,
                    FmisSupplierNumber,
                    FmisVendorSiteCode,
                    ItemIdentifier.FmisSupplierItemCode,
                    ItemIdentifier.FmisItemCode,
                    Quantity,
                    NonCatalogInfo.HasValue ? NonCatalogInfo.Value.ItemDescription : string.Empty,
                    NonCatalogInfo.HasValue ? NonCatalogInfo.Value.FmisCategory.ToString() : string.Empty,
                    NonCatalogInfo.HasValue ? string.Format("{0:0.00}", NonCatalogInfo.Value.UnitPrice)  : string.Empty,
                    NonCatalogInfo.HasValue ? NonCatalogInfo.Value.FmisUnitOfMeasure : string.Empty,
                    OrderId,
                    OrderLineId,
                    FmisRequesterName,
                    FmisDistributionChargeAccount,
                    FmisDeliveryLocationCode,
                    FmisPreparerName,
                    FmisDestinationOrderCode,
                    OptionalInfo.HasValue ? string.Format("{0:dd-MM-yyyy}", OptionalInfo.Value.FmisDeliveryDate) : string.Empty,
                    FmisMultiDistributionFlag,
                    OptionalInfo.HasValue ? OptionalInfo.Value.InformationTemplateAttribute1 : string.Empty,
                    OptionalInfo.HasValue ? OptionalInfo.Value.InformationTemplateAttribute2 : string.Empty,
                    OptionalInfo.HasValue ? OptionalInfo.Value.InformationTemplateAttribute3 : string.Empty,
                    OptionalInfo.HasValue ? OptionalInfo.Value.InformationTemplateAttribute4 : string.Empty,
                    OptionalInfo.HasValue ? OptionalInfo.Value.InformationTemplateAttribute5 : string.Empty,
                    OptionalInfo.HasValue ? OptionalInfo.Value.InformationTemplateAttribute6 : string.Empty,
                    OptionalInfo.HasValue ? OptionalInfo.Value.InformationTemplateAttribute7 : string.Empty,
                    OptionalInfo.HasValue ? OptionalInfo.Value.InformationTemplateAttribute8 : string.Empty,
                    OptionalInfo.HasValue ? OptionalInfo.Value.RequistionNumberFlag : string.Empty,
                    NonCatalogInfo.HasValue ? NonCatalogInfo.Value.NonCatalogFreeTextItemsFlag : string.Empty,
                    OptionalInfo.HasValue ? OptionalInfo.Value.VendorNote : string.Empty,
                    FmisOrgId,
                    FmisInterfaceType) + "|"
            };

            if (DistributionLines != null)
                lines.AddRange(DistributionLines.Select(item => item.ToPipeSeperated()));

            return lines;
        }
    }
}
