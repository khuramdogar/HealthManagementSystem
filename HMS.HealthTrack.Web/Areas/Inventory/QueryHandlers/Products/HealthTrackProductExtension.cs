using System;
using System.Linq;
using System.Linq.Expressions;
using HMS.HealthTrack.Web.Areas.Inventory.Models.ExternalApi.Products;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.QueryHandlers.Products
{
   public class HealthTrackProductExtension  
   {
      public static Expression<Func<Product, ProductDto>> Projection
      {
         get
         {
            return c => new ProductDto()
            {
               ProductId = c.ProductId,
               Name = c.Description,
               SPC = c.SPC,
               UPCs = c.ScanCodes.Select(sc=>sc.Value),
               GeneralLedgeCode = c.GLC,
               Manufacturer = c.Manufacturer,
               Supplier = c.PrimarySupplier.HasValue ? c.PrimarySupplierCompany.companyName : null,
               RebateCode = c.RebateCode,
               Prices = c.Prices.Select(p => new ProductPriceDto
               {
                  BuyPrice = p.BuyPrice,
                  SellPrice = p.SellPrice,
                  PriceId = p.PriceId,
                  PriceName = p.PriceType.Name,
                  UsedFor = p.PriceType.PaymentClassMappings.Select(m => m.PaymentClass)
               })
            };
         }
      }  
   }
}