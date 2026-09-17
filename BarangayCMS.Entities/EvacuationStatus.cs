using System;
using System.ComponentModel.DataAnnotations;

namespace BarangayCMS.Entities
{
    /// <summary>
    /// Singleton na configuration para sa buong-barangay na kalagayan ng
    /// paglikas. Iisang record lamang ito (Id = 1) na ina-update ng Admin/Staff.
    /// Dito nagmumula ang:
    ///   • Overall evacuation status (Normal / Prepared / Active)
    ///   • Mga tagubilin sa paglikas (isang bullet kada linya)
    ///   • Emergency contacts (format kada linya: "Label|Number")
    /// Ang Public view ay nagpapakita ng datos na ito — hindi hardcoded.
    /// </summary>
    public class EvacuationStatus
    {
        [Key]
        public int Id { get; set; }

        // "Normal", "Prepared", "Active"
        public string OverallStatus { get; set; } = "Normal";

        // Maikling paliwanag na ipapakita sa banner (opsyonal).
        public string? StatusMessage { get; set; }

        // Mga tagubilin sa paglikas — isang item kada bagong linya.
        public string Instructions { get; set; } = string.Empty;

        // Emergency contacts — isang entry kada linya, format "Label|Number".
        public string EmergencyContacts { get; set; } = string.Empty;

        // Sino ang huling nag-update at kailan (para sa "Last Updated").
        public string? UpdatedBy { get; set; }
        public DateTime DateUpdated { get; set; } = DateTime.Now;
    }
}
