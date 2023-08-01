using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.ProductSearch
{
   /// <summary>
   /// Fields prefixed with ps (product search) to allow model binding for pages which use the advanced search
   /// </summary>
   public class ProductSearchModel
   {
      [Display(Name = "Deleted")]
      public bool psIncludeDeleted { get; set; }
      [Display(Name = "Description")]
      public string psDescription { get; set; }
      [Display(Name = "UPN")]
      public string psUPN { get; set; }
      [Display(Name = "SPC")]
      public string psSPC { get; set; }
      [Display(Name = "Manufacturer")]
      public string psManufacturer { get; set; }
      [Display(Name = "Supplier")]
      public int? psSupplier { get; set; }
      [Display(Name = "In stock")]
      public string psInStock { get; set; }
      [Display(Name = "Consignment")]
      public string psConsignment { get; set; }
      [Display(Name = "Category")]
      public IEnumerable<string> psCategories { get; set; }
      [Display(Name = "Rebate code")]
      public string psRebateCode { get; set; }
      [Display(Name = "Unclassified")]
      public string psUnclassified { get; set; }
      [Display(Name = "Had stock take")]
      public string psHasHadStockTake { get; set; }
      [Display(Name = "Consumed pending")]
      public string psPendingConsumedProducts { get; set; }
      [Display(Name = "Status")]
      public string psStatus { get; set; }
      [Display(Name = "In error")]
      public string psInError { get; set; }
      [Display(Name = "Managed stock")]
      public string psManageStock { get; set; }
   }
}