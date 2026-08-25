using System;
using System.ComponentModel.DataAnnotations;

namespace BarangayCMS.Areas.Staff.ViewModels
{
    public class ComplaintViewModel
    {
        public int ComplaintId { get; set; }

        [Required(ErrorMessage = "Pumili ng residente.")]
        [Display(Name = "Resident Name")]
        public int ResidentId { get; set; }

        public string ResidentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ang Subject ay kinakailangan.")]
        [MaxLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Pakilagay ang detalye ng reklamo.")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Date Submitted")]
        public DateTime DateSubmitted { get; set; } = DateTime.Now;

        public string Status { get; set; } = "Pending";
    }
}