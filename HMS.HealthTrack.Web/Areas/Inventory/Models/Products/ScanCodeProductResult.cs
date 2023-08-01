using AutoMapper;
using HMS.HealthTrack.Web.Areas.Inventory.Models.ScanCode;
using HMS.HealthTrack.Web.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.Products
{
   public class ScanCodeProductResult : IHaveCustomMappings
   {
      public int ProductId { get; set; }
      public string SPC { get; set; }

      public string UPN
      {
         get
         {
            return UPNs == null || !UPNs.Any() ? string.Empty : UPNs.Aggregate((current, next) => current + "," + next).TrimEnd(',');
         }
      }

      public List<string> UPNs { get; set; }
      public string Description { get; set; }
      public bool ManageStock { get; set; }

      public string SerialNumber { get; set; }
      public string LotNumber { get; set; }
      public string Quantity { get; set; }
      public DateTime? ExpirationDate { get; set; }
      public void CreateMappings(IConfiguration configuration)
      {
         configuration.CreateMap<Product, ScanCodeProductResult>()
            .ForMember(result=>result.UPNs,options=>options.MapFrom(p=>p.ScanCodes.Select(sc=>sc.Value).ToList()));
         configuration.CreateMap<ScanCodeResult, ScanCodeProductResult>();

      }
   }
}