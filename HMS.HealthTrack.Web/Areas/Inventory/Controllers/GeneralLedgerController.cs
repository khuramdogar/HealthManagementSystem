using AutoMapper;
using HMS.HealthTrack.Web.Areas.Inventory.Models.GeneralLedger;
using HMS.HealthTrack.Web.Areas.Inventory.Utils;
using Kendo.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Inventory.Common.Constants;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork;
using GeneralLedgerType = HMS.HealthTrack.Web.Areas.Inventory.Models.GeneralLedger.GeneralLedgerType;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers
{
   [HealthTrackAuthorize(HealthTrackPermission = ("Inventory"))]
   public class GeneralLedgerController : Controller
   {
      private readonly IGeneralLedgerUnitOfWork _unitOfWork;
      private readonly ICustomLogger _logger;
      private readonly IPropertyProvider _propertyProvider;

      public GeneralLedgerController(IGeneralLedgerUnitOfWork unitOfWork, ICustomLogger logger,
         IPropertyProvider propertyProvider)
      {
         _unitOfWork = unitOfWork;
         _logger = logger;
         _propertyProvider = propertyProvider;
      }

      // GET: Inventory/GeneralLedgerController
      public ActionResult Index()
      {
         var ledgerTypes = _propertyProvider.LedgerTypes.Select(Mapper.Map<Web.Data.Model.Inventory.GeneralLedgerType, GeneralLedgerType>).ToList();
         return View(ledgerTypes);
      }

      // IMPORTANT: Automapper is not used to create the GeneralLedgerModel. See AddNode() for model population.
      public JsonResult Get([DataSourceRequest] DataSourceRequest request, int ledgerType)
      {
         var ledgers = new List<GeneralLedgerModel>();

         var generalLedgers = _unitOfWork.GeneralLedgerRepository.GetGeneralLedgers(ledgerType);

         var roots = _unitOfWork.GeneralLedgerRepository.FindRootIds();
         var rootLedgers = generalLedgers.Where(gl => roots.Contains(gl.LedgerId));

         foreach (GeneralLedger root in rootLedgers)
         {
            AddNode(root, null, ledgers);
         }

         var ledgerName = Nameof<GeneralLedgerModel>.Property(p => p.Name);
         var filters = request.Filters;
         foreach (var filterDescriptor in filters)
         {
            var filter = (FilterDescriptor)filterDescriptor;
            if (filter == null) continue;

            if (filter.Member.IsCaseInsensitiveEqual(ledgerName))
            {
               var filteredLedgerIds = _unitOfWork.GeneralLedgerRepository.Search(ledgerType, filter.Value.ToString());
               ledgers = ledgers.Where(l => filteredLedgerIds.Contains(l.LedgerId.Value)).ToList();
            }
         }

         request.Filters.Clear();
         var result = ledgers.ToTreeDataSourceResult(request, e => e.LedgerId, e => e.ParentLedger);
         return Json(result, JsonRequestBehavior.AllowGet);
      }

      private void AddNode(GeneralLedger ledger, int? parentId, List<GeneralLedgerModel> ledgerModels)
      {
         if (ledger.DeletedOn.HasValue) return;
         var hasChildren =
            ledger.GeneralLedgerChildren.Any(c => c.ParentId == ledger.LedgerId && c.Depth > 0);

         ledgerModels.Add(new GeneralLedgerModel
         {
            AlternateCode = ledger.AlternateCode,
            Code = ledger.Code,
            HasChildren = hasChildren,
            Name = ledger.Name,
            LedgerId = ledger.LedgerId,
            LedgerType = ledger.GeneralLedgerTier.LedgerType,
            ParentId = parentId,
            ParentLedger = ledgerModels.Select(l => l.LedgerId).Contains(parentId) ? parentId : null,
            Tier = ledger.GeneralLedgerTier.Name
         });

         foreach (var child in ledger.GeneralLedgerChildren.Where(c => c.Depth == 1).Select(c => c.GeneralLedgerChild)) // direct descendants
         {
            AddNode(child, ledger.LedgerId, ledgerModels);
         }
      }

      public ActionResult Create([DataSourceRequest] DataSourceRequest request, GeneralLedgerModel model)
      {
         if (!ModelState.IsValid)
         {
            _logger.Information("Unable to create general ledger {LedgerName} due to invalid values", model.Name);
            return Json(new[] { model }.ToTreeDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
         }

         if (model.ParentId.HasValue && !_unitOfWork.CanHaveChildren(model.ParentId.Value, model.LedgerType))
         {
            _logger.Information("Cannot create subledger for ledger {LedgerId} as it would not correspond to a tier",
               model.ParentId);
            return Json(new[] { model }.ToTreeDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet); //TODO: Add custom error
         }

         var ledger = Mapper.Map<GeneralLedgerModel, GeneralLedger>(model);
         _unitOfWork.CreateLedger(ledger, model.ParentLedger, User.Identity.Name, model.LedgerType);
         _unitOfWork.Commit();

         var createdLedger = Mapper.Map<GeneralLedger, GeneralLedgerModel>(ledger);

         return Json(new[] { createdLedger }.ToTreeDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
      }

      public ActionResult Update([DataSourceRequest] DataSourceRequest request, GeneralLedgerModel model)
      {
         if (!ModelState.IsValid)
         {
            _logger.Information("Unable to update general ledger {LedgerId} due to invalid values", model.LedgerId);
            return Json(new[] { model }.ToTreeDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
         }

         var ledger = Mapper.Map<GeneralLedgerModel, GeneralLedger>(model);
         try
         {
            var result = _unitOfWork.UpdateLedger(ledger, model.ParentLedger, User.Identity.Name, model.LedgerType);
            if (!result)
            {
               _logger.Information("Invalid update for general ledger {LedgerId}", model.LedgerId);
               return Json(new[] { model }.ToTreeDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
            }
         }
         catch (Exception exception)
         {
            _logger.Error(exception, "Unable to update general ledger {LedgerId}", ledger.LedgerId);
            return Json(new[] { model }.ToTreeDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
         }

         _unitOfWork.Commit();

         var createdLedger = Mapper.Map<GeneralLedger, GeneralLedgerModel>(ledger);

         return Json(new[] { createdLedger }.ToTreeDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
      }

      public JsonResult Destroy([DataSourceRequest] DataSourceRequest request, GeneralLedgerModel model)
      {
         if (!ModelState.IsValid || !model.LedgerId.HasValue)
         {
            _logger.Information("Unable to delete ledger {LedgerId} due to invalid values", model.LedgerId);
            return Json(new[] { model }.ToTreeDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
         }
         var generalLedger = _unitOfWork.GeneralLedgerRepository.Find(model.LedgerId.Value);
         if (generalLedger == null)
         {
            _logger.Information("Unable to delete ledger {LedgerId}, does not exist", model.LedgerId);
            return Json(new[] { model }.ToTreeDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
         }

         _unitOfWork.GeneralLedgerRepository.Remove(model.LedgerId.Value, User.Identity.Name);
         _unitOfWork.Commit();
         var deletedLedger =
            Mapper.Map<GeneralLedger, GeneralLedgerModel>(_unitOfWork.GeneralLedgerRepository.Find(model.LedgerId.Value));

         return Json(new[] { deletedLedger }.ToTreeDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
      }

      public JsonResult GetParentLedgers(int? id, int ledgerType)
      {
         var ledgers =
            _unitOfWork.FindParents(id, ledgerType)
               .Where(gl => gl.LedgerId != id)
               .Select(gl => new { Value = gl.LedgerId, Text = gl.Name }).OrderBy(gl => gl.Text)
               .ToList();
         return Json(ledgers, JsonRequestBehavior.AllowGet);
      }

      // IMPORTANT: Automapper is not used to create the GeneralLedgerModel. See AddNode() for model population.
      public JsonResult GetWithLedgerCode([DataSourceRequest] DataSourceRequest request, int ledgerType)
      {
         var ledgers = new List<GeneralLedgerModel>();

         var generalLedgers = _unitOfWork.GeneralLedgerRepository.GetGeneralLedgers(ledgerType);

         var roots = _unitOfWork.GeneralLedgerRepository.FindRootIds();
         var rootLedgers = generalLedgers.Where(gl => roots.Contains(gl.LedgerId));

         var segmentCount = _unitOfWork.GeneralLedgerTierRepository.GetMaxLevel(ledgerType);
         var delmiter = _propertyProvider.GlcSectionDelimiter;

         foreach (GeneralLedger root in rootLedgers)
         {
            AddNodeWithConstructedCode(ledgerType, root, root.LedgerId, ledgers, segmentCount, delmiter, root.LedgerId);
         }

         var ledgerName = Nameof<GeneralLedgerModel>.Property(p => p.Name);
         var filters = request.Filters;
         foreach (var filterDescriptor in filters)
         {
            var filter = (FilterDescriptor)filterDescriptor;
            if (filter == null) continue;

            if (filter.Member.IsCaseInsensitiveEqual(ledgerName))
            {
               var filteredLedgerIds = _unitOfWork.GeneralLedgerRepository.Search(ledgerType, filter.Value.ToString());
               ledgers = ledgers.Where(l => filteredLedgerIds.Contains(l.LedgerId.Value)).ToList();
            }
         }

         request.Filters.Clear();
         var result = ledgers.ToTreeDataSourceResult(request, e => e.LedgerId, e => e.ParentLedger);
         return Json(result, JsonRequestBehavior.AllowGet);
      }

      private void AddNodeWithConstructedCode(int ledgerType, GeneralLedger ledger, int? parentId, List<GeneralLedgerModel> ledgerModels, int segmentCount, string delimiter, int rootId)
      {
         if (ledger.DeletedOn.HasValue) return;
         var ledgerCode = GeneralLedgerHelper.ConstructLedgerCode(_unitOfWork.GeneralLedgerRepository, _unitOfWork.GeneralLedgerTierRepository, ledgerType, segmentCount, ledger.LedgerId, delimiter, rootId);
         var hasChildren = ledger.GeneralLedgerChildren.Any(c => c.ParentId == ledger.LedgerId && c.Depth > 0);

         ledgerModels.Add(new GeneralLedgerModel
         {
            AlternateCode = ledger.AlternateCode,
            Code = ledgerCode,
            HasChildren = hasChildren,
            Name = ledger.Name,
            LedgerId = ledger.LedgerId,
            LedgerType = ledger.GeneralLedgerTier.LedgerType,
            ParentId = parentId,
            ParentLedger = ledgerModels.Select(l => l.LedgerId).Contains(parentId) ? parentId : null,
            Tier = ledger.GeneralLedgerTier.Name,
         });

         foreach (var child in ledger.GeneralLedgerChildren.Where(c => c.Depth == 1).Select(c => c.GeneralLedgerChild)) // direct descendants
         {
            AddNodeWithConstructedCode(ledgerType, child, ledger.LedgerId, ledgerModels, segmentCount, delimiter, rootId);
         }
      }

      public ActionResult LedgerSelector(int ledgerType)
      {
         return PartialView("_LedgerSelector", ledgerType);
      }

      public JsonResult SearchProductLedgers(int tier, int? parentId)
      {
         var productLedgerType = _propertyProvider.LedgerTypes.Single(lt => lt.Name == InventoryConstants.ProductLedgerType).LedgerTypeId;
         var ledgers = _unitOfWork.GeneralLedgerRepository.GetLedgers(productLedgerType, tier, parentId);
         var selectList = ledgers.ToList().Select(l => new SelectListItem
         {
            Text = string.Format("{0} ({1})", l.Name, l.Code),
            Value = l.LedgerId.ToString()
         }).OrderBy(l => l.Text);
         return Json(selectList, JsonRequestBehavior.AllowGet);
      }

   }
}