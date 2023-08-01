using HMS.HealthTrack.Web.Areas.Inventory.Models.Stock;
using HMS.HealthTrack.Web.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using HMS.HealthTrack.Web.Areas.Inventory.Models.ScanCode;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.Products
{
   public class BulkUpdateProductModel : IProductsViewModel
   {
      #region Ignored product properties
      [Ignore]
      public IEnumerable<ProductPriceViewModel> Prices { get; set; }
      [Ignore]
      public IEnumerable<SelectListItem> CurrencyTypes { get; private set; }
      [Ignore]
      public IEnumerable<StockAtLocationModel> InitialStock { get; set; }

      public IEnumerable<ScanCodeModel> ScanCodes { get; set; }

      [Ignore]
      public IList<SelectListItem> NullableBoolValues { get; private set; }

      public int? ProductId { get; set; }

      [Ignore]
      public string SPC { get; set; }
      [Ignore]
      public string LPC { get; set; }
      [Ignore]
      public string UPN { get; set; }
      [Ignore]
      public string Description { get; set; }
      [Ignore]
      public decimal? BuyPrice { get; set; }
      [Ignore]
      public string BuyCurrency { get; set; }
      [Ignore]
      public string BuyCurrencyRate { get; set; }
      [Ignore]
      public decimal? SellPrice { get; set; }
      [Ignore]
      public string GLC { get; set; }

      [Ignore]
      public int? LastStockTakeId { get; set; }
      [Ignore]
      public int? ExternalId { get; set; }
      #endregion

      [Ignore] //Todo: unignore when markup is added
      public decimal? MarkUp { get; set; }

      [Ignore]
      public bool InError { get; set; }

      [Ignore]
      public bool IsNew { get; set; }

      [Ignore]
      public DateTime? ReplaceAfter { get; set; }

      public List<string> UPNs { get; set; }

      /// <summary>
      /// Product properties which can be updated via bulk update
      /// </summary>

      [Display(Name = "Automatic re-order")]
      public int AutoReorderSetting { get; set; }

      [Display(Name = "Status")]
      public int ProductStatus { get; set; }

      public string Manufacturer { get; set; }

      [Display(Name = "Use expired")]
      public bool? UseExpired { get; set; }

      [Display(Name = "Use sterile")]
      public bool? UseSterile { get; set; }

      [Display(Name = "Rebate code")]
      public string RebateCode { get; set; }

      public string Notes { get; set; }

      [Display(Name = "Primary supplier")]
      public string PrimarySupplier { get; set; }

      [Display(Name = "Secondary supplier")]
      public string SecondarySupplier { get; set; }

      [Display(Name = "Search categories")]
      public IEnumerable<int> SelectedCategories { get; set; }

      [Display(Name = "Consignment")]
      public bool IsConsignment { get; set; }

      [Display(Name = "Minimum order")]
      public int MinimumOrder { get; set; }

      [Display(Name = "Order multiple")]
      public int OrderMultiple { get; set; }

      [Display(Name = "Reorder threshold")]
      public int ReorderThreshold { get; set; }

      [Display(Name = "Target stock level")]
      public int? TargetStockLevel { get; set; }

      [Display(Name = "Product settings")]
      public IEnumerable<string> SelectedSettings { get; set; }

      [Display(Name = "Use category settings")]
      public bool UseCategorySettings { get; set; }

      [Display(Name = "GLC")]
      public int? LedgerId { get; set; }

      [Display(Name = "Use payment class price")]
      public bool UsePaymentClassPrice { get; set; }

      [Display(Name = "Special requirements")]
      public string SpecialRequirements { get; set; }

      [Display(Name = "Manage stock")]
      public bool ManageStock { get; set; }
   }
}