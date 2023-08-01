using System;
using System.Collections.Generic;
using System.Linq;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Inventory.Common.Constants;
using HMS.HealthTrack.Inventory.Test.Inventory.Mocks;
using HMS.HealthTrack.Web.Areas.Inventory.Models.Products;
using HMS.HealthTrack.Web.Areas.Inventory.Utils;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace HMS.HealthTrack.Inventory.Test.Inventory.Products
{
   /// <summary>
   /// Summary description for ProductMergeTests
   /// </summary>
   [TestClass]
   public class ProductMergeTests
   {
      private readonly ICustomLogger _mockedLogger = new Mock<ICustomLogger>().Object;

      private readonly List<string> _excludedMergeProperties = new List<string>
      {
         Nameof<Product>.Property(p => p.CreatedOn),
         Nameof<Product>.Property(p => p.CreatedBy),
         Nameof<Product>.Property(p => p.DeletedOn),
         Nameof<Product>.Property(p => p.DeletedBy),
         Nameof<Product>.Property(p => p.LastModifiedBy),
         Nameof<Product>.Property(p => p.LastModifiedOn),
         Nameof<Product>.Property(p => p.InError), // calculated after merge
         Nameof<Product>.Property(p => p.ProductImports), // straight up ignored. No reason to merge.
         Nameof<Product>.Property(p => p.PriceModelId), // not used
         
         // Manual merge
         Nameof<Product>.Property(p => p.LedgerId), // Manual merge configured as ViewModel uses GLC
         Nameof<Product>.Property(p => p.ProductCategories),
         Nameof<Product>.Property(p => p.ProductSettings),

         // Entities
         Nameof<Product>.Property(p => p.PrimarySupplierCompany), // Entity
         Nameof<Product>.Property(p => p.SecondarySupplierCompany), // Entity
         Nameof<Product>.Property(p => p.GeneralLedger), // Entity

         // Collections - to be managed by ProductRepository Merge()
         Nameof<Product>.Property(p => p.ExternalProductMappings),
         Nameof<Product>.Property(p => p.OrderItems),
         Nameof<Product>.Property(p => p.StockRequests),
         Nameof<Product>.Property(p => p.Stocks),
         Nameof<Product>.Property(p => p.StockSetItems),
         Nameof<Product>.Property(p => p.StockTakeItems),

         // Product partial 
         Nameof<Product>.Property(p => p.RequiresBatch),
         Nameof<Product>.Property(p => p.RequiresSerial),
         Nameof<Product>.Property(p => p.Unorderable),
         Nameof<Product>.Property(p => p.HasHadStockTake),
         Nameof<Product>.Property(p => p.Unclassified),
         Nameof<Product>.Property(p => p.InStock),

         //Order channels not supported in merges
         Nameof<Product>.Property(p => p.OrderChannelProducts)
      };

      #region TestVariables

      private static string TestUser = "UnitTest";

      private static readonly Category CategoryOne = new Category
      {
         CategoryId = 1,
         CategoryName = "Category One",
         StockSettings = new List<StockSetting>
         {
            new StockSetting
            {
               SettingId = InventoryConstants.StockSettings.RequiresBatchNumber
            }
         }
      };

      private static readonly ProductCategory ProductCategoryOne = new ProductCategory
      {
         CategoryId = 1,
         Category = CategoryOne
      };

      private static readonly Category CategoryTwo = new Category
      {
         CategoryId = 2,
         CategoryName = "Category Two",
         StockSettings = new List<StockSetting>
         {
            new StockSetting
            {
               SettingId = InventoryConstants.StockSettings.RequiresSerialNumber
            }
         }
      };

      private static readonly ProductCategory ProductCategoryTwo = new ProductCategory
      {
         CategoryId = 2,
         Category = CategoryTwo
      };

      private static readonly Category CategoryThree = new Category
      {
         CategoryId = 3,
         CategoryName = "Category Three",
         StockSettings = new List<StockSetting>
         {
            new StockSetting
            {
               SettingId = InventoryConstants.StockSettings.Unorderable
            }
         }
      };

      private static readonly ProductCategory ProductCategoryThree = new ProductCategory
      {
         CategoryId = 3,
         Category = CategoryThree
      };

      private static readonly ProductPrice PublicPrice = new ProductPrice
      {
         BuyCurrency = "AUD",
         BuyCurrencyRate = "??",
         BuyPrice = 10,
         PriceId = 1,
         PriceTypeId = 1,
         PriceType = new PriceType
         {
            PriceTypeId = 1,
            Name = "Public"
         },
         SellPrice = 5,
      };

      private static readonly ProductPrice PrivatePrice = new ProductPrice
      {
         BuyCurrency = "USD",
         BuyCurrencyRate = "!!",
         BuyPrice = 99,
         PriceId = 2,
         PriceTypeId = 2,
         PriceType = new PriceType
         {
            PriceTypeId = 2,
            Name = "Private"
         },
         SellPrice = 50,
      };

      private static readonly List<ProductCategory> ProductCategories = new List<ProductCategory>
      {
         ProductCategoryOne,
         ProductCategoryTwo
      };

      private static readonly List<ProductPrice> ProductPrices = new List<ProductPrice>
      {
         PublicPrice,
         PrivatePrice
      };

      private readonly Product _toKeep = new Product
      {
         Description = "Test product to keep",
         ExternalProductMappings = ProductMappings,
         GLC = "00000-00000",
         LedgerId = 1,
         ManageStock = false,
         Prices = ProductPrices,
         PrimarySupplier = 1,
         ProductCategories = ProductCategories,
         ProductId = 1
      };

      private readonly Product _toDelete = new Product
      {
         Description = "Test product to delete",
         ExternalProductMappings = ProductMappingsV2,
         GLC = "111111-11101010",
         LedgerId = 2,
         ManageStock = false,
         Prices = ProductPrices,
         PrimarySupplier = 2,
         ProductCategories = ProductCategories,
         ProductId = 9
      };

      private static readonly List<ExternalProductMapping> ProductMappings = new List<ExternalProductMapping>
      {
         new ExternalProductMapping
         {
            ExternalProductId = 11,
            InventoryProductId = 1
         }
      };

      private static readonly List<ExternalProductMapping> ProductMappingsV2 = new List<ExternalProductMapping>
      {
         new ExternalProductMapping
         {
            ExternalProductId = 99,
            InventoryProductId = 9
         }
      };

      #endregion

      /// <summary>
      /// This test checks that all properties of the Product entity are accounted for when merging products.
      /// EntityCollections are merged in ProductRepository Merge(), once accounted for add collection to the list of ignored properties.
      /// If a property can be directly copied (e.g. a string) it can be added to the MergeProductsViewModel and it will be automatically merged.
      /// Properties requiring intervention upon merging (e.g. categories) will require a manual merge step added to ManualMergeFunctions class then added for ignore.
      /// </summary>
      [TestMethod]
      public void CompareProductProperties()
      {
         var productProperties = new Product().GetType().GetProperties();

         var mergeModelProperties = new MergeProductsViewModel().GetType().GetProperties();

         var unmappedProperties =
            productProperties.Select(s => s.Name)
               .Except(_excludedMergeProperties)
               .Except(mergeModelProperties.Select(m => m.Name))
               .ToList();
         Assert.IsFalse(unmappedProperties.Any(),
            "The following Product properties are not accounted for in the merge process: " +
            String.Join(", ", unmappedProperties));
      }

      [TestMethod]
      public void Merge_NoProperties()
      {
         var toDelete = _toDelete;
         var toKeep = _toKeep;

         var testData = new MockedInventoryProductData();
         var uow = MockedInventoryUnitOfWorkFactory.GetProductUnitOfWork(testData);

         var mergeResult = ProductMergeHelper.MergeProperties(toKeep, toDelete, new List<string>(), TestUser, uow, _mockedLogger);
         var failedProperties = string.Join(", ", mergeResult.FailedProperties.ToArray());
         Assert.AreEqual(true, mergeResult.Success, failedProperties);
         Assert.AreSame(_toKeep, toKeep);
      }

      /// <summary>
      /// Tests AutoMerge functionality for each property type belonging to a product, 
      /// excluding those covered by the manual merges.
      /// </summary>
      [TestMethod]
      public void Merge_AutoMergeProperties()
      {
         var autoMergeProperties = new List<string>
         {
            Nameof<MergeProductsViewModel>.Property(p => p.Description), //string
            Nameof<MergeProductsViewModel>.Property(p => p.MaxUses), //int
            Nameof<MergeProductsViewModel>.Property(p => p.UseCategorySettings), //bool
         };

         var toKeep = _toKeep;
         var testData = new MockedInventoryProductData();
         var uow = MockedInventoryUnitOfWorkFactory.GetProductUnitOfWork(testData);

         var mergeResult = ProductMergeHelper.MergeProperties(toKeep, _toDelete, autoMergeProperties, TestUser, uow, _mockedLogger);

         string failedProperties = string.Empty;
         if (mergeResult.FailedProperties != null)
         {
            failedProperties = string.Join(", ", mergeResult.FailedProperties.ToArray());
         }


         Assert.AreEqual(true, mergeResult.Success, failedProperties);
         Assert.AreEqual(_toDelete.Description, toKeep.Description, failedProperties);
      }


      /// <summary>
      /// Tests AutoMerge functionality for each property type belonging to a product, 
      /// excluding those covered by the manual merges.
      /// </summary>
      [TestMethod]
      public void Merge_ProductBecomesUnmanaged_PreviousConsumptionsArchived()
      {
         var autoMergeProperties = new List<string>
         {
            Nameof<MergeProductsViewModel>.Property(p => p.ManageStock),
         };

         var toKeep = _toKeep;
         toKeep.ManageStock = true;
         var toDelete = _toDelete;
         toDelete.ManageStock = false;

         var healthTrackConsumption = new HealthTrackConsumption
         {
            ConsumedOn = DateTime.Now.AddDays(-9),
            deleted = false,
            ProductId = 321,
            UsedId = 1
         };

         var cnm = new ConsumptionNotificationManagement
         {
            invUsed_ID = 1
         };

         var externalProductMapping = new ExternalProductMapping
         {
            ExternalProductId = 321,
            InventoryProductId = toKeep.ProductId
         };

         var testData = new MockedInventoryProductData(new List<Product>(), new List<PriceType>(),
            new List<HealthTrackConsumption> { healthTrackConsumption },
            new List<ExternalProductMapping> { externalProductMapping }, new List<ConsumptionNotificationManagement> { cnm },
            new List<StockAdjustment>(), new List<Stock>(), new List<ProductCategory>());
         var uow = MockedInventoryUnitOfWorkFactory.GetProductUnitOfWork(testData);

         var mergeResult = ProductMergeHelper.MergeProperties(toKeep, toDelete, autoMergeProperties, TestUser, uow, _mockedLogger);

         string failedProperties = string.Empty;
         if (mergeResult.FailedProperties != null)
         {
            failedProperties = string.Join(", ", mergeResult.FailedProperties.ToArray());
         }

         Assert.AreEqual(true, mergeResult.Success, failedProperties);

         Assert.IsNotNull(cnm.ArchivedBy, "Archived By");
         Assert.IsNotNull(cnm.ArchivedOn, "Archived On");
      }

      /// <summary>
      /// Test whether the merge functionality can successfully merge a product price property
      /// </summary>
      [TestMethod]
      public void Merge_SinglePriceProperty()
      {
         var publicPrice = PublicPrice;
         var publicPriceBuyCurrencyPropertyName = string.Format("{0}{1}_{2}", InventoryConstants.MergePricePrefix,
            Nameof<ProductPriceViewModel>.Property(p => p.BuyCurrency), publicPrice.PriceTypeId);

         var mergePriceProperties = new List<string>
         {
            publicPriceBuyCurrencyPropertyName
         };

         var toDelete = _toDelete;
         var toKeep = _toKeep;

         var publicPriceToMerge = toDelete.Prices.Single(p => p.PriceTypeId == publicPrice.PriceTypeId);
         publicPriceToMerge.BuyCurrency = "GPB";
         var testData = new MockedInventoryProductData();
         var uow = MockedInventoryUnitOfWorkFactory.GetProductUnitOfWork(testData);
         var result = ProductMergeHelper.MergeProperties(toKeep, toDelete, mergePriceProperties, TestUser, uow, _mockedLogger);

         Assert.AreEqual(true, result.Success);
         Assert.AreEqual("GPB", toKeep.Prices.Single(p => p.PriceTypeId == publicPrice.PriceTypeId).BuyCurrency);
      }

      /// <summary>
      /// Check ProductId can never be merged. 
      /// </summary>
      [TestMethod]
      [ExpectedException(typeof(Exception))]
      public void Merge_ProductIdIgnored()
      {
         var toKeep = _toKeep;
         var mergeProperties = new List<string>()
         {
            Nameof<Product>.Property(p => p.ProductId)
         };

         var testData = new MockedInventoryProductData();
         var uow = MockedInventoryUnitOfWorkFactory.GetProductUnitOfWork(testData);

         var mergeResult = ProductMergeHelper.MergeProperties(toKeep, _toDelete, mergeProperties, TestUser, uow, _mockedLogger);
      }

      [TestMethod]
      [ExpectedException(typeof(Exception))]
      public void ProductPriceIdProhibited()
      {
         var publicPrice = PublicPrice;
         var publicPricePriceIdPropertyName = string.Format("{0}{1}_{2}", InventoryConstants.MergePricePrefix,
            Nameof<ProductPriceViewModel>.Property(p => p.PriceId), publicPrice.PriceTypeId);

         var mergePriceProperties = new List<string>
         {
            publicPricePriceIdPropertyName
         };

         var toDelete = _toDelete;
         var toKeep = _toKeep;

         var testData = new MockedInventoryProductData();
         var uow = MockedInventoryUnitOfWorkFactory.GetProductUnitOfWork(testData);

         var mergeResult = ProductMergeHelper.MergeProperties(toKeep, toDelete, mergePriceProperties, TestUser, uow, _mockedLogger);
      }

      [TestMethod]
      [ExpectedException(typeof(Exception))]
      public void ProductPriceTypeIdProhibited()
      {
         var publicPrice = PublicPrice;
         var publicPricePriceTypeIdPropertyName = string.Format("{0}{1}_{2}", InventoryConstants.MergePricePrefix,
            Nameof<ProductPriceViewModel>.Property(p => p.PriceTypeId), publicPrice.PriceTypeId);

         var mergePriceProperties = new List<string>
         {
            publicPricePriceTypeIdPropertyName
         };

         var toDelete = _toDelete;
         var toKeep = _toKeep;

         var testData = new MockedInventoryProductData();
         var uow = MockedInventoryUnitOfWorkFactory.GetProductUnitOfWork(testData);

         var mergeResult = ProductMergeHelper.MergeProperties(toKeep, toDelete, mergePriceProperties, TestUser, uow, _mockedLogger);
      }

      #region ManualPropertyMergeTests

      [TestMethod]
      public void MergeCategories()
      {
         var autoMergeProperties = new List<string>
         {
            Nameof<MergeProductsViewModel>.Property(p => p.SelectedCategories)
         };

         var toKeep = _toKeep;
         var toDelete = _toDelete;
         var productCategories = toKeep.ProductCategories.ToList();

         // setup product to delete with different categories
         toDelete.ProductCategories.Remove(toDelete.ProductCategories.First());
         toDelete.ProductCategories.Add(ProductCategoryThree);

         var testData = new MockedInventoryProductData(new List<Product> (), new List<PriceType>(),
            new List<HealthTrackConsumption>(), new List<ExternalProductMapping>(),
            new List<ConsumptionNotificationManagement>(), new List<StockAdjustment>(), new List<Stock> (), productCategories);
         var uow = MockedInventoryUnitOfWorkFactory.GetProductUnitOfWork(testData);

         var mergeResult = ProductMergeHelper.MergeProperties(toKeep, toDelete, autoMergeProperties, TestUser, uow, _mockedLogger);

         Assert.AreEqual(true, mergeResult.Success);
         testData.MockedProductCategories.Verify(m => m.Remove(It.IsAny<ProductCategory>()), Times.Exactly(productCategories.Count()));

         // this works because the categories are not removed by the merge until the commit and the merge adds the two categories from the product to delete.
         Assert.IsTrue(toKeep.ProductCategories.Count() == productCategories.Count + 2);
      }

      /// <summary>
      /// 
      /// </summary>
      [TestMethod]
      public void MergeSettings()
      {
         var autoMergeProperties = new List<string>
         {
            Nameof<MergeProductsViewModel>.Property(p => p.Settings)
         };

         var toKeep = _toKeep;
         var toDelete = _toDelete;

         // setup product to delete with different categories
         toDelete.ProductSettings.Clear();
         toDelete.ProductSettings.Add(new StockSetting { SettingId = InventoryConstants.StockSettings.SingleOrderItem });

         var testData = new MockedInventoryProductData();
         var uow = MockedInventoryUnitOfWorkFactory.GetProductUnitOfWork(testData);

         var mergeResult = ProductMergeHelper.MergeProperties(toKeep, toDelete, autoMergeProperties, TestUser, uow, _mockedLogger);

         Assert.AreEqual(true, mergeResult.Success);
         Assert.AreEqual(InventoryConstants.StockSettings.SingleOrderItem, toKeep.ProductSettings.Single().SettingId,
            "Product does not have specified setting");
      }

      [TestMethod]
      public void MergeGlc()
      {
         var autoMergeProperties = new List<string>
         {
            Nameof<MergeProductsViewModel>.Property(p => p.GLC)
         };

         var toKeep = _toKeep;
         var testData = new MockedInventoryProductData();
         var uow = MockedInventoryUnitOfWorkFactory.GetProductUnitOfWork(testData);

         var mergeResult = ProductMergeHelper.MergeProperties(toKeep, _toDelete, autoMergeProperties, TestUser, uow, _mockedLogger);

         Assert.AreEqual(true, mergeResult.Success);
         Assert.AreEqual(_toDelete.GLC, toKeep.GLC);
         Assert.AreEqual(_toDelete.LedgerId, toKeep.LedgerId);
      }

      #endregion

      /// <summary>
      /// Ensure that the there is only one mapping per ExternalProductId
      /// </summary>
      [TestMethod]
      public void AvoidDuplicateMappings()
      {
         var toKeep = _toKeep;
         var toDelete = _toDelete;
         toDelete.ExternalProductMappings.Add(new ExternalProductMapping
         {
            ExternalProductId = toKeep.ExternalProductMappings.First().ExternalProductId,
            InventoryProductId = toDelete.ProductId
         });

         var testData = new MockedInventoryProductData();
         var repo = MockedInventoryRepositoryFactory.GetProductRepository(testData);

         repo.MergeRelatedItems(toKeep, toDelete, "USER");

         var allMappings = toKeep.ExternalProductMappings.Union(toDelete.ExternalProductMappings).ToList();

         Assert.AreEqual(3, allMappings.Count(), "Total Number of external product mappings");
         Assert.AreEqual(2, allMappings.Count(epm => epm.DeletedOn == null), "Number of active external product mappings");
         Assert.IsTrue(allMappings.Where(epm => epm.DeletedOn == null).All(epm => epm.InventoryProductId == toKeep.ProductId), "Ensure all active mapping InventoryProductIds map to this product");
         Assert.IsTrue(allMappings.Where(epm => epm.DeletedOn == null).Select(epm => epm.ExternalProductId).Distinct().Count() == allMappings.Where(epm => epm.DeletedOn == null).Count(), "Check for duplicate ExternalProductIds");
      }

      [TestMethod]
      public void MergeManageStock_BothOneForOneReplace_NoReplaceAfterDate()
      {
         var toKeep = _toKeep;
         toKeep.AutoReorderSetting = ReorderSettings.OneForOneReplace;
         var toDelete = _toDelete;
         toDelete.AutoReorderSetting = ReorderSettings.OneForOneReplace;

         var testData = new MockedInventoryProductData();
         var uow = MockedInventoryUnitOfWorkFactory.GetProductUnitOfWork(testData);

         ManualMergeFunctions.MergeManageStock(toKeep, toDelete, uow);

         Assert.IsFalse(toKeep.ManageStock);
         Assert.AreEqual(ReorderSettings.OneForOneReplace, toKeep.AutoReorderSetting);
         Assert.IsNull(toKeep.ReplaceAfter);
      }

      [TestMethod]
      public void MergeManageStock_BothOneForOneReplace_ToKeepReplaceAfterHasDate()
      {
         var toKeep = _toKeep;
         toKeep.AutoReorderSetting = ReorderSettings.OneForOneReplace;
         toKeep.ReplaceAfter = DateTime.Now.AddDays(-1);
         var toDelete = _toDelete;
         toDelete.AutoReorderSetting = ReorderSettings.OneForOneReplace;

         var testData = new MockedInventoryProductData();
         var uow = MockedInventoryUnitOfWorkFactory.GetProductUnitOfWork(testData);

         ManualMergeFunctions.MergeManageStock(toKeep, toDelete, uow);

         Assert.IsFalse(toKeep.ManageStock);
         Assert.AreEqual(ReorderSettings.OneForOneReplace, toKeep.AutoReorderSetting);
         Assert.IsNull(toKeep.ReplaceAfter);
      }

      [TestMethod]
      public void MergeManageStock_BothOneForOneReplace_ToDeleteReplaceAfterHasDate()
      {
         var toKeep = _toKeep;
         toKeep.AutoReorderSetting = ReorderSettings.OneForOneReplace;
         toKeep.ReplaceAfter = DateTime.Now.AddDays(-1);

         var toDelete = _toDelete;
         toDelete.AutoReorderSetting = ReorderSettings.OneForOneReplace;

         var testData = new MockedInventoryProductData();
         var uow = MockedInventoryUnitOfWorkFactory.GetProductUnitOfWork(testData);

         ManualMergeFunctions.MergeManageStock(toKeep, toDelete, uow);

         Assert.IsFalse(toKeep.ManageStock);
         Assert.AreEqual(ReorderSettings.OneForOneReplace, toKeep.AutoReorderSetting);
         Assert.IsNull(toKeep.ReplaceAfter);
      }

      [TestMethod]
      public void MergeManageStock_BothOneForOneReplace_ToDeleteReplaceAfterIsOlder()
      {
         var oldestDate = DateTime.Now.AddDays(-7);
         var toKeep = _toKeep;
         toKeep.AutoReorderSetting = ReorderSettings.OneForOneReplace;
         toKeep.ReplaceAfter = DateTime.Now.AddDays(-1);

         var toDelete = _toDelete;
         toDelete.ReplaceAfter = oldestDate;
         toDelete.AutoReorderSetting = ReorderSettings.OneForOneReplace;

         var testData = new MockedInventoryProductData();
         var uow = MockedInventoryUnitOfWorkFactory.GetProductUnitOfWork(testData);

         ManualMergeFunctions.MergeManageStock(toKeep, toDelete, uow);

         Assert.IsFalse(toKeep.ManageStock);
         Assert.AreEqual(ReorderSettings.OneForOneReplace, toKeep.AutoReorderSetting);
         Assert.AreEqual(oldestDate, toKeep.ReplaceAfter);
      }

      [TestMethod]
      public void MergeManageStock_BothOneForOneReplace_ToKeepReplaceAfterIsOlder()
      {
         var oldestDate = DateTime.Now.AddDays(-7);
         var toKeep = _toKeep;
         toKeep.AutoReorderSetting = ReorderSettings.OneForOneReplace;
         toKeep.ReplaceAfter = oldestDate;
         var toDelete = _toDelete;
         toDelete.AutoReorderSetting = ReorderSettings.OneForOneReplace;
         toDelete.ReplaceAfter = DateTime.Now.AddDays(-1);

         var testData = new MockedInventoryProductData();
         var uow = MockedInventoryUnitOfWorkFactory.GetProductUnitOfWork(testData);

         ManualMergeFunctions.MergeManageStock(toKeep, toDelete, uow);

         Assert.IsFalse(toKeep.ManageStock);
         Assert.AreEqual(oldestDate, toKeep.ReplaceAfter);
      }

      [TestMethod]
      public void MergeManageStock_ToKeepOneForOneReplace()
      {
         var oldestDate = DateTime.Now.AddDays(-7);
         var toKeep = _toKeep;
         toKeep.AutoReorderSetting = ReorderSettings.OneForOneReplace;
         toKeep.ReplaceAfter = oldestDate;

         var toDelete = _toDelete;
         toDelete.AutoReorderSetting = ReorderSettings.SpecifyLevels;
         toDelete.ManageStock = true;
         toDelete.ReplaceAfter = DateTime.Now.AddDays(-1);

         var testData = new MockedInventoryProductData();
         var uow = MockedInventoryUnitOfWorkFactory.GetProductUnitOfWork(testData);

         ManualMergeFunctions.MergeManageStock(toKeep, toDelete, uow);

         Assert.IsTrue(toKeep.ManageStock);
      }

      [TestMethod]
      public void MergeManageStock_ToDeleteOneForOneReplace()
      {
         var oldestDate = DateTime.Now.AddDays(-7);
         var toKeep = _toKeep;
         toKeep.AutoReorderSetting = ReorderSettings.SpecifyLevels;
         toKeep.ReplaceAfter = DateTime.Now.AddDays(-1);
         toKeep.ManageStock = true;

         var toDelete = _toDelete;
         toDelete.AutoReorderSetting = ReorderSettings.OneForOneReplace;
         toDelete.ReplaceAfter = oldestDate;

         var testData = new MockedInventoryProductData();
         var uow = MockedInventoryUnitOfWorkFactory.GetProductUnitOfWork(testData);

         ManualMergeFunctions.MergeManageStock(toKeep, toDelete, uow);

         Assert.IsFalse(toKeep.ManageStock);
         Assert.AreEqual(oldestDate, toKeep.ReplaceAfter);
      }

      [TestMethod]
      public void MergeManageStock_BothManaged()
      {
         var toKeep = _toKeep;
         toKeep.ManageStock = true;
         toKeep.AutoReorderSetting = ReorderSettings.SpecifyLevels;
         var toDelete = _toDelete;
         toDelete.ManageStock = true;
         toKeep.AutoReorderSetting = ReorderSettings.SpecifyLevels;

         var testData = new MockedInventoryProductData();
         var uow = MockedInventoryUnitOfWorkFactory.GetProductUnitOfWork(testData);

         ManualMergeFunctions.MergeManageStock(toKeep, toDelete, uow);

         Assert.IsTrue(toKeep.ManageStock);
         Assert.IsNull(toKeep.ReplaceAfter);
      }
   }
}