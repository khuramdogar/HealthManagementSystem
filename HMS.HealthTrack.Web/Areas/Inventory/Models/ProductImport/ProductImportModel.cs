using HMS.HealthTrack.Web.Infrastructure;
using HMS.HealthTrack.Web.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.ProductImport
{
   /// <summary>
   /// ** IMPORTANT ** 
   /// When updating validation on this model, also update validation in the ProductImportValidation class and the ProductsToImport model of datasource.
   /// </summary>
   public class ProductImportModel : IMapFrom<Data.Model.Inventory.ProductImport>
   {
      public int ProductImportId { get; set; }
      public int ProductImportDataId { get; set; }
      public bool Processed { get; set; }
      public DateTime? ProcessedOn { get; set; }

      [Required, Display(Name = ProductImportColumnNames.Spc), MinLength(1)]
      public string SPC { get; set; }

      [Display(Name = ProductImportColumnNames.Lpc)]
      public string LPC { get; set; }

      [Required, Display(Name = ProductImportColumnNames.Description)]
      public string Description { get; set; }

      [Display(Name = ProductImportColumnNames.Notes)]
      public string Notes { get; set; }

      [Display(Name = ProductImportColumnNames.Glc)]
      public string GLC { get; set; }

      [Display(Name = ProductImportColumnNames.Glc)]
      public string LedgerId { get; set; }

      [Display(Name = ProductImportColumnNames.Manufacturer)]
      public string Manufacturer { get; set; }

      [Display(Name = ProductImportColumnNames.Supplier), Remote("SingleSupplierExists", "Validation", AdditionalFields = "SupplierId")]
      public string Supplier { get; set; }

      [Display(Name = ProductImportColumnNames.Supplier)]
      public string SupplierId { get; set; }

      [Display(Name = ProductImportColumnNames.Category)]
      public string Category { get; set; }

      [NumericString, Display(Name = ProductImportColumnNames.MinOrder)]
      public string MinimumOrder { get; set; }

      [NumericString, Display(Name = ProductImportColumnNames.OrderMultiple)]
      public string OrderMultiple { get; set; }

      [NumericString, Display(Name = ProductImportColumnNames.ReorderThreshold)]
      public string ReorderThreshold { get; set; }

      [NumericString, Display(Name = ProductImportColumnNames.TargetStockLevel)]
      public string TargetStockLevel { get; set; }

      [DecimalString, Display(Name = ProductImportColumnNames.PublicBuyPrice)]
      public string PublicUnitPrice { get; set; }

      [DecimalString, Display(Name = ProductImportColumnNames.PrivateBuyPrice)]
      public string PrivateUnitPrice { get; set; }

      [BoolString, Display(Name = ProductImportColumnNames.Consignment)]
      public string Consignment { get; set; }

      [BoolString, Display(Name = ProductImportColumnNames.Sterile)]
      public string Sterile { get; set; }

      [NumericString, Display(Name = ProductImportColumnNames.ProductId)]
      public string ProductId { get; set; }

      [NumericString, Display(Name = ProductImportColumnNames.ProductId)]
      public string InternalProductId { get; set; }

      [Display(Name = ProductImportColumnNames.Upn)]
      public string UPN { get; set; }

      [Display(Name = ProductImportColumnNames.AutoReorderSetting), Remote("ValidReorderSetting", "Validation")]
      public string AutoReorderSetting { get; set; }

      [Display(Name = ProductImportColumnNames.ProductSettings), Remote("ValidProductSettings", "Validation")]
      public string ProductSettings { get; set; }

      [BoolString, Display(Name = ProductImportColumnNames.UseCategorySettings)]
      public string UseCategorySettings { get; set; }

      public bool Invalid { get; set; }

      public bool Valid
      {
         get { return !Invalid; }
      }

      public bool ValidGlc { get; set; }

      public List<GeneralLedgerTierCodeModel> TierCodeModels { get; set; }

      public string Message { get; set; }

      public string RebateCode { get; set; }
   }
}