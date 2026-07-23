using System;
using System.ComponentModel.DataAnnotations;

namespace BarangayCMS.Web.Areas.Admin.Models
{
    public class AnnouncementViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ang pamagat (Title) ay kinakailangan.")]
        [MaxLength(150, ErrorMessage = "Hindi pwedeng lumagpas sa 150 characters ang pamagat.")]
        [Display(Name = "Pamagat (Title)")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ang nilalaman (Content) ay kinakailangan.")]
        [Display(Name = "Nilalaman (Content)")]
        public string Content { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Petsa ng Pagka-post")]
        public DateTime DatePosted { get; set; } = DateTime.Now;

        [DataType(DataType.DateTime)]
        [Display(Name = "Petsa ng Pagka-expire")]
        public DateTime? ExpiryDate { get; set; }
    }
}