using HMS.HealthTrack.Web.Areas.Inventory.Models.Products;
using HMS.HealthTrack.Web.Areas.Inventory.Models.Stock;
using System.Linq;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;

namespace HMS.HealthTrack.Web.Utils
{
   internal class ProductFormHelper
   {
      internal static CreateProductsViewModel ConstructProductModel(ICompanyRepository companyRepository, IProductPriceRepository productPriceRepository, ICategoryRepository categoryRepository, IStockLocationRepository stockLocationRepository)
      {
         var model = new CreateProductsViewModel();

         //Add prices
         var priceTypes = productPriceRepository.FindAllPriceTypes();

         model.Prices = priceTypes.Select(type => new ProductPriceViewModel { PriceTypeId = type.PriceTypeId, PriceTypeName = type.Name }).ToList();

         model.UseCategorySettings = true;

         var locations = stockLocationRepository.FindAll();

         model.InitialStock = locations.Select(location => new StockAtLocationModel
         {
            LocationId = location.LocationId,
            Location = location.Name,
            Quantity = null
         }).ToList();

         return model;
      }
   }
}
