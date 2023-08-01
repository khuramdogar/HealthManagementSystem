using AutoMapper;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Infrastructure;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.Stock
{
   public class ReceiveOrderViewModel : IHaveCustomMappings
   {
      public int InventoryOrderId { get; set; }
      public string Name { get; set; }
      public string Location { get; set; }
      public string DeliveryLocationId { get; set; }
      public string Status { get; set; }


      public void CreateMappings(IConfiguration configuration)
      {
         configuration.CreateMap<Order, ReceiveOrderViewModel>()
            .ForMember(m => m.Location,
               opt => opt.MapFrom(src => src.DeliveryLocation != null ? src.DeliveryLocation.Name : string.Empty));
      }
   }
}