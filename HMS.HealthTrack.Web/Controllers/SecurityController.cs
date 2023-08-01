using System;
using System.Collections.Generic;
using System.Web.Mvc;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Model.Security;
using HMS.HealthTrack.Web.Data.Repositories.Security;

namespace HMS.HealthTrack.Web.Controllers
{
   public class SecurityController : Controller
   {
      private readonly IUserRepository _userRepo;
      private readonly ICustomLogger _logger;

      public SecurityController(IUserRepository userRepository, ICustomLogger logger)
      {
         _userRepo = userRepository;
         _logger = logger;
      }

      public ActionResult Permissions()
      {
         try
         {
            IEnumerable<HealthTrackPermission> userPrivs = _userRepo.GetPermissionsForUser(User.Identity.Name);
            return View("Permissions", userPrivs);
         }
         catch (Exception exception)
         {
            var message = string.Format("Failed to get permissions for user {0}", User.Identity.Name);
            _logger.Fatal(message, exception);
            ModelState.AddModelError("", message);
         }
         return View("Permissions", new List<HealthTrackPermission>());
      }
   }
}