using System.ComponentModel.DataAnnotations;

namespace BarangayCMS.Web.Models
{
    public class ComplaintSubmissionModel
    {
        [Required(ErrorMessage = "Ang iyong pangalan ay kinakailangan.")]
        [Display(Name = "Complainant's Full Name")]
        public string ComplainantName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Maglagay ng contact number para sa updates.")]
        [Phone(ErrorMessage = "Hindi balido ang ibinigay na numero.")]
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; } = string.Empty;

        [Display(Name = "Respondent Name")]
        public string? RespondentName { get; set; }

        [Required(ErrorMessage = "Pumili ng kategorya ng insidente.")]
        [Display(Name = "Incident Category")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ibigay ang lokasyon kung saan naganap ang insidente.")]
        [Display(Name = "Location of Incident")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Isulat ang detalye o salaysay ng pangyayari.")]
        [Display(Name = "Statement of Facts")]
        public string Statement { get; set; } = string.Empty;
    }
}

