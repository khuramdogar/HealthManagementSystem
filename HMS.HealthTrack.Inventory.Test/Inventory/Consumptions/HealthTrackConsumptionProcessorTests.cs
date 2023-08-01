using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HMS.HealthTrack.Inventory.Common.Constants;
using HMS.HealthTrack.Inventory.Test.Inventory.Mocks;
using HMS.HealthTrack.Inventory.Test.Inventory.Mocks.HealthTrackMocks;
using HMS.HealthTrack.Web.Areas.Inventory;
using HMS.HealthTrack.Web.Data.Model;
using HMS.HealthTrack.Web.Data.Model.HealthTrackInventory;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace HMS.HealthTrack.Inventory.Test.Inventory.Consumptions
{
   /// <summary>
   /// Summary description for ConsumptionProcessorTests
   /// </summary>
   [TestClass]
   public class HealthTrackConsumptionProcessorTests
   {
      #region Set up variables

      private Inventory_Used TestInventoryUsed
      {
         get
         {
            return new Inventory_Used
            {
               invUsed_ID = 1,
               container_ID = 123,
               patient_ID = 666,
               invItem_ID = 2,
               invUsed_Qty = 2,
               invUsed_SPC = "illegitimate SPC, should be in the product, not the consumption",
               invUsed_Location = 4,
               dateCreated = DateTime.Now.AddDays(-1),
               Inventory_Master = new Inventory_Master
               {
                  Inv_UPN = "UPN",
                  Inv_Description = "ProductDescription",
                  Inv_SPC = "SPC",
               }
            };
         }
      }

      private List<Inventory_Used> TestInventoryUseds
      {
         get
         {
            return new List<Inventory_Used>
            {
               TestInventoryUsed
            };
         }
      }

      private ConsumptionNotificationManagement TestConsumptionNotification
      {
         get
         {
            return new ConsumptionNotificationManagement
            {
               invUsed_ID = 1
            };
         }
      }

      private List<StockLocation> StockLocations
      {
         get { return new List<StockLocation>(); }
      }

      private List<StockLocationMapping> StockLocationMappings
      {
         get
         {
            return new List<StockLocationMapping>()
            {
               new StockLocationMapping
               {
                  HealthTrackLocationId = 4,
                  LocationId = 20,
                  Location = new StockLocation
                  {
                     LocationId = 20,
                  }
               }
            };
         }
      }

      private List<ExternalProductMapping> ProductMappings
      {
         get
         {
            return new List<ExternalProductMapping>()
            {
               new ExternalProductMapping
               {
                  ProductMappingId = 1,
                  ExternalProductId = TestInventoryUsed.invItem_ID.Value,
                  InventoryProductId = 111,
                  ProductSource = ProductMappingSource.HealthTrack,
                  Product = new Product
                  {
                     ManageStock = true,
                     ProductId = 111,
                     StockTakeItems = new List<StockTakeItem>()
                     {
                        new StockTakeItem
                        {
                           StockTake = new StockTake
                           {
                              LocationId = StockLocationMappings[0].LocationId,
                              StockTakeDate = TestInventoryUsed.dateCreated.Value.AddDays(-1),
                           },
                           Status = StockTakeItemStatus.Complete
                        }
                     },
                     ProductSettings = new List<StockSetting>()
                  }
               }
            };
         }
      }

      // Product with stock take done the day before the consumption's at the mapped location
      private List<Product> Products
      {
         get
         {
            return new List<Product>()
            {
               ProductMappings[0].Product
            };
         }
      }

      private List<Stock> Stocks
      {
         get
         {
            return new List<Stock>()
            {
               new Stock
               {
                  ProductId = ProductMappings[0].InventoryProductId,
                  Quantity = TestInventoryUsed.invUsed_Qty.Value,
                  StockId = 1,
                  StockStatus = StockStatus.Available,
                  StoredAt = StockLocationMappings[0].LocationId,
               }
            };
         }
      }

      private List<HealthTrackConsumption> HtConsumptions
      {
         get
         {
            return new List<HealthTrackConsumption>
         {
            new HealthTrackConsumption()
            {
               ContainerId = 123,
               UsedId = 1,
            }
         };
         }
      }

      private List<ConsumptionNotificationManagement> Notifications
      {
         get { return new List<ConsumptionNotificationManagement>(); }
      }

      private List<ClinicalConsumption> ClinicalConsumptions
      {
         get
         {
            return new List<ClinicalConsumption>
            {
               new ClinicalConsumption
               {
                  container_ID = 123,
                  testDate = DateTime.Now.AddDays(-1),
                  UsedId = 1,
               }
            };
         }
      }


      private const string Username = "TestUser";

      #endregion

      private HealthTrackProductConsumptionProcessor GetProcessor(MockedInventoryConsumptionProcessorData testData,
         MockedHealthTrackConsumptionProcessorData testHtData)
      {
         var uow = MockedInventoryUnitOfWorkFactory.GetConsumptionUnitOfWork(testData);


         var testHealthTrackData = new MockedHealthTrackConsumptionProcessorData(TestInventoryUseds);
         var inventoryUsedRepository = testHtData == null
            ? MockedHealthTrackRepositoryFactory.GetInventoryUsedRepository(testHealthTrackData)
            : MockedHealthTrackRepositoryFactory.GetInventoryUsedRepository(testHtData);

         var processor = new HealthTrackProductConsumptionProcessor(uow, testData.MockedLogger.Object,
            testData.MockedPropertyProvider.Object, inventoryUsedRepository);
         return processor;
      }

      [TestMethod]
      public void ProcessConsumption_SuccessfulDeduction()
      {
         var notification = TestConsumptionNotification;

         var testData = new MockedInventoryConsumptionProcessorData(ProductMappings, StockLocations,
            StockLocationMappings, Products, Stocks, HtConsumptions, Notifications, ClinicalConsumptions);
         var processor = GetProcessor(testData, null);

         processor.ProcessConsumptionNotification(notification, Username);

         Assert.AreEqual(ConsumptionProcessingStatus.Processed, notification.ProcessingStatus);
         testData.MockedAdjustments.Verify(m => m.Add(It.IsAny<StockAdjustment>()), Times.Once());
      }

      [TestMethod]
      public void ProcessConsumption_ProductDeleted_ConsumptionError()
      {
         var notification = TestConsumptionNotification;
         var product = Products[0];
         product.DeletedOn = DateTime.Now;
         product.DeletedBy = "User";

         var mapping = ProductMappings[0];
         mapping.DeletedOn = DateTime.Now;
         mapping.DeletedBy = "User";

         var testData = new MockedInventoryConsumptionProcessorData(new List<ExternalProductMapping> {mapping}, StockLocations,
            StockLocationMappings, new List<Product>{product}, Stocks, HtConsumptions, Notifications, ClinicalConsumptions);
         var processor = GetProcessor(testData, null);

         processor.ProcessConsumptionNotification(notification, Username);

         Assert.AreEqual(ConsumptionProcessingStatus.Error, notification.ProcessingStatus);
         testData.MockedAdjustments.Verify(m => m.Add(It.IsAny<StockAdjustment>()), Times.Never());
      }

      [TestMethod]
      public void ProcessConsumption_StockNotManaged_NoDeduction()
      {
         var notification = TestConsumptionNotification;

         var productMappings = ProductMappings;
         productMappings[0].Product.ManageStock = false;

         var testData = new MockedInventoryConsumptionProcessorData(productMappings, StockLocations,
            StockLocationMappings, Products, Stocks, HtConsumptions, Notifications, ClinicalConsumptions);
         var processor = GetProcessor(testData, null);

         processor.ProcessConsumptionNotification(notification, Username);

         Assert.AreEqual(ConsumptionProcessingStatus.Processed, notification.ProcessingStatus);
         testData.MockedAdjustments.Verify(m => m.Add(It.IsAny<StockAdjustment>()), Times.Never());
         testData.MockedStock.Verify(m => m.Add(It.IsAny<Stock>()), Times.Never());
      }

      [TestMethod]
      public void ProcessConsumption_SuccessfulDeduction_NegativeStock()
      {
         var notification = TestConsumptionNotification;
         var stocks = Stocks;
         stocks[0].Quantity = 0;

         var productMappings = ProductMappings;

         var testData = new MockedInventoryConsumptionProcessorData(productMappings, StockLocations,
            StockLocationMappings, Products, stocks, HtConsumptions, Notifications, ClinicalConsumptions);
         var processor = GetProcessor(testData, null);

         processor.ProcessConsumptionNotification(notification, Username);

         Assert.AreEqual(ConsumptionProcessingStatus.Processed, notification.ProcessingStatus);
         testData.MockedAdjustments.Verify(m => m.Add(It.Is<StockAdjustment>(sd => sd.Quantity == 2)), Times.Once(),
            "Negative stock deducted");

         var negativeStocks = productMappings[0].Product.Stocks;
         Assert.AreEqual(1, negativeStocks.Count(), "Addition of negative stock entity to product");
         Assert.AreEqual(true, negativeStocks.First().IsNegative, "Stock is created as Negative Stock");
         Assert.AreEqual(2, negativeStocks.First().ReceivedQuantity, "Received quantity");
         Assert.AreEqual(0, negativeStocks.First().Quantity, "Available stock");
         Assert.AreEqual(StockStatus.Deducted, negativeStocks.First().StockStatus, "Stock has been deducted");
      }

      [TestMethod]
      public void ProcessConsumption_SuccessfulDeductionIncompleteStockTake()
      {
         var notification = TestConsumptionNotification;
         var products = Products;
         products[0].StockTakeItems.First().Status = StockTakeItemStatus.Created;

         var stocks = Stocks;

         var testData = new MockedInventoryConsumptionProcessorData(ProductMappings, StockLocations,
            StockLocationMappings, products, stocks, HtConsumptions, Notifications, ClinicalConsumptions);
         var processor = GetProcessor(testData, null);

         processor.ProcessConsumptionNotification(notification, Username);

         Assert.AreEqual(ConsumptionProcessingStatus.Processed, notification.ProcessingStatus);
         testData.MockedAdjustments.Verify(m => m.Add(It.IsAny<StockAdjustment>()), Times.Once());
         Assert.AreEqual(0, stocks[0].Quantity);
      }

      [TestMethod]
      public void ProcessConsumption_NoHTProductId()
      {
         var notification = TestConsumptionNotification;

         // Remove HT product ID
         var consumptions = TestInventoryUseds;
         consumptions[0].invItem_ID = null;

         var testData = new MockedInventoryConsumptionProcessorData(ProductMappings, StockLocations,
            StockLocationMappings, Products, Stocks, HtConsumptions, Notifications, ClinicalConsumptions);
         var testHtData = new MockedHealthTrackConsumptionProcessorData(consumptions);

         var processor = GetProcessor(testData, testHtData);
         processor.ProcessConsumptionNotification(notification, Username);

         Assert.AreEqual(ConsumptionProcessingStatus.Error, notification.ProcessingStatus);
         testData.MockedAdjustments.Verify(m => m.Add(It.IsAny<StockAdjustment>()), Times.Never());
      }

      [TestMethod]
      public void ProcessConsumption_NoHTLocationId()
      {
         var notification = TestConsumptionNotification;

         // Remove HT location
         var consumptions = TestInventoryUseds;
         consumptions[0].invUsed_Location = null;

         var testData = new MockedInventoryConsumptionProcessorData(ProductMappings, StockLocations,
            StockLocationMappings, Products, Stocks, HtConsumptions, Notifications, ClinicalConsumptions);
         var testHtData = new MockedHealthTrackConsumptionProcessorData(consumptions);
         var processor = GetProcessor(testData, testHtData);

         processor.ProcessConsumptionNotification(notification, Username);

         Assert.AreEqual(ConsumptionProcessingStatus.Error, notification.ProcessingStatus);
         testData.MockedAdjustments.Verify(m => m.Add(It.IsAny<StockAdjustment>()), Times.Never());
      }

      [TestMethod]
      public void ProcessConsumption_DuplicateProductMappings()
      {
         // Add duplicate product mapping
         var productMappings = ProductMappings;
         productMappings.Add(new ExternalProductMapping
         {
            ProductMappingId = 2,
            ExternalProductId = TestInventoryUsed.invItem_ID.Value,
            InventoryProductId = 123,
            ProductSource = ProductMappingSource.HealthTrack
         });

         var notification = TestConsumptionNotification;

         var testData = new MockedInventoryConsumptionProcessorData(productMappings, StockLocations,
            StockLocationMappings, Products, Stocks, HtConsumptions, Notifications, ClinicalConsumptions);
         var processor = GetProcessor(testData, null);

         processor.ProcessConsumptionNotification(notification, Username);

         Assert.AreEqual(ConsumptionProcessingStatus.Error, notification.ProcessingStatus);
         testData.MockedAdjustments.Verify(m => m.Add(It.IsAny<StockAdjustment>()), Times.Never());
      }

      [TestMethod]
      public void ProcessConsumption_NoLocationMapping()
      {
         var notification = TestConsumptionNotification;

         // Remove stock location mappings
         var emptyStockLocationMappings = new List<StockLocationMapping>();

         var testData = new MockedInventoryConsumptionProcessorData(ProductMappings, StockLocations,
            emptyStockLocationMappings, Products, Stocks, HtConsumptions, Notifications, ClinicalConsumptions);
         var processor = GetProcessor(testData, null);

         processor.ProcessConsumptionNotification(notification, Username);

         Assert.AreEqual(ConsumptionProcessingStatus.Error, notification.ProcessingStatus);
         testData.MockedAdjustments.Verify(m => m.Add(It.IsAny<StockAdjustment>()), Times.Never());
      }

      [TestMethod]
      public void ProcessConsumption_TooManyLocationMappings()
      {
         // Add second location mapping
         var locationMappings = StockLocationMappings;
         locationMappings.Add(new StockLocationMapping
         {
            HealthTrackLocationId = 4,
            LocationId = 55,
            Location = new StockLocation
            {
               LocationId = 55,
            }
         });

         var notification = TestConsumptionNotification;
         var testData = new MockedInventoryConsumptionProcessorData(ProductMappings, StockLocations,
            locationMappings, Products, Stocks, HtConsumptions, Notifications, ClinicalConsumptions);
         var processor = GetProcessor(testData, null);

         processor.ProcessConsumptionNotification(notification, Username);

         Assert.AreEqual(ConsumptionProcessingStatus.Error, notification.ProcessingStatus);
         testData.MockedAdjustments.Verify(m => m.Add(It.IsAny<StockAdjustment>()), Times.Never());
      }

      [TestMethod]
      public void ProcessConsumption_ProductWithoutStockTake_SuccessfulDeduction()
      {
         // Remove stock take from product
         var productMappings = ProductMappings;
         productMappings[0].Product.StockTakeItems = null;

         var stocks = Stocks;
         var notification = TestConsumptionNotification;
         var testData = new MockedInventoryConsumptionProcessorData(productMappings, StockLocations,
            StockLocationMappings, Products, stocks, HtConsumptions, Notifications, ClinicalConsumptions);
         var processor = GetProcessor(testData, null);

         processor.ProcessConsumptionNotification(notification, Username);

         Assert.AreEqual(ConsumptionProcessingStatus.Processed, notification.ProcessingStatus,
            notification.ProcessingStatusMessage);
         testData.MockedAdjustments.Verify(m => m.Add(It.IsAny<StockAdjustment>()), Times.Once());
         testData.MockedStock.Verify(m => m.Add(It.IsAny<Stock>()), Times.Never());
         Assert.AreEqual(0, stocks[0].Quantity, "Remaining stock");
      }

      [TestMethod]
      public void ProcessConsumption_ProductWithStockTakeAfterConsumption_Archived()
      {
         var notification = TestConsumptionNotification;

         // Set stock date date after consumption
         var productMappings = ProductMappings;
         productMappings[0].Product.StockTakeItems.Single().StockTake.StockTakeDate =
            ClinicalConsumptions[0].testDate.Value.AddDays(1);

         var testData = new MockedInventoryConsumptionProcessorData(productMappings, StockLocations,
            StockLocationMappings, Products, Stocks, HtConsumptions, Notifications, ClinicalConsumptions);
         var processor = GetProcessor(testData, null);

         processor.ProcessConsumptionNotification(notification, Username);

         Assert.AreEqual(ConsumptionProcessingStatus.Processed, notification.ProcessingStatus,
            notification.ProcessingStatusMessage);
         Assert.IsNotNull(notification.ArchivedOn);
         testData.MockedAdjustments.Verify(m => m.Add(It.IsAny<StockAdjustment>()), Times.Never());
      }

      /// <summary>
      /// No deductions will be created against the new product as default is for ManageStock to be false
      /// </summary>
      [TestMethod]
      public void ProcessConsumption_HasSpc_NoMapping_NoMatch_NewProductNoDeductions()
      {
         var notification = TestConsumptionNotification;

         var products = Products;
         products[0].SPC = "DifferentSPC";

         var productMappings = new List<ExternalProductMapping>();

         var testData = new MockedInventoryConsumptionProcessorData(productMappings, StockLocations,
            StockLocationMappings, products, Stocks, HtConsumptions, Notifications, ClinicalConsumptions);
         var processor = GetProcessor(testData, null);

         processor.ProcessConsumptionNotification(notification, Username);

         Assert.AreEqual(ConsumptionProcessingStatus.Processed, notification.ProcessingStatus,
            notification.ProcessingStatusMessage);
         testData.MockedProducts.Verify(m => m.Add(It.Is<Product>(p =>
            p.SPC == "SPC" &&
            p.Stocks.Count == 0)), Times.Once(), "Product added with no stock");

         testData.MockedExternalProductMappings.Verify(
            m => m.Add(It.Is<ExternalProductMapping>(epm => epm.ProductSource == ProductMappingSource.HealthTrack)),
            "Product mapping added");
         testData.MockedAdjustments.Verify(m => m.Add(It.Is<StockAdjustment>(sd => sd.Quantity == 2)), Times.Never(),
            "No deductions recorded");
      }

      [TestMethod]
      public void ProcessConsumption_HasSpc_StockManaged_CreateMapping()
      {
         var notification = TestConsumptionNotification;

         var products = Products;
         products[0].SPC = "SPC";
         products[0].ManageStock = true;

         var productMappings = new List<ExternalProductMapping>();

         var testData = new MockedInventoryConsumptionProcessorData(productMappings, StockLocations,
            StockLocationMappings, products, Stocks, HtConsumptions, Notifications, ClinicalConsumptions);
         var processor = GetProcessor(testData, null);

         processor.ProcessConsumptionNotification(notification, Username);

         Assert.AreEqual(ConsumptionProcessingStatus.Processed, notification.ProcessingStatus,
            notification.ProcessingStatusMessage);
         Debug.Write(notification.ProcessingStatusMessage);
         testData.MockedExternalProductMappings.Verify(m => m.Add(It.IsAny<ExternalProductMapping>()), Times.Once());
      }

      [TestMethod]
      public void ProcessConsumption_HasSpc_StockNotManagedCreateMapping()
      {
         var notification = TestConsumptionNotification;

         var products = Products;
         products[0].SPC = "SPC";
         products[0].ManageStock = false;

         var productMappings = new List<ExternalProductMapping>();

         var testData = new MockedInventoryConsumptionProcessorData(productMappings, StockLocations,
            StockLocationMappings, products, Stocks, HtConsumptions, Notifications, ClinicalConsumptions);
         var processor = GetProcessor(testData, null);

         processor.ProcessConsumptionNotification(notification, Username);

         Assert.AreEqual(ConsumptionProcessingStatus.Processed, notification.ProcessingStatus,
            notification.ProcessingStatusMessage);
         Debug.Write(notification.ProcessingStatusMessage);
         testData.MockedExternalProductMappings.Verify(m => m.Add(It.IsAny<ExternalProductMapping>()), Times.Once());
      }

      [TestMethod]
      public void ProcessConsumption_RequiresBatch_BatchMissing()
      {
         var notification = TestConsumptionNotification;

         var productMappings = ProductMappings;
         productMappings[0].Product.ProductSettings.Add(new StockSetting()
         {
            SettingId = InventoryConstants.StockSettings.RequiresBatchNumber
         });

         var testData = new MockedInventoryConsumptionProcessorData(productMappings, StockLocations,
            StockLocationMappings, Products, Stocks, HtConsumptions, Notifications, ClinicalConsumptions);
         var processor = GetProcessor(testData, null);

         processor.ProcessConsumptionNotification(notification, Username);

         Assert.IsTrue(productMappings[0].Product.RequiresBatch);
         Assert.AreEqual(ConsumptionProcessingStatus.Error, notification.ProcessingStatus,
            notification.ProcessingStatusMessage);
         testData.MockedAdjustments.Verify(m => m.Add(It.IsAny<StockAdjustment>()), Times.Never());
         testData.MockedStock.Verify(m => m.Add(It.IsAny<Stock>()), Times.Never());
      }

      [TestMethod]
      public void ProcessConsumption_RequiresBatch_BatchSupplied()
      {
         var notification = TestConsumptionNotification;
         var batchNumber = "BATCH";
         var consumptions = TestInventoryUseds;
         consumptions[0].LOTNO = batchNumber;

         var products = Products;
         products[0].ProductSettings.Add(new StockSetting()
         {
            SettingId = InventoryConstants.StockSettings.RequiresBatchNumber
         });

         var testData = new MockedInventoryConsumptionProcessorData(ProductMappings, StockLocations,
            StockLocationMappings, products, Stocks, HtConsumptions, Notifications, ClinicalConsumptions);

         var testHtData = new MockedHealthTrackConsumptionProcessorData(consumptions);
         var processor = GetProcessor(testData, testHtData);

         processor.ProcessConsumptionNotification(notification, Username);

         Assert.IsTrue(products[0].RequiresBatch);
         Assert.AreEqual(ConsumptionProcessingStatus.Processed, notification.ProcessingStatus);
         testData.MockedAdjustments.Verify(
            m => m.Add(It.Is<StockAdjustment>(sd => sd.StockAdjustmentStocks.First().Stock.BatchNumber == batchNumber)),
            Times.Once());
         testData.MockedStock.Verify(m => m.Add(It.IsAny<Stock>()), Times.Never());
      }

      [TestMethod]
      public void ProcessConsumption_RequiresSerial_SerialMissing()
      {
         var notification = TestConsumptionNotification;

         var productMappings = ProductMappings;
         productMappings[0].Product.ProductSettings.Add(new StockSetting()
         {
            SettingId = InventoryConstants.StockSettings.RequiresSerialNumber
         });

         var testData = new MockedInventoryConsumptionProcessorData(productMappings, StockLocations,
            StockLocationMappings, Products, Stocks, HtConsumptions, Notifications, ClinicalConsumptions);
         var processor = GetProcessor(testData, null);

         processor.ProcessConsumptionNotification(notification, Username);

         Assert.IsTrue(productMappings[0].Product.RequiresSerial);
         Assert.AreEqual(ConsumptionProcessingStatus.Error, notification.ProcessingStatus);
         testData.MockedAdjustments.Verify(m => m.Add(It.IsAny<StockAdjustment>()), Times.Never());
         testData.MockedStock.Verify(m => m.Add(It.IsAny<Stock>()), Times.Never());
      }

      [TestMethod]
      public void ProcessConsumption_RequiresSerial_SerialSupplied_SplitStock()
      {
         var notification = TestConsumptionNotification;
         var serialNumber = "SERIAL";
         var consumptions = TestInventoryUseds;
         consumptions[0].invUsed_SerialNo = serialNumber;

         var products = Products;
         products[0].ProductSettings.Add(new StockSetting()
         {
            SettingId = InventoryConstants.StockSettings.RequiresSerialNumber
         });

         var testData = new MockedInventoryConsumptionProcessorData(ProductMappings, StockLocations,
            StockLocationMappings, products, Stocks, HtConsumptions, Notifications, ClinicalConsumptions);

         var testHtData = new MockedHealthTrackConsumptionProcessorData(consumptions);
         var processor = GetProcessor(testData, testHtData);

         processor.ProcessConsumptionNotification(notification, Username);

         Assert.IsTrue(products[0].RequiresSerial);
         Assert.AreEqual(ConsumptionProcessingStatus.Processed, notification.ProcessingStatus);
         // Verify new stock added with supplied Serial Number
         testData.MockedAdjustments.Verify(
            m =>
               m.Add(
                  It.Is<StockAdjustment>(
                     sd =>
                        sd.StockAdjustmentStocks.First().Stock.SerialNumber == serialNumber &&
                        sd.StockAdjustmentStocks.First().Stock.StockId == 0)), Times.Once());
      }

      [TestMethod]
      public void ProcessConsumption_RequiresSerial_SerialSupplied_NoSplitStock()
      {
         var notification = TestConsumptionNotification;

         var serialNumber = "SERIAL";
         var consumptions = TestInventoryUseds;
         consumptions[0].invUsed_SerialNo = serialNumber;
         consumptions[0].invUsed_Qty = 1;

         var products = Products;
         products[0].ProductSettings.Add(new StockSetting()
         {
            SettingId = InventoryConstants.StockSettings.RequiresSerialNumber
         });

         var stocks = Stocks;
         stocks[0].Quantity = 1;

         var testData = new MockedInventoryConsumptionProcessorData(ProductMappings, StockLocations,
            StockLocationMappings, products, stocks, HtConsumptions, Notifications, ClinicalConsumptions);

         var testHtData = new MockedHealthTrackConsumptionProcessorData(consumptions);
         var processor = GetProcessor(testData, testHtData);

         processor.ProcessConsumptionNotification(notification, Username);

         Assert.IsTrue(products[0].RequiresSerial);
         Debug.WriteLine(notification.ProcessingStatusMessage);
         Assert.AreEqual(ConsumptionProcessingStatus.Processed, notification.ProcessingStatus);
         testData.MockedAdjustments.Verify(
            m =>
               m.Add(
                  It.Is<StockAdjustment>(
                     sd =>
                        sd.StockAdjustmentStocks.First().Stock.SerialNumber == serialNumber &&
                        sd.StockAdjustmentStocks.First().Stock.StockId != 0)), Times.Once());
         testData.MockedStock.Verify(m => m.Add(It.IsAny<Stock>()), Times.Never());
      }

      [TestMethod]
      public void ProcessConsumption_UnmanagedStock_ConsumedBeforeReplaceAfter_Archived()
      {
         var notification = TestConsumptionNotification;

         var productMappings = ProductMappings;
         productMappings[0].Product.ReplaceAfter = DateTime.Now;
         productMappings[0].Product.ManageStock = false;

         var testData = new MockedInventoryConsumptionProcessorData(productMappings, StockLocations,
            StockLocationMappings, Products, Stocks, HtConsumptions, Notifications, ClinicalConsumptions);
         var processor = GetProcessor(testData, null);

         processor.ProcessConsumptionNotification(notification, Username);

         Assert.AreEqual(ConsumptionProcessingStatus.Processed, notification.ProcessingStatus);
         testData.MockedAdjustments.Verify(m => m.Add(It.IsAny<StockAdjustment>()), Times.Never());

         Assert.IsNotNull(notification.ArchivedBy);
         Assert.IsNotNull(notification.ArchivedOn);
      }

      [TestMethod]
      public void ProcessConsumption_UnmanagedStock_ConsumedAfterReplaceAfter_NotArchived()
      {
         var notification = TestConsumptionNotification;

         var productMappings = ProductMappings;
         productMappings[0].Product.ReplaceAfter = DateTime.Now.AddDays(-4);
         productMappings[0].Product.ManageStock = false;

         var testData = new MockedInventoryConsumptionProcessorData(productMappings, StockLocations,
            StockLocationMappings, Products, Stocks, HtConsumptions, Notifications, ClinicalConsumptions);
         var processor = GetProcessor(testData, null);

         processor.ProcessConsumptionNotification(notification, Username);

         Assert.AreEqual(ConsumptionProcessingStatus.Processed, notification.ProcessingStatus);
         testData.MockedAdjustments.Verify(m => m.Add(It.IsAny<StockAdjustment>()), Times.Never());

         Assert.IsNull(notification.ArchivedBy);
         Assert.IsNull(notification.ArchivedOn);
      }
   }
}