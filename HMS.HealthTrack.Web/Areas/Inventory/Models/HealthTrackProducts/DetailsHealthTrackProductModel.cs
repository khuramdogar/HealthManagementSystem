using HMS.HealthTrack.Web.Infrastructure;
using HMS.HealthTrack.Web.Utils;
using System;
using HMS.HealthTrack.Web.Data.Model.HealthTrackInventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.HealthTrackProducts
{
   [ModelMetaType(typeof(Inventory_MasterMeta))]
   public class DetailsHealthTrackProductModel : IMapFrom<Inventory_Master>
   {
      public int Inv_ID { get; set; } // Inv_ID (Primary key)
      public string Inv_SPC { get; set; } // Inv_SPC. Supplier Product Code
      public string Inv_LPC { get; set; } // Inv_LPC. Local Product Code (eg Hospital)
      public string Inv_UPN { get; set; } // Inv_UPN. Universal Product Number (EAN UPN )
      public string Inv_Description { get; set; } // Inv_Description
      public string Inv_Group { get; set; } // Inv_Group
      public string Inv_SubGroup { get; set; } // Inv_SubGroup
      public string Inv_GL { get; set; } // Inv_GL. General Ledger Code
      public string Manufacturer { get; set; } // Manufacturer. Manufacturer
      public bool? deleted { get; set; } // deleted
      public DateTime? deletionDate { get; set; } // deletionDate
      public string deletionUser { get; set; } // deletionUser
      public DateTime? dateLastModified { get; set; } // dateLastModified
      public string userLastModified { get; set; } // userLastModified
      public DateTime? dateCreated { get; set; } // dateCreated
      public string userCreated { get; set; } // userCreated
      public string Billing_Code { get; set; } // Billing_Code
      public string Description_Additional { get; set; } // Description_Additional
   }
}