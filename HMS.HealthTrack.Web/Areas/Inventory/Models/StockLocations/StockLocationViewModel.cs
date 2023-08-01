using HMS.HealthTrack.Web.Infrastructure;
using HMS.HealthTrack.Web.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.StockLocations
{
   [ModelMetaType(typeof(StockLocationMeta))]
   public class StockLocationViewModel : IMapFrom<StockLocation>
   {
      public int LocationId { get; set; }
      public string Name { get; set; }
      public DateTime CreatedOn { get; set; }
      public string CreatedBy { get; set; }
      public DateTime? LastModifiedOn { get; set; }
      public string LastModifiedUser { get; set; }
      public DateTime? DeletedOn { get; set; }
      public string DeletedBy { get; set; }
      public Address Address { get; set; }
      public byte[] LogoImage { get; set; }
      [Display(Name = "Mapped locations")]
      public IEnumerable<int> HealthTrackLocations { get; set; }
      [Display(Name = "Mapped locations")]
      public string HealthTrackLocationNames { get; set; }

      public void SetMappedLocationNames(ICollection<HealthTrackLocation> locations)
      {
         if (locations == null || !locations.Any()) return;
         HealthTrackLocationNames = String.Join(", ", locations.Select(l => l.name));
         HealthTrackLocations = locations.Select(l => l.location_ID);
      }
   }

}