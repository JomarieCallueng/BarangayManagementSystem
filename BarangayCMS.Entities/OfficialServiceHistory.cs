using System;
using System.ComponentModel.DataAnnotations;

namespace BarangayCMS.Entities
{
    /// <summary>
    /// Isang termino/service record ng isang partikular na <see cref="BarangayOfficial"/>.
    /// Isang tao (BarangayOfficial) → maraming service history records.
    /// Naka-ugnay sa pamamagitan ng <see cref="BarangayOfficialId"/> kaya ang history
    /// ay pag-aari LAMANG ng tamang tao — hindi kailanman naghahalo sa ibang opisyal.
    /// </summary>
    public class OfficialServiceHistory
    {
        [Key]
        public int Id { get; set; }

        // 🔑 Kung kaninong opisyal ang termino na ito.
        public int BarangayOfficialId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Position { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Committee { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        // null = kasalukuyang termino (ongoing / hanggang ngayon).
        public DateTime? EndDate { get; set; }

        // "Current" o "Completed"
        public string Status { get; set; } = "Completed";

        // --- EF Core Navigation Property ---
        public BarangayOfficial BarangayOfficial { get; set; } = null!;
    }
}
