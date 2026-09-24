using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarangayCMS.Entities
{
    // 💸 Itemized na gastos na naka-ugnay sa isang partikular na Proyekto (real ProjectId FK).
    // Ito ang tumatawid sa pagitan ng Projects at Budget Tracker modules.
    public class ProjectExpense
    {
        [Key]
        public int ExpenseId { get; set; }

        // 🔗 REAL relationship papuntang Project (hindi pangalan lang).
        public int ProjectId { get; set; }
        public Project? Project { get; set; }

        // Materials, Labor, Equipment, Permits, Others, atbp.
        [Required]
        public string Category { get; set; } = "Materials";

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime ExpenseDate { get; set; }

        public string Description { get; set; } = string.Empty;

        public string LoggedBy { get; set; } = string.Empty;

        public DateTime DateLogged { get; set; }

        // Soft-delete / void support — hindi buburahin ang financial history.
        public bool IsVoided { get; set; }
    }
}
