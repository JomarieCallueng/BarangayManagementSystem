using System;

namespace BarangayCMS.DTO
{
    public class BudgetDTO
    {
        public int Id { get; set; }
        public int Year { get; set; }
        public string Category { get; set; } = string.Empty; // General Fund, SK Fund, IRA, Calamity Fund
        public string Description { get; set; } = string.Empty;

        public decimal TotalAllocation { get; set; }
        public decimal DisbursedAmount { get; set; }
        public decimal AvailableBalance => TotalAllocation - DisbursedAmount;

        public string LoggedBy { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
    }
}
