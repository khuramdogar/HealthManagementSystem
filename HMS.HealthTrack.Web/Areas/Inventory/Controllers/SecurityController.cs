using AutoMapper;
using HMS.HealthTrack.Web.Areas.Inventory.Models;
using System;
using System.Linq;
using System.Web.Mvc;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Areas.Inventory.Utils;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers
{
   [HealthTrackAuthorize(HealthTrackPermission = ("Inventory"))]
   public class SecurityController : Controller
   {
      private readonly IPreferenceRepository _preferenceRepository;
      private readonly ICustomLogger _logger;

      public SecurityController(IPreferenceRepository preferenceRepository, ICustomLogger logger)
      {
         _preferenceRepository = preferenceRepository;
         _logger = logger;
      }

      public ActionResult Preferences()
      {
         try
         {
            var preferences = _preferenceRepository.GetPreferencesForUser(User.Identity).SingleOrDefault();
            if (preferences == null)
            {
               return View(new InventoryPreferenceModel
               {
                  UserId = User.Identity.Name
               });
            }
            return View(Mapper.Map<UserPreference, InventoryPreferenceModel>(preferences));
         }
         catch (Exception exception)
         {
            _logger.Error(exception, "Failed to display preferences for use {User}", User.Identity.Name);
            return View("Error", new HandleErrorInfo(exception, "Security", "Preferences"));
         }
      }

      public ActionResult EditPreferences()
      {
         try
         {
            var preferences = _preferenceRepository.GetPreferencesForUser(User.Identity).SingleOrDefault();
            if (preferences == null)
            {
               return View(new InventoryPreferenceModel
               {
                  UserId = User.Identity.Name
               });
            }
            return View(Mapper.Map<UserPreference, InventoryPreferenceModel>(preferences));
         }
         catch (Exception exception)
         {
            _logger.Error(exception, "Failed to display preferences for use {User}", User.Identity.Name);
            return View("Error", new HandleErrorInfo(exception, "Security", "Preferences"));
         }
      }

      [HttpPost]
      public ActionResult EditPreferences(InventoryPreferenceModel model)
      {
         try
         {
            if (!model.UserId.Equals(User.Identity.Name))
            {
               _logger.Warning("Cannot update preferences for another user");
               ModelState.AddModelError("", "Cannot update preferences for another user.");
            }

            if (!ModelState.IsValid)
            {
               _logger.Warning("Attempting to create/update user preferences with invalid data");
               ModelState.AddModelError("LocationId", "Please set a location.");
               return View(model);
            }

            var preference = Mapper.Map<InventoryPreferenceModel, UserPreference>(model);
            _preferenceRepository.Update(preference);
            _preferenceRepository.Commit();

            var preferencesHandle = UserUtils.GetUnconfiguredInventoryPreferencesHandle(User);
            if (HttpContext.Session != null) HttpContext.Session.Remove(preferencesHandle);
            return RedirectToAction("Preferences");
         }
         catch (Exception exception)
         {
            _logger.Error(exception, "Could not update user preferences");
            return View("Error", new HandleErrorInfo(exception, "Security", "Preferences"));
         }
      }
   }
}