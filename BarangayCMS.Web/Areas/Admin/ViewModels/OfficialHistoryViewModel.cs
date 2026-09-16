using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BarangayCMS.Web.Areas.Admin.Models
{
    /// <summary>Isang termino/service record row para sa admin history page.</summary>
    public class ServiceHistoryItem
    {
        public int Id { get; set; }
        public string Position { get; set; } = string.Empty;
        public string Committee { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } = "Completed";
    }

    /// <summary>Buong page model para sa "Manage Service History" ng isang opisyal.</summary>
    public class OfficialHistoryViewModel
    {
        public int OfficialId { get; set; }
        public string OfficialName { get; set; } = string.Empty;
        public string CurrentPosition { get; set; } = string.Empty;

        public List<ServiceHistoryItem> Terms { get; set; } = new List<ServiceHistoryItem>();

        // --- Fields para sa pagdadagdag ng bagong termino ---
        [Required(ErrorMessage = "Kailangan ang posisyon.")]
        [MaxLength(100)]
        [Display(Name = "Posisyon")]
        public string NewPosition { get; set; } = string.Empty;

        [MaxLength(100)]
        [Display(Name = "Komite (Committee)")]
        public string NewCommittee { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kailangan ang taon ng simula.")]
        [Range(1900, 2200, ErrorMessage = "Di-wastong taon.")]
        [Display(Name = "Taon ng Simula (Start Year)")]
        public int? NewStartYear { get; set; }

        [Range(1900, 2200, ErrorMessage = "Di-wastong taon.")]
        [Display(Name = "Taon ng Pagtatapos (End Year)")]
        public int? NewEndYear { get; set; }
    }
}
