using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace BarangayCMS.Areas.Staff.ViewModels
{
    public class HealthViewModel
    {
        // 🛠️ Mapped sa @item.Id para sa "History" button link
        public int Id { get; set; }

        public int HealthRecordId { get; set; }

        [Required(ErrorMessage = "Pumili ng isang Resident para sa health record na ito.")]
        [Display(Name = "Patient / Resident Name")]
        public int ResidentId { get; set; }

        // 🆕 Idinagdag para sa @item.ResidentName ng iyong table row
        [Display(Name = "Patient Name")]
        public string ResidentName { get; set; } = string.Empty;

        // --- Vital signs / anthropometrics ---
        [Range(0, 500, ErrorMessage = "Maglagay ng wastong timbang (0–500 kg).")]
        [Display(Name = "Weight (kg)")]
        public double WeightKg { get; set; }

        [Range(0, 300, ErrorMessage = "Maglagay ng wastong taas (0–300 cm).")]
        [Display(Name = "Height (cm)")]
        public double HeightCm { get; set; }

        // 🆕 Idinagdag para sa @item.BloodType display (e.g., "A+", "O-", "N/A")
        [MaxLength(5)]
        [Display(Name = "Blood Type")]
        public string BloodType { get; set; } = "N/A";

        // 🆕 Idinagdag para sa @item.HealthClassification (e.g., "Maternal", "Infant", "General Consultation")
        [MaxLength(100)]
        [Display(Name = "Classification")]
        public string HealthClassification { get; set; } = "General";

        [Required(ErrorMessage = "Ang Medical Condition ay kinakailangan.")]
        [MaxLength(150, ErrorMessage = "Hindi pwedeng lumampas sa 150 karakter.")]
        [Display(Name = "Medical Condition / Diagnosis")]
        public string MedicalCondition { get; set; } = string.Empty;

        // 🆕 Idinagdag para sa @item.IsVaccinated boolean conditional style
        [Display(Name = "Is Vaccinated?")]
        public bool IsVaccinated { get; set; } = false;

        // 🆕 Idinagdag para sa @item.LastCheckupDate.ToString("MM/dd/yyyy")
        [Required(ErrorMessage = "Ang Petsa ng Check-up ay kinakailangan.")]
        [Display(Name = "Last Check-up Date")]
        public DateTime LastCheckupDate { get; set; } = DateTime.Now;

        [MaxLength(120)]
        [Display(Name = "Attending Health Worker")]
        public string AttendingHealthWorker { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Hindi pwedeng lumampas sa 500 karakter.")]
        [Display(Name = "Remarks & Clinical Notes")]
        public string Remarks { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ang Petsa ng Pagtatala ay kinakailangan.")]
        [Display(Name = "Date Recorded")]
        public DateTime DateRecorded { get; set; } = DateTime.Now;

        // Display properties for informational use
        public string ResidentFullName { get; set; } = string.Empty;

        // --- Computed (read-only) ---
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

        // For population of Dropdown Lists in Create and Edit views
        public List<SelectListItem>? ResidentDataSource { get; set; }
    }
}