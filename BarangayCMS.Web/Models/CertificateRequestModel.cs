using System.ComponentModel.DataAnnotations;

namespace BarangayManagementSystem.Models
{
    public class CertificateRequestModel
    {
        [Required(ErrorMessage = "Full name is required")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Purpose is required")]
        public string Purpose { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact number is required")]
        [RegularExpression(@"^09\d{9}$", ErrorMessage = "Must be a valid format (09XXXXXXXXX)")]
        public string ContactNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a type of certificate")]
        public string CertificateType { get; set; } = string.Empty;
    }

    public class CertificateTrackerViewModel
    {
        public string RequestId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string CertificateType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // Pending, Processing, Ready for Release, Released
    }

    public class CertificatesPageViewModel
    {
        public CertificateRequestModel FormModel { get; set; } = new CertificateRequestModel();
        public List<CertificateTrackerViewModel> TrackedRequests { get; set; } = new List<CertificateTrackerViewModel>();
    }
}