using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BarangayCMS.DAL.Context;
using BarangayCMS.Entities;
using BarangayCMS.Web.Areas.Admin.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BarangayCMS.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DisasterController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DisasterController(ApplicationDbContext context)
        {
            _context = context;
        }

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

            return View(disasters);
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

        // 7. GET: Admin/Disaster/EvacuationCenters
        public IActionResult EvacuationCenters()
        {
            return View();
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
    }
}