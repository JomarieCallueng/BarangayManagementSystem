using System.Collections.Generic;

namespace BarangayManagementSystem.Models.ViewModels
{
    public class WasteCollectionScheduleItem
    {
        public string ZoneOrPurok { get; set; } = string.Empty;
        public string WasteType { get; set; } = string.Empty; // Biodegradable, Non-Biodegradable, Recyclable
        public string CollectionDay { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // Scheduled, Completed, Delayed
    }

    public class EnvironmentalManagementPageViewModel
    {
        // Analytic Highlight Metrics
        public int ActiveCleanupsCount { get; set; } = 2;
        public string MonthlyWasteCollected { get; set; } = "1.2 Tons";
        public int MonitoredAreasCount { get; set; } = 4;
        public int EcoVolunteersCount { get; set; } = 28;

        // Lists
        public List<string> SystemFeatures { get; set; } = new List<string>();
        public List<WasteCollectionScheduleItem> CollectionSchedules { get; set; } = new List<WasteCollectionScheduleItem>();
    }
}