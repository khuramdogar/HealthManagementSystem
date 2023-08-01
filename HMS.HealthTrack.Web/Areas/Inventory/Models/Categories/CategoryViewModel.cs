using HMS.HealthTrack.Web.Areas.Inventory.Models.StockSettings;
using HMS.HealthTrack.Web.Infrastructure;
using HMS.HealthTrack.Web.Utils;
using System;
using System.Collections.Generic;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.Categories
{
   [ModelMetaType(typeof(CategoryMeta))]
   public class CategoryViewModel : IMapFrom<Category>
   {
      public int? CategoryId { get; set; }
      public string CategoryName { get; set; }
      public bool Deleted { get; set; }
      public string DeletedBy { get; set; }
      public DateTime? DeletedOn { get; set; }
      public DateTime? LastModifiedDate { get; set; }
      public string LastModifiedUser { get; set; }
      public DateTime CreationDate { get; set; }
      public string UserCreated { get; set; }
      public IEnumerable<StockSettingViewModel> StockSettings { get; set; }
      public int? ParentId { get; set; }
      public bool HasChildren { get; set; }
      public bool Disinherit { get; set; }

   }
}