using System;
using System.ComponentModel.DataAnnotations;

namespace BarangayCMS.Entities
{
    public class HealthRecord
    {
        [Key]
        public int HealthRecordId { get; set; } // Database Primary Key

        public int ResidentId { get; set; }

        public double WeightKg { get; set; }
        public double HeightCm { get; set; }
        public string BloodType { get; set; } = string.Empty;

        // Barangay Programs Tracking
        public bool IsVaccinated { get; set; }
        public string MedicalCondition { get; set; } = string.Empty; // Hypertension, Diabetes, Pregnant, None
        public string HealthClassification { get; set; } = string.Empty; // Infant, Senior Citizen, PWD, General

        public DateTime LastCheckupDate { get; set; }
        public string AttendingHealthWorker { get; set; } = string.Empty;

        public DateTime DateLogged { get; set; }

        // EF Core Navigation Property para sa Resident
        public Resident Resident { get; set; } = null!;
    }
}