using AutoMapper;
using HMS.HealthTrack.Web.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using HMS.HealthTrack.Inventory.Common.Constants;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.ProductUpdates
{
   public class ProductUpdate : IHaveCustomMappings
   {
      public int ProductId { get; set; }
      public string SPC { get; set; }
      public string LPC { get; set; }
      public string UPN { get; set; }
      public string Description { get; set; }
      public string Manufacturer { get; set; }
      public string Notes { get; set; }
      public string SupplierName { get; set; }
      public bool IsConsignment { get; set; }
      public string RebateCode { get; set; }
      public bool RequiresSerial { get { return Settings != null && Settings.Any(s => s == InventoryConstants.StockSettings.RequiresSerialNumber); } }
      public bool RequiresBatch { get { return Settings != null && Settings.Any(s => s == InventoryConstants.StockSettings.RequiresBatchNumber); } }
      public bool RequiresExpiry { get { return false; } } //TODO: Expires
      public bool RequiresPatientConsumptionDetails { get { return Settings != null && Settings.Any(s => s == InventoryConstants.StockSettings.RequiresPatientDetails); } }

      public DateTime? LastStockTakeDate { get; set; }
      public IEnumerable<string> Settings { get; set; }


      public void CreateMappings(IConfiguration configuration)
      {
         configuration.CreateMap<Product, ProductUpdate>()
            .ForMember(dest => dest.SupplierName,
               opt =>
                  opt.MapFrom(
                     src => src.PrimarySupplierCompany == null ? string.Empty : src.PrimarySupplierCompany.companyName))
            .ForMember(pu => pu.RequiresSerial,
               opt =>
                  opt.MapFrom(
                     p =>
                        p.ProductSettings.Any(
                           pd => pd.SettingId == InventoryConstants.StockSettings.RequiresSerialNumber)))
            .ForMember(m => m.Settings, opt => opt.MapFrom(ip => ip.UseCategorySettings
               ? ip.ProductCategories.SelectMany(pc => pc.Category.StockSettings).Select(s => s.SettingId).Distinct()
               : ip.ProductSettings.Select(ps => ps.SettingId)))
            .ForMember(m => m.LastStockTakeDate,
               opt =>
                  opt.MapFrom(
                     src =>
                        src.StockTakeItems.Any(
                           sti => sti.Status == StockTakeItemStatus.Complete && !sti.DeletedOn.HasValue)
                           ? src.StockTakeItems.Where(
                              sti => sti.Status == StockTakeItemStatus.Complete && !sti.DeletedOn.HasValue)
                              .Select(s => s.StockTake)
                              .OrderByDescending(s => s.StockTakeDate)
                              .First()
                              .StockTakeDate
                           : (DateTime?)null));
      }
   }
}