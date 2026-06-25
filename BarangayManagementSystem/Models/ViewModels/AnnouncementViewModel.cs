namespace BarangayManagementSystem.Models.ViewModels
{
    public class AnnouncementItemViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // Event, Program, Reminder, Emergency, Health
        public string DatePosted { get; set; } = string.Empty;
    }

    public class EmergencyHotlineViewModel
    {
        public string Label { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
    }

    public class AnnouncementsPageViewModel
    {
        public List<AnnouncementItemViewModel> Announcements { get; set; } = new List<AnnouncementItemViewModel>();
        public List<EmergencyHotlineViewModel> Hotlines { get; set; } = new List<EmergencyHotlineViewModel>();
    }
}