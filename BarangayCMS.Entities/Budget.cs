using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarangayCMS.Entities
{
    public class Budget
    {
        [Key]
        public int BudgetId { get; set; } // Database Primary Key

        [Required]
        public int Year { get; set; }

        [Required]
        public string Category { get; set; } = string.Empty; // General Fund, SK Fund, IRA, Calamity Fund

        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAllocation { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DisbursedAmount { get; set; }

        public string LoggedBy { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
    }
}