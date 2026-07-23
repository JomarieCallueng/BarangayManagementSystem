using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BarangayCMS.DAL.Context;
using BarangayCMS.Entities; // Para sa Resident at HealthRecord entities
using BarangayCMS.Web.Areas.Admin.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BarangayCMS.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HealthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HealthController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. GET: Admin/Health
        public async Task<IActionResult> Index()
        {
            // 🔑 LUNAS: Ginagamit ang totoong properties ng HealthRecord (.HealthRecordId at .DateLogged)
            var records = await _context.Set<HealthRecord>()
                .Select(h => new HealthViewModel
                {
                    Id = h.HealthRecordId,
                    ResidentId = h.ResidentId,
                    MedicalCondition = h.MedicalCondition,
                    Remarks = h.AttendingHealthWorker, // Pansamantalang ipinapakita ang remarks dito dahil walang Remarks column sa entity
                    DateRecorded = h.DateLogged,

                    // Kukunin ang pangalan ng Residente para sa Table list ng Index page
                    ResidentName = _context.Residents
                        .Where(r => r.ResidentId == h.ResidentId)
                        .Select(r => r.LastName + ", " + r.FirstName + (string.IsNullOrEmpty(r.MiddleName) ? "" : " " + r.MiddleName))
                        .FirstOrDefault() ?? "Unknown Resident"
                }).ToListAsync();

            return View(records);
        }

        // 2. GET: Admin/Health/Create
        public async Task<IActionResult> Create()
        {
            // Kukunin ang mga aktibong residente para sa dropdown ng View
            var residents = await _context.Residents
                .Where(r => r.IsResident)
                .OrderBy(r => r.LastName)
                .Select(r => new
                {
                    Id = r.ResidentId,
                    FullName = r.LastName + ", " + r.FirstName + (string.IsNullOrEmpty(r.MiddleName) ? "" : " " + r.MiddleName.Substring(0, 1) + ".")
                })
                .ToListAsync();

            ViewBag.Residents = new SelectList(residents, "Id", "FullName");

            return View(new HealthViewModel { DateRecorded = DateTime.Now });
        }

        // 3. POST: Admin/Health/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HealthViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 🔑 LUNAS: Ipinasa ang data sa mga eksaktong pangalan ng kolum sa iyong Database Entity
                var healthRecord = new HealthRecord
                {
                    ResidentId = model.ResidentId,
                    MedicalCondition = model.MedicalCondition,
                    AttendingHealthWorker = !string.IsNullOrEmpty(model.Remarks) ? model.Remarks : "Barangay Health Worker", // Sinalo ang Remarks field ng UI
                    DateLogged = model.DateRecorded,
                    LastCheckupDate = model.DateRecorded, // Default na kapareho ng Date Recorded

                    // Default/Fallback values para sa iba mo pang required entity fields para hindi mag-error sa save
                    WeightKg = 0,
                    HeightCm = 0,
                    BloodType = "N/A",
                    HealthClassification = "General",
                    IsVaccinated = false
                };

                _context.Add(healthRecord);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // Kung may validation error sa form, ire-load ang dropdown gamit ang tamang ResidentId para hindi mag-crash
            var residentsList = await _context.Residents
                .Where(r => r.IsResident)
                .OrderBy(r => r.LastName)
                .Select(r => new
                {
                    Id = r.ResidentId,
                    FullName = r.LastName + ", " + r.FirstName
                })
                .ToListAsync();

            ViewBag.Residents = new SelectList(residentsList, "Id", "FullName");

            return View(model);
        }
        // 4. GET: Admin/Health/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Hahanapin ang rekord gamit ang tamang Primary Key (.HealthRecordId)
            var record = await _context.Set<HealthRecord>()
                .FirstOrDefaultAsync(h => h.HealthRecordId == id);

            if (record == null)
            {
                return NotFound();
            }

            // I-map ang data mula sa Entity papunta sa iyong HealthViewModel
            var model = new HealthViewModel
            {
                Id = record.HealthRecordId,
                ResidentId = record.ResidentId,
                MedicalCondition = record.MedicalCondition,
                Remarks = record.AttendingHealthWorker,
                DateRecorded = record.DateLogged
            };

            // Ire-load ang mga residente para sa dropdown
            var residents = await _context.Residents
                .Where(r => r.IsResident)
                .OrderBy(r => r.LastName)
                .Select(r => new
                {
                    Id = r.ResidentId,
                    FullName = r.LastName + ", " + r.FirstName + (string.IsNullOrEmpty(r.MiddleName) ? "" : " " + r.MiddleName.Substring(0, 1) + ".")
                })
                .ToListAsync();

            ViewBag.Residents = new SelectList(residents, "Id", "FullName", model.ResidentId);

            return View(model);
        }

        // 5. POST: Admin/Health/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, HealthViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Kunin ang orihinal na rekord mula sa database
                    var recordToUpdate = await _context.Set<HealthRecord>()
                        .FirstOrDefaultAsync(h => h.HealthRecordId == id);

                    if (recordToUpdate == null)
                    {
                        return NotFound();
                    }

                    // I-update ang mga fields
                    recordToUpdate.ResidentId = model.ResidentId;
                    recordToUpdate.MedicalCondition = model.MedicalCondition;
                    recordToUpdate.AttendingHealthWorker = !string.IsNullOrEmpty(model.Remarks) ? model.Remarks : "Barangay Health Worker";
                    recordToUpdate.DateLogged = model.DateRecorded;
                    recordToUpdate.LastCheckupDate = model.DateRecorded;

                    _context.Update(recordToUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Set<HealthRecord>().Any(e => e.HealthRecordId == model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            // Kung may error sa validation, ire-load ang dropdown
            var residentsList = await _context.Residents
                .Where(r => r.IsResident)
                .OrderBy(r => r.LastName)
                .Select(r => new { Id = r.ResidentId, FullName = r.LastName + ", " + r.FirstName })
                .ToListAsync();

            ViewBag.Residents = new SelectList(residentsList, "Id", "FullName", model.ResidentId);

            return View(model);
        }
        // 6. GET: Admin/Health/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Hahanapin ang record sa database
            var record = await _context.Set<HealthRecord>()
                .FirstOrDefaultAsync(h => h.HealthRecordId == id);

            if (record == null)
            {
                return NotFound();
            }

            // I-map ang data para sa confirmation view
            var model = new HealthViewModel
            {
                Id = record.HealthRecordId,
                ResidentId = record.ResidentId,
                MedicalCondition = record.MedicalCondition,
                Remarks = record.AttendingHealthWorker,
                DateRecorded = record.DateLogged,
                ResidentName = _context.Residents
                    .Where(r => r.ResidentId == record.ResidentId)
                    .Select(r => r.LastName + ", " + r.FirstName)
                    .FirstOrDefault() ?? "Unknown Resident"
            };

            return View(model);
        }

        // 7. POST: Admin/Health/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var record = await _context.Set<HealthRecord>()
                .FirstOrDefaultAsync(h => h.HealthRecordId == id);

            if (record != null)
            {
                _context.Set<HealthRecord>().Remove(record);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}