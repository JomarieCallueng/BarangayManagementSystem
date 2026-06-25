using Microsoft.AspNetCore.Mvc;
using BarangayManagementSystem.Models;
using System.Collections.Generic;

namespace BarangayManagementSystem.Controllers
{
    public class ModulesController : Controller
    {
        // ==========================================
        // MODULE 1: RESIDENT INFORMATION MANAGEMENT
        // ==========================================

        // GET: Modules/ResidentInformation
        public IActionResult ResidentInformation()
        {
            var residents = new List<ResidentViewModel>
            {
                new ResidentViewModel { Name = "Juan Dela Cruz", Address = "Purok 1", IsActive = true },
                new ResidentViewModel { Name = "Maria Santos", Address = "Purok 2", IsActive = true },
                new ResidentViewModel { Name = "Pedro Reyes", Address = "Purok 3", IsActive = true },
                new ResidentViewModel { Name = "Ana Garcia", Address = "Purok 1", IsActive = true },
                new ResidentViewModel { Name = "Roberto Lim", Address = "Purok 4", IsActive = false },
                new ResidentViewModel { Name = "Luz Manalo", Address = "Purok 2", IsActive = true }
            };

            return View(residents);
        }

        // ==========================================
        // MODULE 2: CERTIFICATES & PERMITS MANAGEMENT
        // ==========================================

        // GET: Modules/CertificatesPermits
        public IActionResult CertificatesPermits()
        {
            var viewModel = new CertificatesPageViewModel
            {
                TrackedRequests = GetMockRequests()
            };
            return View(viewModel);
        }

        // POST: Modules/CertificatesPermits
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CertificatesPermits(CertificatesPageViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Process request data here (e.g., save to a database)
                TempData["SuccessMessage"] = "Certificate request submitted successfully!";
                return RedirectToAction(nameof(CertificatesPermits));
            }

            // Reload records if validation checks fail
            model.TrackedRequests = GetMockRequests();
            return View(model);
        }

        private List<CertificateTrackerViewModel> GetMockRequests()
        {
            return new List<CertificateTrackerViewModel>
            {
                new CertificateTrackerViewModel { RequestId = "REQ-001", Name = "Juan Dela Cruz", CertificateType = "Barangay Clearance", Status = "Ready for Release" },
                new CertificateTrackerViewModel { RequestId = "REQ-002", Name = "Maria Santos", CertificateType = "Residency Certificate", Status = "Processing" },
                new CertificateTrackerViewModel { RequestId = "REQ-003", Name = "Pedro Reyes", CertificateType = "Business Permit", Status = "Pending" },
                new CertificateTrackerViewModel { RequestId = "REQ-004", Name = "Ana Garcia", CertificateType = "Good Moral Certificate", Status = "Released" }
            };
        }

        // ==========================================
        // MODULE 3: COMPLAINTS & DISPUTES RESOLUTION
        // ==========================================

        // GET: Modules/ComplaintsDisputes
        public IActionResult ComplaintsDisputes()
        {
            var viewModel = new ComplaintsPageViewModel
            {
                TrackedCases = GetMockCases()
            };
            return View(viewModel);
        }

        // POST: Modules/ComplaintsDisputes
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ComplaintsDisputes(ComplaintsPageViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Process case filing details here (e.g., save to database logs)
                TempData["SuccessMessage"] = "Incident complaint recorded successfully!";
                return RedirectToAction(nameof(ComplaintsDisputes));
            }

            // Reload records if validation checks fail
            model.TrackedCases = GetMockCases();
            return View(model);
        }

        private List<CaseTrackerViewModel> GetMockCases()
        {
            return new List<CaseTrackerViewModel>
            {
                new CaseTrackerViewModel { CaseId = "001", ComplaintType = "Noise Complaint", Complainant = "Juan Dela Cruz", DateFiled = "2026-05-10", Status = "Ongoing" },
                new CaseTrackerViewModel { CaseId = "002", ComplaintType = "Property Dispute", Complainant = "Maria Santos", DateFiled = "2026-05-08", Status = "Resolved" },
                new CaseTrackerViewModel { CaseId = "003", ComplaintType = "Disturbance", Complainant = "Pedro Reyes", DateFiled = "2026-05-05", Status = "Pending" },
                new CaseTrackerViewModel { CaseId = "004", ComplaintType = "Illegal Dumping", Complainant = "Ana Garcia", DateFiled = "2026-05-03", Status = "Ongoing" }
            };
        }

        // ==========================================
        // MODULE 4: ANNOUNCEMENTS & NOTIFICATIONS
        // ==========================================

        // GET: Modules/Announcements
        public IActionResult Announcements()
        {
            var viewModel = new AnnouncementsPageViewModel
            {
                Announcements = new List<AnnouncementItemViewModel>
                {
                    new AnnouncementItemViewModel { Title = "Barangay Assembly on Friday", Description = "All residents are invited to attend the quarterly barangay assembly on May 23, 2026 at 9:00 AM at the Barangay Hall.", Category = "Event", DatePosted = "May 20, 2026" },
                    new AnnouncementItemViewModel { Title = "Feeding Program Registration", Description = "Parents with children ages 0-5 may register for the feeding program at the Barangay Health Center. Slots are limited.", Category = "Program", DatePosted = "May 19, 2026" },
                    new AnnouncementItemViewModel { Title = "Curfew Reminder", Description = "Minors below 18 years old are reminded of the curfew from 10:00 PM to 5:00 AM as per barangay ordinance.", Category = "Reminder", DatePosted = "May 17, 2026" },
                    new AnnouncementItemViewModel { Title = "Flood Warning Advisory", Description = "Residents in low-lying areas are advised to prepare for possible flooding due to continuous rainfall. Please monitor updates.", Category = "Emergency", DatePosted = "May 16, 2026" },
                    new AnnouncementItemViewModel { Title = "Free Medical Check-up", Description = "A free medical check-up will be conducted on May 25 at the Barangay Health Center. Bring your barangay ID.", Category = "Health", DatePosted = "May 15, 2026" }
                },
                Hotlines = new List<EmergencyHotlineViewModel>
                {
                    new EmergencyHotlineViewModel { Label = "Barangay Office", Number = "0912-345-6789" },
                    new EmergencyHotlineViewModel { Label = "Fire Station", Number = "160" },
                    new EmergencyHotlineViewModel { Label = "Police Station", Number = "117" },
                    new EmergencyHotlineViewModel { Label = "Rescue Team", Number = "911" }
                }
            };

            return View(viewModel);
        }

        // ==========================================
        // MODULE 5: BUDGET & PROJECT MONITORING
        // ==========================================

        // GET: Modules/BudgetProjects
        public IActionResult BudgetProjects()
        {
            var viewModel = new BudgetProjectsPageViewModel
            {
                TotalBudgetFormatted = "₱1.05M",
                UtilizedAmountFormatted = "₱230,000",
                UtilizationPercentage = 22,
                ActiveProjectsCount = 2,
                CompletedProjectsCount = 1,
                SystemFeatures = new List<string>
                {
                    "Budget Allocation Tracking",
                    "Expense Monitoring",
                    "Project Progress Tracking",
                    "Financial Reports",
                    "Fund Utilization Reports",
                    "Procurement Monitoring"
                },
                Allocations = new List<BudgetAllocationSummary>
                {
                    new BudgetAllocationSummary { Category = "Infrastructure", Amount = 500000 },
                    new BudgetAllocationSummary { Category = "Health Programs", Amount = 200000 },
                    new BudgetAllocationSummary { Category = "Disaster Preparedness", Amount = 150000 },
                    new BudgetAllocationSummary { Category = "Environmental Projects", Amount = 100000 }
                },
                Projects = new List<ProjectProgressItem>
                {
                    new ProjectProgressItem { Name = "Road Repair", Budget = 150000, ProgressPercentage = 80, Status = "Ongoing" },
                    new ProjectProgressItem { Name = "Feeding Program", Budget = 50000, ProgressPercentage = 100, Status = "Completed" },
                    new ProjectProgressItem { Name = "Drainage Project", Budget = 200000, ProgressPercentage = 30, Status = "Ongoing" },
                    new ProjectProgressItem { Name = "Street Lighting", Budget = 80000, ProgressPercentage = 0, Status = "Pending" }
                }
            };

            return View(viewModel);
        }

        // ==========================================
        // MODULE 6: DISASTER PREPAREDNESS MANAGEMENT
        // ==========================================

        // GET: Modules/DisasterPreparedness
        public IActionResult DisasterPreparedness()
        {
            var viewModel = new DisasterPreparednessPageViewModel
            {
                ActiveCentersCount = 3,
                AlertLevel = "Normal (Level 0)",
                WeatherCondition = "Sunny / Clear",
                AvailableResponders = 15,
                SystemFeatures = new List<string>
                {
                    "Evacuation Center Management",
                    "Emergency Alert System",
                    "Relief Goods Inventory",
                    "Disaster Response Logistics",
                    "Incident Reporting",
                    "Volunteer Management"
                },
                EvacuationCenters = new List<EvacuationCenterViewModel>
                {
                    new EvacuationCenterViewModel { CenterName = "Barangay Covered Court", Location = "Purok 2 (Main Street)", Capacity = 500, CurrentOccupants = 0, Status = "Open" },
                    new EvacuationCenterViewModel { CenterName = "Elementary School Gym", Location = "Purok 1", Capacity = 300, CurrentOccupants = 0, Status = "Open" },
                    new EvacuationCenterViewModel { CenterName = "Barangay Health Annex", Location = "Purok 4", Capacity = 100, CurrentOccupants = 0, Status = "Open" },
                    new EvacuationCenterViewModel { CenterName = "Multipurpose Hall", Location = "Purok 3", Capacity = 150, CurrentOccupants = 0, Status = "Closed" }
                }
            };

            return View(viewModel);
        }

        // ==========================================
        // MODULE 7: HEALTH & WELFARE MANAGEMENT
        // ==========================================

        // GET: Modules/HealthWelfare
        public IActionResult HealthWelfare()
        {
            var viewModel = new HealthWelfarePageViewModel
            {
                PendingConsultations = 8,
                TotalPatientsToday = 24,
                NextProgramDate = "May 28, 2026",
                NextProgramName = "Immunization Drive",
                SystemFeatures = new List<string>
                {
                    "Patient Records Management",
                    "Medical Consultation Logs",
                    "Medicine Inventory Tracking",
                    "Health Programs Scheduling",
                    "Barangay Nutrition Tracking",
                    "Maternal & Child Health Care"
                },
                MedicalInventory = new List<HealthInventoryItem>
                {
                    new HealthInventoryItem { ItemName = "Paracetamol 500mg", Category = "Medicine", StockAvailable = 1200, Status = "In Stock" },
                    new HealthInventoryItem { ItemName = "Amoxicillin 500mg", Category = "Medicine", StockAvailable = 45, Status = "Low Stock" },
                    new HealthInventoryItem { ItemName = "Vitamin C Syrup", Category = "Supplies", StockAvailable = 150, Status = "In Stock" },
                    new HealthInventoryItem { ItemName = "BCG Vaccine Vial", Category = "Vaccine", StockAvailable = 0, Status = "Out of Stock" }
                }
            };

            return View(viewModel);
        }

        // ==========================================
        // MODULE 8: ENVIRONMENTAL MANAGEMENT
        // ==========================================

        // GET: Modules/EnvironmentalManagement
        public IActionResult EnvironmentalManagement()
        {
            var viewModel = new EnvironmentalManagementPageViewModel
            {
                ActiveCleanupsCount = 2,
                MonthlyWasteCollected = "1.2 Tons",
                MonitoredAreasCount = 4,
                EcoVolunteersCount = 28,
                SystemFeatures = new List<string>
                {
                    "Waste Collection Scheduling",
                    "Community Cleanup Drives",
                    "Batas Kalikasan Compliance Tracking",
                    "Eco-Volunteer Roster Management",
                    "Sanitation & Drainage Monitoring",
                    "Material Recovery Facility (MRF) Logs"
                },
                CollectionSchedules = new List<WasteCollectionScheduleItem>
                {
                    new WasteCollectionScheduleItem { ZoneOrPurok = "Purok 1 & Purok 2", WasteType = "Biodegradable", CollectionDay = "Monday, Wednesday", Status = "Scheduled" },
                    new WasteCollectionScheduleItem { ZoneOrPurok = "Purok 3 & Purok 4", WasteType = "Biodegradable", CollectionDay = "Tuesday, Thursday", Status = "Scheduled" },
                    new WasteCollectionScheduleItem { ZoneOrPurok = "Entire Barangay Village", WasteType = "Recyclable / Plastic", CollectionDay = "Friday", Status = "Scheduled" },
                    new WasteCollectionScheduleItem { ZoneOrPurok = "Commercial Zone (Main Road)", WasteType = "Residual / Non-Bio", CollectionDay = "Saturday", Status = "Completed" }
                }
            };

            return View(viewModel);
        }
    }
}