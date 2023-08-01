using System.ComponentModel.DataAnnotations;

namespace HMS.HealthTrack.Web.Models.Account
{
   public class LoginModel
   {
      [Required]
      public string Username { get; set; }

      [Required]
      [DataType(DataType.Password)]
      public string Password { get; set; }

      [Display(Name = "Remember me?")]
      public bool RememberMe { get; set; }

      public string ReturnUrl { get; set; }

   }
}