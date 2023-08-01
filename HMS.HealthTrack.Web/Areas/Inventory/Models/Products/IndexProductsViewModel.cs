using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Infrastructure;
using HMS.HealthTrack.Web.Utils;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.Products
{
   [ModelMetaType(typeof(ProductMeta))]
   public class IndexProductsViewModel : IHaveCustomMappings
   {
      public int ProductId { get; set; }
      public string LPC { get; set; }
      public string UPN
      {
         get
         {
            return UPNs == null || !UPNs.Any() ? string.Empty : UPNs.Aggregate((current, next) => current + "," + next).TrimEnd(',');
         }
      }
      public List<string> UPNs { get; set; }
      public string SPC { get; set; }
      public string Description { get; set; }
      public string GLC { get; set; }
      public string Manufacturer { get; set; }
      public string RebateCode { get; set; }
      public decimal UnitPrice { get; set; }
      public int? PrimarySupplier { get; set; }
      public string Supplier { get; set; }
      public bool Unorderable { get; set; }
      public string Status { get; set; }
      public string Categories
      {
         get { return string.Empty; }
      }

      public bool InStock { get; set; }
      public bool Unclassified { get; set; }
      public bool HasHadStockTake { set; get; }
      public bool IsConsignment { get; set; }
      public bool PendingConsumedProducts { get; set; }
      public bool InError { get; set; }
      public bool ManageStock { get; set; }
      public bool IncludeDeleted { get; set; }

      public void CreateMappings(IConfiguration configuration)
      {
         configuration.CreateMap<Product, IndexProductsViewModel>()
            .ForMember(dest => dest.Supplier,
               opt =>
                  opt.MapFrom(src => src.PrimarySupplier != null ? src.PrimarySupplierCompany.companyName : string.Empty))
            .ForMember(dest => dest.PendingConsumedProducts, opt => opt.MapFrom(src => src.ProductStatus == ProductStatus.Pending))
            .ForMember(result => result.UPNs, options => options.MapFrom(p => p.ScanCodes.Select(sc => sc.Value).ToList()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)src.ProductStatus));
      }
   }
}