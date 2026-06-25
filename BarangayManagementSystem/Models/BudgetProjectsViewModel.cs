using System.Collections.Generic;

namespace BarangayManagementSystem.Models
{
    public class BudgetAllocationSummary
    {
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class ProjectProgressItem
    {
        public string Name { get; set; } = string.Empty;
        public decimal Budget { get; set; }
        public int ProgressPercentage { get; set; }
        public string Status { get; set; } = string.Empty; // Ongoing, Completed, Pending
    }

    public class BudgetProjectsPageViewModel
    {
        // Top row highlight metrics
        public string TotalBudgetFormatted { get; set; } = "₱1.05M";
        public string UtilizedAmountFormatted { get; set; } = "₱230,000";
        public int UtilizationPercentage { get; set; } = 22;
        public int ActiveProjectsCount { get; set; } = 2;
        public int CompletedProjectsCount { get; set; } = 1;

        // Structured feature lists & tables
        public List<string> SystemFeatures { get; set; } = new List<string>();
        public List<BudgetAllocationSummary> Allocations { get; set; } = new List<BudgetAllocationSummary>();
        public List<ProjectProgressItem> Projects { get; set; } = new List<ProjectProgressItem>();
    }
}