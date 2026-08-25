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
        [DataType(DataType.DateTime)]
        // Render the <input type="datetime-local"> value to the minute only — no seconds / milliseconds.
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime ActivityDate { get; set; } = DateTime.Now;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
    }
}