using System.ComponentModel.DataAnnotations;

namespace BarangayCMS.Web.Areas.Admin.Models
{
    public class ProjectViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ang pangalan ng proyekto ay kinakailangan.")]
        [MaxLength(150, ErrorMessage = "Hindi pwedeng lumagpas sa 150 characters.")]
        [Display(Name = "Pangalan ng Proyekto")]
        public string ProjectName { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Hindi pwedeng lumagpas sa 500 characters.")]
        [Display(Name = "Deskripsyon / Detalye ng Proyekto")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ang nakalaang pondo o budget ay kinakailangan.")]
        [Range(0, 999999999.99, ErrorMessage = "Mangyaring maglagay ng tamang halaga.")]
        [Display(Name = "Pondo / Budget (PHP)")]
        public decimal Budget { get; set; }

        [Required(ErrorMessage = "Ang petsa ng pagsisimula ay kinakailangan.")]
        [DataType(DataType.Date)]
        [Display(Name = "Araw ng Simula")]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Ang petsa ng pagtatapos ay kinakailangan.")]
        [DataType(DataType.Date)]
        [Display(Name = "Inaasahang Tapos")]
        public DateTime EndDate { get; set; } = DateTime.Now.AddMonths(1);

        [Required]
        [MaxLength(30)]
        [Display(Name = "Kasalukuyang Status")]
        public string Status { get; set; } = "Planned";
    }
}