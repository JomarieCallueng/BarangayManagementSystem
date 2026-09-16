using System;

namespace BarangayCMS.DTO
{
    public class ContactMessageDTO
    {
        public int Id { get; set; }
        public string ContactNumber { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Status { get; set; } = "Unread"; // Unread | Read | Replied | Archived
        public DateTime CreatedAt { get; set; }

        public string? AdminReply { get; set; }
        public DateTime? RepliedAt { get; set; }

        public bool HasReply => !string.IsNullOrWhiteSpace(AdminReply);

        public string MessagePreview
        {
            get
            {
                var body = (Message ?? string.Empty).Replace("\r", " ").Replace("\n", " ").Trim();
                return body.Length > 80 ? body.Substring(0, 80) + "…" : body;
            }
        }
    }
}
