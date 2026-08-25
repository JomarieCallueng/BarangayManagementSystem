using System;
using System.ComponentModel.DataAnnotations;

namespace BarangayCMS.Areas.Staff.ViewModels
{
    public class EnvironmentViewModel
    {
        public int EnvironmentRecordId { get; set; }

        [Required(ErrorMessage = "Ang Pangalan ng Aktibidad (Activity Name) ay kinakailangan.")]
        [MaxLength(150, ErrorMessage = "Hindi pwedeng lumampas sa 150 karakter.")]
        [Display(Name = "Environmental Activity")]
        public string ActivityName { get; set; } = string.Empty;

        [MaxLength(150, ErrorMessage = "Hindi pwedeng lumampas sa 150 karakter.")]
        [Display(Name = "Venue / Location")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ang Petsa at Oras ng Aktibidad ay kinakailangan.")]
        [Display(Name = "Activity Schedule")]
        public DateTime ActivityDate { get; set; } = DateTime.Now;

        [MaxLength(500, ErrorMessage = "Hindi pwedeng lumampas sa 500 karakter.")]
        [Display(Name = "Project Description & Details")]
        public string Description { get; set; } = string.Empty;
    }
}