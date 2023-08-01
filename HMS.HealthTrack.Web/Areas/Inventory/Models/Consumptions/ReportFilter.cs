using System;
using System.Collections.Generic;
using System.Linq;
using Kendo.Mvc;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.Consumptions
{
   public class ReportFilter
   {
      public string PatientId { get; set; }
      public DateTime? FromDate { get; set; }
      public DateTime? ToDate { get; set; }

      public ReportFilter(ICollection<IFilterDescriptor> filters)
      {
         ParseFilters(filters,this);
      }

      internal static void ParseFilters(ICollection<IFilterDescriptor> filters, ReportFilter reportFilter)
      {
         if (!filters.Any())
            return;

         foreach (var filter in filters)
         {
            var descriptor = filter as FilterDescriptor;
            if (descriptor != null)
            {
               SetSearchParam(descriptor, reportFilter);
            }
            else if (filter is CompositeFilterDescriptor)
            {
               ParseFilters(((CompositeFilterDescriptor)filter).FilterDescriptors, reportFilter);
            }
         }
         return;
      }

      internal static void SetSearchParam(FilterDescriptor filterDescriptor, ReportFilter reportFilter)
      {
         switch (filterDescriptor.Member)
         {
            case "FromDate":
               reportFilter.FromDate = DateTime.Parse(filterDescriptor.Value.ToString());
               break;
            case "ToDate":
               reportFilter.ToDate = DateTime.Parse(filterDescriptor.Value.ToString());
               break;
            case "PatientId":
               reportFilter.PatientId = filterDescriptor.Value.ToString();
               break;
            case "Surname":
               reportFilter.Surname = filterDescriptor.Value.ToString();
               break;
            case "Firstname":
               reportFilter.Firstname = filterDescriptor.Value.ToString();
               break;
         }
      }

      public string Firstname { get; set; }

      public string Surname { get; set; }
   }


   
}