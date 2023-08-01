using HMS.HealthTrack.Web.Areas.Inventory.Models.Stock;
using HMS.HealthTrack.Web.Content;
using HMS.HealthTrack.Web.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using HMS.HealthTrack.Web.Areas.Inventory.Models.ScanCode;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.Products
{
   public class EditProductsViewModel : IProductsViewModel, IMapFrom<Product>
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
      public string PrimarySupplier { get; set; }
      public string SecondarySupplier { get; set; }

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
      public int? ExternalId { get; set; }
      public int? LedgerId { get; set; }
      public int? LastStockTakeId { get; set; }
      public bool UsePaymentClassPrice { get; set; }
      public string SpecialRequirements { get; set; }
      public int ProductStatus { get; set; }
      public bool InError { get; set; }
      public bool ManageStock { get; set; }
      public bool IsNew { get { return false; } }
      public DateTime? ReplaceAfter { get; set; }
   }
}