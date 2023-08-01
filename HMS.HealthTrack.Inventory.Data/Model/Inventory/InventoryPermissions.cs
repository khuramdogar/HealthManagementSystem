using System.Collections.Generic;

namespace HMS.HealthTrack.Web.Data.Model.Inventory
{
   public static class InventoryPermissions
   {
      private const string Inventory = "Inventory";
      private const string ViewOrders = "ViewOrders";
      private const string EditOrder = "EditOrder";
      private const string CreateOrders = "CreateOrders";
      private const string ApproveOrders = "ApproveOrders";
      private const string ConsumeStock = "ConsumeStock";
      private const string ViewConsumption = "ViewConsumption";
      private const string ViewClinicalConsumption = "ViewClinicalConsumption";
      private const string ReceiveProducts = "ReceiveProducts";
      private const string ReceiveOrders = "ReceiveOrders";
      private const string ViewRequests = "ViewRequests";
      private const string EditRequests = "EditRequests";
      private const string CreateRequests = "CreateRequests";
      private const string ManageStockSets = "ManageStockSets";
      private const string ManageCategories = "ManageCategories";
      private const string ManageProducts = "ManageProducts";
      private const string ManageConsumptionProcessing = "ManageConsumptionProcessing";
      private const string ManageProductMappings = "ManageProductMappings";
      private const string ManageSystemSettings = "ManageSystemSettings";
      private const string ManageUserPermissions = "ManageUserPermissions";

      public static List<string> All()
      {
         return new List<string>
         {
            Inventory,
            ViewOrders,
            EditOrder,
            CreateOrders,
            ApproveOrders,
            ConsumeStock,
            ViewConsumption,
            ViewClinicalConsumption,
            ReceiveProducts,
            ReceiveOrders,
            ViewRequests,
            EditRequests,
            CreateRequests,
            ManageStockSets,
            ManageCategories,
            ManageProducts,
            ManageConsumptionProcessing,
            ManageProductMappings,
            ManageSystemSettings,
            ManageUserPermissions
         };
      }
   }
}