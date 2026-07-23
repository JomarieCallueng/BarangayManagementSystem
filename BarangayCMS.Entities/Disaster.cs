using System;
using System.ComponentModel.DataAnnotations;

namespace BarangayCMS.Entities
{
    public class Disaster
    {
        [Key]
        public int DisasterId { get; set; } // Database Primary Key

        [Required]
        public string IncidentName { get; set; } = string.Empty; // Typhoon Egay, Fire Incident Sitio 3

        [Required]
        public string DisasterType { get; set; } = string.Empty; // Flood, Typhoon, Earthquake, Fire

        public DateTime OccurrenceDate { get; set; }

        // Impact Metrics
        public int AffectedHouseholdsCount { get; set; }
        public int DisplacedIndividualsCount { get; set; }
        public int CasualtiesCount { get; set; }

        public string EvacuationCenterStatus { get; set; } = string.Empty; // Open, Closed
        public string ReliefDistributionStatus { get; set; } = string.Empty; // Ongoing, Completed

        public string LoggedBy { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
    }
}