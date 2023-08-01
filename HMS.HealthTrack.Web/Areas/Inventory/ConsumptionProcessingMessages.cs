using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory
{
   public static class ConsumptionProcessingMessages
   {
      public static string LocationMissing(Consumption consumption) => $"Failed to process consumption '{consumption.ConsumptionReference}': Location mapping missing for '{consumption.LocationId}'";
      public static string BatchMissing => "Consumption requires a batch number before it can be processed";
      public static string MissingStockLocation(Consumption consumption) => $"Failed to process consumption '{consumption.ConsumptionReference}': Unable to determine which stock location item originated from.";
      public static string ConsumptionReversed(ConsumptionReversal reversedConsumption) => $"Consumption has been reversed by reversal {reversedConsumption.ConsumptionReversalId}";
   }
}