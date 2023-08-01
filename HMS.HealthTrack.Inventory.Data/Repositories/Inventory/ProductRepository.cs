using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using HMS.HealthTrack.Inventory.Common.Constants;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using LinqKit;

namespace HMS.HealthTrack.Web.Data.Repositories.Inventory
{
   public sealed class ProductRepository : IProductRepository
   {
      private readonly IDbContextInventoryContext _context;
      private readonly IPropertyProvider _propertyProvider;

      public ProductRepository(IDbContextInventoryContext context, IPropertyProvider propertyProvider)
      {
         _context = context;
         _propertyProvider = propertyProvider;
      }

      public ProductIdentifierResult GetProductOrderReferenceIdentifier(int productId, int? orderChannelId = null)
      {
         //Get the product
         var product = _context.Products
            .Include(p => p.OrderChannelProducts)
            .Include(x => x.ScanCodes)
            .SingleOrDefault(p => p.ProductId == productId);

         if (product == null)
            return new ProductIdentifierResult(string.Empty, ProductIdentifierType.None);
         ;

         //Try and get the order channel referece
         if (orderChannelId.HasValue)
         {
            var orderChannelAssignment = product.OrderChannelProducts.SingleOrDefault(ocp => ocp.OrderChannelId == orderChannelId.Value);
            if (orderChannelAssignment != null)
               return new ProductIdentifierResult(orderChannelAssignment.Reference, ProductIdentifierType.OrderChannel);
         }

         //Use the SPC
         if (!string.IsNullOrWhiteSpace(product.SPC))
            return new ProductIdentifierResult(product.SPC, ProductIdentifierType.SPC);

         //Resort the first UPN
         if (product.ScanCodes.Any())
            return new ProductIdentifierResult(product.ScanCodes.First().Value, ProductIdentifierType.UPN);

         //No result possible
         return new ProductIdentifierResult(string.Empty, ProductIdentifierType.None);
      }

      public void RemoveCategoriesFromProduct(IList<ProductCategory> categories)
      {
         foreach (var productCategory in categories.ToList()) _context.ProductCategories.Remove(productCategory);
      }

      public void Add(Product product, IEnumerable<string> productSettings, IEnumerable<int> categories)
      {
         // insert default prices if none exist
         if (product.Prices == null || !product.Prices.Any())
         {
            var priceTypes = _context.PriceTypes.ToList();
            product.Prices = priceTypes.Select(type => new ProductPrice {PriceType = type}).ToList();
         }

         //settings
         if (!product.UseCategorySettings && productSettings != null)
         {
            var settingsToAdd = (from s in _context.StockSettings
               where productSettings.Contains(s.SettingId)
               select s).ToList(); //Must enumerate

            foreach (var newSetting in settingsToAdd)
               // Add the settings which are not in the list of products's settings
               product.ProductSettings.Add(newSetting);
         }

         if (categories != null)
            foreach (var category in categories.Where(c => c > 0))
               product.ProductCategories.Add(new ProductCategory
               {
                  CategoryId = category
               });
         product.LastModifiedOn = DateTime.Now;

         if (!ValidateProduct(product)) product.InError = true;

         _context.Products.Add(product);
      }

      public void Add(Product product)
      {
         Add(product, null, null);
      }

      public void UpdateProductProperties(Product entity, IEnumerable<string> selectedSettings, IEnumerable<int> selectedCategories, Product existing)
      {
         ValidateProductsWithSameCodeAsProduct(existing, entity);

         entity.LastModifiedOn = DateTime.Now;

         _context.Entry(existing).CurrentValues.SetValues(entity);
         _context.Entry(existing).Property(p => p.CreatedBy).IsModified = false;

         // update product prices
         foreach (var price in existing.Prices.ToList())
         {
            var matchingPrice = entity.Prices.Single(xx => xx.PriceTypeId == price.PriceTypeId);
            matchingPrice.PriceId = price.PriceId;
            matchingPrice.ProductId = price.ProductId;
            _context.Entry(price).CurrentValues.SetValues(matchingPrice);
         }

         UpdateSelectedCategories(existing, selectedCategories);

         UpdateSelectedProductSettings(existing, selectedSettings);

         existing.InError = !ValidateProduct(existing);
      }

      public void UpdateSelectedCategories(Product existing, IEnumerable<int> selectedCategories)
      {
         selectedCategories = selectedCategories == null
            ? new List<int>()
            : selectedCategories.Where(sc => sc > 0).ToList();
         // remove removed categories
         var categoriesToRemove =
            existing.ProductCategories.Where(pc => !selectedCategories.Contains(pc.CategoryId)).ToList();
         foreach (var categoryToRemove in categoriesToRemove)
         {
            existing.ProductCategories.Remove(categoryToRemove);
            _context.Entry(categoryToRemove).State = EntityState.Deleted;
         }

         // add categories that don't exist
         var existingCategories = existing.ProductCategories.Select(pc => pc.CategoryId);
         foreach (var selectedCategory in selectedCategories.Where(sc => !existingCategories.Contains(sc)))
            existing.ProductCategories.Add(new ProductCategory
            {
               CategoryId = selectedCategory
            });
      }

      public void UpdateSelectedProductSettings(Product product, IEnumerable<string> selectedSettings)
      {
         if (selectedSettings == null) selectedSettings = new List<string>();
         // Remove the settings which are not in the list of new settings
         var settingsToRemove = (from s in product.ProductSettings
            where !selectedSettings.Contains(s.SettingId)
            select s).ToList(); //Must enumerate

         foreach (var setting in settingsToRemove)
            // Remove the settings which are not in the list of new settings
            product.ProductSettings.Remove(setting);

         var settingsToAdd = (from s in _context.StockSettings
            where s.Products.All(p => p.ProductId != product.ProductId)
                  && selectedSettings.Contains(s.SettingId)
            select s).ToList(); //Must enumerate

         foreach (var newSetting in settingsToAdd)
            // Add the settings which are not in the list of products's settings
            product.ProductSettings.Add(newSetting);
      }

      public Product Find(int id)
      {
         return (from i in _context.Products
               where i.ProductId == id && !i.DeletedOn.HasValue
               select i)
            .Include(i => i.ProductCategories.Select(c => c.Category))
            .Include(i => i.PrimarySupplierCompany)
            .Include(i => i.SecondarySupplierCompany)
            .Include(i => i.ProductSettings)
            .Include(i => i.Prices.Select(p => p.PriceType))
            .Include(i => i.StockTakeItems)
            .Include(i => i.ScanCodes)
            .SingleOrDefault();
      }

      public Product FindIncludingDeleted(int id)
      {
         return (from i in _context.Products
               where i.ProductId == id
               select i)
            .Include(i => i.ProductCategories.Select(c => c.Category))
            .Include(i => i.PrimarySupplierCompany)
            .Include(i => i.SecondarySupplierCompany)
            .Include(i => i.ProductSettings)
            .Include(i => i.Prices.Select(p => p.PriceType))
            .Include(i => i.StockTakeItems)
            .Include(i => i.ScanCodes)
            .Include(i => i.ExternalProductMappings)
            .SingleOrDefault();
      }

      public IQueryable<Product> FindAll()
      {
         return FindAll(false);
      }

      public IQueryable<Product> FindAll(bool includeDeleted)
      {
         var prodQuery =
            _context.Products.Include(p => p.ProductSettings)
               .Include(p => p.Prices)
               .Include(p => p.ProductCategories.Select(pc => pc.Category.StockSettings))
               .Include(p => p.PrimarySupplierCompany)
               .Include(p => p.Stocks)
               .Include(p => p.StockTakeItems)
               .Include(p => p.ScanCodes);
         if (includeDeleted)
            return prodQuery;
         return prodQuery.Where(i => i.DeletedOn.HasValue == false);
      }

      public IQueryable<Product> GetCurrentErroredProducts()
      {
         return from p in FindAll() where p.InError && p.ProductStatus != ProductStatus.Disabled select p;
      }

      public IEnumerable<Product> FindAllLocal()
      {
         return _context.Products.Local.Where(i => i.DeletedOn.HasValue == false);
      }

      public IQueryable<Product> FindAllInStock()
      {
         return FindAll().Where(p => p.Stocks.Any(s => s.StockStatus == StockStatus.Available));
      }

      public IQueryable<Product> FindAllAsExpandable()
      {
         return _context.Products.AsExpandable().Where(p => p.DeletedOn.HasValue == false).GroupBy(p => p.ProductId).Select(g => g.FirstOrDefault());
      }

      public IQueryable<Product> FindByCategory(int categoryId)
      {
         return from p in _context.Products
            let pcp = p.ProductCategories.Where(pc => pc.CategoryId == categoryId).Select(pc => pc.Product.ProductId)
            where !p.DeletedOn.HasValue && pcp.Contains(p.ProductId)
            select p;
      }

      public IQueryable<Product> FindByCode(ProductSearchCriteria productSearchCriteria)
      {
         var predicate = PredicateBuilder.False<Product>();

         if (!string.IsNullOrWhiteSpace(productSearchCriteria.SPC))
            predicate = predicate.Or(p => p.SPC == productSearchCriteria.SPC);

         if (!string.IsNullOrWhiteSpace(productSearchCriteria.UPC))
            predicate = predicate.Or(p => p.ScanCodes.Any(c => c.Value == productSearchCriteria.UPC));

         if (!string.IsNullOrWhiteSpace(productSearchCriteria.LPC))
            predicate = predicate.Or(p => p.LPC == productSearchCriteria.LPC);

         predicate.And(p => p.ProductStatus != ProductStatus.Disabled);

         return FindAllAsExpandable().Where(predicate);
      }

      public IQueryable<Product> FindByCodeUsingContains(ProductSearchCriteria criteria)
      {
         var predicate = PredicateBuilder.False<Product>();

         if (!string.IsNullOrWhiteSpace(criteria.SPC))
            predicate = predicate.Or(p => p.SPC.Contains(criteria.SPC));

         if (!string.IsNullOrWhiteSpace(criteria.UPC))
            predicate = predicate.Or(p => p.ScanCodes.Any(c => c.Value.Contains(criteria.UPC)));

         if (!string.IsNullOrWhiteSpace(criteria.LPC))
            predicate = predicate.Or(p => p.LPC.Contains(criteria.LPC));

         predicate.And(p => p.ProductStatus != ProductStatus.Disabled);

         return FindAllAsExpandable().Where(predicate);
      }

      /// <summary>
      ///    Updates the Product for the corresponding relationships:
      ///    - ExternalProductMapping
      ///    - OrderItem
      ///    - ProductStockRequest
      ///    - Stock
      ///    - StockSetItem
      ///    - StockTakeItem
      /// </summary>
      /// <param name="toKeep"></param>
      /// <param name="toDelete"></param>
      /// <param name="merger"></param>
      public void MergeRelatedItems(Product toKeep, Product toDelete, string merger)
      {
         var alreadyMappedExternalIds = toKeep.ExternalProductMappings.Select(epm => epm.ExternalProductId).ToList();

         foreach (
            var mapping in
            toDelete.ExternalProductMappings)
            if (alreadyMappedExternalIds.Contains(mapping.ExternalProductId))
            {
               mapping.DeletedBy = merger;
               mapping.DeletedOn = DateTime.Now;
            }
            else
            {
               mapping.InventoryProductId = toKeep.ProductId;
            }

         foreach (var orderItem in toDelete.OrderItems) orderItem.ProductId = toKeep.ProductId;

         foreach (var request in toDelete.StockRequests) request.ProductId = toKeep.ProductId;

         foreach (var stock in toDelete.Stocks) stock.ProductId = toKeep.ProductId;

         foreach (var stockSetItem in toDelete.StockSetItems) stockSetItem.ProductId = toKeep.ProductId;

         foreach (var stockTakeItem in toDelete.StockTakeItems) stockTakeItem.ProductId = toKeep.ProductId;
      }

      public Product Create(string username)
      {
         return new Product
         {
            AutoReorderSetting = ReorderSettings.DoNotReorder,
            CreatedBy = username,
            CreatedOn = DateTime.Now,
            LastModifiedBy = username,
            LastModifiedOn = DateTime.Now,
            ManageStock = false
         };
      }

      public ProductPrice GetPrimaryPrice(int productId)
      {
         var product = Find(productId);
         return product == null
            ? null
            : product.Prices.FirstOrDefault(p => p.PriceTypeId == _propertyProvider.PrimaryBuyPriceTypeId);
      }

      public bool ContainsUnorderableProducts(IEnumerable<int> productIds)
      {
         var products = FindAll().Where(p => productIds.Contains(p.ProductId)).ToList();

         return products.Select(product => (product.UseCategorySettings ? product.ProductCategories.SelectMany(pc => pc.Category.StockSettings) : product.ProductSettings))
            .Select(settings => settings.Any(s => s.SettingId == InventoryConstants.StockSettings.Unorderable)).Any(unorderableExists => unorderableExists);
      }

      public IQueryable<Product> FindByDescription(string term)
      {
         return _context.Products.Where(p => p.Description.Contains(term));
      }

      public bool Exists(int productId)
      {
         return _context.Products.Any(p => p.ProductId == productId);
      }

      public void Remove(Product entity, string username)
      {
         var item = Find(entity.ProductId);

         if (item == null) return;
         item.DeletedOn = DateTime.Now;
         item.DeletedBy = username;

         if (entity.InError)
         {
            // need to validate 
            var duplicateSpcProducts = GetProductsWithDuplicateSpc(entity);
            var duplicateUpnProducts = GetProductsWithDuplicateUpn(entity);
            item.DeletedOn = DateTime.Now;
            if (duplicateSpcProducts.Any())
               foreach (var product in duplicateSpcProducts)
                  product.InError = ValidateProduct(product);

            if (duplicateUpnProducts.Any())
               foreach (var product in duplicateUpnProducts)
                  product.InError = ValidateProduct(product);
         }
      }

      public int Commit()
      {
         return _context.ObjectContext.SaveChanges();
      }

      public List<KeyValuePair<int, string>> ComplexSearch(string phrase)
      {
         var terms = phrase.Split(' ');

         var spcSearch = _context.Products.Where(product => product.DeletedOn == null && product.ProductStatus != ProductStatus.Disabled).AsQueryable();
         var descriptionSearch = _context.Products.Where(product => product.DeletedOn == null && product.ProductStatus != ProductStatus.Disabled).AsQueryable();

         foreach (var term in terms)
         {
            var keyword = term.Trim();
            spcSearch = spcSearch.Where(p => p.SPC.Contains(keyword));
            descriptionSearch = descriptionSearch.Where(p => p.Description.Contains(keyword));
         }

         var spcKvp =
            spcSearch.AsEnumerable()
               .Select(
                  p => new KeyValuePair<int, string>(p.ProductId, string.IsNullOrWhiteSpace(p.SPC) && p.Description != null
                     ? p.Description
                     : string.Format("{0}: {1}", p.SPC, p.Description)));
         var descriptionKvp =
            descriptionSearch.AsEnumerable().Select(p => new KeyValuePair<int, string>(p.ProductId, p.Description));

         return descriptionKvp.Union(spcKvp).ToList();
      }

      public IQueryable<ProductForExport> FindByConsumptionDate(DateTime? filterDate)
      {
         var products = from pwcfe in _context.ProductsWithConsumptionForExports
            join p in _context.Products.Include(i => i.StockTakeItems) on pwcfe.ProductId equals p.ProductId
            select new ProductForExport
            {
               Categories = p.ProductCategories.Select(pc => pc.Category),
               HadStockTake = p.StockTakeItems.Any(sti => !sti.DeletedOn.HasValue && sti.Status == StockTakeItemStatus.Complete),
               Prices = p.Prices,
               Product = p,
               ProductWithConsumption = pwcfe
            };

         if (filterDate.HasValue) products = products.Where(p => p.ProductWithConsumption.MostRecentConsumption > filterDate.Value);

         return products;
      }

      public List<KeyValuePair<int, string>> UpnSearch(string upn)
      {
         upn = upn.Trim();

         var products = (from c in _context.ScanCodes
            where c.Value.Contains(upn)
                  && c.Product.ProductStatus != ProductStatus.Disabled
            select c).Take(50).AsEnumerable().Select(p => new KeyValuePair<int, string>(p.ProductId, p.Value)).ToList();

         return products;
      }

      public IQueryable<Product> FindBySpc(string spc)
      {
         return _context.Products.Where(p => p.SPC == spc);
      }

      public string GetDescription(int id)
      {
         var product = _context.Products.SingleOrDefault(p => p.ProductId == id);
         return product == null ? string.Empty : product.Description;
      }

      public IQueryable<Product> FindMissingStockTake()
      {
         return from p in FindAll()
            join st in _context.StockTakeItems on p.ProductId equals st.ProductId
            where st.StockTake.Status == StockTakeStatus.Complete
            select p;
      }

      public IQueryable<ProductForExport> GetProductsForExport(FilterPeriod filterPeriod)
      {
         DateTime? filterDate;
         var now = DateTime.Now;
         switch (filterPeriod)
         {
            case FilterPeriod.Day:
               filterDate = now.AddDays(-1);
               break;
            case FilterPeriod.Week:
               filterDate = now.AddDays(-7);
               break;
            case FilterPeriod.Month:
               filterDate = now.AddMonths(-1);
               break;
            case FilterPeriod.Year:
               filterDate = now.AddYears(-1);
               break;
            default:
               filterDate = null;
               break;
         }

         var products = FindByConsumptionDate(filterDate);

         if (filterPeriod != FilterPeriod.None) products = products.Where(p => p.ProductWithConsumption.ConsumedQuantity > 0);
         return products;
      }

      public IQueryable<Product> GetStockTakeProducts(StockTakeProductFilter filter, int? filterLocation)
      {
         var products = FindAll();
         if (filterLocation.HasValue) products = products.Where(p => p.Stocks.Any(s => s.StoredAt == filterLocation));

         switch (filter)
         {
            case StockTakeProductFilter.All:
               break;
            case StockTakeProductFilter.WithoutStockTake:
               products = products.Where(p => !p.StockTakeItems.Any(sti => !sti.DeletedOn.HasValue && sti.Status == StockTakeItemStatus.Complete)); // TODO: make location specific
               break;
         }

         return products;
      }

      public DateTime? MostRecentStockTake(Product product, int locationId)
      {
         if (product == null) return null;

         return product.StockTakeItems != null && product.StockTakeItems.Any(sti => sti.StockTake.LocationId == locationId && !sti.DeletedOn.HasValue && sti.Status == StockTakeItemStatus.Complete)
            ? product.StockTakeItems.Where(sti => !sti.DeletedOn.HasValue && sti.Status == StockTakeItemStatus.Complete).Select(sti => sti.StockTake).Where(st => st.LocationId == locationId)
               .OrderByDescending(st => st.StockTakeDate).First().StockTakeDate
            : (DateTime?) null;
      }

      public IQueryable<Product> FindPendingProducts()
      {
         return FindAll().Where(p => p.ProductStatus == ProductStatus.Pending);
      }

      public List<KeyValuePair<int, string>> SpcSearch(string spc)
      {
         spc = spc.Trim();

         var products = (from p in _context.Products
            where p.SPC.Contains(spc) && p.ProductStatus != ProductStatus.Disabled
            select p).Take(50).AsEnumerable().Select(p => new KeyValuePair<int, string>(p.ProductId, p.SPC)).ToList();

         return products;
      }

      /// <summary>
      ///    Returns true if the product is valid.
      /// </summary>
      /// <param name="product"></param>
      /// <returns></returns>
      public bool ValidateProduct(Product product)
      {
         var uniqueSpc = true;
         var uniqueUpn = true;
         // stock handling fields
         var validStockHandling = ValidStockHandling(product);
         // spc or upn presence
         var hasCode = ProductCodesPresent(product);

         // duplicate spc
         var duplicateSpcProducts = GetProductsWithDuplicateSpc(product);
         if (duplicateSpcProducts.Any())
         {
            uniqueSpc = false;
            foreach (var duplicateSpcProduct in duplicateSpcProducts) duplicateSpcProduct.InError = true;
         }

         // duplicate upn
         var duplicateUpnProducts = GetProductsWithDuplicateUpn(product);
         if (duplicateUpnProducts.Any())
         {
            uniqueUpn = false;
            foreach (var duplicateUpnProduct in duplicateUpnProducts) duplicateUpnProduct.InError = true;
         }

         return uniqueSpc && uniqueUpn && validStockHandling && hasCode;
      }

      public bool ProductCodesPresent(Product product)
      {
         return !string.IsNullOrWhiteSpace(product.SPC) || product.ScanCodes != null && product.ScanCodes.Any(sc => sc.ProductId == product.ProductId);
      }

      public IEnumerable<ProductErrorModel> GetProductErrors(Product product)
      {
         var errors = new List<ProductErrorModel>();
         var duplicateSpcProducts = GetProductsWithDuplicateSpc(product);
         if (duplicateSpcProducts.Any())
            errors.Add(new ProductErrorModel
            {
               Products = duplicateSpcProducts.ToDictionary(p => p.ProductId, p => p.Description),
               Reason = ProductErrorReason.DuplicateSpc
            });

         var duplicateUpnProducts = GetProductsWithDuplicateUpn(product);
         if (duplicateUpnProducts.Any())
            errors.Add(new ProductErrorModel
            {
               Products = duplicateUpnProducts.ToDictionary(p => p.ProductId, p => p.Description),
               Reason = ProductErrorReason.DuplicateUpn
            });

         if (!ValidStockHandling(product))
            errors.Add(new ProductErrorModel
            {
               Reason = ProductErrorReason.StockHandling
            });

         if (string.IsNullOrWhiteSpace(product.SPC) && !product.ScanCodes.Any())
            errors.Add(new ProductErrorModel
            {
               Reason = ProductErrorReason.MissingCode
            });

         return errors;
      }

      public void UpdateProductField(PropertyInfo propertyInfo, Product product, object value, string username)
      {
         propertyInfo.SetValue(product, value, null);
         product.InError = !ValidateProduct(product);
         product.LastModifiedBy = username;
         product.LastModifiedOn = DateTime.Now;
      }

      public void Restore(Product product, string username)
      {
         product.DeletedOn = null;
         product.DeletedBy = null;
         product.LastModifiedBy = username;
         product.LastModifiedOn = DateTime.Now;
      }

      private void ValidateProductsWithSameCodeAsProduct(Product existing, Product updated)
      {
         if (!existing.InError) return;

         //
         //if (existing.UPN != updated.UPN)
         //{
         //   // validate products with this upn
         //   foreach (var product in FindAll().Where(p => p.ProductId != existing.ProductId && p.UPN == existing.UPN))
         //   {
         //      product.InError = ValidateProduct(product);
         //   }
         //}

         if (existing.SPC != updated.SPC)
            foreach (var product in FindAll().Where(p => p.ProductId != existing.ProductId && p.SPC == existing.SPC))
               product.InError = ValidateProduct(product);
      }

      /// <summary>
      ///    Return TRUE when the settings are VALID.
      ///    Worth mentioning because boolean logic is hard.
      /// </summary>
      /// <param name="product"></param>
      /// <returns></returns>
      public static bool ValidStockHandling(Product product)
      {
         if (product.ManageStock)
         {
            if (product.AutoReorderSetting == ReorderSettings.OneForOneReplace) return false;

            if (product.AutoReorderSetting == ReorderSettings.SpecifyLevels) return product.TargetStockLevel >= product.ReorderThreshold; // this is the valid configuration
         }

         if (!product.ManageStock && product.AutoReorderSetting == ReorderSettings.SpecifyLevels) return false;

         return true;
      }

      internal IQueryable<Product> GetProductsWithDuplicateSpc(Product product)
      {
         return !string.IsNullOrWhiteSpace(product.SPC) ? FindAll().Where(p => p.SPC == product.SPC && p.ProductId != product.ProductId) : new List<Product>().AsQueryable();
      }

      internal IQueryable<Product> GetProductsWithDuplicateUpn(Product product)
      {
         var upns = product.ScanCodes.Select(c => c.Value);
         return from p in FindAll() where p.ScanCodes.Any(sc => upns.Contains(sc.Value)) && p.ProductId != product.ProductId select p;
      }
   }

   public interface IProductRepository
   {
      Product Find(int id);
      IQueryable<Product> FindAll();
      IQueryable<Product> FindAll(bool includeDeleted);
      void Remove(Product entity, string username);
      int Commit();
      void Add(Product newEntity);
      void Add(Product product, IEnumerable<string> productSettings, IEnumerable<int> categories);
      List<KeyValuePair<int, string>> ComplexSearch(string phrase);
      IQueryable<Product> FindByCategory(int categoryId);
      ProductPrice GetPrimaryPrice(int productId);
      bool ContainsUnorderableProducts(IEnumerable<int> productIds);
      IQueryable<Product> FindByDescription(string term);
      bool Exists(int productId);
      IQueryable<ProductForExport> FindByConsumptionDate(DateTime? filterDate);
      List<KeyValuePair<int, string>> UpnSearch(string upn);
      IQueryable<Product> FindBySpc(string spc);
      string GetDescription(int id);
      IQueryable<Product> FindMissingStockTake();
      IQueryable<Product> FindAllInStock();
      IQueryable<Product> FindAllAsExpandable();
      IQueryable<ProductForExport> GetProductsForExport(FilterPeriod filterPeriod);
      IQueryable<Product> GetStockTakeProducts(StockTakeProductFilter filter, int? filterLocation);
      DateTime? MostRecentStockTake(Product product, int locationId);
      IEnumerable<Product> FindAllLocal();
      IQueryable<Product> FindPendingProducts();
      List<KeyValuePair<int, string>> SpcSearch(string spc);
      void UpdateSelectedProductSettings(Product product, IEnumerable<string> selectedSettings);
      void UpdateSelectedCategories(Product existing, IEnumerable<int> selectedCategories);
      bool ValidateProduct(Product product);
      void UpdateProductField(PropertyInfo propertyInfo, Product product, object value, string username);
      IEnumerable<ProductErrorModel> GetProductErrors(Product product);
      IQueryable<Product> FindByCode(ProductSearchCriteria productSearchCriteria);
      bool ProductCodesPresent(Product product);
      IQueryable<Product> FindByCodeUsingContains(ProductSearchCriteria criteria);
      void MergeRelatedItems(Product toKeep, Product toDelete, string merger);
      Product Create(string username);
      void UpdateProductProperties(Product entity, IEnumerable<string> selectedSettings, IEnumerable<int> selectedCategories, Product existing);
      IQueryable<Product> GetCurrentErroredProducts();
      ProductIdentifierResult GetProductOrderReferenceIdentifier(int productId, int? orderChannelId = null);
      void RemoveCategoriesFromProduct(IList<ProductCategory> categories);
      Product FindIncludingDeleted(int id);
      void Restore(Product product, string username);
   }
}