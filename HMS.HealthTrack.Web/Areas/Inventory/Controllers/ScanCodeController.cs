using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;
using HMS.HealthTrack.Web.Areas.Inventory.Models.ScanCode;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers
{
   public class ScanCodeController : Controller
   {
      private readonly IScanCodeRepository _codeRepository;

      public ScanCodeController(IScanCodeRepository codeRepository)
      {
         _codeRepository = codeRepository;
      }

      [HttpGet]
      public ActionResult ScanCodeWindow()
      {
         return PartialView("_ScanCodeWindow");
      }

      public ActionResult AddOrUpdateScanCodeForProduct([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<ScanCodeModel> scanCodeModels)
      {
         var newCodes = new List<ScanCode>();

         if (scanCodeModels != null)
         {
            foreach (var scanCode in scanCodeModels)
            {
               //Add new if it's a newly scanned item
               if (scanCode.ScanCodeId == 0)
               {
                  var createdScanCode = _codeRepository.Create(scanCode.ProductId, scanCode.Value);
                  newCodes.Add(createdScanCode);
               }
               else //Update existing
               {
                  _codeRepository.UpdateCode(scanCode.ScanCodeId, scanCode.Value);
               }
            }
         }

         _codeRepository.Commit();

         return Json(newCodes.Select(Mapper.Map<ScanCode, ScanCodeModel>).ToDataSourceResult(request, ModelState));
      }

      [HttpPost]
      public JsonResult GetScanCodesForProduct([DataSourceRequest] DataSourceRequest request,int productId)
      {
         var codes = from sc in _codeRepository.FindAll() where sc.ProductId == productId select sc;
         var responseData = codes.ToList().Select(Mapper.Map<ScanCode, ScanCodeModel>);
         return Json(responseData.ToDataSourceResult(request));
      }

      [AcceptVerbs(HttpVerbs.Post)]
      public ActionResult RemoveScanCodeFromProduct([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<ScanCodeModel> scanCodeModels)
      {
         var scanCodes = scanCodeModels as IList<ScanCodeModel> ?? scanCodeModels.ToList();
         if (scanCodes.Any())
         {
            foreach (var scanCode in scanCodes)
            {
               _codeRepository.Remove(scanCode.ScanCodeId);
            }
         }

         _codeRepository.Commit();

         return Json(scanCodes.ToDataSourceResult(request, ModelState));
      }
   }
}