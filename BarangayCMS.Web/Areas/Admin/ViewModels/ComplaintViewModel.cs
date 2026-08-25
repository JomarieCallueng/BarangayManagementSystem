using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BarangayCMS.Web.Areas.Admin.Models
{
    public class ComplaintViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select a complainant.")]
        [Display(Name = "Complainant")]
        public int ResidentId { get; set; }

        [Display(Name = "Complainant Full Name")]
        public string ResidentFullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Subject is required.")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Complainant Name (Form)")]
        public string ComplainantName { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date Submitted")]
        public DateTime DateSubmitted { get; set; } = DateTime.Now;

        [Required]
        public string Status { get; set; } = "Pending"; // Pending, Assigned, Investigation, Resolved, Dismissed

        // Dropdown data context list helper
        public IEnumerable<SelectListItem>? ResidentList { get; set; }
    }
}