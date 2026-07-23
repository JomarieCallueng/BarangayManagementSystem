using System;
using System.ComponentModel.DataAnnotations;

namespace BarangayCMS.Entities
{
    public class Announcement
    {
        [Key]
        public int AnnouncementId { get; set; } // Database Primary Key

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty; // General, Health, Advisory, Holiday, Emergency
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsPinned { get; set; }

        public DateTime PublishDate { get; set; }
        public DateTime? ExpiryDate { get; set; }

        
        public string AuthorName { get; set; } = string.Empty;
    }
}