using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HMS.HealthTrack.Web.Models
{
   // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
   public class ApplicationUser : IdentityUser
   {
      public async Task<ClaimsIdentity> GenerateUserIdentityAsync(SecurityUserManager userManager)
      {
         // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
         var userIdentity = await userManager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);

         // Add custom user claims here
         return userIdentity;
      }
   }
}