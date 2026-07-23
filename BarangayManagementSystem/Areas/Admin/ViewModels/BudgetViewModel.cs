using System.ComponentModel.DataAnnotations;

namespace BarangayCMS.Web.Areas.Admin.Models
{
    public class BudgetViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ang kategorya ay kinakailangan.")]
        [MaxLength(100, ErrorMessage = "Hindi pwedeng lumagpas sa 100 characters ang kategorya.")]
        [Display(Name = "Kategorya (Category)")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ilagay ang inilaang pondo.")]
        [Range(0, 999999999, ErrorMessage = "Dapat positibong numero ang pondo.")]
        [Display(Name = "Inilaang Pondo (Allocated Amount)")]
        public decimal AllocatedAmount { get; set; }

        [Required(ErrorMessage = "Ilagay ang nagamit na pondo.")]
        [Range(0, 999999999, ErrorMessage = "Dapat positibong numero ang halaga.")]
        [Display(Name = "Nagamit na Pondo (Used Amount)")]
        public decimal UsedAmount { get; set; }

        [Required(ErrorMessage = "Ang Taon ng Pananalapi ay kinakailangan.")]
        [Display(Name = "Taon ng Pananalapi (Fiscal Year)")]
        public int FiscalYear { get; set; } = DateTime.Now.Year;

        [MaxLength(500, ErrorMessage = "Hindi pwedeng lumagpas sa 500 characters ang remarks.")]
        [Display(Name = "Mga Tala / Remarks")]
        public string Remarks { get; set; } = string.Empty;

        // Custom helpers para sa UI
        [Display(Name = "Natitirang Pondo")]
        public decimal RemainingAmount => AllocatedAmount - UsedAmount;

        public double PercentUsed => AllocatedAmount > 0 ? (double)(UsedAmount / AllocatedAmount) * 100 : 0;
    }
}
