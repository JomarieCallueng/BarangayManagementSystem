using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

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

        [MaxLength(50)]
        [Display(Name = "Kategorya (Category)")]
        public string Category { get; set; } = "General";

        // Server-generated, application-relative image path (e.g. /uploads/announcements/abc123.png).
        // Ito ay OUTPUT ng image upload — awtomatikong pinupunan pagkatapos mag-upload,
        // hindi na mano-manong tine-type ng admin. Iniiwasan ang DataType.Url dahil
        // ang app-relative path ay hindi absolute URL (ma-re-reject sana ng type="url").
        [MaxLength(500)]
        [Display(Name = "Image URL")]
        public string ImageUrl { get; set; } = string.Empty;

        // Napiling image file — awtomatikong ina-upload sa server sa pamamagitan ng
        // AJAX (UploadImage). Ang ibinabalik na path ang naka-store sa ImageUrl.
        [Display(Name = "Upload Image")]
        public IFormFile? ImageFile { get; set; }

        // True kapag may naka-store nang larawan sa database para sa record na ito.
        public bool HasImage { get; set; }

        [Display(Name = "I-pin bilang mahalaga")]
        public bool IsPinned { get; set; }

        [Display(Name = "May-akda")]
        public string AuthorName { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Petsa ng Pagka-post")]
        public DateTime DatePosted { get; set; } = DateTime.Now;

        [DataType(DataType.DateTime)]
        [Display(Name = "Petsa ng Pagka-expire")]
        public DateTime? ExpiryDate { get; set; }
    }
}
