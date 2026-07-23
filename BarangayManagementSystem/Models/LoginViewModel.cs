using System.ComponentModel.DataAnnotations;

namespace BarangayCMS.Web.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Ang Email address ay kinakailangan.")]
        [EmailAddress(ErrorMessage = "Hindi valid ang format ng iyong email.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ang password ay kinakailangan.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }
}
