using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarangayCMS.Entities
{
    public class ReportLog
    {
        [Key]
        public int ReportLogId { get; set; } // Database Primary Key

        public string TargetReportType { get; set; } = string.Empty; // Resident, Financial, Blotter, Executive
        public DateTime GeneratedAt { get; set; }

        // Naka-snapshot ang values para sa historical tracking
        public int TotalPopulation { get; set; }
        public int TotalRegisteredVoters { get; set; }
        public int TotalHouseholds { get; set; }
        public int ActiveComplaintsCount { get; set; }
        public int ResolvedComplaintsCount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAllocatedBudget { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalProjectExpenses { get; set; }

        public int TotalRecordedIncidents { get; set; }
        public int DisplacedFamiliesCount { get; set; }

        public string GeneratedBy { get; set; } = string.Empty; // Admin o Staff name
    }
}