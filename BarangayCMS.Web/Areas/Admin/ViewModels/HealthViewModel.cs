using System;
using System.ComponentModel.DataAnnotations;

namespace BarangayCMS.Web.Areas.Admin.Models
{
    public class HealthViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ang residente ay kinakailangan.")]
        [Display(Name = "Residente")]
        public int ResidentId { get; set; }

        [Display(Name = "Pangalan ng Residente")]
        public string ResidentName { get; set; } = string.Empty;

        // --- Vital signs / anthropometrics -----------------------------------
        [Range(0, 500, ErrorMessage = "Maglagay ng wastong timbang (0–500 kg).")]
        [Display(Name = "Timbang (kg)")]
        public double WeightKg { get; set; }

        [Range(0, 300, ErrorMessage = "Maglagay ng wastong taas (0–300 cm).")]
        [Display(Name = "Taas (cm)")]
        public double HeightCm { get; set; }

        [MaxLength(5)]
        [Display(Name = "Blood Type")]
        public string BloodType { get; set; } = "N/A";

        // --- Program tracking -------------------------------------------------
        [MaxLength(100)]
        [Display(Name = "Klasipikasyon")]
        public string HealthClassification { get; set; } = "General";

        [Display(Name = "Nabakunahan na?")]
        public bool IsVaccinated { get; set; }

        [Required(ErrorMessage = "Ang medikal na kondisyon ay kinakailangan.")]
        [MaxLength(150, ErrorMessage = "Hindi pwedeng lumagpas sa 150 characters.")]
        [Display(Name = "Medikal na Kondisyon / Diagnosis")]
        public string MedicalCondition { get; set; } = string.Empty;

        [MaxLength(120)]
        [Display(Name = "Attending Health Worker")]
        public string AttendingHealthWorker { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Hindi pwedeng lumagpas sa 500 characters.")]
        [Display(Name = "Mga Tala / Remarks / Reseta")]
        public string Remarks { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Huling Check-up")]
        public DateTime LastCheckupDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Ang petsa ay kinakailangan.")]
        [DataType(DataType.Date)]
        [Display(Name = "Petsa ng Pagtatala")]
        public DateTime DateRecorded { get; set; } = DateTime.Now;

        // --- Computed (read-only) --------------------------------------------
        public double Bmi
        {
            get
            {
                if (HeightCm <= 0 || WeightKg <= 0) return 0;
                var m = HeightCm / 100.0;
                return Math.Round(WeightKg / (m * m), 1);
            }
        }

        public string BmiCategory
        {
            get
            {
                var b = Bmi;
                if (b <= 0) return "—";
                if (b < 18.5) return "Underweight";
                if (b < 25) return "Normal";
                if (b < 30) return "Overweight";
                return "Obese";
            }
        }
    }
}
