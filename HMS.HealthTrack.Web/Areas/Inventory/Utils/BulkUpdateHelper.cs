using AutoMapper;
using HMS.HealthTrack.Web.Areas.Inventory.Models.Products;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Inventory.Common.Constants;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork;

namespace HMS.HealthTrack.Web.Areas.Inventory.Utils
{
   public static class BulkUpdateHelper
   {
      private static readonly string SelectedCategoriesPropertyName = Nameof<BulkUpdateProductModel>.Property(p => p.SelectedCategories);
      private static readonly string SelectedSettingsPropertyName = Nameof<BulkUpdateProductModel>.Property(p => p.SelectedSettings);
      private static readonly string AutoReorderSettingPropertyName = Nameof<BulkUpdateProductModel>.Property(p => p.AutoReorderSetting);
      private static readonly string ManageStockPropertyName = Nameof<BulkUpdateProductModel>.Property(p => p.ManageStock);


      public static PropertyInfo GetProductPropertyInfo(string propertyName)
      {
         PropertyInfo propertyInfo;

         if (propertyName == SelectedCategoriesPropertyName || propertyName == SelectedSettingsPropertyName)
            return null;
         try
         {
            propertyInfo = new Product().GetType().GetProperty(propertyName);
            if (propertyInfo == null)
            {
               throw new Exception(String.Format("No property exists for Product with name {0}", propertyName));
            }
         }
         catch (Exception exception)
         {
            throw;
         }
         return propertyInfo;
      }

      public static object GetConvertedValue(PropertyInfo propertyInfo, string propertyName, string value)
      {
         try
         {

            if (propertyInfo != null)
            {
               //http://stackoverflow.com/questions/18015425/invalid-cast-from-system-int32-to-system-nullable1system-int32-mscorlib
               var type = propertyInfo.PropertyType;
               if (!type.IsValueType)
               {
                  return value;
               }

               if (type.IsEnum)
               {
                  //http://stackoverflow.com/questions/507059/convert-changetype-and-converting-to-enums  Handy enum conversion
                  return (StringComparison)Enum.ToObject(typeof(StringComparison), int.Parse(value));
               }

               if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
               {
                  if (value == null)
                  {
                     return null;
                  }
                  type = Nullable.GetUnderlyingType(type);
               }

               return Convert.ChangeType(value, type);
            }

            if (propertyName == SelectedCategoriesPropertyName)
               return value.Split(',').Select(Int32.Parse).ToList();

            if (propertyName == SelectedSettingsPropertyName)
               return value.Split(',').ToList();
         }
         catch (Exception exception)
         {
            throw;
         }
         return null;
      }

      public static void UpdateProductsProperties(IQueryable<Product> products, PropertyInfo propertyInfo, string propertyName, object convertedValue, IProductUnitOfWork productUnitOfWork, string username)
      {
         var valueToSet = convertedValue;
         // set value for clearing
         if (convertedValue == null)
         {
            if (propertyName == SelectedCategoriesPropertyName)
            {
               valueToSet = new List<int>();
            }
            else if (propertyName == SelectedSettingsPropertyName)
            {
               valueToSet = new List<string>();
            }
            else
            {
               valueToSet = null;
            }
         }

         if (propertyName == SelectedCategoriesPropertyName)
         {
            var categoryIds = valueToSet as List<int>;
            if (categoryIds == null)
               throw new Exception("Unable to interpret converted value for bulk product update");

            foreach (var product in products)
            {
               productUnitOfWork.ProductRepository.UpdateSelectedCategories(product, categoryIds);
               product.LastModifiedBy = username;
               product.LastModifiedOn = DateTime.Now;
            }
            return;
         }

         if (propertyName == SelectedSettingsPropertyName)
         {
            var settings = valueToSet as List<string>;
            if (settings == null)
               throw new Exception("Unable to interpret converted value for bulk product update");

            foreach (var product in products)
            {
               productUnitOfWork.ProductRepository.UpdateSelectedProductSettings(product, settings);
               product.LastModifiedBy = username;
               product.LastModifiedOn = DateTime.Now;
            }
            return;
         }

         if (propertyName == AutoReorderSettingPropertyName)
         {
            var setting = (ReorderSettings)valueToSet;

            foreach (var product in products)
            {
               var currentSetting = product.AutoReorderSetting;
               product.AutoReorderSetting = setting;
               productUnitOfWork.HandleReorderSettingUpdate(product, currentSetting);
            }
         }

         if (propertyName == ManageStockPropertyName)
         {
            var manageStock = valueToSet as bool?;
            if (manageStock == null)
               throw new Exception("Unable to interpret converted value for bulk product update");

            foreach (var product in products)
            {
               productUnitOfWork.HandleManageStockUpdate(product, manageStock.Value, username);
            }
         }
         try

         {
            // normal property
            foreach (var product in products)
            {
               productUnitOfWork.ProductRepository.UpdateProductField(propertyInfo, product, valueToSet, username);
            }
         }
         catch (Exception exception)
         {
            throw;
         }
      }

      public static void UpdateProductPriceProperties(IQueryable<Product> productsToUpdate, PropertyInfo propertyInfo, string propertyName, object convertedValue, IProductRepository productRepository, string username)
      {

         var priceModel = propertyName.Replace(InventoryConstants.BulkUpdatePriceTypePrefix, string.Empty);
         try
         {
            var priceTypeId = int.Parse(priceModel);
            // normal property
            foreach (var product in productsToUpdate)
            {
               var price = product.Prices.Single(p => p.PriceTypeId == priceTypeId);
               propertyInfo.SetValue(price, convertedValue, null);
               product.LastModifiedBy = username;
               product.LastModifiedOn = DateTime.Now;
            }
         }
         catch (Exception exception)
         {
            throw;
         }
      }

      public static IQueryable<Product> GetFilteredProductsFromIndexViewModelFilters(IEnumerable<Product> products, DataSourceRequest request, IProductRepository productRepository)
      {
         var filteredProducts = products.Select(Mapper.Map<Product, IndexProductsViewModel>).ToDataSourceResult(request).Data;
         var filteredProductsModels = filteredProducts as IEnumerable<IndexProductsViewModel>;
         if (filteredProductsModels == null)
         {
            throw new Exception("Unable to construct DataSourceResult from products");
         }
         var productIdsToUpdate = filteredProductsModels.Select(p => p.ProductId).ToList();
         return productRepository.FindAll().Where(p => productIdsToUpdate.Contains(p.ProductId));
      }

      public static PropertyInfo GetPricePropertyInfo(string subDetail)
      {
         PropertyInfo propertyInfo;

         try
         {
            propertyInfo = new ProductPrice().GetType().GetProperty(subDetail);
            if (propertyInfo == null)
            {
               throw new Exception(String.Format("No property exists for Product Price with name {0}", subDetail));
            }
         }
         catch (Exception exception)
         {
            throw;
         }
         return propertyInfo;
      }
   }
}