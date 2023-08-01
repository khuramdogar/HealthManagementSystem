using AutoMapper;
using HMS.HealthTrack.Web.Infrastructure;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.StockLocations
{
   public class LocationStockModel : IHaveCustomMappings
   {
      public int StockId { get; set; }
      public int ProductId { get; set; }
      public string Description { get; set; }
      public string Supplier { get; set; }
      public string BatchNumber { get; set; }
      public string SerialNumber { get; set; }
      public int Quantity { get; set; }
      public void CreateMappings(IConfiguration configuration)
      {
         configuration.CreateMap<Data.Model.Inventory.Stock, LocationStockModel>()
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(s => s.ProductId))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(s => s.Product.Description))
            .ForMember(dest => dest.Supplier, opt => opt.MapFrom(s => s.Product.PrimarySupplierCompany != null ? s.Product.PrimarySupplierCompany.companyName : string.Empty));
      }
   }
}