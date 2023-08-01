
namespace HMS.HealthTrack.Inventory.Common.Constants
{
   public static class InventoryConstants
   {
      //Stock settings - These must be kept in line with the names stored in the StockSetting table
      public static class StockSettings
      {
         public const string RequiresSerialNumber = "RSN";
         public const string RequiresBatchNumber = "RBN";
         public const string RequiresPaymentClass = "RPC";
         public const string SingleOrderItem = "SOI";
         public const string RequiresPatientDetails = "RPD";
         public const string Unorderable = "UNO";
         public const string SupplierInvoiceOnUse = "RI";
      }


      public static class StockAdjustmentReasons
      {
         public const string StockManagementWriteOff = "Stock management write off";
         public const string InitialStock = "Intial stock";
      }

      //Notifications
      public const string OrderItems = "OrderItems";
      public const string ConsumptionNotificationProcessingErrors = "ConsumptionNotificationProcessingErrors";
      public const string UnclassifiedProducts = "UnclassifiedProducts";
      public const string MissingPaymentClass = "MissingPaymentClass";
      public const string UnmappedPaymentClasses = "UnmappedPaymentClasses";
      public const string NegativeStock = "NegativeStock";
      public const string PendingConsumedProducts = "PendingConsumedProducts";
      public const string ProductsInError = "ProductsInError";

      public const string Unclassified = "Unclassified";


      //Ledger types
      public const string OrderLedgerType = "Order";
      public const string ProductLedgerType = "Product";

      //Claim types
      public const string HealthTrackUsername = "HealthTrackUsername";

      public const string GeneralLedgerTierNameExportSuffix = " ledger code";


      public const string BulkUpdatePriceTypePrefix = "PriceType_";


      //Users
      public const string UnconfiguredInventoryPrefsSuffix = "_UnconfiguredInventoryPreferences";

      public const string MergeControlPrefix = "merge";
      public const string MergePricePrefix = "MergePrice_";

   }
}
