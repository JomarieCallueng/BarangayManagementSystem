using System.ComponentModel.DataAnnotations;

namespace BarangayCMS.Web.Areas.Admin.Models
{
    public class EnvironmentViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ang pangalan ng aktibidad ay kinakailangan.")]
        [MaxLength(150)]
        public string ActivityName { get; set; } = string.Empty;

        [MaxLength(150)]
        public string Location { get; set; } = string.Empty;

        [Required]
        public DateTime ActivityDate { get; set; } = DateTime.Now;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
    }
}