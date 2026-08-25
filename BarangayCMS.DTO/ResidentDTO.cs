using System;
using System.ComponentModel.DataAnnotations;

namespace BarangayCMS.DTO
{
    public class ResidentDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kailangan ang First Name.")]
        public string FirstName { get; set; } = string.Empty;

        public string MiddleName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kailangan ang Last Name.")]
        public string LastName { get; set; } = string.Empty;

        public string Suffix { get; set; } = string.Empty; // Jr., III, etc.

        public DateTime BirthDate { get; set; }

        public string Gender { get; set; } = string.Empty;
        public string CivilStatus { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kailangan ang Contact Number.")]
        [RegularExpression(@"^09\d{9}$", ErrorMessage = "Ang contact number ay dapat 11 digits at nagsisimula sa '09' (hal. 09123456789).")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "Eksaktong 11 digits ang kailangan.")]
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