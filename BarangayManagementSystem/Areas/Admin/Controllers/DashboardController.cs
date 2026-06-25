using Microsoft.AspNetCore.Mvc;

namespace BarangayManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        // URL: /Admin/Dashboard
        public IActionResult Index()
        {
            // Dito mapupunta ang admin pagkatapos mag-login
            return View();
        }


        // Idagdag ito sa loob ng iyong DashboardController.cs
        [HttpGet]
        public IActionResult Residents()
        {
            // Dito muna natin ihahanda ang sample data para sa table ng mga residente
            ViewBag.ResidentsList = new List<dynamic>
    {
        new { Id = "001", Name = "Juan Dela Cruz", Address = "Purok 1", Birthdate = "1985-03-12", CivilStatus = "Married", Status = "Active" },
        new { Id = "002", Name = "Maria Santos", Address = "Purok 2", Birthdate = "1990-07-22", CivilStatus = "Single", Status = "Active" },
        new { Id = "003", Name = "Pedro Reyes", Address = "Purok 3", Birthdate = "1978-11-05", CivilStatus = "Widowed", Status = "Active" },
        new { Id = "004", Name = "Ana Garcia", Address = "Purok 1", Birthdate = "2001-02-14", CivilStatus = "Single", Status = "Active" },
        new { Id = "005", Name = "Roberto Lim", Address = "Purok 4", Birthdate = "1965-09-30", CivilStatus = "Married", Status = "Inactive" },
        new { Id = "006", Name = "Luz Manalo", Address = "Purok 2", Birthdate = "1995-06-18", CivilStatus = "Single", Status = "Active" },
        new { Id = "007", Name = "Carlos Cruz", Address = "Purok 3", Birthdate = "1982-04-25", CivilStatus = "Married", Status = "Active" },
        new { Id = "008", Name = "Rosa Villanueva", Address = "Purok 1", Birthdate = "1953-12-01", CivilStatus = "Widowed", Status = "Active" }
    };

            return View();
        }


        // Idagdag ito sa loob ng DashboardController.cs sa ilalim ng Residents action
        [HttpGet]
        public IActionResult Complaints()
        {
            // Mock Data para sa listahan ng mga reklamo / dispute cases
            ViewBag.ComplaintsList = new List<dynamic>
    {
        new { CaseId = "C-001", Complaint = "Noise Complaint", Complainant = "Juan Dela Cruz", Respondent = "Ricardo Ocampo", AssignedOfficer = "—", DateFiled = "2026-05-10", Status = "Pending" },
        new { CaseId = "C-002", Complaint = "Property Dispute", Complainant = "Maria Santos", Respondent = "Ernesto Bautista", AssignedOfficer = "Kagawad Ramos", DateFiled = "2026-05-08", Status = "Resolved" },
        new { CaseId = "C-003", Complaint = "Disturbance", Complainant = "Pedro Reyes", Respondent = "Unknown", AssignedOfficer = "Kagawad dela Vega", DateFiled = "2026-05-05", Status = "Ongoing" },
        new { CaseId = "C-004", Complaint = "Illegal Dumping", Complainant = "Ana Garcia", Respondent = "Neighbor", AssignedOfficer = "Kagawad Ramos", DateFiled = "2026-05-03", Status = "Ongoing" },
        new { CaseId = "C-005", Complaint = "Domestic Issue", Complainant = "Luz Manalo", Respondent = "Confidential", AssignedOfficer = "—", DateFiled = "2026-05-01", Status = "Pending" }
    };

            return View();
        }


        // Idagdag ito sa loob ng DashboardController.cs sa ilalim ng Complaints action
        [HttpGet]
        public IActionResult Certificates()
        {
            // Mock Data para sa Certificate Requests Table
            ViewBag.CertificateRequests = new List<dynamic>
    {
        new { RequestId = "REQ-001", Name = "Juan Dela Cruz", Type = "Barangay Clearance", Purpose = "Employment", Fee = "₱50", Payment = "Paid", Status = "Ready for Release" },
        new { RequestId = "REQ-002", Name = "Maria Santos", Type = "Residency Certificate", Purpose = "Scholarship", Fee = "₱50", Payment = "Paid", Status = "Processing" },
        new { RequestId = "REQ-003", Name = "Pedro Reyes", Type = "Business Permit", Purpose = "Business", Fee = "₱200", Payment = "Pending", Status = "Pending" },
        new { RequestId = "REQ-004", Name = "Ana Garcia", Type = "Good Moral Certificate", Purpose = "School", Fee = "₱50", Payment = "Paid", Status = "Released" }
    };

            return View();
        }
    }
}