using AutoMapper;
using HMS.HealthTrack.Web.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using HMS.HealthTrack.Web.Data.Helpers;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.Products
{
   public class ExportProductsViewModel : IHaveCustomMappings
   {
      public int ProductProductId { get; set; }
      public string SPC { get; set; }
      public string ProductLPC { get; set; }
      public string UPN { get; set; }
      public int? ProductLedgerId { get; set; }
      public string Description { get; set; }
      public string Manufacturer { get; set; }
      public bool? ProductUseSterile { get; set; }
      public string ProductNotes { get; set; }
      public string PrimarySupplier { get; set; }
      public bool Consignment { get; set; }
      public string RebateCode { get; set; }
      public int ProductMinimumOrder { get; set; }
      public int ProductOrderMultiple { get; set; }
      public int ProductReorderThreshold { get; set; }
      public int? ProductTargetStockLevel { get; set; }
      public bool ProductUseCategorySettings { get; set; }

      public string Categories { get; set; }

      public string PublicBuyPrice { get; set; }
      public string PrivateBuyPrice { get; set; }

      public IEnumerable<ProductPriceViewModel> Prices { get; set; }
      public DateTime? MostRecentConsumption { get; set; }
      public int ConsumptionCount { get; set; }
      public ReorderSettings AutoReorderSetting { get; set; }
      public string Settings { get; set; }
      public bool HasHadStockTake { get; set; }
      public bool InStock { get; set; }
      public bool Unclassified { get; set; }

      public void CreateMappings(IConfiguration configuration)
      {
         configuration.CreateMap<ProductForExport, ExportProductsViewModel>()
            .ForMember(m => m.PrimarySupplier,
               opt =>
                  opt.MapFrom(
                     ip =>
                        ip.ProductWithConsumption.PrimarySupplierName ?? string.Empty))
            .ForMember(m => m.PublicBuyPrice,
               opt =>
                  opt.MapFrom(
                     ip =>
                        ip.Prices != null && ip.Prices.Any(p => p.PriceTypeId == SystemSettings.PublicPriceTypeId)
                           ? ip.Prices.First(p => p.PriceTypeId == SystemSettings.PublicPriceTypeId).BuyPrice.ToString()
                           : string.Empty))
            .ForMember(m => m.PrivateBuyPrice,
               opt =>
                  opt.MapFrom(
                     ip =>
                        ip.Prices != null && ip.Prices.Any(p => p.PriceTypeId == SystemSettings.PrivatePriceTypeId)
                           ? ip.Prices.First(p => p.PriceTypeId == SystemSettings.PrivatePriceTypeId)
                              .BuyPrice.ToString()
                           : string.Empty))
            .ForMember(m => m.MostRecentConsumption,
               opt => opt.MapFrom(cp => cp.ProductWithConsumption.MostRecentConsumption))
            .ForMember(m => m.ConsumptionCount, opt => opt.MapFrom(cp => cp.ProductWithConsumption.ConsumedQuantity))
            .ForMember(m => m.Categories,
               opt =>
                  opt.MapFrom(
                     cp =>
                        cp.Categories != null && cp.Categories.Any()
                           ? string.Join(", ", cp.Categories.Select(c => c.CategoryName))
                           : string.Empty))
            .ForMember(m => m.Settings,
               opt =>
                  opt.MapFrom(
                     cp =>
                        cp.Product.UseCategorySettings
                           ? ProductImportStaticValidation.GetProductSettingString(
                              cp.Categories.SelectMany(c => c.StockSettings))
                           : ProductImportStaticValidation.GetProductSettingString(cp.Product.ProductSettings)))
            .ForMember(m => m.SPC, opt => opt.MapFrom(src => src.Product.SPC))
            .ForMember(m => m.Description, opt => opt.MapFrom(src => src.Product.Description))
            .ForMember(m => m.Manufacturer, opt => opt.MapFrom(src => src.Product.Manufacturer))
            .ForMember(m => m.RebateCode, opt => opt.MapFrom(src => src.Product.RebateCode))
            .ForMember(m => m.Consignment, opt => opt.MapFrom(src => src.Product.IsConsignment))
            .ForMember(m => m.HasHadStockTake,
               opt =>
                  opt.MapFrom(
                     src =>
                        src.Product.StockTakeItems.Any(
                           sti => !sti.DeletedOn.HasValue && sti.Status == StockTakeItemStatus.Complete)))
            .ForMember(m => m.InStock,
               opt =>
                  opt.MapFrom(
                     src => src.Product.Stocks.Any(s => s.StockStatus == StockStatus.Available && s.Quantity > 0)))
            .ForMember(m => m.Unclassified, opt => opt.MapFrom(src => !src.Product.ProductCategories.Any()));
      }
   }
}