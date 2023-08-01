
using AutoMapper;
using HMS.HealthTrack.Web.Infrastructure;
using HMS.HealthTrack.Web.Utils;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using HMS.HealthTrack.Web.Areas.Inventory.Models.ScanCode;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.Products
{
   public class MergeProductsModel
   {
      public MergeProductsViewModel ToKeep { get; set; }
      public MergeProductsViewModel ToDelete { get; set; }
   }

   [ModelMetaType(typeof(ProductMeta))]
   public class MergeProductsViewModel : IHaveCustomMappings
   {
      private List<string> UPNs;
      public int ProductId { get; set; }
      public string SPC { get; set; }
      public string LPC { get; set; }
      public string UPN
      {
         get
         {
            return UPNs == null || !UPNs.Any() ? string.Empty : UPNs.Aggregate((current, next) => current + "," + next).TrimEnd(',');
         }
      }

      public string GLC { get; set; }
      public string Description { get; set; }
      public decimal? BuyPrice { get; set; }
      public string BuyCurrency { get; set; }
      public string BuyCurrencyRate { get; set; }
      public decimal? SellPrice { get; set; }
      public string Manufacturer { get; set; }
      public string UseExpired { get; set; }
      public string UseSterile { get; set; }
      public string Notes { get; set; }
      public int? MaxUses { get; set; }
      public string PrimarySupplier { get; set; }
      public string SecondarySupplier { get; set; }
      public string IsConsignment { get; set; }
      public string RebateCode { get; set; }
      public int? MinimumOrder { get; set; }
      public int? OrderMultiple { get; set; }
      public int? ReorderThreshold { get; set; }
      [Display(Name = "Automatic re-order")]
      public string AutoReorderSetting { get; set; }
      public int? TargetStockLevel { get; set; }
      [Display(Name = "Use category settings")]
      public bool? UseCategorySettings { get; set; }
      public string UsePaymentClassPrice { get; set; }
      public string SpecialRequirements { get; set; }
      [Display(Name = "Status", Description = "The status of the product")]
      public ProductStatus? ProductStatus { get; set; }
      public string SelectedCategories { get; set; }
      public IEnumerable<ProductPriceViewModel> Prices { get; set; }
      public IEnumerable<ScanCodeModel> ScanCodes { get; set; }
      [Display(Name = "Settings")]
      public IEnumerable<string> Settings { get; set; }
      public decimal? MarkUp { get; set; }
      public bool ManageStock { get; set; }
      public DateTime? ReplaceAfter { get; set; }

      public void CreateMappings(IConfiguration configuration)
      {
         configuration.CreateMap<Product, MergeProductsViewModel>()
            .ForMember(m => m.Settings,
               opt =>
                  opt.MapFrom(
                     ip =>
                        ip.UseCategorySettings
                           ? ip.ProductCategories.SelectMany(pc => pc.Category.StockSettings)
                              .Select(s => s.Name)
                              .Distinct()
                           : ip.ProductSettings.Select(ps => ps.Name)))
            .ForMember(m => m.PrimarySupplier,
               opt =>
                  opt.MapFrom(
                     ip => ip.PrimarySupplierCompany != null ? ip.PrimarySupplierCompany.companyName : string.Empty))
            .ForMember(m => m.SecondarySupplier,
               opt =>
                  opt.MapFrom(
                     ip =>
                        ip.SecondarySupplierCompany != null ? ip.SecondarySupplierCompany.companyName : string.Empty))
            .ForMember(m => m.Prices, opt => opt.MapFrom(ip => ip.Prices))
            .ForMember(result => result.UPNs, options => options.MapFrom(p => p.ScanCodes.Select(sc => sc.Value).ToList()))
            .ForMember(m => m.AutoReorderSetting,
               opt => opt.MapFrom(ip => HelperMethods.GetEnumDisplayName<ReorderSettings>(ip.AutoReorderSetting)))
               .ForMember(m => m.SelectedCategories, opt => opt.MapFrom(ip => ip.ProductCategories.Select(c => c.Category.CategoryName).Join(", ")));
      }
   }
}