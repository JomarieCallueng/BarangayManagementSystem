using System;

namespace BarangayCMS.DTO
{
    public class ResidentDTO
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Suffix { get; set; } = string.Empty; // Jr., III, etc.
        public string FullName => $"{FirstName} {(string.IsNullOrEmpty(MiddleName) ? "" : MiddleName + " ")}{LastName} {Suffix}".Trim();

        public DateTime BirthDate { get; set; }
        public int Age => DateTime.Today.Year - BirthDate.Year - (DateTime.Today.DayOfYear < BirthDate.DayOfYear ? 1 : 0);
        public string Gender { get; set; } = string.Empty;
        public string CivilStatus { get; set; } = string.Empty;

        public string ContactNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // Address Details within the Barangay
        public string HouseNumber { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string SitioPurok { get; set; } = string.Empty;
        public string FullAddress => $"{HouseNumber} {Street}, {SitioPurok}".Trim();

        public bool IsVoter { get; set; }
        public bool IsResident { get; set; } // Active or Moved Out
        public DateTime CreatedAt { get; set; }
    }
}
