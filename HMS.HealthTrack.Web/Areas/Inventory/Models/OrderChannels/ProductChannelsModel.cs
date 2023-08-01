using AutoMapper;
using HMS.HealthTrack.Web.Infrastructure;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.OrderChannels
{
   public class ProductChannelsModel : IHaveCustomMappings
   {
      public int ProductId { get; set; }
      public int OrderChannelProductId { get; set; }
      public int OrderChannelId { get; set; }
      public string Reference { get; set; }
      public bool AutomaticOrder { get; set; }
      
      public OrderChannelModel Channel { get; set; }

      public void CreateMappings(IConfiguration configuration)
      {
         configuration.CreateMap<Web.Data.Model.Inventory.OrderChannelProduct, ProductChannelsModel>()
            .ForMember(m => m.Channel, opt => opt.MapFrom(src => new OrderChannelModel {OrderChannelId = src.OrderChannelId, Name = src.OrderChannel?.Name}));
      }
   }
}