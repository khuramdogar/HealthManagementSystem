using AutoMapper;
using HealthTrack.InventoryScanner.ScanCodeParsers;
using HMS.HealthTrack.Web.Infrastructure;
using System;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.ScanCode
{
   public class ScanCodeResult : IHaveCustomMappings
   {
      public DateTime ExpirationDate { get; set; }
      public int? InventoryProductId { get; set; }
      public string LotNumber { get; set; }
      public string Quantity { get; set; }
      public string SPC { get; set; }
      public string SerialNumber { get; set; }
      public int UnitOfMeasure { get; set; }
      public string UPN { get; set; }
      public void CreateMappings(IConfiguration configuration)
      {
         configuration.CreateMap<HibcParser, ScanCodeResult>()
            .ForMember(m => m.SPC, opt => opt.MapFrom(src => src.ProductNumber));
      }
   }
}