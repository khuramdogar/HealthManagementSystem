using HMS.HealthTrack.Web.App_Start;
using HMS.HealthTrack.Web.Models;
using HMS.HealthTrack.Web.Models.Account;
using HMS.HealthTrack.Web.Utils;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using HMS.HealthTrack.Inventory.Common;

namespace HMS.HealthTrack.Web.Controllers
{
   public class AccountController : Controller
   {
      private readonly ICustomLogger _logger;

      public AccountController(ICustomLogger logger)
      {
         _logger = logger;
      }

      // GET: Account
      [AllowAnonymous]
      public ActionResult Login(string returnUrl)
      {
         ViewBag.ReturnUrl = returnUrl;
         return View();
      }

      [HttpPost]
      [AllowAnonymous]
      [ValidateAntiForgeryToken]
      public async Task<ActionResult> Login(LoginModel model)
      {
         if (!ModelState.IsValid)
         {
            return View(model);
         }

         var user = new ApplicationUser
         {
            UserName = model.Username,
         };

         var res = await HttpContext.GetOwinContext().Get<SecurityUserManager>().CheckPasswordAsync(user, model.Password);
         if (res)
         {
            var identity = await HttpContext.GetOwinContext().Get<SecurityAuthenticationManager>().CreateUserIdentityAsync(user);
            HttpContext.GetOwinContext().Authentication.SignIn(new AuthenticationProperties() { IsPersistent = false }, identity);
            _logger.Information("User {Username} logged in to Inventory", model.Username);
            if (!string.IsNullOrWhiteSpace(model.ReturnUrl))
            {
               return Redirect(model.ReturnUrl);
            }
            return RedirectToAction("Index", "Home");
         }
         ModelState.AddModelError("", "Invalid login attempt.");
         return View(model);
      }

      [HttpGet]
      public ActionResult Logout()
      {
         //Dump permissions
         Session[SecurityUtils.GetCachedCredentialsHandle(User)] = null;

         //Logout
         HttpContext.GetOwinContext().Authentication.SignOut();

         _logger.Information("User has been logged out.");
         return RedirectToAction("Index", "Home");
      }
   }
}