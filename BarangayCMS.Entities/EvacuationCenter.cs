using System;
using System.ComponentModel.DataAnnotations;

namespace BarangayCMS.Entities
{
    /// <summary>
    /// Isang pisikal na evacuation center na pinamamahalaan ng Admin/Staff.
    /// Ito ang IISANG pinagmumulan ng datos para sa Public Evacuation view —
    /// walang hardcoded na listahan sa frontend. Ang bawat pagbabago rito
    /// (kapasidad, okupante, status) ay agad na makikita ng publiko.
    /// </summary>
    public class EvacuationCenter
    {
        [Key]
        public int EvacuationCenterId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty; // e.g. Barangay Covered Court

        [Required]
        public string Address { get; set; } = string.Empty; // e.g. Barangay Main Road

        // Purok / Sitio / Zone kung saan matatagpuan ang center.
        public string SitioPurok { get; set; } = string.Empty;

        // Maximum na bilang ng tao na kayang tanggapin.
        public int Capacity { get; set; }

        // Kasalukuyang bilang ng okupante (in-uupdate ng Admin/Staff).
        public int CurrentOccupants { get; set; }

        // Contact number ng center o naka-assign na tanod/coordinator.
        public string ContactNumber { get; set; } = string.Empty;

        // Opsyonal na mga tala/tagubilin para sa center na ito.
        public string? Notes { get; set; }

        // Aktibo/available ba ang center para sa publiko? Kapag false,
        // hindi ito lalabas sa Public view ngunit nananatili ang record
        // (hindi sinisira ang kasaysayan).
        public bool IsActive { get; set; } = true;

        public DateTime DateCreated { get; set; } = DateTime.Now;
        public DateTime? DateUpdated { get; set; }
    }
}
