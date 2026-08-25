using System;
using System.ComponentModel.DataAnnotations;

namespace BarangayCMS.Areas.Staff.ViewModels
{
    public class AnnouncementViewModel
    {
        // Ginawang Id o AnnouncementId para maging flexible sa loop mo (@item.Id)
        public int Id { get; set; }
        public int AnnouncementId { get; set; }

        [Required(ErrorMessage = "Ang Pamagat (Title) ay kinakailangan.")]
        [MaxLength(150, ErrorMessage = "Ang Pamagat ay hindi pwedeng lumampas sa 150 karakter.")]
        [Display(Name = "Announcement Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ang Nilalaman (Content) ay kinakailangan.")]
        [Display(Name = "Content / Details")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Date Posted")]
        public DateTime DatePosted { get; set; } = DateTime.Now;

        // 📌 Idinagdag para sumwak sa kailangan ng Index.cshtml table layout mo:
        public string Category { get; set; } = "General";
        public string AuthorName { get; set; } = "Staff";
        public DateTime PublishDate { get; set; } = DateTime.Now;
        public DateTime? ExpiryDate { get; set; }
        public bool IsPinned { get; set; }
    }
}