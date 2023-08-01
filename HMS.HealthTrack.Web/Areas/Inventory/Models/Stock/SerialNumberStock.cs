using AutoMapper;
using HMS.HealthTrack.Web.Infrastructure;
using System;
using System.ComponentModel.DataAnnotations;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.Stock
{
   public class SerialNumberStock : IHaveCustomMappings
   {
      public int StockId { get; set; }
      public int ProductId { get; set; }
      public int LocationId { get; set; }
      [Required]
      public string SerialNumber { get; set; }
      public DateTime ReceivedOn { get; set; }
      public string ReceivedBy { get; set; }
      public void CreateMappings(IConfiguration configuration)
      {
         configuration.CreateMap<Data.Model.Inventory.Stock, SerialNumberStock>()
            .ForMember(dest => dest.LocationId, opt => opt.MapFrom(src => src.StoredAt));
      }
   }
}