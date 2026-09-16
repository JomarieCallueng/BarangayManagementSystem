using System;
using System.Linq;
using System.Threading.Tasks;
using BarangayCMS.DAL.Context;
using BarangayCMS.Entities;
using Microsoft.EntityFrameworkCore;

namespace BarangayCMS.Web.Data
{
    /// <summary>
    /// Idempotent na demo-data seeder. Pinupunan nito ang bawat module ng
    /// sample na datos gamit LAMANG ang tatlong pangalan:
    ///   • Jomarie Callueng
    ///   • Mark Dave Casao Cardenas
    ///   • Klarence Alfred Z. Villar
    ///
    /// Ligtas patakbuhin nang paulit-ulit — bawat record ay may guard check
    /// kaya hindi nagdudobol kahit tumakbo ito sa bawat startup.
    /// </summary>
    public static class DbSeeder
    {
        // Ang tatlong pinapayagang pangalan.
        private const string NAME_JOMARIE = "Jomarie Callueng";
        private const string NAME_MARKDAVE = "Mark Dave Casao Cardenas";
        private const string NAME_KLARENCE = "Klarence Alfred Z. Villar";

        public static async Task SeedDemoDataAsync(ApplicationDbContext db)
        {
            await SeedSystemSettingAsync(db);
            await SeedResidentsAsync(db);
            await SeedCertificateTypesAsync(db);
            await SeedCertificateRequirementsAsync(db);
            await SeedBarangayOfficialsAsync(db);
            await SeedHealthRecordsAsync(db);
            await SeedCertificatesAsync(db);
            await SeedComplaintsAsync(db);
            await SeedAnnouncementsAsync(db);
            await SeedBudgetsAsync(db);
            await SeedDisastersAsync(db);
            await SeedEnvironmentAsync(db);
            await SeedProjectsAsync(db);
            await SeedSmsAlertsAsync(db);

            await db.SaveChangesAsync();
        }

        // ---------------------------------------------------------------
        // System Setting
        // ---------------------------------------------------------------
        private static async Task SeedSystemSettingAsync(ApplicationDbContext db)
        {
            if (!await db.SystemSettings.AnyAsync())
            {
                db.SystemSettings.Add(new SystemSetting
                {
                    BarangayName = "Barangay Tatalon",
                    CityMunicipality = "Quezon City"
                });
            }
        }

        // ---------------------------------------------------------------
        // Residents — ang tatlong pangalan bilang aktibong residente
        // ---------------------------------------------------------------
        private static async Task SeedResidentsAsync(ApplicationDbContext db)
        {
            await EnsureResidentAsync(db, "Jomarie", "", "Callueng", "Male", "Single",
                "09171234501", new DateTime(1999, 3, 12), "12", "Ohio St.", "Chicago / Ohio Area", true, false);

            await EnsureResidentAsync(db, "Mark Dave", "Casao", "Cardenas", "Male", "Single",
                "09171234502", new DateTime(1998, 7, 25), "45", "Kalasag St.", "Kalasag Area", true, false);

            await EnsureResidentAsync(db, "Klarence Alfred", "Z.", "Villar", "Male", "Single",
                "09171234503", new DateTime(2000, 11, 5), "7", "Tagalog St.", "Tagalog Area", true, true);
        }

        private static async Task EnsureResidentAsync(ApplicationDbContext db,
            string first, string middle, string last, string gender, string civil,
            string contact, DateTime birth, string house, string street, string purok,
            bool voter, bool pwd)
        {
            var existing = await db.Residents
                .FirstOrDefaultAsync(r => r.FirstName == first && r.LastName == last);

            if (existing == null)
            {
                db.Residents.Add(new Resident
                {
                    FirstName = first,
                    MiddleName = middle,
                    LastName = last,
                    Gender = gender,
                    CivilStatus = civil,
                    ContactNumber = contact,
                    BirthDate = birth,
                    HouseNumber = house,
                    Street = street,
                    SitioPurok = purok,
                    IsVoter = voter,
                    IsPwd = pwd,
                    IsResident = true,
                    CreatedAt = DateTime.Now
                });
                // I-save agad para magkaroon ng ResidentId ang mga sumunod na module.
                await db.SaveChangesAsync();
            }
        }

        private static async Task<int> ResidentIdAsync(ApplicationDbContext db, string last)
        {
            var r = await db.Residents.FirstOrDefaultAsync(x => x.LastName == last);
            return r?.ResidentId ?? 0;
        }

        // ---------------------------------------------------------------
        // Certificate Types
        // ---------------------------------------------------------------
        private static async Task SeedCertificateTypesAsync(ApplicationDbContext db)
        {
            await EnsureCertTypeAsync(db, "Barangay Clearance", 50.00m);
            await EnsureCertTypeAsync(db, "Certificate of Indigency", 0.00m);
            await EnsureCertTypeAsync(db, "Certificate of Residency", 30.00m);
            await EnsureCertTypeAsync(db, "Business Permit Clearance", 100.00m);
        }

        private static async Task EnsureCertTypeAsync(ApplicationDbContext db, string name, decimal price)
        {
            if (!await db.CertificateTypes.AnyAsync(c => c.CertificateName == name))
            {
                db.CertificateTypes.Add(new CertificateType { CertificateName = name, Price = price });
            }
        }

        // ---------------------------------------------------------------
        // Certificate Requirements (dynamic, per certificate type)
        // ---------------------------------------------------------------
        private static async Task SeedCertificateRequirementsAsync(ApplicationDbContext db)
        {
            // Kailangan munang may ID ang bawat CertificateType bago i-seed
            // ang kani-kanilang requirements.
            await db.SaveChangesAsync();

            // name, isRequired, order
            await EnsureRequirementsAsync(db, "Barangay Clearance", new (string, string?, bool, int)[]
            {
                ("Valid Government ID", "Any government-issued photo ID.", true, 1),
                ("Community Tax Certificate / Cedula", "Latest cedula for the current year.", true, 2),
                ("Proof of Residency", "Utility bill or similar, if requested.", false, 3),
            });

            await EnsureRequirementsAsync(db, "Certificate of Indigency", new (string, string?, bool, int)[]
            {
                ("Valid Government ID", "Any government-issued photo ID.", true, 1),
                ("Proof of Indigency", "Barangay assessment or endorsement.", true, 2),
                ("Supporting Document", "Medical, school, or financial document, if applicable.", false, 3),
            });

            await EnsureRequirementsAsync(db, "Certificate of Residency", new (string, string?, bool, int)[]
            {
                ("Valid Government ID", "Any government-issued photo ID.", true, 1),
                ("Proof of Residency", "Utility bill or lease under the applicant's name.", true, 2),
            });

            await EnsureRequirementsAsync(db, "Business Permit Clearance", new (string, string?, bool, int)[]
            {
                ("Valid Government ID", "ID of the business owner.", true, 1),
                ("DTI / Business Registration", "DTI or SEC registration document.", true, 2),
                ("Community Tax Certificate / Cedula", "Latest cedula for the current year.", true, 3),
                ("Barangay Clearance", "Existing barangay clearance, if applicable.", false, 4),
            });
        }

        private static async Task EnsureRequirementsAsync(
            ApplicationDbContext db, string certName, (string Name, string? Desc, bool Required, int Order)[] reqs)
        {
            var type = await db.CertificateTypes.FirstOrDefaultAsync(c => c.CertificateName == certName);
            if (type == null) return;

            foreach (var (name, desc, required, order) in reqs)
            {
                bool exists = await db.CertificateRequirements.AnyAsync(r =>
                    r.CertificateTypeId == type.CertificateTypeId && r.RequirementName == name);

                if (!exists)
                {
                    db.CertificateRequirements.Add(new CertificateRequirement
                    {
                        CertificateTypeId = type.CertificateTypeId,
                        RequirementName = name,
                        Description = desc,
                        IsRequired = required,
                        DisplayOrder = order,
                        IsActive = true
                    });
                }
            }
        }

        // ---------------------------------------------------------------
        // Barangay Officials — gamit ang tatlong pangalan
        // ---------------------------------------------------------------
        private static async Task SeedBarangayOfficialsAsync(ApplicationDbContext db)
        {
            await EnsureOfficialAsync(db, NAME_JOMARIE, "Barangay Captain", "Executive Committee");
            await EnsureOfficialAsync(db, NAME_MARKDAVE, "Barangay Secretary", "Administration");
            await EnsureOfficialAsync(db, NAME_KLARENCE, "SK Chairman", "Committee on Sports and Youth Development");

            // I-save muna para magkaroon ng ID ang bawat opisyal bago i-seed ang history.
            await db.SaveChangesAsync();

            // Kasalukuyang termino + isang nakaraang termino bawat opisyal.
            // Ang current term ay tumutugma sa profile sa kaliwa; may markang "Current".
            await EnsureHistoryAsync(db, NAME_JOMARIE,
                new (string, string, int, int?, string)[]
                {
                    ("Barangay Captain",   "Executive Committee",           2023, null, "Current"),
                    ("Barangay Councilor", "Peace & Order Committee",       2019, 2022, "Completed"),
                });

            await EnsureHistoryAsync(db, NAME_MARKDAVE,
                new (string, string, int, int?, string)[]
                {
                    ("Barangay Secretary", "Administration",                2023, null, "Current"),
                    ("Barangay Councilor", "Committee on Education",        2019, 2022, "Completed"),
                });

            await EnsureHistoryAsync(db, NAME_KLARENCE,
                new (string, string, int, int?, string)[]
                {
                    ("SK Chairman",   "Committee on Sports and Youth Development", 2023, null, "Current"),
                    ("SK Kagawad",    "Committee on Youth Programs",              2021, 2023, "Completed"),
                });

            await db.SaveChangesAsync();
        }

        private static async Task EnsureOfficialAsync(ApplicationDbContext db, string fullName, string position, string committee)
        {
            if (!await db.BarangayOfficials.AnyAsync(o => o.FullName == fullName))
            {
                db.BarangayOfficials.Add(new BarangayOfficial
                {
                    FullName = fullName,
                    Position = position,
                    Committee = committee,
                    IsActive = true
                });
            }
        }

        // I-seed ang service history ng isang opisyal (idempotent — hindi nagdudobol).
        private static async Task EnsureHistoryAsync(ApplicationDbContext db, string fullName,
            (string position, string committee, int startYear, int? endYear, string status)[] terms)
        {
            var official = await db.BarangayOfficials.FirstOrDefaultAsync(o => o.FullName == fullName);
            if (official == null) return;

            foreach (var t in terms)
            {
                bool exists = await db.OfficialServiceHistories.AnyAsync(h =>
                    h.BarangayOfficialId == official.BarangayOfficialId &&
                    h.Position == t.position &&
                    h.StartDate.Year == t.startYear);
                if (exists) continue;

                db.OfficialServiceHistories.Add(new OfficialServiceHistory
                {
                    BarangayOfficialId = official.BarangayOfficialId,
                    Position = t.position,
                    Committee = t.committee,
                    StartDate = new DateTime(t.startYear, 1, 1),
                    EndDate = t.endYear.HasValue ? new DateTime(t.endYear.Value, 12, 31) : (DateTime?)null,
                    Status = t.status
                });
            }
        }

        // ---------------------------------------------------------------
        // Health Records — isa bawat residente
        // ---------------------------------------------------------------
        private static async Task SeedHealthRecordsAsync(ApplicationDbContext db)
        {
            await EnsureHealthAsync(db, "Callueng", 68, 172, "O+", true, "None", "General", "Jomarie Callueng");
            await EnsureHealthAsync(db, "Cardenas", 74, 178, "A+", true, "Hypertension", "General", "Mark Dave Casao Cardenas");
            await EnsureHealthAsync(db, "Villar", 60, 168, "B+", false, "Asthma", "PWD", "Klarence Alfred Z. Villar");
        }

        private static async Task EnsureHealthAsync(ApplicationDbContext db, string last,
            double weight, double height, string blood, bool vaccinated, string condition,
            string classification, string worker)
        {
            var residentId = await ResidentIdAsync(db, last);
            if (residentId == 0) return;
            if (await db.HealthRecords.AnyAsync(h => h.ResidentId == residentId)) return;

            db.HealthRecords.Add(new HealthRecord
            {
                ResidentId = residentId,
                WeightKg = weight,
                HeightCm = height,
                BloodType = blood,
                IsVaccinated = vaccinated,
                MedicalCondition = condition,
                HealthClassification = classification,
                LastCheckupDate = DateTime.Now.AddDays(-20),
                AttendingHealthWorker = worker,
                Remarks = "Routine barangay health check-up.",
                DateLogged = DateTime.Now.AddDays(-20)
            });
        }

        // ---------------------------------------------------------------
        // Certificates
        // ---------------------------------------------------------------
        private static async Task SeedCertificatesAsync(ApplicationDbContext db)
        {
            await EnsureCertificateAsync(db, "BRGY-2026-0001", "Callueng", NAME_JOMARIE,
                "Barangay Clearance", "Local Employment Requirement", 50.00m, "Issued");
            await EnsureCertificateAsync(db, "BRGY-2026-0002", "Cardenas", NAME_MARKDAVE,
                "Certificate of Indigency", "Medical Assistance", 0.00m, "Approved");
            await EnsureCertificateAsync(db, "BRGY-2026-0003", "Villar", NAME_KLARENCE,
                "Certificate of Residency", "School Requirement", 30.00m, "Pending");
        }

        private static async Task EnsureCertificateAsync(ApplicationDbContext db, string control,
            string last, string residentName, string certType, string purpose, decimal fee, string status)
        {
            if (await db.Certificates.AnyAsync(c => c.ControlNumber == control)) return;

            db.Certificates.Add(new Certificate
            {
                ResidentId = await ResidentIdAsync(db, last),
                ResidentName = residentName,
                CertificateType = certType,
                Purpose = purpose,
                ControlNumber = control,
                FeePaid = fee,
                OfficialReceiptNumber = fee > 0 ? "OR-" + control : string.Empty,
                Status = status,
                DateRequested = DateTime.Now.AddDays(-7),
                DateIssued = status == "Issued" ? DateTime.Now.AddDays(-5) : (DateTime?)null,
                IssuedBy = status == "Issued" ? NAME_MARKDAVE : string.Empty
            });
        }

        // ---------------------------------------------------------------
        // Complaints (Blotter)
        // ---------------------------------------------------------------
        private static async Task SeedComplaintsAsync(ApplicationDbContext db)
        {
            await EnsureComplaintAsync(db, "BLOTTER-2026-001", "Callueng", NAME_JOMARIE, "09171234501",
                NAME_MARKDAVE, "Noise complaint tungkol sa videoke tuwing gabi.", "Resolved");
            await EnsureComplaintAsync(db, "BLOTTER-2026-002", "Cardenas", NAME_MARKDAVE, "09171234502",
                NAME_KLARENCE, "Alitan sa hangganan ng bakuran (boundary dispute).", "Ongoing");
            await EnsureComplaintAsync(db, "BLOTTER-2026-003", "Villar", NAME_KLARENCE, "09171234503",
                NAME_JOMARIE, "Nawawalang alagang aso, hinihinalang kinuha.", "Pending");
        }

        private static async Task EnsureComplaintAsync(ApplicationDbContext db, string caseNo,
            string complainantLast, string complainantName, string contact, string respondent,
            string details, string status)
        {
            if (await db.Complaints.AnyAsync(c => c.CaseNumber == caseNo)) return;

            db.Complaints.Add(new Complaint
            {
                CaseNumber = caseNo,
                ResidentId = await ResidentIdAsync(db, complainantLast),
                ComplainantName = complainantName,
                ComplainantContact = contact,
                RespondentName = respondent,
                IncidentLocation = "Barangay Tatalon, Quezon City",
                IncidentDate = DateTime.Now.AddDays(-10),
                Details = details,
                Status = status,
                Remarks = status == "Resolved" ? "Naayos sa pamamagitan ng amicable settlement." : string.Empty,
                ActionTaken = status == "Resolved" ? "Mediation ng Lupong Tagapamayapa." : "Naka-schedule para sa hearing.",
                DateSubmitted = DateTime.Now.AddDays(-10)
            });
        }

        // ---------------------------------------------------------------
        // Announcements
        // ---------------------------------------------------------------
        private static async Task SeedAnnouncementsAsync(ApplicationDbContext db)
        {
            await EnsureAnnouncementAsync(db, "Libreng Anti-Rabies Vaccination",
                "Magkakaroon ng libreng anti-rabies vaccination para sa mga alagang hayop sa Barangay Hall.",
                "Health", NAME_KLARENCE, true);
            await EnsureAnnouncementAsync(db, "Barangay Clean-Up Drive",
                "Sama-samang paglilinis sa buong barangay. Lahat ng residente ay inaanyayahang lumahok.",
                "Advisory", NAME_JOMARIE, false);
            await EnsureAnnouncementAsync(db, "Iskedyul ng Curfew para sa Kabataan",
                "Ipinatutupad ang curfow mula 10:00 PM hanggang 4:00 AM para sa mga menor de edad.",
                "General", NAME_MARKDAVE, false);
        }

        private static async Task EnsureAnnouncementAsync(ApplicationDbContext db, string title,
            string content, string category, string author, bool pinned)
        {
            if (await db.Announcements.AnyAsync(a => a.Title == title)) return;

            db.Announcements.Add(new Announcement
            {
                Title = title,
                Content = content,
                Category = category,
                AuthorName = author,
                IsPinned = pinned,
                PublishDate = DateTime.Now.AddDays(-3),
                ExpiryDate = DateTime.Now.AddMonths(1)
            });
        }

        // ---------------------------------------------------------------
        // Budgets
        // ---------------------------------------------------------------
        private static async Task SeedBudgetsAsync(ApplicationDbContext db)
        {
            int year = DateTime.Now.Year;
            await EnsureBudgetAsync(db, year, "General Fund", "Barangay Operations",
                "Pang-araw-araw na operasyon ng barangay.", 500000m, 220000m, NAME_JOMARIE);
            await EnsureBudgetAsync(db, year, "Educational Fund", "Scholarship Assistance",
                "Tulong-pinansyal para sa mga mag-aaral.", 200000m, 75000m, NAME_MARKDAVE);
            await EnsureBudgetAsync(db, year, "SK Fund", "Youth Sports Program",
                "Pondo para sa mga programang pangkabataan.", 150000m, 40000m, NAME_KLARENCE);
        }

        private static async Task EnsureBudgetAsync(ApplicationDbContext db, int year, string mainFund,
            string category, string description, decimal allocation, decimal disbursed, string loggedBy)
        {
            if (await db.Budgets.AnyAsync(b => b.Year == year && b.Category == category)) return;

            db.Budgets.Add(new Budget
            {
                Year = year,
                MainFundSource = mainFund,
                Category = category,
                Description = description,
                TotalAllocation = allocation,
                DisbursedAmount = disbursed,
                LoggedBy = loggedBy,
                LastUpdated = DateTime.Now
            });
        }

        // ---------------------------------------------------------------
        // Disasters
        // ---------------------------------------------------------------
        private static async Task SeedDisastersAsync(ApplicationDbContext db)
        {
            await EnsureDisasterAsync(db, "Typhoon Kristine", "Typhoon", 120, 45, 0, "Open", "Ongoing", NAME_JOMARIE);
            await EnsureDisasterAsync(db, "Fire Incident Sitio 3", "Fire", 8, 30, 1, "Closed", "Completed", NAME_MARKDAVE);
            await EnsureDisasterAsync(db, "Flash Flood Riverside", "Flood", 60, 25, 0, "Open", "Ongoing", NAME_KLARENCE);
        }

        private static async Task EnsureDisasterAsync(ApplicationDbContext db, string name, string type,
            int affected, int displaced, int casualties, string evac, string relief, string loggedBy)
        {
            if (await db.Disasters.AnyAsync(d => d.IncidentName == name)) return;

            db.Disasters.Add(new Disaster
            {
                IncidentName = name,
                DisasterType = type,
                OccurrenceDate = DateTime.Now.AddDays(-15),
                AffectedHouseholdsCount = affected,
                DisplacedIndividualsCount = displaced,
                CasualtiesCount = casualties,
                EvacuationCenterStatus = evac,
                ReliefDistributionStatus = relief,
                LoggedBy = loggedBy,
                DateCreated = DateTime.Now.AddDays(-15),
                DateUpdated = DateTime.Now.AddDays(-2)
            });
        }

        // ---------------------------------------------------------------
        // Environment Records
        // ---------------------------------------------------------------
        private static async Task SeedEnvironmentAsync(ApplicationDbContext db)
        {
            await EnsureEnvironmentAsync(db, "Coastal Clean-Up Drive", "Riverside Area", "Compliant", 0,
                "Matagumpay na naisagawa ang paglilinis.", NAME_JOMARIE);
            await EnsureEnvironmentAsync(db, "Tree Planting Activity", "Tagalog Area", "Compliant", 0,
                "150 puno ang naitanim.", NAME_MARKDAVE);
            await EnsureEnvironmentAsync(db, "Waste Segregation Inspection", "Kalasag Area", "Warning", 3,
                "May ilang household na hindi sumusunod sa segregation.", NAME_KLARENCE);
        }

        private static async Task EnsureEnvironmentAsync(ApplicationDbContext db, string activity,
            string location, string wasteStatus, int violations, string remarks, string inspector)
        {
            if (await db.EnvironmentRecords.AnyAsync(e => e.ActivityName == activity)) return;

            db.EnvironmentRecords.Add(new EnvironmentRecord
            {
                ActivityName = activity,
                LocationArea = location,
                InspectionOrActivityDate = DateTime.Now.AddDays(-12),
                WasteManagementStatus = wasteStatus,
                ViolationsCount = violations,
                Remarks = remarks,
                InspectorName = inspector,
                DateLogged = DateTime.Now.AddDays(-12)
            });
        }

        // ---------------------------------------------------------------
        // Projects
        // ---------------------------------------------------------------
        private static async Task SeedProjectsAsync(ApplicationDbContext db)
        {
            await EnsureProjectAsync(db, "Barangay Multi-Purpose Hall", "Pagpapatayo ng bagong multi-purpose hall.",
                "Barangay Tatalon", 1200000m, 450000m, "JMC Construction", "Ongoing", NAME_JOMARIE);
            await EnsureProjectAsync(db, "Drainage System Improvement", "Pagpapaayos ng drainage upang maiwasan ang baha.",
                "Riverside Area", 800000m, 800000m, "Cardenas Builders", "Completed", NAME_MARKDAVE);
            await EnsureProjectAsync(db, "Covered Basketball Court", "Pagtatayo ng covered court para sa kabataan.",
                "Tagalog Area", 600000m, 0m, "Villar Contractors", "Planning", NAME_KLARENCE);
        }

        private static async Task EnsureProjectAsync(ApplicationDbContext db, string title, string description,
            string location, decimal budget, decimal expenses, string contractor, string status, string loggedByName)
        {
            if (await db.Projects.AnyAsync(p => p.Title == title)) return;

            db.Projects.Add(new Project
            {
                Title = title,
                Description = description,
                Location = location,
                BudgetAllocated = budget,
                TotalExpenses = expenses,
                Contractor = contractor,
                Status = status,
                StartDate = DateTime.Now.AddMonths(-2),
                EndDate = status == "Completed" ? DateTime.Now.AddDays(-5) : (DateTime?)null,
                DateLogged = DateTime.Now.AddMonths(-2),
                LastUpdated = DateTime.Now
            });
        }

        // ---------------------------------------------------------------
        // SMS Alerts (Disaster Notification History)
        // ---------------------------------------------------------------
        private static async Task SeedSmsAlertsAsync(ApplicationDbContext db)
        {
            await EnsureSmsAlertAsync(db, "Typhoon",
                "PAALALA: Inaasahang malakas na ulan dahil sa bagyo. Manatiling ligtas at maghanda.",
                "All Residents", 3, 3, 0, "Sent", NAME_JOMARIE);
            await EnsureSmsAlertAsync(db, "Flood",
                "BABALA: Tumataas ang tubig sa Riverside. Maghanda para sa posibleng evacuation.",
                "Purok: Riverside Area", 1, 1, 0, "Sent", NAME_KLARENCE);
        }

        private static async Task EnsureSmsAlertAsync(ApplicationDbContext db, string type, string message,
            string group, int recipients, int success, int failed, string status, string sentBy)
        {
            if (await db.SmsAlerts.AnyAsync(s => s.Message == message)) return;

            db.SmsAlerts.Add(new SmsAlert
            {
                EmergencyType = type,
                Message = message,
                RecipientGroup = group,
                RecipientCount = recipients,
                SuccessCount = success,
                FailedCount = failed,
                Status = status,
                SentBy = sentBy,
                SentAt = DateTime.Now.AddDays(-1)
            });
        }
    }
}
