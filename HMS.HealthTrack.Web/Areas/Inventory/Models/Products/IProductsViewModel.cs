using HMS.HealthTrack.Web.Areas.Inventory.Models.Stock;
using HMS.HealthTrack.Web.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using HMS.HealthTrack.Web.Areas.Inventory.Models.ScanCode;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.Products
{
   [ModelMetaType(typeof(ProductMeta))]
   public interface IProductsViewModel
   {
      IEnumerable<ProductPriceViewModel> Prices { get; set; }
      IEnumerable<SelectListItem> CurrencyTypes { get; }
      IEnumerable<StockAtLocationModel> InitialStock { get; set; }
      
      IList<SelectListItem> NullableBoolValues { get; }
      int? ProductId { get; set; }
      string SPC { get; set; }
      string UPN { get; set; }
      string LPC { get; set; }
      string Description { get; set; }
      decimal? BuyPrice { get; set; }
      string BuyCurrency { get; set; }
      string BuyCurrencyRate { get; set; }
      decimal? SellPrice { get; set; }
      string GLC { get; set; }
      string Manufacturer { get; set; }
      bool? UseExpired { get; set; }
      bool? UseSterile { get; set; }
      string RebateCode { get; set; }
      string Notes { get; set; }
      string PrimarySupplier { get; set; }
      string SecondarySupplier { get; set; }
      IEnumerable<int> SelectedCategories { get; set; }
      bool IsConsignment { get; set; }
      [Display(Name = "Automatic re-order")]
      int AutoReorderSetting { get; set; }
      int MinimumOrder { get; set; }
      int OrderMultiple { get; set; }
      int ReorderThreshold { get; set; }
      int? TargetStockLevel { get; set; }
      decimal? MarkUp { get; set; }
      IEnumerable<string> SelectedSettings { get; set; }
      bool UseCategorySettings { get; set; }
      int? ExternalId { get; set; }
      int? LedgerId { get; set; }
      int? LastStockTakeId { get; set; }
      bool UsePaymentClassPrice { get; set; }
      string SpecialRequirements { get; set; }
      int ProductStatus { get; set; }
      bool InError { get; set; }
      bool ManageStock { get; set; }
      bool IsNew { get; }
      DateTime? ReplaceAfter { get; set; }
   }
}