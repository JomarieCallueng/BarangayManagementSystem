using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarangayCMS.Entities
{
    public class CommitteeAssignment
    {
        [Key]
        public int CommitteeAssignmentId { get; set; }

        public int CommitteeId { get; set; }

        [ForeignKey(nameof(CommitteeId))]
        public virtual Committee? Committee { get; set; }

        public int BarangayOfficialId { get; set; }

        [ForeignKey(nameof(BarangayOfficialId))]
        public virtual BarangayOfficial? BarangayOfficial { get; set; }

        [MaxLength(50)]
        public string Role { get; set; } = "Chairman"; // hal. "Chairman", "Vice-Chairman", "Member"

        public DateTime AssignedDate { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;
    }
}