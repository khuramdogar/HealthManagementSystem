using AutoMapper;
using HMS.HealthTrack.Web.Infrastructure;
using System;
using System.ComponentModel.DataAnnotations;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.Consumptions
{
   public class StockConsumptionByDoctor : IHaveCustomMappings
   {
      public int ConsumptionId { get; set; }

      [Display(Name = "Product")]
      public string ProductName { get; set; }

      [Display(Name = "PID", Description = "Product ID")]
      public string ProductId { get; set; }

      [Display(Name = "SPC", Description = "Supplier Product Code")]
      public string SupplierProductCode { get; set; }

      [Display(Name = "Rebate code", Description = "Rebate code")]
      public string RebateCode { get; set; }

      [Display(Name = "Min benefit", Description = "Minimum benefit")]
      public decimal? MinBenefit { get; set; }

      [Display(Name = "Patient ID")]
      public string PatientId { get; set; }

      [Display(Name = "First name")]
      public string PatientFirstName { get; set; }

      [Display(Name = "Last name")]
      public string PatientLastName { get; set; }

      [Display(Name = "Date")]
      public DateTime? ExamDate { get; set; }

      [Display(Name = "Vendor")]
      public string Vendor { get; set; }
      
      [Display(Name = "Doctor")]
      public string DoctorName { get; set; }

      [Display(Name = "Patient name")]
      public string PatientSurnameFirst => $"{PatientLastName}, {PatientFirstName}".ToUpper();

      public void CreateMappings(IConfiguration configuration)
      {
         configuration.CreateMap<ClinicalConsumption, StockConsumptionByDoctor>()
            .ForMember(m => m.ConsumptionId, opt => opt.MapFrom(source => source.UsedId))
            .ForMember(m => m.ProductName, opt => opt.MapFrom(source => source.Name))
            .ForMember(m => m.ProductId, opt => opt.MapFrom(source => source.ProductId))
         .ForMember(m => m.SupplierProductCode, opt => opt.MapFrom(source => source.SPC))
         .ForMember(m => m.RebateCode, opt => opt.MapFrom(source => source.RebateCode))
         .ForMember(m => m.MinBenefit, opt => opt.MapFrom(source => source.MinBenefit))
         .ForMember(m => m.Vendor, opt => opt.MapFrom(source => source.Manufacturer))
         .ForMember(m => m.DoctorName, opt => opt.MapFrom(source => source.StaffName))
         .ForMember(m => m.ExamDate, opt => opt.MapFrom(source => source.testDate))
         .ForMember(m => m.PatientId, opt => opt.MapFrom(source => string.IsNullOrWhiteSpace(source.RemotePatient_ID) ? "HT" + source.patient_ID : source.RemotePatient_ID))
         .ForMember(m => m.PatientFirstName, opt => opt.MapFrom(source => source.firstName))
         .ForMember(m => m.PatientLastName, opt => opt.MapFrom(source => source.surname))
         ;
      }
   }
}