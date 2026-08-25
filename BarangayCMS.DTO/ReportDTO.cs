using System;

namespace BarangayCMS.DTO
{
    public class ReportDTO
    {
        // Core Summary Metrics para sa Dashboard/Printing Reports
        public int TotalPopulation { get; set; }
        public int TotalRegisteredVoters { get; set; }
        public int TotalHouseholds { get; set; }

        // Complaints Analytics
        public int ActiveComplaintsCount { get; set; }
        public int ResolvedComplaintsCount { get; set; }

        // Financials Analytics
        public decimal TotalAllocatedBudget { get; set; }
        public decimal TotalProjectExpenses { get; set; }

        // System Flags
        public int TotalRecordedIncidents { get; set; }
        public int DisplacedFamiliesCount { get; set; }

        public string TargetReportType { get; set; } = string.Empty; // Resident, Financial, Blotter
        public DateTime GeneratedAt { get; set; } = DateTime.Now;
    }
}
