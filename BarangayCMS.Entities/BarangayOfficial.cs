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

        public bool IsActive { get; set; } = true;
    }
}

