using System;
using System.Collections.Generic;

namespace BarangayCMS.Areas.Staff.ViewModels
{
    public class ReportsViewModel
    {
        public string ReportTitle { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; } = DateTime.Now;
        public string FilterRange { get; set; } = "Current Month";

        // Summary Statistics Counters
        public int TotalResidents { get; set; }
        public int TotalCertificatesIssued { get; set; }
        public int ActiveComplaints { get; set; }
        public int ActiveDisasters { get; set; }

        // Generic tabular datasets for specific views
        public List<Dictionary<string, string>> TableData { get; set; } = new();
    }
}