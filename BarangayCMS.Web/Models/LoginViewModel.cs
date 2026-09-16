using System.ComponentModel.DataAnnotations;

namespace BarangayCMS.Web.Models
{
    public class LoginViewModel
    {
        // Tinatanggap nito ang Email Address O Username — kaya walang
        // [EmailAddress] validation para hindi ma-block ang plain username.
        [Required(ErrorMessage = "Ang Email o Username ay kinakailangan.")]
        [Display(Name = "Email or Username")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ang password ay kinakailangan.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }
}
