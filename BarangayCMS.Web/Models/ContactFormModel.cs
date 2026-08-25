using System.ComponentModel.DataAnnotations;

namespace BarangayManagementSystem.Models
{
    public class ContactFormModel
    {
        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact number is required")]
        [RegularExpression(@"^09\d{9}$", ErrorMessage = "Must be a valid format (e.g., 09XXXXXXXXX)")]
        public string ContactNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Subject is required")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Message body is required")]
        [StringLength(1000, ErrorMessage = "Message cannot exceed 1000 characters")]
        public string Message { get; set; } = string.Empty;
    }
}