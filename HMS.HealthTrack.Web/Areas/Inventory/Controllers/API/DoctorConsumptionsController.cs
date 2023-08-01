using AutoMapper;
using HMS.HealthTrack.Web.Areas.Inventory.Models.Consumptions;
using HMS.HealthTrack.Web.Areas.Inventory.Models.Orders;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers.API
{
   [HealthTrackAuthorize(HealthTrackPermission = ("Inventory"))]
   public class DoctorConsumptionsController : ApiController
   {
      private readonly IHealthTrackConsumptionRepository _healthTrackConsumptionRepository;
      private readonly ICustomLogger _logger;

      public DoctorConsumptionsController(IHealthTrackConsumptionRepository healthTrackConsumptionRepository, ICustomLogger logger)
      {
         _healthTrackConsumptionRepository = healthTrackConsumptionRepository;
         _logger = logger;
      }

      // GET api/StockDeductions
      [HttpGet]
      public DataSourceResult DoctorConsumptions([ModelBinder(typeof(WebApiDataSourceRequestModelBinder))] DataSourceRequest request)
      {
         try
         {
            var reportFilter = new ReportFilter(request.Filters);
            var medicalConsumptions = _healthTrackConsumptionRepository.FindClinicalConsumptions();
            
            if(reportFilter.FromDate.HasValue) medicalConsumptions = medicalConsumptions.Where(c=>c.testDate >= reportFilter.FromDate.Value);
            if (reportFilter.ToDate.HasValue) medicalConsumptions = medicalConsumptions.Where(c => c.testDate <= reportFilter.ToDate.Value);
            if (!string.IsNullOrWhiteSpace(reportFilter.Surname)) medicalConsumptions = medicalConsumptions.Where(c => c.surname.Contains(reportFilter.Surname));
            if (!string.IsNullOrWhiteSpace(reportFilter.Firstname)) medicalConsumptions = medicalConsumptions.Where(c => c.firstName.Contains(reportFilter.Firstname));
            int patientId;
            if (!string.IsNullOrWhiteSpace(reportFilter.PatientId) && int.TryParse(reportFilter.PatientId,out patientId)) medicalConsumptions = medicalConsumptions.Where(c => c.patient_ID == patientId);

            request.Filters.Clear();

            var consumption = medicalConsumptions.ToList().Select(c =>
               new StockConsumptionByDoctor
               {
                  ConsumptionId = c.UsedId,
                  ProductName = c.Name,
                  ProductId = c.ProductId.ToString(),
                  SupplierProductCode = c.SPC,
                  RebateCode = c.RebateCode,
                  MinBenefit = c.MinBenefit,
                  Vendor = c.Manufacturer,
                  DoctorName = c.StaffName,
                  ExamDate = c.testDate,
                  PatientId = c.patient_ID.ToString(),
                  PatientFirstName = c.firstName,
                  PatientLastName = c.surname
               });
            return consumption.ToDataSourceResult(request);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, "There was a problem retrieving Doctor Consumptions");
            return new DataSourceResult();
         }
      }
   }
}
