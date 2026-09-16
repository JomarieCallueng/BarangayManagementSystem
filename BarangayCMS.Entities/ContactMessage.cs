using System;
using System.ComponentModel.DataAnnotations;

namespace BarangayCMS.Entities
{
    /// <summary>
    /// Isang mensaheng ipinadala ng publiko/residente mula sa "Send a Message"
    /// form sa /Home/Contact. WALANG kaugnay na account — puro Contact Number at
    /// Message lang. Ang sagot ng admin ay naka-store din dito (AdminReply) para
    /// magkaugnay ang usapan sa iisang record.
    /// </summary>
    public class ContactMessage
    {
        [Key]
        public int Id { get; set; }

        // Contact number na inilagay ng nagpadala (walang account).
        public string ContactNumber { get; set; } = string.Empty;

        // Ang mensahe mismo mula sa residente/publiko.
        public string Message { get; set; } = string.Empty;

        // Unread, Read, Replied, Archived
        public string Status { get; set; } = "Unread";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // --- Sagot ng admin (nakakabit sa parehong mensahe) ---
        public string? AdminReply { get; set; }
        public DateTime? RepliedAt { get; set; }
    }
}
