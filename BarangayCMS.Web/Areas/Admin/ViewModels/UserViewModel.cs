using System.ComponentModel.DataAnnotations;

namespace BarangayCMS.Web.Areas.Admin.Models
{
    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ang pangalan (First Name) ay kinakailangan.")]
        [Display(Name = "Unang Pangalan")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ang apelyido (Last Name) ay kinakailangan.")]
        [Display(Name = "Apelyido")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Buong Pangalan")]
        public string FullName => $"{FirstName} {LastName}";

        [Required(ErrorMessage = "Ang email ay kinakailangan.")]
        [EmailAddress(ErrorMessage = "Maling format ng email.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ang Username ay kinakailangan.")]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Pumili ng tungkulin / role.")]
        [Display(Name = "System Role")]
        public string Role { get; set; } = string.Empty;
    }
}