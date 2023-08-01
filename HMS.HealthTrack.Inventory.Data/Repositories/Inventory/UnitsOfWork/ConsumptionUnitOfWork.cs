using System;
using System.Linq;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Helpers;
using HMS.HealthTrack.Web.Data.Model.HealthTrackInventory;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork
{
   public class ConsumptionUnitOfWork : IConsumptionUnitOfWork
   {
      private readonly IDbContextInventoryContext _context;
      private readonly ICustomLogger _logger;

      public ConsumptionUnitOfWork(IDbContextInventoryContext context, IPropertyProvider propertyProvider, ICustomLogger logger)
      {
         _context = context;
         _logger = logger;
         ExternalProductMappingRepository = new ExternalProductMappingRepository(context, logger);
         HealthTrackConsumptionRepository = new HealthTrackConsumptionRepository(context);
         ProductRepository = new ProductRepository(context, propertyProvider);
         StockLocationRepository = new StockLocationRepository(context);
         StockRepository = new StockRepository(context);
         PaymentClassMappingRepository = new PaymentClassMappingRepository(context);
         StockAdjustmentRepository = new StockAdjustmentRepository(context);
      }

      public IPaymentClassMappingRepository PaymentClassMappingRepository { get; }

      public void Commit()
      {
         _context.ObjectContext.SaveChanges();
      }

      public IExternalProductMappingRepository ExternalProductMappingRepository { get; }

      public IHealthTrackConsumptionRepository HealthTrackConsumptionRepository { get; }

      public IProductRepository ProductRepository { get; }

      public IStockLocationRepository StockLocationRepository { get; }

      public IStockRepository StockRepository { get; }

      public IStockAdjustmentRepository StockAdjustmentRepository { get; }

      public ExternalProductMapping AutoMapConsumedProduct(ConsumptionNotificationManagement consumptionNotification, Inventory_Used consumptionDetails)
      {
         var product = FindOrCreateProduct(consumptionDetails);
         if (product == null)
            return null;

         var externalMapping = CreateHealthTrackProductMapping(consumptionDetails, product);

         return externalMapping;
      }


      public ItemAdjustment CreateAdjustmentFromConsumption(Inventory_Used consumptionDetails, int productId, int stockLocationId, DateTime? testDate)
      {
         if (!consumptionDetails.invUsed_Qty.HasValue)
            throw new Exception($"Cannot create adjustment from consumption '{consumptionDetails.invUsed_ID}' with no quantity.");

         var quantity = Convert.ToInt32(consumptionDetails.invUsed_Qty.Value);
         var adjustment = StockAdjustmentHelper.CreateItemAdjustment(productId, stockLocationId, quantity, false,
            AdjustmentSource.HealthTrack, consumptionDetails.userCreated);

         adjustment.AdjustedOn = testDate ?? DateTime.Now;
         adjustment.BatchNumber = consumptionDetails.LOTNO;
         adjustment.PatientId = consumptionDetails.patient_ID;
         adjustment.SerialNumber = consumptionDetails.invUsed_SerialNo;

         if (consumptionDetails.container_ID.HasValue)
            adjustment.ClinicalRecordId = Convert.ToInt32(consumptionDetails.container_ID.Value);

         return adjustment;
      }

      private Product CreateProductFromConsumption(Inventory_Used consumptionDetails)
      {
         var description = consumptionDetails.invDescription;
         if (string.IsNullOrWhiteSpace(description) && consumptionDetails.Inventory_Master != null) description = consumptionDetails.Inventory_Master.Inv_Description;

         var product = ProductRepository.Create("ConsumptionProcessor");
         product.Description = description;
         product.LPC = consumptionDetails.invUsed_LPC;
         product.SPC = consumptionDetails.Inventory_Master != null
            ? consumptionDetails.Inventory_Master.Inv_SPC
            : string.Empty;

         if (consumptionDetails.Inventory_Master?.Inv_UPN != null)
            product.ScanCodes.Add(new ScanCode {Product = product, Value = consumptionDetails.Inventory_Master.Inv_UPN});

         ProductRepository.Add(product);
         return product;
      }

      public Product FindOrCreateProduct(Consumption consumptionDetails)
      {
         // Attempt to match on combination of SPC and UPN where not null
         var spc = consumptionDetails.SPC;

         var upn = consumptionDetails.UPC;

         if (string.IsNullOrWhiteSpace(spc) && string.IsNullOrWhiteSpace(upn)) return null;

         var products = ProductRepository.FindAll();
         if (!string.IsNullOrWhiteSpace(spc))
            products = products.Where(p => p.SPC == spc);

         if (!string.IsNullOrWhiteSpace(upn))
            products = products.Where(p => p.ScanCodes.Any(c => c.Value == upn));

         if (products.Count() == 1)
            return products.Single();

         // no matches in db, search local for products yet to be committed
         var localProducts = ProductRepository.FindAllLocal();
         if (!string.IsNullOrWhiteSpace(spc))
            localProducts = localProducts.Where(p => p.SPC == spc);

         if (!string.IsNullOrWhiteSpace(upn))
            localProducts = localProducts.Where(p => p.ScanCodes.Any(c => c.Value == upn));

         // single match in local by db
         var matchingProducts = localProducts.ToList();
         if (matchingProducts.Count == 1) return matchingProducts.Single();

         return CreateProductFromConsumption(consumptionDetails);
      }

      public ItemAdjustment CreateAdjustmentFromConsumption(Consumption consumptionDetails, int productId, int stockLocationId, DateTime consumedOn)
      {
         var quantity = consumptionDetails.Quantity;
         var adjustment = StockAdjustmentHelper.CreateItemAdjustment(productId, stockLocationId, quantity, false,
            AdjustmentSource.HealthTrack, consumptionDetails.Consumer);

         adjustment.AdjustedOn = consumedOn;
         adjustment.BatchNumber = consumptionDetails.BatchNumber;
         adjustment.SerialNumber = consumptionDetails.SerialNumber;
         return adjustment;
      }

      private Product CreateProductFromConsumption(Consumption consumptionDetails)
      {
         var description = consumptionDetails.Description;
         var product = ProductRepository.Create("ConsumptionProcessor");
         product.Description = description;
         product.SPC = consumptionDetails.SPC;
         product.ScanCodes.Add(new ScanCode { Product = product, Value = consumptionDetails.UPC });
         ProductRepository.Add(product);
         return product;
      }

      /// <summary>
      ///    Attempts to match a consumption to a product based on SPC or UPN.
      ///    If there are multiple products with matching spc/upn,
      /// </summary>
      /// <param name="consumptionDetails"></param>
      /// <returns></returns>
      internal Product FindOrCreateProduct(Inventory_Used consumptionDetails)
      {
         if (consumptionDetails.Inventory_Master == null)
         {
            _logger.Error(
               "Unable to find associated HealthTrack product with ID {InvItem_ID} associated with the consumption provided with {InvUsed_ID}",
               consumptionDetails.invItem_ID, consumptionDetails.invUsed_ID);
            throw new Exception("Unable to access product associated with consumption");
         }

         // Attempt to match on combination of SPC and UPN where not null
         var spc = consumptionDetails.Inventory_Master.Inv_SPC;

         var upn = consumptionDetails.Inventory_Master.Inv_UPN;

         if (string.IsNullOrWhiteSpace(spc) && string.IsNullOrWhiteSpace(upn)) return null;

         var products = ProductRepository.FindAll();
         if (!string.IsNullOrWhiteSpace(spc))
            products = products.Where(p => p.SPC == spc);

         if (!string.IsNullOrWhiteSpace(upn))
            products = products.Where(p => p.ScanCodes.Any(c => c.Value == upn));

         if (products.Count() == 1)
            return products.Single();

         // no matches in db, search local for products yet to be committed
         var localProducts = ProductRepository.FindAllLocal();
         if (!string.IsNullOrWhiteSpace(spc))
            localProducts = localProducts.Where(p => p.SPC == spc);

         if (!string.IsNullOrWhiteSpace(upn))
            localProducts = localProducts.Where(p => p.ScanCodes.Any(c => c.Value == upn));

         // single match in local by db
         var matchingProducts = localProducts.ToList();
         if (matchingProducts.Count == 1) return matchingProducts.Single();

         return CreateProductFromConsumption(consumptionDetails);
      }

      private ExternalProductMapping CreateHealthTrackProductMapping(Inventory_Used consumptionDetails, Product product)
      {
         var externalMapping = new ExternalProductMapping
         {
            CreatedBy = "Automatic match",
            CreatedOn = DateTime.Now,
            ExternalProductId = consumptionDetails.invItem_ID.GetValueOrDefault(),
            LastModifiedOn = DateTime.Now,
            LastModifiedBy = "Automatic match",
            Product = product,
            ProductSource = ProductMappingSource.HealthTrack
         };

         ExternalProductMappingRepository.Add(externalMapping);
         return externalMapping;
      }
   }

   public interface IConsumptionUnitOfWork
   {
      IExternalProductMappingRepository ExternalProductMappingRepository { get; }
      IStockLocationRepository StockLocationRepository { get; }
      IStockRepository StockRepository { get; }
      IHealthTrackConsumptionRepository HealthTrackConsumptionRepository { get; }
      IProductRepository ProductRepository { get; }
      IPaymentClassMappingRepository PaymentClassMappingRepository { get; }
      IStockAdjustmentRepository StockAdjustmentRepository { get; }

      void Commit();
      ExternalProductMapping AutoMapConsumedProduct(ConsumptionNotificationManagement consumptionNotification, Inventory_Used consumptionDetails);
      ItemAdjustment CreateAdjustmentFromConsumption(Inventory_Used consumptionDetails, int productId, int stockLocationId, DateTime? testDate);
      Product FindOrCreateProduct(Consumption consumption);
      ItemAdjustment CreateAdjustmentFromConsumption(Consumption consumptionDetails, int productId, int stockLocationId, DateTime consumedOn);
   }
}