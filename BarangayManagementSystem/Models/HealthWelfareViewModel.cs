using System.Collections.Generic;

namespace BarangayManagementSystem.Models
{
    public class HealthInventoryItem
    {
        public string ItemName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // Medicine, Vaccine, Supplies
        public int StockAvailable { get; set; }
        public string Status { get; set; } = string.Empty; // In Stock, Low Stock, Out of Stock
    }

    public class HealthWelfarePageViewModel
    {
        // Analytic Highlight Metrics
        public int PendingConsultations { get; set; } = 8;
        public int TotalPatientsToday { get; set; } = 24;
        public string NextProgramDate { get; set; } = "May 28, 2026";
        public string NextProgramName { get; set; } = "Immunization Drive";

        // Lists
        public List<string> SystemFeatures { get; set; } = new List<string>();
        public List<HealthInventoryItem> MedicalInventory { get; set; } = new List<HealthInventoryItem>();
    }
}