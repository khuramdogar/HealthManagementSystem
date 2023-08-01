using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection.Emit;
using AutoMapper;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Infrastructure;
using HMS.HealthTrack.Web.Utils;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.Suppliers
{
   [ModelMetaType(typeof(SupplierModelMeta))]
   public class SuppliersViewModel : IHaveCustomMappings, IMapFrom<SupplierModel>, IValidatableObject
   {
      public int company_ID { get; set; }
      public DateTime CreatedOn { get; set; }
      public string CreatedBy { get; set; }
      public DateTime? LastModifiedOn { get; set; }
      public string LastModifiedBy { get; set; }
      [Required]
      public string Name { get; set; }
      public string Address1 { get; set; }
      public string Address2 { get; set; }
      public string Suburb { get; set; }
      public string State { get; set; }
      public string PostCode { get; set; }
      public string Country { get; set; }
      public string Department { get; set; }
      public string PhoneNumber { get; set; }
      public string FaxNumber { get; set; }
      public string Email { get; set; }
      public string WebSite { get; set; }
      public string ContactTitle { get; set; }
      public string ContactFirstname { get; set; }
      public string ContactSurname { get; set; }

      public void CreateMappings(IConfiguration configuration)
      {
         configuration.CreateMap<Supplier, SuppliersViewModel>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Company.companyName))
            .ForMember(dest => dest.Address1,
               opt =>
                  opt.MapFrom(
                     src =>
                        src.Company.AddressCorporates.FirstOrDefault() != null
                           ? src.Company.AddressCorporates.First().address1
                           : string.Empty))
            .ForMember(dest => dest.Address2,
               opt =>
                  opt.MapFrom(
                     src =>
                        src.Company.AddressCorporates.FirstOrDefault() != null
                           ? src.Company.AddressCorporates.First().address2
                           : string.Empty))
            .ForMember(dest => dest.Suburb,
               opt =>
                  opt.MapFrom(
                     src =>
                        src.Company.AddressCorporates.FirstOrDefault() != null
                           ? src.Company.AddressCorporates.First().suburb
                           : string.Empty))
            .ForMember(dest => dest.State,
               opt =>
                  opt.MapFrom(
                     src =>
                        src.Company.AddressCorporates.FirstOrDefault() != null
                           ? src.Company.AddressCorporates.First().state
                           : string.Empty))
            .ForMember(dest => dest.PostCode,
               opt =>
                  opt.MapFrom(
                     src =>
                        src.Company.AddressCorporates.FirstOrDefault() != null
                           ? src.Company.AddressCorporates.First().postcode
                           : string.Empty))
            .ForMember(dest => dest.Country,
               opt =>
                  opt.MapFrom(
                     src =>
                        src.Company.AddressCorporates.FirstOrDefault() != null
                           ? src.Company.AddressCorporates.First().country
                           : string.Empty))
            .ForMember(dest => dest.Department,
               opt =>
                  opt.MapFrom(
                     src =>
                        src.Company.AddressCorporates.FirstOrDefault() != null
                           ? src.Company.AddressCorporates.First().department
                           : string.Empty))
            .ForMember(dest => dest.PhoneNumber,
               opt =>
                  opt.MapFrom(
                     src =>
                        src.Company.AddressCorporates.FirstOrDefault() != null
                           ? src.Company.AddressCorporates.First().phoneNumber
                           : string.Empty))
            .ForMember(dest => dest.FaxNumber,
               opt =>
                  opt.MapFrom(
                     src =>
                        src.Company.AddressCorporates.FirstOrDefault() != null
                           ? src.Company.AddressCorporates.First().faxNumber
                           : string.Empty))
            .ForMember(dest => dest.Email,
               opt =>
                  opt.MapFrom(
                     src =>
                        src.Company.AddressCorporates.FirstOrDefault() != null
                           ? src.Company.AddressCorporates.First().email
                           : string.Empty))
            .ForMember(dest => dest.WebSite,
               opt =>
                  opt.MapFrom(
                     src =>
                        src.Company.AddressCorporates.FirstOrDefault() != null
                           ? src.Company.AddressCorporates.First().webSite
                           : string.Empty))
            .ForMember(dest => dest.ContactTitle,
               opt =>
                  opt.MapFrom(
                     src =>
                        src.Company.AddressCorporates.FirstOrDefault() != null
                           ? src.Company.AddressCorporates.First().contactTitle
                           : string.Empty))
            .ForMember(dest => dest.ContactFirstname,
               opt =>
                  opt.MapFrom(
                     src =>
                        src.Company.AddressCorporates.FirstOrDefault() != null
                           ? src.Company.AddressCorporates.First().contactFirstname
                           : string.Empty))
            .ForMember(dest => dest.ContactSurname,
               opt =>
                  opt.MapFrom(
                     src =>
                        src.Company.AddressCorporates.FirstOrDefault() != null
                           ? src.Company.AddressCorporates.First().contactSurname
                           : string.Empty));
      }

      public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
      {
         if (string.IsNullOrWhiteSpace(Name))
         {
            yield return new ValidationResult("Please enter a name for this supplier.", new[] { "Name" });
         }
      }
   }
}