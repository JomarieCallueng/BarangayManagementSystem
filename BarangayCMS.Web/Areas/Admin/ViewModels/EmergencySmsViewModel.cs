using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BarangayCMS.Web.Areas.Admin.Models
{
    /// <summary>
    /// ViewModel para sa Emergency SMS Alert form (Disaster Risk Management module).
    /// </summary>
    public class EmergencySmsViewModel
    {
        [Required(ErrorMessage = "Pumili ng uri ng emergency.")]
        [Display(Name = "Disaster / Emergency Type")]
        public string EmergencyType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Pumili ng recipients.")]
        [Display(Name = "Recipients")]
        public string RecipientGroup { get; set; } = "All Residents"; // All Residents | Purok | Selected

        [Display(Name = "Purok / Area")]
        public string? Purok { get; set; }

        // Para sa "Selected Residents"
        public List<int> SelectedResidentIds { get; set; } = new List<int>();

        [Required(ErrorMessage = "Kailangan ang mensahe.")]
        [MaxLength(600, ErrorMessage = "Hindi pwedeng lumampas sa 600 karakter.")]
        [Display(Name = "Message")]
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Read-only row para sa SMS Alert History table.
    /// </summary>
    public class SmsAlertHistoryItem
    {
        public int Id { get; set; }
        public DateTime SentAt { get; set; }
        public string EmergencyType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string RecipientGroup { get; set; } = string.Empty;
        public int RecipientCount { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string SentBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// Lightweight resident option para sa "Selected Residents" picker.
    /// </summary>
    public class ResidentSmsOption
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Purok { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
    }
}
