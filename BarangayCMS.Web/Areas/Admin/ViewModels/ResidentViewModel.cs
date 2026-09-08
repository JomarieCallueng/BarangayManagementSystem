using System;
using System.ComponentModel.DataAnnotations;

namespace BarangayCMS.Web.Areas.Admin.Models
{
    public class ResidentViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ang Pangalan (First Name) ay kinakailangan.")]
        [MaxLength(100, ErrorMessage = "Hindi pwedeng lumampas sa 100 karakter.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ang Apelyido (Last Name) ay kinakailangan.")]
        [MaxLength(100, ErrorMessage = "Hindi pwedeng lumampas sa 100 karakter.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "Hindi pwedeng lumampas sa 100 karakter.")]
        [Display(Name = "Middle Name")]
        public string? MiddleName { get; set; }

        [Required(ErrorMessage = "Pumili ng Kasarian.")]
        [MaxLength(10)]
        [Display(Name = "Gender")]
        public string Gender { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ilagay ang Kaarawan.")]
        [DataType(DataType.Date)]
        [Display(Name = "Birth Date")]
        public DateTime BirthDate { get; set; } = DateTime.Today.AddYears(-18);

        [Required(ErrorMessage = "Ilagay ang Kumpletong Address.")]
        [MaxLength(255, ErrorMessage = "Hindi pwedeng lumampas sa 255 karakter.")]
        [Display(Name = "House No. & Street Address")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Pumili ng Area / Purok.")]
        [MaxLength(100)]
        [Display(Name = "Purok / Area / Sitio")]
        public string Purok { get; set; } = string.Empty;

        [Display(Name = "Contact Number")]
        [Phone(ErrorMessage = "Maling format ng numero.")]
        [MaxLength(20, ErrorMessage = "Hindi pwedeng lumampas sa 20 karakter.")]
        public string? ContactNumber { get; set; }

        [Display(Name = "Civil Status")]
        [MaxLength(50)]
        public string CivilStatus { get; set; } = "Single";

        [MaxLength(100, ErrorMessage = "Hindi pwedeng lumampas sa 100 karakter.")]
        [Display(Name = "Occupation")]
        public string? Occupation { get; set; }

        // 🗳️ VOTER STATUS (Check/Uncheck)
        [Display(Name = "Registered Barangay Voter?")]
        public bool IsVoter { get; set; }

        // 🧑‍🦽 PWD STATUS (Person With Disability) — naka-store sa database
        [Display(Name = "Person With Disability (PWD)?")]
        public bool IsPwd { get; set; }

        // 📅 DATE REGISTERED
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        [Display(Name = "Date Registered")]
        public DateTime DateRegistered { get; set; } = DateTime.Now;

        // 🧮 AGE COMPUTATION
        public int Age
        {
            get
            {
                var today = DateTime.Today;
                var age = today.Year - BirthDate.Year;
                if (BirthDate.Date > today.AddYears(-age)) age--;
                return Math.Max(0, age);
            }
        }

        // 👤 FULL NAME HELPER
        public string FullName => string.IsNullOrWhiteSpace(MiddleName)
            ? $"{LastName}, {FirstName}"
            : $"{LastName}, {FirstName} {MiddleName}".Trim();

        // ==========================================================
        // 🧮 AUTOMATIC DEMOGRAPHIC CLASSIFICATIONS (derived from Age)
        // Batay sa pamantayan ng klasipikasyon ng edad ng sistema:
        //   Child/Minor : 0 – 14
        //   Youth (SK)  : 15 – 30 (RA 8044 / Sangguniang Kabataan)
        //   Adult       : 31 – 59
        //   Senior      : 60 pataas (RA 9994)
        // ==========================================================

        // 🧒 Youth classification — automatic base sa edad
        public bool IsYouth => Age >= 15 && Age <= 30;

        // 👴 Senior Citizen status — automatic base sa edad
        public bool IsSeniorCitizen => Age >= 60;

        // 🏷️ Age group label para sa profile display at analytics
        public string AgeGroup
        {
            get
            {
                if (Age < 15) return "Child (0-14)";
                if (Age <= 30) return "Youth (15-30)";
                if (Age <= 59) return "Adult (31-59)";
                return "Senior (60+)";
            }
        }

        // 🏷️ Text labels para sa resident profile display
        public string YouthClassification => IsYouth ? "Youth (SK)" : "Not Youth";
        public string SeniorClassification => IsSeniorCitizen ? "Senior Citizen" : "Not Senior";
        public string PwdClassification => IsPwd ? "PWD" : "Non-PWD";
    }
}