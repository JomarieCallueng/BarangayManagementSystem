using System;

namespace BarangayCMS.DTO
{
    public class EnvironmentDTO
    {
        public int Id { get; set; }
        public string ActivityName { get; set; } = string.Empty; // Clean-up Drive, Tree Planting
        public string LocationArea { get; set; } = string.Empty; // Purok/Sitio or Riverside
        public DateTime InspectionOrActivityDate { get; set; }

        // Monitoring Matrix
        public string WasteManagementStatus { get; set; } = string.Empty; // Compliant, Warning, Violation
        public int ViolationsCount { get; set; }
        public string Remarks { get; set; } = string.Empty;

        public string InspectorName { get; set; } = string.Empty;
    }
}
