using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BarangayCMS.Entities
{
    public class Resident
    {
        [Key]
        public int ResidentId { get; set; } // Database Primary Key

        [Required]
        public string FirstName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;
        public string Suffix { get; set; } = string.Empty;

        public DateTime BirthDate { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string CivilStatus { get; set; } = string.Empty;

        public string ContactNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string HouseNumber { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string SitioPurok { get; set; } = string.Empty;

        public bool IsVoter { get; set; }

        
        public bool IsResident { get; set; } = true; // Default na active resident
        public DateTime CreatedAt { get; set; }

        // --- EF Core Navigation Properties (Opsyonal pero maganda para sa ugnayan ng tables) ---
        public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
        public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();
        public ICollection<HealthRecord> HealthRecords { get; set; } = new List<HealthRecord>();
    }
}