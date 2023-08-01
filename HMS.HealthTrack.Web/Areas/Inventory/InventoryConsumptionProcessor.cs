using System;
using HMS.HealthTrack.Web.Data.Exceptions;
using HMS.HealthTrack.Web.Data.Helpers;
using HMS.HealthTrack.Web.Data.Model;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork;
using Serilog;

namespace HMS.HealthTrack.Web.Areas.Inventory
{
   internal class InventoryConsumptionProcessor
   {
      private readonly IConsumptionRepository _consumptionRepository;
      private readonly IProductRepository _productRepository;
      private readonly IConsumptionUnitOfWork _consumptionUnitOfWork;
      private readonly StockAdjustmentHelper _deductionHelper;
      private static string ProcessorUsername => "Processor";

      public InventoryConsumptionProcessor(IConsumptionRepository consumptionRepository, 
         IProductRepository productRepository,
         IConsumptionUnitOfWork consumptionUnitOfWork)
      {
         _consumptionRepository = consumptionRepository;
         _productRepository = productRepository;
         _consumptionUnitOfWork = consumptionUnitOfWork;
         _deductionHelper = new StockAdjustmentHelper(_consumptionUnitOfWork.StockRepository, _consumptionUnitOfWork.StockAdjustmentRepository);
      }
      internal void ProcessConsumptions()
      {
         var consumptions = _consumptionRepository.UnprocessedConsumptions;
         foreach (var consumption in consumptions)
         {
            //check for reversals
            var reversedConsumption = _consumptionRepository.ReversedConsumption(consumption.ConsumptionReference);
            if (reversedConsumption != null)
            {
               ReverseConsumption(consumption, reversedConsumption);
               continue;
            }

            //Process Consumption
            try
            {
               ProcessConsumption(consumption);
            }
            catch (Exception e)
            {
               RecordUnprocessedConsumptionProcessingError(e, consumption);
            }
            
         }
      }

      internal ConsumptionManagement ProcessConsumption(Consumption consumption)
      {
         //Create a management record
         var conManagement = _consumptionRepository.CreateConsumptionManagement(consumption);

         //Find product
         var product = consumption.ProductId.HasValue
            ? _productRepository.Find(consumption.ProductId.Value)
            : _consumptionUnitOfWork.FindOrCreateProduct(consumption);

         if(product == null)
            throw new Exception($"Unable to find or create product for consumption {consumption.ConsumptionReference}");
         
         //Mark consumption for that product
         consumption.ProductId = product.ProductId;

         //Handle deleted products
         if (product.DeletedOn.HasValue)
         {
            // mark as in error
            var error =
               $"Cannot process inventory consumption '{consumption.ConsumptionReference}', the product has been deleted. This consumption can either be ignored or the product can be reinstated in order to process the consumption.";
            conManagement.ProcessingStatus = (int)ConsumptionProcessingStatus.Error;
            conManagement.ProcessingStatusMessage =
               $"Cannot process due to deleted product. This consumption can either be ignored or the <a href=\'~\\Inventory\\Products\\Details\\{product.ProductId}\' associated product> associated product </a>can be reinstated in order to process the consumption.";
            Log.Warning(error);
            _consumptionRepository.Commit();
            return conManagement;
         }

         if (product.RequiresBatch && string.IsNullOrWhiteSpace(consumption.BatchNumber))
         {
            conManagement = _consumptionRepository.CreateConsumptionManagement(consumption, ConsumptionProcessingMessages.BatchMissing);
            _consumptionRepository.Commit();
            return conManagement;
         }

         if (product.RequiresSerial && string.IsNullOrWhiteSpace(consumption.SerialNumber))
         {
            conManagement.ProcessingStatus = (int)ConsumptionProcessingStatus.Error;
            conManagement.ProcessingStatusMessage = "Consumption requires a serial number before it can be processed";
            _consumptionRepository.Commit();
            return conManagement;
         }

         // Find corresponding stock locations for HealthTrack location
         var locationMapper = consumption.LocationId == 0 ? null :
            _consumptionUnitOfWork.StockLocationRepository.GetHealthTrackLocationMapper(consumption.LocationId);

         if (locationMapper == null)
         {
            conManagement = _consumptionRepository.CreateConsumptionManagement(consumption, ConsumptionProcessingMessages.LocationMissing(consumption));
            _consumptionRepository.Commit();
            return conManagement;
         }

         var stockLocation = locationMapper.GetMappedStockLocation();

         // Mappings exist for location but cannot decide which to use
         if (!stockLocation.HasValue)
         {
            conManagement = _consumptionRepository.CreateConsumptionManagement(consumption, ConsumptionProcessingMessages.MissingStockLocation(consumption));
            _consumptionRepository.Commit();
            return conManagement;
         }

         // Only perform deduction is stock is managed
         if (product.ManageStock)
         {
            // Create stock deduction for used inventory item
            var deduction = _consumptionUnitOfWork.CreateAdjustmentFromConsumption(consumption, product.ProductId,
               stockLocation.Value, consumption.ConsumedOn);

            //Check when the last stock take was
            var lastStockTakeBeforeConsumption =
               _productRepository.MostRecentStockTake(product, stockLocation.Value);

            // the last stock take was after the consumption date
            if (lastStockTakeBeforeConsumption.HasValue &&
                lastStockTakeBeforeConsumption.Value > deduction.AdjustedOn)
            {
               // archive as this consumption will not be acted upon (deducted)
               _consumptionRepository.ArchiveConsumptionNotification(conManagement, ProcessorUsername);
            }
            else
            {
               // perform the deduction
               _deductionHelper.AdjustItem(deduction, product, ProcessorUsername);
            }
         }
         else
         {
            // if unmanaged stock and the consumption was consumed before the UnmanageFrom date then ignore it
            if (product.ReplaceAfter == null || consumption.ConsumedOn < product.ReplaceAfter)
            {
               _consumptionRepository.ArchiveConsumptionNotification(conManagement, ProcessorUsername);
            }
         }

         conManagement.ProcessingStatus = (int)ConsumptionProcessingStatus.Processed;
         conManagement.ProcessingStatusMessage = "Processed:" + DateTime.Now;
         _consumptionRepository.Commit();
         return conManagement;
      }

      private void RecordUnprocessedConsumptionProcessingError(Exception exception, Consumption consumption)
      {
         Log.Logger.Error(exception, "Failed to process consumption {ConsumptionReference}", consumption.ConsumptionReference);
         var errorMessage = $"Failed to process used inventory item {consumption.ConsumptionReference}: {exception.Message}";
         var conMan = _consumptionRepository.CreateConsumptionManagement(consumption, exception);
         if (exception.GetType().IsSubclassOf(typeof(StockException)))
         {
            conMan.ProcessingStatusMessage = exception.Message;
         }
         else
         {
            conMan.ProcessingStatusMessage = errorMessage;
            Log.Error(exception, errorMessage);
         }
         _consumptionRepository.Commit();
      }

      internal ConsumptionManagement ReverseConsumption(Consumption consumption, ConsumptionReversal reversedConsumption)
      {
         if (consumption == null)
            throw new ConsumptionProcessingException("Unable to reverse a consumption the consumption is missing ");
         if (reversedConsumption == null)
            throw new ConsumptionProcessingException("Unable to reverse a consumption the consumption reversal is missing ");

         var consumptionManagement = _consumptionRepository.CreateConsumptionManagement(consumption);
         consumptionManagement.ProcessingStatus = (int) ConsumptionProcessingStatus.Ignored;
         consumptionManagement.ProcessingStatusMessage = ConsumptionProcessingMessages.ConsumptionReversed(reversedConsumption);
         reversedConsumption.Processed = true;
         reversedConsumption.ProcessedOn = DateTime.Now;
         _consumptionRepository.Commit();

         return consumptionManagement;
      }
   }
}