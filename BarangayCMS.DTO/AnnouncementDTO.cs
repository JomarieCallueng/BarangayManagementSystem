using System;

namespace BarangayCMS.DTO
{
    public class AnnouncementDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // General, Health, Advisory, Holiday, Emergency

        public string ImageUrl { get; set; } = string.Empty; // Para sa banners sa index.cshtml
        public bool IsPinned { get; set; } // Lalabas sa Hero section ng Home page

        public DateTime PublishDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string AuthorName { get; set; } = string.Empty;
    }
}
