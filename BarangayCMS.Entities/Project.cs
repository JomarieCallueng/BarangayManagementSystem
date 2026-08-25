using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarangayCMS.Entities
{
    public class Project
    {
        [Key]
        public int ProjectId { get; set; } // Database Primary Key

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal BudgetAllocated { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalExpenses { get; set; }

        public string Contractor { get; set; } = string.Empty;

        public string Status { get; set; } = "Planning"; // Planning, Ongoing, Completed, Suspended

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public DateTime DateLogged { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}