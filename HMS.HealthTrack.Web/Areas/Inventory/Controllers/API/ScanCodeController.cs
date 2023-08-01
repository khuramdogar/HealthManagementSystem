using HMS.HealthTrack.Web.Areas.Inventory.Models.ScanCode;
using HMS.HealthTrack.Web.Areas.Inventory.Utils;
using System;
using System.Web.Http;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using ScanCode = HMS.HealthTrack.Web.Areas.Inventory.Models.Products.ScanCode;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers.API
{
   [HealthTrackAuthorize(HealthTrackPermission = ("Inventory"))]
   public class ScanCodeController : ApiController
   {
      private readonly ICustomLogger _logger;

      public ScanCodeController(ICustomLogger logger)
      {
         _logger = logger;
      }

      [HttpPost, Route("api/Inventory/ScanCode/GetGs1CodeData")]
      public IHttpActionResult GetGs1CodeData(ScanCode input)
      {
         _logger.Debug("Attempting to parse the code {Code} using the GS1 parser", input.ScannedCode);

         try
         {
            var codeResult = ScanCodeHelper.TryParseGs1(input.ScannedCode, _logger);
            return codeResult != null ? (IHttpActionResult)Ok(codeResult) : BadRequest("Unable to extract any codes");
         }
         catch (Exception exception)
         {
            _logger.Error(exception, "Unable to parse HIBC code {Code}", input.ScannedCode);
            return InternalServerError();
         }
      }

      [HttpPost, Route("api/Inventory/ScanCode/GetHibcCodeData")]
      public IHttpActionResult GetHibcCodeData(ScanCode input)
      {
         _logger.Debug("Attempting to parse the code {Code} using the HIBC parser", input.ScannedCode);
         try
         {
            var codeResult = ScanCodeHelper.TryParseHibc(input.ScannedCode, _logger);
            return codeResult != null ? (IHttpActionResult)Ok(codeResult) : BadRequest("Unable to extract any codes");
         }
         catch (Exception exception)
         {
            _logger.Error(exception, "Unable to parse HIBC code {Code}", input.ScannedCode);
            return InternalServerError();
         }
      }

      [HttpPost, Route("api/Inventory/ScanCode/ExtractCodeValue")]
      public IHttpActionResult ExtractCodeValue(ExtractScanCodeModel model)
      {
         _logger.Debug("Attempting to extract the {ScanCodeValue} from the code {Code}", model.ScanCodeValue, model.Code);

         try
         {

            ScanCodeResult codeResult;

            try
            {
               codeResult = ScanCodeHelper.TryParseHibc(model.Code, _logger);
            }
            catch
            {
               throw;
            }

            if (codeResult == null)
            {
               try
               {
                  codeResult = ScanCodeHelper.TryParseGs1(model.Code, _logger);
               }
               catch
               {
                  throw;
               }
            }

            switch (model.ScanCodeValue)
            {
               case ScanCodeValue.Spc:
                  if (string.IsNullOrWhiteSpace(codeResult.SPC))
                  {
                     _logger.Debug("Unable to find SPC within the barcode {Code}", model.Code);
                     return BadRequest("No SPC exists within the barcode.");
                  }

                  return Ok(codeResult.SPC);

               case ScanCodeValue.Upc:
                  if (string.IsNullOrWhiteSpace(codeResult.UPN))
                  {
                     _logger.Debug("Unable to find UPC within the barcode {Code}", model.Code);
                     return BadRequest("No UPN exists within the barcode.");
                  }
                  return Ok(codeResult.UPN);

               default:
                  _logger.Error("Unable to extract unknown value {ScanCodeValue}", model.ScanCodeValue);
                  return BadRequest("Unable to extract unknown value from barcode.");
            }
         }
         catch (Exception exception)
         {
            _logger.Error(exception, "An error occurred while attempting to extract the {ScanCodeValue} from the code {Code}", model.ScanCodeValue, model.Code);
            return InternalServerError();
         }
      }
   }
}
