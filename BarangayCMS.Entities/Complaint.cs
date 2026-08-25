using System;
using System.ComponentModel.DataAnnotations;

namespace BarangayCMS.Entities
{
    public class Complaint
    {
        [Key]
        public int ComplaintId { get; set; } // Binago mula Id patungong ComplaintId para sa EF Core convention

        public string CaseNumber { get; set; } = string.Empty;

        // Complainant (Maaaring itugma sa ResidentId mo)
        public int ResidentId { get; set; }
        public string ComplainantName { get; set; } = string.Empty;
        public string ComplainantContact { get; set; } = string.Empty;

        // Respondent
        public string RespondentName { get; set; } = string.Empty;

        public string IncidentLocation { get; set; } = string.Empty;
        public DateTime IncidentDate { get; set; }
        public string Details { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending";
        public string Remarks { get; set; } = string.Empty;
        public string ActionTaken { get; set; } = string.Empty;

        public DateTime DateSubmitted { get; set; } // Katumbas ng CreatedDate mo kanina

        // EF Core Navigation Property
        public Resident Resident { get; set; } = null!;
    }
}