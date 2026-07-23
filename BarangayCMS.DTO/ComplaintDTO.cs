using System;

namespace BarangayCMS.DTO
{
    public class ComplaintDTO
    {
        public int Id { get; set; }
        public string CaseNumber { get; set; } = string.Empty; // Halimbawa: BLOTTER-2026-0001

        // Complainant (Pwedeng Registered Resident o Walk-in)
        public int? ComplainantResidentId { get; set; }
        public string ComplainantName { get; set; } = string.Empty;
        public string ComplainantContact { get; set; } = string.Empty;

        // Respondent (Ang inirereklamo)
        public int? RespondentResidentId { get; set; }
        public string RespondentName { get; set; } = string.Empty;

        public string IncidentLocation { get; set; } = string.Empty;
        public DateTime IncidentDate { get; set; }
        public string Details { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending"; // Pending, Scheduled for Mediation, Resolved, Dismissed, Referred to Court
        public string Remarks { get; set; } = string.Empty;
        public string ActionTaken { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; }
    }
}
