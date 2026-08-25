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

        // External/relative image path (legacy / optional). Kept for backward compatibility.
        public string ImageUrl { get; set; } = string.Empty;

        // The uploaded image stored directly in the database so it opens on any device,
        // served via /Announcement/Image/{id}.
        public byte[]? ImageData { get; set; }
        public string? ImageContentType { get; set; }

        public bool IsPinned { get; set; }

        public DateTime PublishDate { get; set; }
        public DateTime? ExpiryDate { get; set; }

        
        public string AuthorName { get; set; } = string.Empty;
    }
}