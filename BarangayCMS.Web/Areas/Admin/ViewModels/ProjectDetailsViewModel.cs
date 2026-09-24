using System;

namespace BarangayCMS.Web.Areas.Admin.Models
{
    // 📋 Details ng proyekto + kalakip na financial summary (approved / used / remaining + recent expenses).
    public class ProjectDetailsViewModel
    {
        public int Id { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Contractor { get; set; } = string.Empty;
        public decimal Budget { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // Buong buod ng pananalapi ng proyektong ito.
        public ProjectBudgetSummaryViewModel Financial { get; set; } = new();
    }
}
