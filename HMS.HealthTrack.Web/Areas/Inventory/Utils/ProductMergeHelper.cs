using HMS.HealthTrack.Web.Areas.Inventory.Models.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Inventory.Common.Constants;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork;

namespace HMS.HealthTrack.Web.Areas.Inventory.Utils
{
   public static class ProductMergeHelper
   {
      private static readonly string ProductIdPropertyName = Nameof<Product>.Property(p => p.ProductId);
      private static readonly string ProductPricePriceId = Nameof<ProductPrice>.Property(p => p.PriceId);
      public static readonly string ManageStockName = Nameof<Product>.Property(p => p.ManageStock);

      public static MergeResult MergeProperties(Product toKeep, Product toDelete, ICollection<string> propertiesToMerge,
         string username, IProductUnitOfWork unitOfWork, ICustomLogger logger)
      {
         if (propertiesToMerge == null || !propertiesToMerge.Any())
         {
            return new MergeResult
            {
               FailedProperties = new List<string>(),
               Message = "No properties to merge",
               Success = true
            };
         }

         // if setting the product from manage stock to not manage stock, ignore previous consumptions of product to keep
         if (propertiesToMerge.Contains(ManageStockName) && toKeep.ManageStock && !toDelete.ManageStock)
         {
            unitOfWork.IgnorePreviousConsumptionNotifications(toKeep.ProductId, DateTime.Now, username);
         }

         return MergeProperties(toKeep, toDelete, propertiesToMerge.ToList(), unitOfWork, logger);
      }

      public static readonly List<ManualProductMerge> ManualMerges = new List<ManualProductMerge>
      {
         new ManualProductMerge
         {
            Property = Nameof<MergeProductsViewModel>.Property(p => p.SelectedCategories),
            Merge = ManualMergeFunctions.MergeCategories
         },
         new ManualProductMerge
         {
            Property = Nameof<MergeProductsViewModel>.Property(p => p.Settings),
            Merge = ManualMergeFunctions.MergeSettings
         },
         new ManualProductMerge
         {
            Property = Nameof<MergeProductsViewModel>.Property(p => p.GLC),
            Merge = ManualMergeFunctions.MergeGlc
         },
         new ManualProductMerge
         {
            Property = Nameof<MergeProductsViewModel>.Property(p => p.ManageStock),
            Merge = ManualMergeFunctions.MergeManageStock
         },
         new ManualProductMerge
         {
            Property = Nameof<MergeProductsViewModel>.Property(p => p.UPN),
            Merge = ManualMergeFunctions.MergeScanCodes
         }
      };

      private static readonly string ProductPricePriceTypeId = Nameof<ProductPrice>.Property(p => p.PriceTypeId);

      private static MergeResult MergeProperties(Product toKeep, Product toDelete, List<string> propertiesToMerge, IProductUnitOfWork unitOfWork, ICustomLogger logger)
      {
         if (propertiesToMerge.Contains(ProductIdPropertyName))
         {
            throw new Exception("Probitied merge fields specified: " + ProductIdPropertyName);
         }

         // price properties
         var priceProperties = propertiesToMerge.Where(p => p.StartsWith(InventoryConstants.MergePricePrefix)).ToList();
         var failedPriceMergeProperties = MergePrices(toKeep, toDelete, priceProperties, logger);

         // automerge properties
         var autoMergeProperties = propertiesToMerge.Except(ManualMerges.Select(m => m.Property))
            .Except(priceProperties);
         var failedAutoMergeProperties = AutoMergeProperties(toKeep, toDelete, autoMergeProperties, logger).ToList();

         // properties requiring manual merge (i.e. not auto)
         // IMPORTANT: This step must come after AutoMergeProperties.
         var manualMerges = ManualMerges.Where(s => propertiesToMerge.Contains(s.Property)).ToList();
         var failedMergeProperties =
            (from merge in manualMerges where !merge.Merge(toKeep, toDelete, unitOfWork) select merge.Property).ToList();

         var mergeResult = new MergeResult
         {
            FailedProperties =
               failedMergeProperties.Union(failedPriceMergeProperties).Union(failedAutoMergeProperties).ToList()
         };

         mergeResult.Success = !mergeResult.FailedProperties.Any();

         return mergeResult;
      }

      private static IEnumerable<string> AutoMergeProperties(Product toKeep, Product toDelete,
         IEnumerable<string> autoMergeProperties, ICustomLogger logger)
      {
         var failedProperties = new List<string>();
         foreach (var propertyName in autoMergeProperties)
         {
            try
            {
               var propertyInfo = BulkUpdateHelper.GetProductPropertyInfo(propertyName);
               var valueToSet = propertyInfo.GetValue(toDelete);
               propertyInfo.SetValue(toKeep, valueToSet, null);
            }
            catch (Exception ex)
            {
               failedProperties.Add(propertyName);
               logger.Error(ex, "Unable to auto merge the product property {PropertyName}", propertyName);
            }
         }
         return failedProperties;
      }

      private static IEnumerable<string> MergePrices(Product toKeep, Product toDelete,
         IEnumerable<string> priceProperties, ICustomLogger logger)
      {
         var failedProperties = new List<string>();

         var productPriceType = new ProductPrice().GetType();

         foreach (var priceProperty in priceProperties)
         {
            try
            {
               var result = priceProperty.Replace(InventoryConstants.MergePricePrefix, String.Empty).Split('_');
               var priceTypeId = Int32.Parse(result[1]);
               var pricePropertyName = result[0];

               if (pricePropertyName.Equals(ProductIdPropertyName) || pricePropertyName.Equals(ProductPricePriceId) ||
                   pricePropertyName.Equals(ProductPricePriceTypeId))
               {
                  throw new Exception("Probitied merge field specified: " + pricePropertyName);
               }

               var propertyInfo = productPriceType.GetProperty(pricePropertyName);
               var priceToUpdate = toKeep.Prices.Single(p => p.PriceTypeId == priceTypeId);
               var priceToMerge = toDelete.Prices.Single(p => p.PriceTypeId == priceTypeId);
               var valueToSet = propertyInfo.GetValue(priceToMerge);
               propertyInfo.SetValue(priceToUpdate, valueToSet, null);
            }
            catch (Exception exception)
            {
               failedProperties.Add(priceProperty);
               logger.Error(exception, "Unable to merge product price property {ProductPriceProperty}", priceProperty);
               throw;
            }
         }

         return failedProperties;
      }
   }

   public static class ManualMergeFunctions
   {
      public static bool MergeCategories(Product toKeep, Product toDelete, IProductUnitOfWork uow)
      {
         // remove product category association from database
         var categoriesToRemove = toKeep.ProductCategories.ToList();
         uow.ProductRepository.RemoveCategoriesFromProduct(categoriesToRemove);

         // assign new categories
         var categoryIds = toDelete.ProductCategories.Select(c => c.CategoryId).ToList();
         foreach (var categoryId in categoryIds)
         {
            toKeep.ProductCategories.Add(new ProductCategory
            {
               CategoryId = categoryId
            });
         }
         return true;
      }

      public static bool MergeSettings(Product toKeep, Product toDelete, IProductUnitOfWork uow)
      {
         toKeep.ProductSettings.Clear();
         var settingIds = toDelete.ProductSettings.Select(ps => ps.SettingId).ToList();

         foreach (var settingId in settingIds)
         {
            toKeep.ProductSettings.Add(new StockSetting
            {
               SettingId = settingId
            });
         }
         return true;
      }

      public static bool MergeGlc(Product toKeep, Product toDelete, IProductUnitOfWork uow)
      {
         toKeep.LedgerId = toDelete.LedgerId;
         toKeep.GLC = toDelete.GLC;
         return true;
      }

      public static bool MergeManageStock(Product toKeep, Product toDelete, IProductUnitOfWork uow)
      {
         if (!toKeep.ManageStock && !toDelete.ManageStock) // both unmanaged
         {
            if (toKeep.ReplaceAfter == null || toDelete.ReplaceAfter == null)
            {
               // set ReplaceAfter to NULL if either are NULL
               toKeep.ReplaceAfter = null;
            }
            else if (toDelete.ReplaceAfter < toKeep.ReplaceAfter)
            {
               // toKeep's ReplaceAfter must be the oldest date
               toKeep.ReplaceAfter = toDelete.ReplaceAfter;
            }
         }
         else if (!toDelete.ManageStock)
         {
            // update ReplaceAfter, make sure that toKeep's consumptions are archived prior to DateTime.Now
            toKeep.ReplaceAfter = toDelete.ReplaceAfter;
         }

         toKeep.ManageStock = toDelete.ManageStock;

         return true;
      }


      public static bool MergeScanCodes(Product toKeep, Product toDelete, IProductUnitOfWork uow)
      {
         //Swap the new ones over
         foreach (var scanCode in toDelete.ScanCodes)
         {
            scanCode.ProductId = toKeep.ProductId;
         }

         return true;
      }
   }

   public class MergeResult
   {
      public bool Success { get; set; }
      public string Message { get; set; }
      public List<string> FailedProperties { get; set; }
   }

   public class ManualProductMerge
   {
      public string Property { get; set; }
      public Func<Product, Product, IProductUnitOfWork, bool> Merge;
   }
}