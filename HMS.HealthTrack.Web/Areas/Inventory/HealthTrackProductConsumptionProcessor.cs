using Hangfire;
using System;
using System.Linq;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Inventory.Common.Constants;
using HMS.HealthTrack.Web.Data.Exceptions;
using HMS.HealthTrack.Web.Data.Helpers;
using HMS.HealthTrack.Web.Data.Model;
using HMS.HealthTrack.Web.Data.Model.HealthTrackInventory;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.HealthTrackInventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork;

namespace HMS.HealthTrack.Web.Areas.Inventory
{
   [Queue(BackgroundQueues.Web)]
   public class HealthTrackProductConsumptionProcessor
   {
      private readonly IConsumptionUnitOfWork _unitOfWork;
      private readonly ICustomLogger _logger;
      private readonly IPropertyProvider _propertyProvider;
      private readonly IInventoryUsedRepository _inventoryUsedRepository;
      private readonly StockAdjustmentHelper _deductionHelper;

      public HealthTrackProductConsumptionProcessor(IConsumptionUnitOfWork unitOfWork, ICustomLogger logger,
         IPropertyProvider propertyProvider, IInventoryUsedRepository inventoryUsedRepository)
      {
         _logger = logger;
         _propertyProvider = propertyProvider;
         _inventoryUsedRepository = inventoryUsedRepository;
         _unitOfWork = unitOfWork;
         _deductionHelper = new StockAdjustmentHelper(_unitOfWork.StockRepository, _unitOfWork.StockAdjustmentRepository);
      }

      internal void ProcessConsumptionNotification(int consumptionId, string username)
      {
         var consumptionNotification = _unitOfWork.HealthTrackConsumptionRepository.FindConsumptionNotification(consumptionId);

         if (consumptionNotification == null)
         {
            _logger.Information(
               "Cannot find managed consumption for processing. Attempting to manage consumption with Id {InvUsed_ID}",
               consumptionId);
            consumptionNotification = _unitOfWork.HealthTrackConsumptionRepository.ManageConsumption(consumptionId);

            if (consumptionNotification == null)
            {
               _logger.Warning("Could not find consumption {InvUsed_ID} for management.", consumptionId);
               return;
            }
            _logger.Information("Managed consumption {InvUsed_ID}", consumptionId);
         }

         ProcessConsumptionNotification(consumptionNotification, username);

         _unitOfWork.Commit();
      }

      [Queue(BackgroundQueues.Web)]
      public void ProcessAllInventoryUsed(string username)
      {
         //manage unmanaged consumptions
         _unitOfWork.HealthTrackConsumptionRepository.ManageNewConsumptions(username);
         _unitOfWork.Commit();

         var processingChunkSize = _propertyProvider.ConsumptionProcessingSize;
         var unprocessedCount = _unitOfWork.HealthTrackConsumptionRepository.GetUnprocessed().Count();
         while (unprocessedCount > 0)
         {
            //Get the first chunk
            var records = _unitOfWork.HealthTrackConsumptionRepository.GetUnprocessed().Take(processingChunkSize);
            foreach (var consumptionNotification in records)
            {
               unprocessedCount--;
               try
               {
                  ProcessConsumptionNotification(consumptionNotification, username);
               }
               catch (Exception exception)
               {
                  _logger.Error(exception, "Consumption processing error for {ConsumptionId}", consumptionNotification.invUsed_ID);
                  consumptionNotification.ProcessingStatus = ConsumptionProcessingStatus.Error;
                  consumptionNotification.ProcessingStatusMessage = String.Format("Processing error for consumption '{0}' : {1}"
                                             , consumptionNotification.invUsed_ID
                                             , exception.Message);
               }
            }

            var usedPaymentClasses = _unitOfWork.HealthTrackConsumptionRepository.FindUsedPaymentClasses();
            _unitOfWork.PaymentClassMappingRepository.AddNewlyUsedPaymentClasses(usedPaymentClasses);
            _unitOfWork.Commit();
         }
      }

      [Queue(BackgroundQueues.Web)]
      public void ProcessConsumptionsForExternalProduct(int externalProductId, string username)
      {
         var consumptions = _unitOfWork.HealthTrackConsumptionRepository.GetUnprocessedConsumptionNotifications(externalProductId);
         foreach (var consumptionNotification in consumptions)
         {
            ProcessConsumptionNotification(consumptionNotification, username);
         }
         _unitOfWork.Commit();
      }

      public void ProcessConsumptionNotification(ConsumptionNotificationManagement consumptionNotification, string username)
      {
         var consumptionDetails = _inventoryUsedRepository.Find(consumptionNotification.invUsed_ID);

         if (consumptionDetails == null)
         {
            consumptionNotification.ProcessingStatus = ConsumptionProcessingStatus.Ignored;
            consumptionNotification.ProcessingStatusMessage = "Original consumption record no longer exists";
            return;
         }

         // set test date to first that isn't null: container date, inventory_used dateCreated, now.
         var testDate = GetTestDateFromConsumptionInformation(consumptionDetails);

         consumptionNotification.LastModifiedBy = username;
         consumptionNotification.LastModifiedOn = DateTime.Now;

         if (!consumptionDetails.invItem_ID.HasValue || consumptionDetails.Inventory_Master == null)
         {
            var error = $"Cannot process inventory consumption notification '{consumptionNotification.invUsed_ID}' as it has no product";
            consumptionNotification.ProcessingStatus = ConsumptionProcessingStatus.Error;
            consumptionNotification.ProcessingStatusMessage = "Cannot process due to missing product";
            _logger.Warning(error);
            return;
         }

         if (!consumptionDetails.invUsed_Location.HasValue)
         {
            var error = String.Format(
               "Cannot process inventory consumption notification '{0}' as it has no location id",
               consumptionNotification.invUsed_ID);
            consumptionNotification.ProcessingStatus = ConsumptionProcessingStatus.Error;
            consumptionNotification.ProcessingStatusMessage = "Cannot process due to missing location value";
            _logger.Warning(error);
            return;
         }

         if (!consumptionDetails.invUsed_Qty.HasValue)
         {
            var error = String.Format("Cannot process inventory consumption notification '{0}' as it has no quantity", consumptionNotification.invUsed_ID);
            consumptionNotification.ProcessingStatus = ConsumptionProcessingStatus.Error;
            consumptionNotification.ProcessingStatusMessage = "Cannot process due to missing quantity";
            _logger.Warning(error);
            return;
         }

         if (string.IsNullOrWhiteSpace(consumptionDetails.Inventory_Master.Inv_SPC) && string.IsNullOrWhiteSpace(consumptionDetails.Inventory_Master.Inv_UPN))
         {
            var error =
               String.Format("Cannot process inventory consumption notification '{0}' as there is no SPC or UPN", consumptionNotification.invUsed_ID);
            consumptionNotification.ProcessingStatus = ConsumptionProcessingStatus.Error;
            consumptionNotification.ProcessingStatusMessage = "Cannot process due to missing SPC and UPN. At least one is required.";
            _logger.Warning(error);
            return;
         }

         try
         {
            // Find mapped product for used inventory item
            var productMapping =
               _unitOfWork.ExternalProductMappingRepository.GetHealthTrackProductMapping(consumptionDetails.invItem_ID.Value);

            if (productMapping != null)
            {
               if (productMapping.DeletedOn.HasValue)
               {
                  var deletedProductId = productMapping.InventoryProductId;
                  // mark as in error
                  var error =
                     String.Format(
                        "Cannot process inventory consumption notification '{0}' the Inventory product has been deleted. This consumption can either be ignored or the product can be reinstated in order to process the consumption.",
                        consumptionNotification.invUsed_ID);
                  consumptionNotification.ProcessingStatus = ConsumptionProcessingStatus.Error;
                  consumptionNotification.ProcessingStatusMessage =
                     "Cannot process due to deleted product. This consumption can either be ignored or the <a href='~\\Inventory\\Products\\Details\\" +
                     deletedProductId +
                     "' associated product> associated product </a>can be reinstated in order to process the consumption.";
                  _logger.Warning(error);
                  return;
               }
            }
            else
            {
               productMapping = _unitOfWork.AutoMapConsumedProduct(consumptionNotification, consumptionDetails);
            }
            

            if (productMapping == null || productMapping.Product == null)
            {
               consumptionNotification.ProcessingStatus = ConsumptionProcessingStatus.Error;
               consumptionNotification.ProcessingStatusMessage =
                  string.Format("Unable to find existing product or create new product for consumption");
               return;
            }

            var product = productMapping.Product;
            // Check if has required batch or serial number
            if (product.RequiresBatch && string.IsNullOrWhiteSpace(consumptionDetails.LOTNO))
            {
               consumptionNotification.ProcessingStatus = ConsumptionProcessingStatus.Error;
               consumptionNotification.ProcessingStatusMessage =
                  string.Format("Consumption requires a batch number before it can be processed");
               return;
            }

            if (product.RequiresSerial &&
                string.IsNullOrWhiteSpace(consumptionDetails.invUsed_SerialNo))
            {
               consumptionNotification.ProcessingStatus = ConsumptionProcessingStatus.Error;
               consumptionNotification.ProcessingStatusMessage =
                  string.Format("Consumption requires a serial number before it can be processed");
               return;
            }

            // Find corresponding stock locations for HealthTrack location
            var locationMapper =
               _unitOfWork.StockLocationRepository.GetHealthTrackLocationMapper(
                  consumptionDetails.invUsed_Location.Value);

            if (locationMapper == null)
            {
               var error =
                  String.Format(
                     "Failed to process used inventory item '{0}' into stock. No location mapping exists for '{1}'.",
                     consumptionNotification.invUsed_ID, consumptionDetails.invUsed_Location.Value);
               consumptionNotification.ProcessingStatus = ConsumptionProcessingStatus.Error;
               consumptionNotification.ProcessingStatusMessage = error;
               return;
            }

            var stockLocation = locationMapper.GetMappedStockLocation();

            // Mappings exist for location but cannot decide which to use
            if (!stockLocation.HasValue)
            {
               var error =
                  String.Format(
                     "Failed to process used inventory item '{0}' into stock. Unable to determine which stock location item originated from.",
                     consumptionNotification.invUsed_ID);
               consumptionNotification.ProcessingStatus = ConsumptionProcessingStatus.Error;
               consumptionNotification.ProcessingStatusMessage = error;
               return;
            }

            // Only perform deduction is stock is managed
            if (product.ManageStock)
            {
               // Create stock deduction for used inventory item
               var deduction = _unitOfWork.CreateAdjustmentFromConsumption(consumptionDetails, product.ProductId,
                  stockLocation.Value, testDate);

               //Check when the last stock take was
               var lastStockTakeBeforeConsumption =
                  _unitOfWork.ProductRepository.MostRecentStockTake(productMapping.Product, stockLocation.Value);

               // the last stock take was after the consumption date
               if (lastStockTakeBeforeConsumption.HasValue &&
                   lastStockTakeBeforeConsumption.Value > deduction.AdjustedOn)
               {
                  // archive as this consumption will not be acted upon (deducted)
                  _unitOfWork.HealthTrackConsumptionRepository.ArchiveConsumptionNotification(consumptionNotification, username);
               }
               else
               {
                  // perform the deduction
                  _deductionHelper.AdjustItem(deduction, product, username);
               }
            }
            else
            {
               // if unmanaged stock and the consumption was consumed before the UnmanageFrom date then ignore it
               if (product.ReplaceAfter == null || testDate < product.ReplaceAfter)
               {
                  _unitOfWork.HealthTrackConsumptionRepository.ArchiveConsumptionNotification(consumptionNotification, username);
               }
            }

            consumptionNotification.ProcessingStatus = ConsumptionProcessingStatus.Processed;
            consumptionNotification.ProcessingStatusMessage = "Processed:" + DateTime.Now;
         }
         catch (Exception exception)
         {
            consumptionNotification.ProcessingStatus = ConsumptionProcessingStatus.Error;
            var errorMessage = String.Format("Failed to process used inventory item {0}: {1}",
               consumptionNotification.invUsed_ID, exception.Message);

            if (exception.GetType().IsSubclassOf(typeof(StockException)))
            {
               consumptionNotification.ProcessingStatusMessage = exception.Message;
            }
            else
            {
               consumptionNotification.ProcessingStatusMessage = errorMessage;
               _logger.Error(exception, errorMessage);
            }
         }
      }

      private DateTime? GetTestDateFromConsumptionInformation(Inventory_Used consumptionDetails)
      {
         DateTime? nullableTestDate = null;
         if (consumptionDetails.container_ID.HasValue)
         {
            var htConsumption = _unitOfWork.HealthTrackConsumptionRepository.FindHealthTrackConsumptions()
               .SingleOrDefault(
                  c => c.UsedId == consumptionDetails.invUsed_ID && c.ContainerId == consumptionDetails.container_ID);

            if (htConsumption != null)
               nullableTestDate = htConsumption.testDate;

            if (nullableTestDate == null)
            {
               nullableTestDate = consumptionDetails.dateCreated;
            }
         }

         return nullableTestDate ?? (DateTime.Now);
      }
   }
}