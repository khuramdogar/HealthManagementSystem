using System;
using System.ComponentModel.DataAnnotations;

namespace HMS.HealthTrack.Web.Data.Model.HealthTrackInventory
{
   #region InventoryMaster

   [MetadataType(typeof(Inventory_MasterMeta))]
   public partial class Inventory_Master
   {
   }

   public class Inventory_MasterMeta
   {
      [Key]
      [Display(Name = "Product ID")]
      public int Inv_ID { get; set; }

      [Display(Name = "SPC")]
      public string Inv_SPC { get; set; } // Supplier Product Code

      [Display(Name = "LPC")]
      public string Inv_LPC { get; set; } // Local Product Code (eg Hospital)

      [Display(Name = "UPN")]
      public string Inv_UPN { get; set; } // Universal Product Number (EAN UPN )

      [Display(Name = "Description")]
      public string Inv_Description { get; set; }

      [Display(Name = "Group")]
      public string Inv_Group { get; set; }

      [Display(Name = "Sub group")]
      public string Inv_SubGroup { get; set; }

      [Display(Name = "Buy price")]
      public decimal? Inv_BuyPrice { get; set; }

      [Display(Name = "Buy currency")]
      public string Inv_BuyCurrency { get; set; }

      [Display(Name = "Buy currency rate")]
      public double? Inv_BuyCurrencyRate { get; set; }

      [Display(Name = "Sell price")]
      public decimal? Inv_SellPrice { get; set; }

      [Display(Name = "General ledger")]
      public string Inv_GL { get; set; } // General Ledger Code

      public string Manufacturer { get; set; } // Manufacturer
      public bool? Inv_UseExpired { get; set; }
      public bool? Inv_UseSterile { get; set; }
      public bool? deleted { get; set; }

      [Display(Name = "Deleted on")]
      public DateTime? deletionDate { get; set; }

      [Display(Name = "Deleted by")]
      public string deletionUser { get; set; }

      [Display(Name = "Modified on")]
      public DateTime? dateLastModified { get; set; }

      [Display(Name = "Modified by")]
      public string userLastModified { get; set; }

      [Display(Name = "Created by")]
      public DateTime? dateCreated { get; set; }

      [Display(Name = "Created on")]
      public string userCreated { get; set; }

      public string Billing_Code { get; set; }
      public decimal? Min_Benefit { get; set; }
      public decimal? Max_Benefit { get; set; }

      [Display(Name = "Notes")]
      public string Description_Additional { get; set; }

      public string Dimensions { get; set; }
      public string ProductModel { get; set; }
      public string SpecialRequirements { get; set; }
      public int? Markup { get; set; }
      public int? PriceModelID { get; set; }
   }

   #endregion
}