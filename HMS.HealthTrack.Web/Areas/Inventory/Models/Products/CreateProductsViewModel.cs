using AutoMapper;
using HMS.HealthTrack.Web.Areas.Inventory.Models.Stock;
using HMS.HealthTrack.Web.Content;
using HMS.HealthTrack.Web.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using HMS.HealthTrack.Web.Areas.Inventory.Models.ScanCode;
using HMS.HealthTrack.Web.Data.Model.HealthTrackInventory;
using HMS.HealthTrack.Web.Data.Model.Inventory;


namespace HMS.HealthTrack.Web.Areas.Inventory.Models.Products
{
   public class CreateProductsViewModel : IHaveCustomMappings, IProductsViewModel
   {
      public IEnumerable<ProductPriceViewModel> Prices { get; set; }

      public IEnumerable<SelectListItem> CurrencyTypes
      {
         get { return new SelectList(Currencies.CurrencyList); }
      }

      public IEnumerable<StockAtLocationModel> InitialStock { get; set; }
      public string UPN { get; set; }

      public IList<SelectListItem> NullableBoolValues
      {
         get
         {
            return new List<SelectListItem>()
            {
               new SelectListItem() {Text = "Yes", Value = "True"},
               new SelectListItem() {Text = "No", Value = "False"},
            };
         }
      }

      public int? ProductId { get; set; }

      [Required]
      public string SPC { get; set; }

      public string LPC { get; set; }
      
      [Required]
      public string Description { get; set; }

      public decimal? BuyPrice { get; set; }
      public string BuyCurrency { get; set; }
      public string BuyCurrencyRate { get; set; }
      public decimal? SellPrice { get; set; }
      public string GLC { get; set; }
      public string Manufacturer { get; set; }
      public bool? UseExpired { get; set; }
      public bool? UseSterile { get; set; }
      public string RebateCode { get; set; }
      public string Notes { get; set; }
      public int MaxUses { get; set; }
      public IEnumerable<int> SelectedCategories { get; set; }
      public bool IsConsignment { get; set; }
      public int AutoReorderSetting { get; set; }
      public int MinimumOrder { get; set; }
      public int OrderMultiple { get; set; }
      public int ReorderThreshold { get; set; }
      public int? TargetStockLevel { get; set; }
      public decimal? MarkUp { get; set; }
      public IEnumerable<string> SelectedSettings { get; set; }
      public bool UseCategorySettings { get; set; }
      public string PrimarySupplier { get; set; }
      public string SecondarySupplier { get; set; }
      public int? ExternalId { get; set; }
      public int? LedgerId { get; set; }
      public int? LastStockTakeId { get; set; }
      public bool UsePaymentClassPrice { get; set; }
      public string SpecialRequirements { get; set; }
      public int ProductStatus { get; set; }
      public bool InError { get; set; }
      public bool ManageStock { get; set; }
      public bool IsNew { get { return true; } }
      public DateTime? ReplaceAfter { get; set; }

      public void CreateMappings(IConfiguration configuration)
      {
         configuration.CreateMap<Inventory_Master, CreateProductsViewModel>()
           .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Inv_Description ?? string.Empty))
           .ForMember(dest => dest.SPC, opt => opt.MapFrom(src => src.Inv_SPC))
           .ForMember(dest => dest.LPC, opt => opt.MapFrom(src => src.Inv_LPC))
           .ForMember(dest => dest.BuyPrice, opt => opt.MapFrom(src => src.Inv_BuyPrice))
           .ForMember(dest => dest.BuyCurrency, opt => opt.MapFrom(src => src.Inv_BuyCurrency))
           .ForMember(dest => dest.BuyCurrencyRate, opt => opt.MapFrom(src => src.Inv_BuyCurrencyRate.HasValue ? src.Inv_BuyCurrencyRate.ToString() : null))
           .ForMember(dest => dest.SellPrice, opt => opt.MapFrom(src => src.Inv_SellPrice))
           .ForMember(dest => dest.GLC, opt => opt.MapFrom(src => src.Inv_GL))
           .ForMember(dest => dest.UseSterile, opt => opt.MapFrom(src => src.Inv_UseSterile))
           .ForMember(dest => dest.RebateCode, opt => opt.MapFrom(src => src.Billing_Code))
           .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Description_Additional));

         configuration.CreateMap<Product, CreateProductsViewModel>();
         configuration.CreateMap<CreateProductsViewModel, Product>();
      }
   }
}