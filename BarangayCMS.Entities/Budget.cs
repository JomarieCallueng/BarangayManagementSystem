using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarangayCMS.Entities
{
    public class Budget
    {
        [Key]
        public int BudgetId { get; set; }

        [Required]
        public int Year { get; set; }

        // 🔑 PINAGMULAN NG PONDO (Mother Fund e.g., Educational Fund, General Fund, SK Fund)
        [Required]
        public string MainFundSource { get; set; } = string.Empty;

        // GAGAMITAN NG PONDO / SUB-CATEGORY (e.g., Educational Allowance, School Supplies)
        [Required]
        public string Category { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAllocation { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DisbursedAmount { get; set; }

        public string LoggedBy { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
    }
}