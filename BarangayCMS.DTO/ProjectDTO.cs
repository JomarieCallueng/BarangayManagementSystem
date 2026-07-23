using System;

namespace BarangayCMS.DTO
{
    public class ProjectDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;

        public decimal BudgetAllocated { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal RemainingBudget => BudgetAllocated - TotalExpenses;

        public string Contractor { get; set; } = string.Empty;
        public string Status { get; set; } = "Planning"; // Planning, Ongoing, Completed, Suspended

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
