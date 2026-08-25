using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BarangayCMS.Web.Areas.Admin.Models
{
    public class BarangayOfficialViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ang buong pangalan ay kinakailangan.")]
        [MaxLength(100, ErrorMessage = "Hindi pwedeng lumagpas sa 100 characters ang pangalan.")]
        [Display(Name = "Buong Pangalan")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ang posisyon ay kinakailangan.")]
        [MaxLength(100, ErrorMessage = "Hindi pwedeng lumagpas sa 100 characters ang posisyon.")]
        [Display(Name = "Posisyon")]
        public string Position { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "Hindi pwedeng lumagpas sa 100 characters ang komite.")]
        [Display(Name = "Komite (Committee)")]
        public string Committee { get; set; } = string.Empty;

        [Display(Name = "E-Signature / Lagda")]
        public string SignaturePath { get; set; } = string.Empty;

        [Display(Name = "Kasalukuyang Nakaupo (Active)")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "I-upload ang Lagda (Image File)")]
        public IFormFile? SignatureFile { get; set; }
    }
}
