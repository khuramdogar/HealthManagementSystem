using AutoMapper;
using HMS.HealthTrack.Web.Infrastructure;
using HMS.HealthTrack.Web.Utils;
using System;
using HMS.HealthTrack.Web.Data.Model;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.Consumptions
{
   [ModelMetaType(typeof(HealthTrackConsumptionMeta))]
   public class IndexConsumptionsViewModel : IHaveCustomMappings
   {

      public int ConsumptionId { get; set; }
      public int ProductId { get; set; }
      public string SPC { get; set; }
      public string LPC { get; set; }
      public string ScanCode { get; set; }
      public string Name { get; set; }
      public string SerialNumber { get; set; }
      public short? Quantity { get; set; }
      public string Location { get; set; }
      public string PatientMRN { get; set; }
      public string PatientFirstName { get; set; }
      public string PatientSurname { get; set; }
      public long? ContainerId { get; set; }
      public int? PatientId { get; set; }
      public string Manufacturer { get; set; }
      public string GL { get; set; }
      public string RebateCode { get; set; }
      public string Description { get; set; }
      public decimal? Price { get; set; }
      public string BuyCurrency { get; set; }
      public double? BuyCurrencyRate { get; set; }
      public bool? deleted { get; set; }
      public bool Reported { get; set; }
      public DateTime? deletionDate { get; set; }
      public string deletionUser { get; set; }
      public DateTime? dateLastModified { get; set; }
      public string userLastModified { get; set; }
      public DateTime? dateCreated { get; set; }
      public string userCreated { get; set; }
      public DateTime? ConsumedOn { get; set; }
      public ConsumptionProcessingStatus ProcessingStatus { get; set; }
      public int? InventoryProductId { get; set; }

      public string Status
      {
         get { return ProcessingStatus.ToString(); }
      }

      public string ProcessingStatusMessage { get; set; }

      public void CreateMappings(IConfiguration configuration)
      {
         configuration.CreateMap<HealthTrackConsumption, IndexConsumptionsViewModel>()
            .ForMember(m => m.SPC, opt => opt.MapFrom(source => string.IsNullOrWhiteSpace(source.SPC) ? "SPC missing" : source.SPC))
            .ForMember(m => m.ConsumptionId, opt => opt.MapFrom(source => source.UsedId))
            .ForMember(m => m.Location, opt => opt.MapFrom(source => source.LocationName))
            .ForMember(m => m.Reported, opt => opt.MapFrom(source => source.Reported.GetValueOrDefault()));
      }
   }


}