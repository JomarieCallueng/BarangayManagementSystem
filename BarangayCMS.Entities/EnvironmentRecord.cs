using System;
using System.ComponentModel.DataAnnotations;

namespace BarangayCMS.Entities
{
    public class EnvironmentRecord
    {
        [Key]
        public int EnvironmentRecordId { get; set; } // Database Primary Key

        [Required]
        public string ActivityName { get; set; } = string.Empty; // Clean-up Drive, Tree Planting

        [Required]
        public string LocationArea { get; set; } = string.Empty; // Purok/Sitio or Riverside

        public DateTime InspectionOrActivityDate { get; set; }

        // Monitoring Matrix
        public string WasteManagementStatus { get; set; } = string.Empty; // Compliant, Warning, Violation
        public int ViolationsCount { get; set; }
        public string Remarks { get; set; } = string.Empty;

        public string InspectorName { get; set; } = string.Empty;
        public DateTime DateLogged { get; set; }
    }
}