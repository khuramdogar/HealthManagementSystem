using System.Collections.Generic;
using System.Linq;
using HMS.HealthTrack.Inventory.OrderingIntegration.Common;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;

namespace HMS.HealthTrack.Inventory.OrderingIntegration.Oracle
{
   internal class HealthTrackToOracleOrderConverter : IHealthTrackToOracleOrderConverter
   {
      private readonly IProductRepository _productRepository;
      private readonly OrderChannel _orderChannel;

      public HealthTrackToOracleOrderConverter(IProductRepository productRepository, OrderChannel orderChannel)
      {
         _productRepository = productRepository;
         _orderChannel = orderChannel;
      }

      public OracleOutboundOrder ConvertOrder(Order order)
      {
         var outboundOrder = new OracleOutboundOrder {OrderItems = order.Items.Select(item => ConvertOrderItem(order.InventoryOrderId, item))};

         return outboundOrder;
      }

      public OracleOutboundOrderItem ConvertOrderItem(int orderId, OrderItem item)
      {
         var fmisId = item.ProductId.ToString();
         // Get the Item identifier
         if (_orderChannel != null)
         {
            var refResult = _productRepository.GetProductOrderReferenceIdentifier(item.ProductId, _orderChannel.OrderChannelId);
            if (refResult.IdentifierType != ProductIdentifierType.None)
               fmisId = refResult.Value;
         }

         var itemIdentifier = new OracleOutboundOrderItemIdentifier(item.Product.SPC, fmisId);

         //TODO: Handle split distributions
         var distributionLines = new List<OracleOutboundOrderDistributionLine>
         {
            new OracleOutboundOrderDistributionLine(
               'D',
               orderId,
               item.OrderItemId,
               item.Quantity,
               item.Order.GeneralLedger?.Code) //charge account again ?
         };

         var outboundOrderItem = new OracleOutboundOrderItem()
         {
            OrderId = orderId,
            OrderLineId = item.OrderItemId,
            HeaderItemIndicator = 'H', //Header indicator
            FmisSupplierNumber = item.Product.PrimarySupplier.GetValueOrDefault(),
            FmisVendorSiteCode = _orderChannel?.Reference,
            ItemIdentifier = itemIdentifier,
            Quantity = item.Quantity,
            FmisRequesterName = item.Order.CreatedBy,
            FmisDistributionChargeAccount = item.Order.GeneralLedger?.AlternateCode,
            FmisDeliveryLocationCode = item.Order.DeliveryLocationId?.ToString(),
            FmisPreparerName = item.Order.CreatedBy,
            FmisDestinationOrderCode = item.Order.DeliveryLocationId?.ToString(),
            FmisMultiDistributionFlag = distributionLines.Count() > 1 ? 'Y' : 'N',
            FmisOrgId = _orderChannel != null ? _orderChannel.OrganisationId.GetValueOrDefault() : -1,
            FmisInterfaceType = FmisAttributes.InterfaceType,
            DistributionLines = distributionLines
         };

         return outboundOrderItem;
      }
   }
}