using System.Collections.Generic;

namespace BarangayManagementSystem.Models.ViewModels
{
    public class EvacuationCenterViewModel
    {
        public string CenterName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int CurrentOccupants { get; set; }
        public string Status { get; set; } = string.Empty; // Open, Full, Closed
    }

    public class DisasterPreparednessPageViewModel
    {
        // Analytic Counter Cards Metrics
        public int ActiveCentersCount { get; set; } = 3;
        public string AlertLevel { get; set; } = "Normal (Level 0)";
        public string WeatherCondition { get; set; } = "Sunny / Clear";
        public int AvailableResponders { get; set; } = 15;

        // Structured feature lists & table models
        public List<string> SystemFeatures { get; set; } = new List<string>();
        public List<EvacuationCenterViewModel> EvacuationCenters { get; set; } = new List<EvacuationCenterViewModel>();
    }
}