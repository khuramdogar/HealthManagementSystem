using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers
{
   [HealthTrackAuthorize(HealthTrackPermission = ("Inventory"))]
   public class StockSetsController : Controller
   {
      private readonly IStockSetRepository _stockSetRepository;
      private readonly ICustomLogger _logger;

      public StockSetsController(IStockSetRepository stockSetRepository, ICustomLogger logger)
      {
         _stockSetRepository = stockSetRepository;
         _logger = logger;
      }

      // GET: Inventory/StockSets
      public ActionResult Index()
      {
         return View();
      }

      // GET: Inventory/StockSets/Details/5
      public ActionResult Details(int? id)
      {
         try
         {
            if (!id.HasValue)
               return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var stockSet = _stockSetRepository.Find(id.Value);
            if (stockSet == null)
            {
               return HttpNotFound();
            }
            return View(stockSet);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem retrieving the Stock Set with ID '{0}'.", id.HasValue ? id.Value.ToString() : string.Empty));
            return View("Error", new HandleErrorInfo(exception, "StockSets", "Details"));
         }
      }

      // GET: Inventory/StockSets/Create
      public ActionResult Create()
      {
         return View();
      }

      // POST: Inventory/StockSets/Create
      // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
      // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
      [HttpPost]
      [ValidateAntiForgeryToken]
      public ActionResult Create([Bind(Include = "StockSetId,Name")] StockSet stockSet)
      {
         try
         {
            if (ModelState.IsValid)
            {
               _stockSetRepository.Add(stockSet);
               _stockSetRepository.Commit();
               return RedirectToAction("Index");
            }

            return View(stockSet);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem creating the Stock Set with Name '{0}'.", stockSet.Name));
            return View("Error", new HandleErrorInfo(exception, "StockSets", "Create"));
         }
      }

      // GET: Inventory/StockSets/Edit/5
      public ActionResult Edit(int? id)
      {
         try
         {
            if (!id.HasValue)
               return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var stockSet = _stockSetRepository.Find(id.Value);
            if (stockSet == null)
               return HttpNotFound();

            return View(stockSet);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem retrieving the Stock Set with ID '{0}'.", id.HasValue ? id.Value.ToString() : string.Empty));
            return View("Error", new HandleErrorInfo(exception, "StockSets", "Edit"));
         }
      }

      // POST: Inventory/StockSets/Edit/5
      // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
      // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
      [HttpPost]
      [ValidateAntiForgeryToken]
      public ActionResult Edit([Bind(Include = "StockSetId,Name")] StockSet stockSet)
      {
         try
         {
            if (ModelState.IsValid)
            {
               _stockSetRepository.Update(stockSet);
               _stockSetRepository.Commit();
               return RedirectToAction("Index");
            }
            return View(stockSet);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem editing the Stock Set with ID '{0}'.", stockSet.StockSetId));
            return View("Error", new HandleErrorInfo(exception, "StockSets", "Edit"));
         }
      }

      // GET: Inventory/StockSets/Delete/5
      public ActionResult Delete(int? id)
      {
         try
         {
            if (id == null)
            {
               return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var stockSet = _stockSetRepository.Find(id.Value);
            if (stockSet == null)
            {
               return HttpNotFound();
            }
            return View(stockSet);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem retrieving the Stock Set with ID '{0}'.", id.HasValue ? id.Value.ToString() : string.Empty));
            return View("Error", new HandleErrorInfo(exception, "StockSets", "Delete"));
         }
      }

      // POST: Inventory/StockSets/Delete/5
      [HttpPost, ActionName("Delete")]
      [ValidateAntiForgeryToken]
      public ActionResult DeleteConfirmed(int id)
      {
         try
         {
            var stockSet = _stockSetRepository.Find(id);
            stockSet.DeletedBy = User.Identity.Name;
            _stockSetRepository.Remove(stockSet, User.Identity.Name);
            _stockSetRepository.Commit();
            return RedirectToAction("Index");
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem deleting the Stock Set with ID '{0}'.", id));
            return View("Error", new HandleErrorInfo(exception, "StockSets", "Delete"));
         }
      }

      [HttpPost]
      public ActionResult ChangeQuantity(StockSetItem stockSetItem)
      {
         try
         {
            var lineItem = _stockSetRepository.GetSetItem(stockSetItem.StockSetId, stockSetItem.ProductId);

            if (lineItem == null)
               return HttpNotFound();

            int stockSetId = lineItem.StockSetId;
            lineItem.Quantity = stockSetItem.Quantity;
            _stockSetRepository.Commit();

            return RedirectToAction("Edit", new { id = stockSetId });
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem updating the quantity of the Stock Set Item with ID '{0}'.", stockSetItem.StockSetItemId));
            return View("Error", new HandleErrorInfo(exception, "StockSets", "ChangeQuantity"));
         }
      }

      public ActionResult RemoveItem(int stockSetId, int productId)
      {
         try
         {
            //Get  item
            var lineItem = _stockSetRepository.GetSetItem(stockSetId, productId);

            if (lineItem == null)
               return HttpNotFound();

            _stockSetRepository.RemoveSetItem(lineItem);
            _stockSetRepository.Commit();

            return RedirectToAction("Edit", new { id = stockSetId });
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem retrieving the Stock Set Item for Stock Set with ID '{0}' and Product ID '{1}'.", stockSetId, productId));
            return View("Error", new HandleErrorInfo(exception, "StockSets", "RemoveItem"));
         }
      }

      [HttpPost]
      public ActionResult AddItem(StockSetItem item)
      {
         try
         {
            //Get stock set
            var set = _stockSetRepository.Find(item.StockSetId);
            if (set == null)
               throw new Exception("Failed to find order " + item.StockSetId);

            //Just adjust the quantity if the product is already in the order
            var existingItem = set.Items.SingleOrDefault(si => si.ProductId == item.ProductId);
            if (existingItem != null)
            {
               existingItem.Quantity = existingItem.Quantity + item.Quantity;
            }
            else
            {
               //Add it to the set
               var newItem = new StockSetItem() { ProductId = item.ProductId, Quantity = item.Quantity };
               set.Items.Add(newItem);
            }

            _stockSetRepository.Commit();

            return RedirectToAction("Edit", new { id = item.StockSetId });
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem adding the Stock Set Item for Product with ID '{0}' and quantity of '{1}' to the Stock Set with ID '{2}'.", item.ProductId, item.Quantity, item.StockSetId));
            return View("Error", new HandleErrorInfo(exception, "StockSets", "AddItem"));
         }
      }
   }
}
