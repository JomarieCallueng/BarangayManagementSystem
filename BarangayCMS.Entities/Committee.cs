using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BarangayCMS.Entities
{
    public class Committee
    {
        [Key]
        public int CommitteeId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Category { get; set; } = string.Empty; // "SB" o "SK"

        public string Description { get; set; } = string.Empty;

        [MaxLength(100)]
        public string IconClass { get; set; } = string.Empty;

        public virtual ICollection<CommitteeAssignment> Assignments { get; set; } = new List<CommitteeAssignment>();
    }
}