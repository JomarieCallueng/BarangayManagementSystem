using System.ComponentModel.DataAnnotations;

namespace BarangayManagementSystem.Models
{
    public class ComplaintFormModel
    {
        [Required(ErrorMessage = "Your full name is required")]
        public string ComplainantName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Respondent name or group involved is required")]
        public string RespondentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please specify the type of complaint")]
        public string ComplaintType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please provide an incident description")]
        public string Description { get; set; } = string.Empty;
    }

    public class CaseTrackerViewModel
    {
        public string CaseId { get; set; } = string.Empty;
        public string ComplaintType { get; set; } = string.Empty;
        public string Complainant { get; set; } = string.Empty;
        public string DateFiled { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // Ongoing, Resolved, Pending
    }

    public class ComplaintsPageViewModel
    {
        public ComplaintFormModel FormModel { get; set; } = new ComplaintFormModel();
        public List<CaseTrackerViewModel> TrackedCases { get; set; } = new List<CaseTrackerViewModel>();
    }
}