using System;

namespace BarangayCMS.DTO
{
    public class DisasterDTO
    {
        public int Id { get; set; }
        public string IncidentName { get; set; } = string.Empty; // Typhoon Egay, Fire Incident Sitio 3
        public string DisasterType { get; set; } = string.Empty; // Flood, Typhoon, Earthquake, Fire
        public DateTime OccurrenceDate { get; set; }

        // Impact Metrics
        public int AffectedHouseholdsCount { get; set; }
        public int DisplacedIndividualsCount { get; set; }
        public int CasualtiesCount { get; set; }

        public string EvacuationCenterStatus { get; set; } = string.Empty; // Open, Closed
        public string ReliefDistributionStatus { get; set; } = string.Empty; // ongoing, completed
        public string LoggedBy { get; set; } = string.Empty;
    }
}
