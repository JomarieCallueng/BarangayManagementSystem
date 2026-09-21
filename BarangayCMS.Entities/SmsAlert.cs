using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarangayCMS.Entities
{
    /// <summary>
    /// SMS Alert History record para sa Disaster Risk Management / Emergency Notification module.
    /// Itinatago ang bawat Emergency SMS blast na ipinadala sa mga residente.
    /// Hindi kailanman itinatago dito ang Semaphore API key.
    /// </summary>
    public class SmsAlert
    {
        [Key]
        public int SmsAlertId { get; set; }

        // Uri ng kalamidad/emergency (Typhoon, Flood, Earthquake, Fire, Evacuation Order, Other)
        [Required]
        [MaxLength(100)]
        public string EmergencyType { get; set; } = string.Empty;

        // Nilalaman ng mensaheng ipinadala
        [Required]
        public string Message { get; set; } = string.Empty;

        // Recipient group description: "All Residents", "Purok: Area 5", "Selected Residents"
        [MaxLength(150)]
        public string RecipientGroup { get; set; } = string.Empty;

        // Bilang ng valid na numero na pinadalhan
        public int RecipientCount { get; set; }

        // Bilang ng matagumpay na naipadala
        public int SuccessCount { get; set; }

        // Bilang ng nabigong maipadala (invalid/failed)
        public int FailedCount { get; set; }

        // Kabuuang status: Sent, Partial, Failed
        [MaxLength(30)]
        public string Status { get; set; } = string.Empty;

        // Sino ang nagpadala (username/email ng Admin)
        [MaxLength(150)]
        public string SentBy { get; set; } = string.Empty;

        // Petsa at oras ng pagpapadala
        public DateTime SentAt { get; set; } = DateTime.Now;

        // Alias para sa DateSent (NotMapped para hindi na mag-migration sa DB)
        [NotMapped]
        public DateTime DateSent => SentAt;
    }
}