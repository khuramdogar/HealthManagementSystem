using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace HMS.HealthTrack.Web.Controllers
{
   public class HomeController : Controller
   {
      public ActionResult Index()
      {
         ViewBag.Title = "Home Page";

         return View();
      }

      public ActionResult Angular()
      {
         ViewBag.Title = "Home Page";

         return View();
      }

      public ActionResult TodoItems()
      {
         return Json(new List<Todo>
         {
            new Todo
            {
               Id = 1,
               CreatedAt = DateTime.Now.ToString(),
               Description = "Complete team meeting"
            },
            new Todo
            {
               Id = 2,
               CreatedAt = DateTime.Now.ToString(),
               Description = "Goto market"
            },
            new Todo
            {
               Id = 3,
               CreatedAt = DateTime.Now.ToString(),
               Description = "Eat food"
            }
         }, JsonRequestBehavior.AllowGet);
      }
   }

   public class Todo
   {
      public int Id { get; set; }
      public string Description { get; set; }
      public string CreatedAt { get; set; }
   }
}