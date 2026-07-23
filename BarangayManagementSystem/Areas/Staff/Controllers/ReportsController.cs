using Microsoft.AspNetCore.Mvc;
using BarangayCMS.Areas.Staff.ViewModels;
using BarangayCMS.DAL.Context;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace BarangayCMS.Areas.Staff.Controllers
{
    [Area("Staff")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Helper para sa 4 main dashboard cards gamit ang ReportsViewModel
        private ReportsViewModel GetBaseSummary()
        {
            return new ReportsViewModel
            {
                TotalResidents = _context.Residents.Count(r => r.IsResident),
                TotalCertificatesIssued = _context.Certificates.Count(c => c.Status == "Issued" || c.Status == "Approved"),
                ActiveComplaints = _context.Complaints.Count(c => c.Status == "Active" || c.Status == "Pending" || c.Status == "Ongoing"),
                ActiveDisasters = _context.Disasters.Count(d => d.EvacuationCenterStatus == "Open" || d.ReliefDistributionStatus == "Ongoing"),
                GeneratedDate = DateTime.Now
            };
        }

        #region Core Report Views
        // 1. GET: /Staff/Reports/Index
        public IActionResult Index()
        {
            var model = GetBaseSummary();
            model.ReportTitle = "System Comprehensive Executive Summary";
            return View(model);
        }

        // 2. GET: /Staff/Reports/Residents
        public IActionResult Residents(DateTime? startDate, DateTime? endDate)
        {
            var model = GetBaseSummary();
            model.ReportTitle = "Resident Demographic Census Analytics";
            model.TableData = GetResidentsData(startDate, endDate);
            return View(model);
        }

        // 3. GET: /Staff/Reports/Certificates
        public IActionResult Certificates(DateTime? startDate, DateTime? endDate)
        {
            var model = GetBaseSummary();
            model.ReportTitle = "Barangay Clearance & Certification Logs";
            model.TableData = GetCertificatesData(startDate, endDate);
            return View(model);
        }

        // 4. GET: /Staff/Reports/Complaints
        public IActionResult Complaints(DateTime? startDate, DateTime? endDate)
        {
            var model = GetBaseSummary();
            model.ReportTitle = "Blotter Intake & Incident Reports Analysis";
            model.TableData = GetComplaintsData(startDate, endDate);
            return View(model);
        }

        // 5. GET: /Staff/Reports/Disaster
        public IActionResult Disaster(DateTime? startDate, DateTime? endDate)
        {
            var model = GetBaseSummary();
            model.ReportTitle = "Crisis Management Operations Record";
            model.TableData = GetDisasterData(startDate, endDate);
            return View(model);
        }

        // 6. GET: /Staff/Reports/Environment
        public IActionResult Environment(DateTime? startDate, DateTime? endDate)
        {
            var model = GetBaseSummary();
            model.ReportTitle = "Ecological Activities & Clean-up Drive Reports";
            model.TableData = GetEnvironmentData(startDate, endDate);
            return View(model);
        }

        // 7. GET: /Staff/Reports/Health
        public IActionResult Health(DateTime? startDate, DateTime? endDate)
        {
            var model = GetBaseSummary();
            model.ReportTitle = "Community Vaccination & Health Mission Records";
            model.TableData = GetHealthData(startDate, endDate);
            return View(model);
        }

        // 8. GET: /Staff/Reports/Export
        public IActionResult Export()
        {
            var model = GetBaseSummary();
            model.ReportTitle = "Reports Export Panel";
            return View(model);
        }
        #endregion

        #region Data Retrieval Helpers (Dito kumukuha ang View at Excel Engine)
        private List<Dictionary<string, string>> GetResidentsData(DateTime? start, DateTime? end)
        {
            var query = _context.Residents.AsQueryable();
            if (start.HasValue) query = query.Where(r => r.CreatedAt >= start.Value);
            if (end.HasValue) query = query.Where(r => r.CreatedAt <= end.Value);

            return query.Select(r => new Dictionary<string, string>
            {
                { "Resident ID", "RES-" + r.ResidentId.ToString() },
                { "Full Name", (r.FirstName + " " + (string.IsNullOrEmpty(r.MiddleName) ? "" : r.MiddleName + " ") + r.LastName + " " + r.Suffix).Trim() },
                { "Age", (DateTime.Now.Year - r.BirthDate.Year).ToString() },
                { "Voter Status", r.IsVoter ? "Registered" : "Non-Registered" }
            }).ToList();
        }

        private List<Dictionary<string, string>> GetCertificatesData(DateTime? start, DateTime? end)
        {
            var query = _context.Certificates.Include(c => c.Resident).AsQueryable();
            if (start.HasValue) query = query.Where(c => c.DateRequested >= start.Value);
            if (end.HasValue) query = query.Where(c => c.DateRequested <= end.Value);

            return query.Select(c => new Dictionary<string, string>
            {
                { "Certificate ID", string.IsNullOrEmpty(c.ControlNumber) ? "CERT-" + c.CertificateId : c.ControlNumber },
                { "Resident Name", (c.Resident.FirstName + " " + c.Resident.LastName) ?? "Unknown Resident" },
                { "Type of Clearance", c.CertificateType },
                { "Amount Paid", "₱" + c.FeePaid.ToString("N2") },
                { "Status", c.Status }
            }).ToList();
        }

        private List<Dictionary<string, string>> GetComplaintsData(DateTime? start, DateTime? end)
        {
            var query = _context.Complaints.AsQueryable();
            if (start.HasValue) query = query.Where(c => c.DateSubmitted >= start.Value);
            if (end.HasValue) query = query.Where(c => c.DateSubmitted <= end.Value);

            return query.OrderByDescending(c => c.DateSubmitted).Select(c => new Dictionary<string, string>
            {
                { "Case ID", string.IsNullOrEmpty(c.CaseNumber) ? "BLTR-" + c.ComplaintId.ToString() : c.CaseNumber },
                { "Complainant", string.IsNullOrEmpty(c.ComplainantName) ? "Anonymous" : c.ComplainantName },
                { "Subject / Details", c.RespondentName + " (" + c.Details + ")" },
                { "Date Filed", c.DateSubmitted.ToString("MM/dd/yyyy") },
                { "Status", c.Status }
            }).ToList();
        }

        private List<Dictionary<string, string>> GetDisasterData(DateTime? start, DateTime? end)
        {
            var query = _context.Disasters.AsQueryable();
            if (start.HasValue) query = query.Where(d => d.OccurrenceDate >= start.Value);
            if (end.HasValue) query = query.Where(d => d.OccurrenceDate <= end.Value);

            return query.Select(d => new Dictionary<string, string>
            {
                { "Event ID", "DIS-" + d.DisasterId.ToString() },
                { "Incident Name", d.IncidentName },
                { "Location / Impact", d.DisasterType + " (" + d.AffectedHouseholdsCount + " Households Affected)" },
                { "Operation Status", "Evac: " + d.EvacuationCenterStatus + " / Relief: " + d.ReliefDistributionStatus }
            }).ToList();
        }

        private List<Dictionary<string, string>> GetEnvironmentData(DateTime? start, DateTime? end)
        {
            var query = _context.EnvironmentRecords.AsQueryable();
            if (start.HasValue) query = query.Where(e => e.InspectionOrActivityDate >= start.Value);
            if (end.HasValue) query = query.Where(e => e.InspectionOrActivityDate <= end.Value);

            return query.Select(e => new Dictionary<string, string>
            {
                { "Activity ID", "ENV-" + e.EnvironmentRecordId.ToString() },
                { "Environmental Activity", e.ActivityName },
                { "Venue / Area", e.LocationArea },
                { "Schedule Date", e.InspectionOrActivityDate.ToString("MM/dd/yyyy") }
            }).ToList();
        }

        private List<Dictionary<string, string>> GetHealthData(DateTime? start, DateTime? end)
        {
            var query = _context.HealthRecords.Include(h => h.Resident).AsQueryable();
            if (start.HasValue) query = query.Where(h => h.DateLogged >= start.Value);
            if (end.HasValue) query = query.Where(h => h.DateLogged <= end.Value);

            return query.Select(h => new Dictionary<string, string>
            {
                { "Log ID", "HLTH-" + h.HealthRecordId.ToString() },
                { "Health Program", h.HealthClassification + " (" + h.MedicalCondition + ")" },
                { "Attendees Summary", (h.Resident.FirstName + " " + h.Resident.LastName) ?? "Patient" },
                { "Attending Medical Officer", h.AttendingHealthWorker + " | " + (h.IsVaccinated ? "Vaccinated" : "Not Vaccinated") }
            }).ToList();
        }
        #endregion

        #region Excel Engines (Native & Robust)
        // 9. GET: /Staff/Reports/ExportExcel (Pangkalahatang Summary)
        public IActionResult ExportExcel()
        {
            var summary = GetBaseSummary();

            var htmlBuilder = new StringBuilder();
            htmlBuilder.AppendLine("<html>");
            htmlBuilder.AppendLine("<head><meta charset=\"utf-8\"><style>th { background-color: #212529; color: #ffffff; font-weight: bold; } td, th { border: 1px solid #dee2e6; padding: 8px; font-family: sans-serif; }</style></head>");
            htmlBuilder.AppendLine("<body>");
            htmlBuilder.AppendLine($"<h2>PANGKALAHATANG ULAT NG BARANGAY - STAFF</h2>");
            htmlBuilder.AppendLine($"<p><b>Generated Date:</b> {DateTime.Now:MMMM dd, yyyy hh:mm tt}</p><br/>");

            htmlBuilder.AppendLine("<table style=\"border-collapse: collapse; width: 60%;\">");
            htmlBuilder.AppendLine("<thead><tr><th style=\"text-align:left;\">Metriko</th><th style=\"text-align:right;\">Bilang / Kasalukuyang Datos</th></tr></thead>");
            htmlBuilder.AppendLine("<tbody>");
            htmlBuilder.AppendLine($"<tr><td>Total Residents</td><td style=\"text-align:right;\">{summary.TotalResidents}</td></tr>");
            htmlBuilder.AppendLine($"<tr><td>Total Certificates Issued</td><td style=\"text-align:right;\">{summary.TotalCertificatesIssued}</td></tr>");
            htmlBuilder.AppendLine($"<tr><td>Active Complaints (Blotter)</td><td style=\"text-align:right;\">{summary.ActiveComplaints}</td></tr>");
            htmlBuilder.AppendLine($"<tr><td>Active Disasters (Crisis Operations)</td><td style=\"text-align:right;\">{summary.ActiveDisasters}</td></tr>");
            htmlBuilder.AppendLine("</tbody>");
            htmlBuilder.AppendLine("</table>");
            htmlBuilder.AppendLine("</body>");
            htmlBuilder.AppendLine("</html>");

            var fileBytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(htmlBuilder.ToString())).ToArray();
            return File(fileBytes, "application/vnd.ms-excel", $"Barangay_Staff_Summary_{DateTime.Now:yyyyMMdd}.xls");
        }

        // 10. GET: /Staff/Reports/ExportReportToExcel?reportType=residents (Dynamic Sub-Reports Engine)
        public IActionResult ExportReportToExcel(string reportType, DateTime? startDate, DateTime? endDate)
        {
            var model = GetBaseSummary();
            List<Dictionary<string, string>> targetData = null;
            string filename = "Barangay_Sub_Report";

            switch (reportType?.ToLower())
            {
                case "residents":
                    model.ReportTitle = "Resident Demographic Census Analytics";
                    targetData = GetResidentsData(startDate, endDate);
                    filename = "Resident_Demographics_Report";
                    break;
                case "certificates":
                    model.ReportTitle = "Barangay Clearance & Certification Logs";
                    targetData = GetCertificatesData(startDate, endDate);
                    filename = "Certificates_Issued_Report";
                    break;
                case "complaints":
                    model.ReportTitle = "Blotter Intake & Incident Reports Analysis";
                    targetData = GetComplaintsData(startDate, endDate);
                    filename = "Complaints_Report";
                    break;
                case "disaster":
                    model.ReportTitle = "Crisis Management Operations Record";
                    targetData = GetDisasterData(startDate, endDate);
                    filename = "Disaster_Operations_Report";
                    break;
                case "environment":
                    model.ReportTitle = "Ecological Activities & Clean-up Drive Reports";
                    targetData = GetEnvironmentData(startDate, endDate);
                    filename = "Environmental_Activities_Report";
                    break;
                case "health":
                    model.ReportTitle = "Community Vaccination & Health Mission Records";
                    targetData = GetHealthData(startDate, endDate);
                    filename = "Health_Center_Report";
                    break;
                default:
                    return RedirectToAction(nameof(Index));
            }

            if (targetData == null || !targetData.Any())
            {
                return Content("<script>alert('Walang datos na makikita sa piniling petsa.'); window.history.back();</script>", "text/html");
            }

            // Pagbuo ng Excel XML/HTML structure gamit ang malinis na data formatting
            var htmlBuilder = new StringBuilder();
            htmlBuilder.AppendLine("<html>");
            htmlBuilder.AppendLine("<head><meta charset=\"utf-8\"><style>th { background-color: #212529; color: #ffffff; font-weight: bold; } td, th { border: 1px solid #dee2e6; padding: 8px; font-family: sans-serif; }</style></head>");
            htmlBuilder.AppendLine("<body>");
            htmlBuilder.AppendLine($"<h2>{model.ReportTitle}</h2>");
            htmlBuilder.AppendLine($"<p><b>Generated Date:</b> {model.GeneratedDate:MMMM dd, yyyy hh:mm tt}</p><br/>");

            htmlBuilder.AppendLine("<table style=\"border-collapse: collapse; width: 100%;\">");

            // Auto Headers
            htmlBuilder.AppendLine("<thead><tr>");
            var headers = targetData.First().Keys;
            foreach (var header in headers)
            {
                htmlBuilder.AppendLine($"<th style=\"text-align:left;\">{header}</th>");
            }
            htmlBuilder.AppendLine("</tr></thead>");

            // Data Rows
            htmlBuilder.AppendLine("<tbody>");
            foreach (var row in targetData)
            {
                htmlBuilder.AppendLine("<tr>");
                foreach (var header in headers)
                {
                    htmlBuilder.AppendLine($"<td>{row[header]}</td>");
                }
                htmlBuilder.AppendLine("</tr>");
            }
            htmlBuilder.AppendLine("</tbody>");
            htmlBuilder.AppendLine("</table>");
            htmlBuilder.AppendLine("</body>");
            htmlBuilder.AppendLine("</html>");

            var fileBytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(htmlBuilder.ToString())).ToArray();
            return File(fileBytes, "application/vnd.ms-excel", $"{filename}_{DateTime.Now:yyyyMMdd}.xls");
        }
        #endregion
    }
}