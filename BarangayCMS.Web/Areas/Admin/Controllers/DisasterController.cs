using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BarangayCMS.BLL.Interfaces;
using BarangayCMS.DAL.Context;
using BarangayCMS.DTO;
using BarangayCMS.Entities;
using BarangayCMS.Web.Areas.Admin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BarangayCMS.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DisasterController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ISemaphoreService _semaphoreService;
        private readonly IEvacuationService _evacuationService;

        // Ang Emergency SMS feature ay para LAMANG sa Admin (RBAC).
        private const string AdminRoles = "Admin,SuperAdmin";

        public DisasterController(ApplicationDbContext context, ISemaphoreService semaphoreService, IEvacuationService evacuationService)
        {
            _context = context;
            _semaphoreService = semaphoreService;
            _evacuationService = evacuationService;
        }

        // Helper: nakikita ba ng kasalukuyang user ang Emergency SMS feature?
        private bool IsAdminUser() => User.IsInRole("Admin") || User.IsInRole("SuperAdmin");

        // 1. GET: Admin/Disaster
        public async Task<IActionResult> Index()
        {
            var disasters = await _context.Disasters
                .OrderByDescending(d => d.OccurrenceDate)
                .Select(d => new DisasterViewModel
                {
                    Id = d.DisasterId,
                    DisasterType = d.DisasterType,
                    // Pinapakita ang maikling detalye
                    Description = d.IncidentName,

                    // 🔑 LUNAS: Kung may ' | ' sa IncidentName, kukunin ang lokasyon. Kung wala, gagamit ng fallback text.
                    Location = d.IncidentName.Contains(" | Lokasyon: ")
                        ? d.IncidentName.Split(new string[] { " | Lokasyon: " }, StringSplitOptions.None)[1]
                        : "Barangay Jurisdiction",

                    DateOccurred = d.OccurrenceDate,
                    Status = $"Relief: {d.ReliefDistributionStatus} | Evac: {d.EvacuationCenterStatus}"
                }).ToListAsync();

            // 📱 Emergency SMS feature data — para LAMANG sa Admin
            if (IsAdminUser())
            {
                await PopulateEmergencySmsViewDataAsync();
            }

            return View(disasters);
        }

        // ==========================================================
        // Helper: i-load ang Purok list, resident options, at SMS history
        // para sa Emergency Notification section ng Disaster/Index page.
        // ==========================================================
        private async Task PopulateEmergencySmsViewDataAsync()
        {
            var residents = await _context.Residents
                .Where(r => r.IsResident)
                .OrderBy(r => r.SitioPurok).ThenBy(r => r.LastName)
                .Select(r => new ResidentSmsOption
                {
                    Id = r.ResidentId,
                    FullName = (r.LastName + ", " + r.FirstName).Trim(),
                    Purok = string.IsNullOrWhiteSpace(r.SitioPurok) ? "N/A" : r.SitioPurok,
                    ContactNumber = r.ContactNumber
                })
                .ToListAsync();

            var purokOptions = residents
                .Where(r => !string.IsNullOrWhiteSpace(r.Purok) && r.Purok != "N/A")
                .Select(r => r.Purok)
                .Distinct()
                .OrderBy(p => p)
                .ToList();

            var history = await _context.SmsAlerts
                .OrderByDescending(s => s.SentAt)
                .Take(50)
                .Select(s => new SmsAlertHistoryItem
                {
                    Id = s.SmsAlertId,
                    SentAt = s.SentAt,
                    EmergencyType = s.EmergencyType,
                    Message = s.Message,
                    RecipientGroup = s.RecipientGroup,
                    RecipientCount = s.RecipientCount,
                    SuccessCount = s.SuccessCount,
                    FailedCount = s.FailedCount,
                    Status = s.Status,
                    SentBy = s.SentBy
                })
                .ToListAsync();

            ViewBag.IsAdminUser = true;
            ViewBag.PurokOptions = purokOptions;
            ViewBag.ResidentOptions = residents;
            ViewBag.SmsHistory = history;
        }

        // 2. GET: Admin/Disaster/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var disaster = await _context.Disasters.FindAsync(id.Value);
            if (disaster == null) return NotFound();

            var locationText = disaster.IncidentName.Contains(" | Lokasyon: ")
                ? disaster.IncidentName.Split(new string[] { " | Lokasyon: " }, StringSplitOptions.None)[1]
                : "Barangay Jurisdiction";

            var viewModel = new DisasterViewModel
            {
                Id = disaster.DisasterId,
                DisasterType = disaster.DisasterType,
                Description = disaster.IncidentName.Split(new string[] { " | Lokasyon: " }, StringSplitOptions.None)[0],
                Location = locationText,
                DateOccurred = disaster.OccurrenceDate,
                Status = $"Relief: {disaster.ReliefDistributionStatus} | Evac: {disaster.EvacuationCenterStatus}"
            };

            return View(viewModel);
        }

        // 3. GET: Admin/Disaster/Create
        public IActionResult Create()
        {
            return View(new DisasterViewModel { DateOccurred = DateTime.Now });
        }

        // 4. POST: Admin/Disaster/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DisasterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 🔑 LUNAS: Pagsasamahin natin ang Description at Location sa loob ng IncidentName (dahil walang Location field ang database)
                string fullIncidentDetails = !string.IsNullOrEmpty(model.Description) ? model.Description : $"{model.DisasterType} Incident";
                if (!string.IsNullOrEmpty(model.Location))
                {
                    fullIncidentDetails += $" | Lokasyon: {model.Location}";
                }

                var disaster = new Disaster
                {
                    IncidentName = fullIncidentDetails, // Dito itatago ang description at location
                    DisasterType = model.DisasterType,
                    OccurrenceDate = model.DateOccurred,

                    // Default values para sa entity metrics
                    AffectedHouseholdsCount = 0,
                    DisplacedIndividualsCount = 0,
                    CasualtiesCount = 0,
                    EvacuationCenterStatus = "Open",
                    ReliefDistributionStatus = "Ongoing",

                    // Metadata tracking logs
                    LoggedBy = User.Identity?.Name ?? "Admin",
                    DateCreated = DateTime.Now
                };

                _context.Disasters.Add(disaster);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // 5. GET: Admin/Disaster/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var disaster = await _context.Disasters.FindAsync(id.Value);
            if (disaster == null) return NotFound();

            string cleanDescription = disaster.IncidentName;
            string cleanLocation = "Barangay Jurisdiction";

            if (disaster.IncidentName.Contains(" | Lokasyon: "))
            {
                var parts = disaster.IncidentName.Split(new string[] { " | Lokasyon: " }, StringSplitOptions.None);
                cleanDescription = parts[0];
                cleanLocation = parts[1];
            }

            var viewModel = new DisasterViewModel
            {
                Id = disaster.DisasterId,
                DisasterType = disaster.DisasterType,
                Description = cleanDescription,
                Location = cleanLocation,
                DateOccurred = disaster.OccurrenceDate,
                Status = disaster.ReliefDistributionStatus
            };

            return View(viewModel);
        }

        // 6. POST: Admin/Disaster/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DisasterViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var disaster = await _context.Disasters.FindAsync(id);
                    if (disaster == null) return NotFound();

                    // 🔑 LUNAS: Muling pagsasamahin ang binagong Description at Location para mai-save
                    string fullIncidentDetails = !string.IsNullOrEmpty(model.Description) ? model.Description : $"{model.DisasterType} Incident";
                    if (!string.IsNullOrEmpty(model.Location))
                    {
                        fullIncidentDetails += $" | Lokasyon: {model.Location}";
                    }

                    disaster.IncidentName = fullIncidentDetails;
                    disaster.DisasterType = model.DisasterType;
                    disaster.OccurrenceDate = model.DateOccurred;

                    // Pagpapanatili ng tracking controls
                    disaster.LoggedBy = User.Identity?.Name ?? "Admin";
                    disaster.DateUpdated = DateTime.Now;

                    _context.Disasters.Update(disaster);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Disasters.Any(e => e.DisasterId == model.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // ==========================================================
        // 🏫 EVACUATION CENTERS MANAGEMENT (database-driven)
        // Ito ang IISANG pinagmumulan ng datos para sa Public Evacuation view.
        // ==========================================================

        // 7. GET: Admin/Disaster/EvacuationCenters
        public async Task<IActionResult> EvacuationCenters()
        {
            var centers = await _evacuationService.GetAllCentersAsync();
            ViewBag.EvacuationStatus = await _evacuationService.GetStatusAsync();
            return View(centers);
        }

        // 7a. POST: Admin/Disaster/CreateEvacuationCenter
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvacuationCenter(EvacuationCenterDTO model)
        {
            if (string.IsNullOrWhiteSpace(model.Name) || string.IsNullOrWhiteSpace(model.Address))
            {
                TempData["EvacError"] = "Kailangan ang Name at Address ng evacuation center.";
                return RedirectToAction(nameof(EvacuationCenters));
            }

            var ok = await _evacuationService.AddCenterAsync(model);
            TempData[ok ? "EvacSuccess" : "EvacError"] = ok
                ? $"Naidagdag ang '{model.Name}' sa mga evacuation center."
                : "Nabigo ang pag-save ng evacuation center.";
            return RedirectToAction(nameof(EvacuationCenters));
        }

        // 7b. POST: Admin/Disaster/EditEvacuationCenter
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvacuationCenter(EvacuationCenterDTO model)
        {
            if (string.IsNullOrWhiteSpace(model.Name) || string.IsNullOrWhiteSpace(model.Address))
            {
                TempData["EvacError"] = "Kailangan ang Name at Address ng evacuation center.";
                return RedirectToAction(nameof(EvacuationCenters));
            }

            var ok = await _evacuationService.UpdateCenterAsync(model);
            TempData[ok ? "EvacSuccess" : "EvacError"] = ok
                ? $"Na-update ang '{model.Name}'."
                : "Nabigo ang pag-update — maaaring wala na ang record.";
            return RedirectToAction(nameof(EvacuationCenters));
        }

        // 7c. POST: Admin/Disaster/ToggleEvacuationCenter
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleEvacuationCenter(int id)
        {
            var ok = await _evacuationService.ToggleCenterActiveAsync(id);
            TempData[ok ? "EvacSuccess" : "EvacError"] = ok
                ? "Na-update ang availability ng center."
                : "Nabigo ang pag-toggle ng center.";
            return RedirectToAction(nameof(EvacuationCenters));
        }

        // 7d. POST: Admin/Disaster/DeleteEvacuationCenter
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvacuationCenter(int id)
        {
            var ok = await _evacuationService.DeleteCenterAsync(id);
            TempData[ok ? "EvacSuccess" : "EvacError"] = ok
                ? "Natanggal ang evacuation center."
                : "Nabigo ang pagtanggal ng center.";
            return RedirectToAction(nameof(EvacuationCenters));
        }

        // 7e. POST: Admin/Disaster/UpdateEvacuationStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEvacuationStatus(EvacuationStatusDTO model)
        {
            model.UpdatedBy = User.Identity?.Name ?? "Admin";
            var ok = await _evacuationService.UpdateStatusAsync(model);
            TempData[ok ? "EvacSuccess" : "EvacError"] = ok
                ? "Na-update ang buong-barangay na evacuation status."
                : "Nabigo ang pag-update ng status.";
            return RedirectToAction(nameof(EvacuationCenters));
        }

        // 8. GET: Admin/Disaster/HazardMaps
        public IActionResult HazardMaps()
        {
            return View();
        }

        // 9. GET: Admin/Disaster/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var disaster = await _context.Disasters.FindAsync(id.Value);
            if (disaster == null) return NotFound();

            string cleanLocation = disaster.IncidentName.Contains(" | Lokasyon: ")
                ? disaster.IncidentName.Split(new string[] { " | Lokasyon: " }, StringSplitOptions.None)[1]
                : "Barangay Jurisdiction";

            var viewModel = new DisasterViewModel
            {
                Id = disaster.DisasterId,
                DisasterType = disaster.DisasterType,
                Location = cleanLocation,
                DateOccurred = disaster.OccurrenceDate
            };

            return View(viewModel);
        }

        // 10. POST: Admin/Disaster/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var disaster = await _context.Disasters.FindAsync(id);
            if (disaster != null)
            {
                _context.Disasters.Remove(disaster);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // ==========================================================
        // 📱 EMERGENCY SMS ALERT (ADMIN ONLY)
        // Bahagi ng Disaster Risk Management module.
        // ==========================================================

        // 11. POST: Admin/Disaster/SendEmergencyAlert
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AdminRoles)]
        public async Task<IActionResult> SendEmergencyAlert(EmergencySmsViewModel model)
        {
            // Server-side validation — huwag umasa sa UI lamang
            if (string.IsNullOrWhiteSpace(model.EmergencyType) || string.IsNullOrWhiteSpace(model.Message))
            {
                TempData["SmsError"] = "Kailangan ang Emergency Type at Message bago magpadala.";
                return RedirectToAction(nameof(Index));
            }

            // 1. Kunin ang mga contact number base sa napiling recipient group
            var residentsQuery = _context.Residents.Where(r => r.IsResident);
            string recipientGroupLabel;

            switch (model.RecipientGroup)
            {
                case "Purok":
                    if (string.IsNullOrWhiteSpace(model.Purok))
                    {
                        TempData["SmsError"] = "Pumili ng Purok/Area para sa recipient group na ito.";
                        return RedirectToAction(nameof(Index));
                    }
                    residentsQuery = residentsQuery.Where(r => r.SitioPurok == model.Purok);
                    recipientGroupLabel = $"Purok: {model.Purok}";
                    break;

                case "Selected":
                    if (model.SelectedResidentIds == null || model.SelectedResidentIds.Count == 0)
                    {
                        TempData["SmsError"] = "Pumili ng kahit isang residente para sa 'Selected Residents'.";
                        return RedirectToAction(nameof(Index));
                    }
                    residentsQuery = residentsQuery.Where(r => model.SelectedResidentIds.Contains(r.ResidentId));
                    recipientGroupLabel = $"Selected Residents ({model.SelectedResidentIds.Count})";
                    break;

                default: // "All Residents"
                    recipientGroupLabel = "All Residents";
                    break;
            }

            var numbers = await residentsQuery
                .Select(r => r.ContactNumber)
                .Where(n => n != null && n != "")
                .ToListAsync();

            if (numbers.Count == 0)
            {
                TempData["SmsError"] = "Walang natagpuang registered mobile number para sa napiling recipients.";
                return RedirectToAction(nameof(Index));
            }

            // 2. Ipadala gamit ang Semaphore (may validation + error handling sa service)
            var result = await _semaphoreService.SendBulkSmsAsync(numbers, model.Message);

            // 3. I-record sa SMS Alert History (WALANG API key na itinatago dito)
            var alert = new SmsAlert
            {
                EmergencyType = model.EmergencyType,
                Message = model.Message,
                RecipientGroup = recipientGroupLabel,
                RecipientCount = result.TotalRecipients,
                SuccessCount = result.SuccessCount,
                FailedCount = result.FailedCount + result.InvalidNumbers.Count,
                Status = result.Status,
                SentBy = User.Identity?.Name ?? "Admin",
                SentAt = DateTime.Now
            };
            _context.SmsAlerts.Add(alert);
            await _context.SaveChangesAsync();

            // 4. Ibalik ang resulta sa user
            if (result.Status == "Sent")
            {
                TempData["SmsSuccess"] = $"Matagumpay na naipadala ang emergency alert sa {result.SuccessCount} residente.";
            }
            else if (result.Status == "Partial")
            {
                TempData["SmsWarning"] = $"Bahagyang naipadala: {result.SuccessCount} tagumpay, {alert.FailedCount} nabigo/invalid.";
            }
            else
            {
                TempData["SmsError"] = "Nabigo ang pagpapadala ng emergency alert. Suriin ang Semaphore configuration o ang mga numero.";
            }

            return RedirectToAction(nameof(Index));
        }

        // 12. GET: Admin/Disaster/SmsHistory (buong history view — Admin only)
        [HttpGet]
        [Authorize(Roles = AdminRoles)]
        public async Task<IActionResult> SmsHistory()
        {
            var history = await _context.SmsAlerts
                .OrderByDescending(s => s.SentAt)
                .Select(s => new SmsAlertHistoryItem
                {
                    Id = s.SmsAlertId,
                    SentAt = s.SentAt,
                    EmergencyType = s.EmergencyType,
                    Message = s.Message,
                    RecipientGroup = s.RecipientGroup,
                    RecipientCount = s.RecipientCount,
                    SuccessCount = s.SuccessCount,
                    FailedCount = s.FailedCount,
                    Status = s.Status,
                    SentBy = s.SentBy
                })
                .ToListAsync();

            return View(history);
        }
    }
}