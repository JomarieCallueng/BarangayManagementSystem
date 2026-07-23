using System.ComponentModel.DataAnnotations;

namespace BarangayCMS.Web.Areas.Admin.Models
{
    public class HealthViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ang residente ay kinakailangan.")]
        [Display(Name = "Residente")]
        public int ResidentId { get; set; }

        [Display(Name = "Pangalan ng Residente")]
        public string ResidentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ang medikal na kondisyon ay kinakailangan.")]
        [MaxLength(150, ErrorMessage = "Hindi pwedeng lumagpas sa 150 characters.")]
        [Display(Name = "Medikal na Kondisyon / Diagnosis")]
        public string MedicalCondition { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Hindi pwedeng lumagpas sa 500 characters.")]
        [Display(Name = "Mga Tala / Remarks / Reseta")]
        public string Remarks { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ang petsa ay kinakailangan.")]
        [DataType(DataType.Date)]
        [Display(Name = "Petsa ng Pagtatala")]
        public DateTime DateRecorded { get; set; } = DateTime.Now;
    }
}