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
    public class EnvironmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EnvironmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. GET: Admin/Environment
        public async Task<IActionResult> Index()
        {
            var records = await _context.EnvironmentRecords
                .OrderByDescending(e => e.InspectionOrActivityDate) // Inayos mula ActivityDate -> InspectionOrActivityDate
                .Select(e => new EnvironmentViewModel
                {
                    Id = e.EnvironmentRecordId,
                    ActivityName = e.ActivityName,
                    Location = e.LocationArea, // Inayos mula Location -> LocationArea
                    ActivityDate = e.InspectionOrActivityDate,
                    Description = e.Remarks // Inayos mula Description -> Remarks
                }).ToListAsync();

            return View(records);
        }

        // 2. GET: Admin/Environment/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var record = await _context.EnvironmentRecords.FindAsync(id.Value);
            if (record == null) return NotFound();

            var viewModel = new EnvironmentViewModel
            {
                Id = record.EnvironmentRecordId,
                ActivityName = record.ActivityName,
                Location = record.LocationArea,
                ActivityDate = record.InspectionOrActivityDate,
                Description = record.Remarks
            };

            return View(viewModel);
        }

        // 3. GET: Admin/Environment/Create
        public IActionResult Create()
        {
            return View(new EnvironmentViewModel { ActivityDate = DateTime.Now });
        }

        // 4. POST: Admin/Environment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EnvironmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var record = new EnvironmentRecord
                {
                    ActivityName = model.ActivityName,
                    LocationArea = model.Location ?? string.Empty,
                    InspectionOrActivityDate = model.ActivityDate,
                    Remarks = model.Description ?? string.Empty,

                    // Default fallbacks para sa mga bagong monitoring at logging fields ng entity
                    WasteManagementStatus = "Compliant",
                    ViolationsCount = 0,
                    InspectorName = User.Identity?.Name ?? "Admin",
                    DateLogged = DateTime.Now
                };

                _context.EnvironmentRecords.Add(record); // Tahasang tinukoy ang DbSet (.EnvironmentRecords)
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // 5. GET: Admin/Environment/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var record = await _context.EnvironmentRecords.FindAsync(id.Value);
            if (record == null) return NotFound();

            var viewModel = new EnvironmentViewModel
            {
                Id = record.EnvironmentRecordId,
                ActivityName = record.ActivityName,
                Location = record.LocationArea,
                ActivityDate = record.InspectionOrActivityDate,
                Description = record.Remarks
            };

            return View(viewModel);
        }

        // 6. POST: Admin/Environment/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EnvironmentViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var record = await _context.EnvironmentRecords.FindAsync(id);
                    if (record == null) return NotFound();

                    record.ActivityName = model.ActivityName;
                    record.LocationArea = model.Location ?? string.Empty;
                    record.InspectionOrActivityDate = model.ActivityDate;
                    record.Remarks = model.Description ?? string.Empty;
                    // I-update din ang pangalan ng huling humawak kung kinakailangan
                    record.InspectorName = User.Identity?.Name ?? "Admin";

                    _context.EnvironmentRecords.Update(record);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.EnvironmentRecords.Any(e => e.EnvironmentRecordId == model.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // 7. GET: Admin/Environment/CleanupActivities
        public IActionResult CleanupActivities()
        {
            return View();
        }

        // 8. GET: Admin/Environment/WasteCollection
        public IActionResult WasteCollection()
        {
            return View();
        }

        // 9. GET: Admin/Environment/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var record = await _context.EnvironmentRecords.FindAsync(id.Value);
            if (record == null) return NotFound();

            var viewModel = new EnvironmentViewModel
            {
                Id = record.EnvironmentRecordId,
                ActivityName = record.ActivityName,
                Location = record.LocationArea,
                ActivityDate = record.InspectionOrActivityDate
            };

            return View(viewModel);
        }

        // 10. POST: Admin/Environment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var record = await _context.EnvironmentRecords.FindAsync(id);
            if (record != null)
            {
                _context.EnvironmentRecords.Remove(record);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}