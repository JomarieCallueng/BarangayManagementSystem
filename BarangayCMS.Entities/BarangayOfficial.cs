using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BarangayCMS.Entities
{
    public class BarangayOfficial
    {
        [Key]
        public int BarangayOfficialId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Position { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Committee { get; set; } = string.Empty;

        [MaxLength(255)]
        public string SignaturePath { get; set; } = string.Empty;

        // Profile photo filename (served mula sa /uploads/officials/). Optional.
        [MaxLength(255)]
        public string ProfileImagePath { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        // --- EF Core Navigation Property ---
        // Isang tao → maraming termino/service record.
        public ICollection<OfficialServiceHistory> ServiceHistories { get; set; } = new List<OfficialServiceHistory>();
    }
}

