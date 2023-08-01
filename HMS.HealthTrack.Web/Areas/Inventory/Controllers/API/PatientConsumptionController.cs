using HMS.HealthTrack.Web.Areas.Inventory.Models.StockAdjustment;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers.API
{
   [HealthTrackAuthorize(HealthTrackPermission = ("Inventory"))]
   public class PatientConsumptionController : ApiController
   {
      private readonly IStockAdjustmentRepository _adjustmentRepository;
      private readonly ICustomLogger _logger;

      public PatientConsumptionController(IStockAdjustmentRepository adjustmentRepository, ICustomLogger logger)
      {
         _adjustmentRepository = adjustmentRepository;
         _logger = logger;
      }
      
      [HttpGet]
      public DataSourceResult PatientBookingsMissingPaymentClass([ModelBinder(typeof(WebApiDataSourceRequestModelBinder))] DataSourceRequest request, string report)
      {
         try
         {
            if (report != "MissingPaymentClass")
               throw new NotSupportedException($"Report '{report}' is not supported");

            var bookingsMissingPaymentClass = from c in _adjustmentRepository.FindDeductionsRequiringPaymentClass()
               where !c.PaymentClass.HasValue
               select new PatientBookingConsumptionDetails
               {
                  AdjustmentId = c.StockAdjustmentId,
                  AppointmentDate = c.dateTimeStart,
                  FirstName = c.firstName,
                  Surname = c.surname,
                  PatientNumber = c.patient_ID.ToString(),
               };
            return bookingsMissingPaymentClass.ToDataSourceResult(request);

         }
         catch (Exception exception)
         {
            _logger.Error(exception, $"There was a problem retrieving Patient Booking Consumption Details where the report is '{report}'.");
            return new DataSourceResult();
         }
      }

   }
}
