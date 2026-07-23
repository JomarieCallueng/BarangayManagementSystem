using System;

namespace BarangayCMS.DTO
{
    public class HealthDTO
    {
        public int Id { get; set; }
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = string.Empty;

        public double WeightKg { get; set; }
        public double HeightCm { get; set; }
        public string BloodType { get; set; } = string.Empty;

        // Barangay Programs Tracking
        public bool IsVaccinated { get; set; }
        public string MedicalCondition { get; set; } = string.Empty; // Hypertension, Diabetes, Pregnant, None
        public string HealthClassification { get; set; } = string.Empty; // Infant, Senior Citizen, PWD, General

        public DateTime LastCheckupDate { get; set; }
        public string AttendingHealthWorker { get; set; } = string.Empty;
    }
}
