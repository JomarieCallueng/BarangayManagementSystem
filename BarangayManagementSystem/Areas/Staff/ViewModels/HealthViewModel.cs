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

        [MaxLength(500, ErrorMessage = "Hindi pwedeng lumampas sa 500 karakter.")]
        [Display(Name = "Remarks & Clinical Notes")]
        public string Remarks { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ang Petsa ng Pagtatala ay kinakailangan.")]
        [Display(Name = "Date Recorded")]
        public DateTime DateRecorded { get; set; } = DateTime.Now;

        // Display properties for informational use
        public string ResidentFullName { get; set; } = string.Empty;

        // For population of Dropdown Lists in Create and Edit views
        public List<SelectListItem>? ResidentDataSource { get; set; }
    }
}