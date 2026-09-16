using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BarangayCMS.DAL.Context;
using BarangayCMS.Entities;
using BarangayCMS.Web.Areas.Admin.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace BarangayCMS.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BarangayOfficialsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BarangayOfficialsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // 1. GET: Admin/BarangayOfficials
        public async Task<IActionResult> Index()
        {
            var officials = await _context.BarangayOfficials
                .OrderBy(o => o.Position == "Barangay Captain" ? 0 : 1)
                .ThenBy(o => o.BarangayOfficialId)
                .Select(o => new BarangayOfficialViewModel
                {
                    Id = o.BarangayOfficialId,
                    FullName = o.FullName,
                    Position = o.Position,
                    Committee = o.Committee,
                    SignaturePath = o.SignaturePath,
                    ProfileImagePath = o.ProfileImagePath,
                    IsActive = o.IsActive
                }).ToListAsync();

            return View(officials);
        }

        // 2. GET: Admin/BarangayOfficials/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var official = await _context.BarangayOfficials
                .FirstOrDefaultAsync(m => m.BarangayOfficialId == id);

            if (official == null) return NotFound();

            return View(ToViewModel(official));
        }

        // 3. GET: Admin/BarangayOfficials/Create
        public IActionResult Create()
        {
            return View(new BarangayOfficialViewModel { IsActive = true });
        }

        // 4. POST: Admin/BarangayOfficials/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BarangayOfficialViewModel model)
        {
            if (ModelState.IsValid)
            {
                var official = new BarangayOfficial
                {
                    FullName = model.FullName,
                    Position = model.Position,
                    Committee = model.Committee ?? string.Empty,
                    SignaturePath = await SaveUploadAsync(model.SignatureFile, "signatures"),
                    ProfileImagePath = await SaveUploadAsync(model.ProfileImageFile, "officials"),
                    IsActive = model.IsActive
                };

                _context.BarangayOfficials.Add(official);
                await _context.SaveChangesAsync();

                // Awtomatikong itala ang kasalukuyang termino sa service history ng taong ito
                // para agad may laman ang public na "Official History" section.
                _context.OfficialServiceHistories.Add(new OfficialServiceHistory
                {
                    BarangayOfficialId = official.BarangayOfficialId,
                    Position = official.Position,
                    Committee = official.Committee,
                    StartDate = DateTime.Now,
                    EndDate = null,
                    Status = "Current"
                });
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // 5. GET: Admin/BarangayOfficials/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var official = await _context.BarangayOfficials.FindAsync(id.Value);
            if (official == null) return NotFound();

            return View(ToViewModel(official));
        }

        // 6. POST: Admin/BarangayOfficials/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BarangayOfficialViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var official = await _context.BarangayOfficials.FindAsync(id);
                    if (official == null) return NotFound();

                    // E-Signature: palitan lang kung may bagong in-upload.
                    if (model.SignatureFile != null && model.SignatureFile.Length > 0)
                    {
                        DeleteUpload(official.SignaturePath, "signatures");
                        official.SignaturePath = await SaveUploadAsync(model.SignatureFile, "signatures");
                    }

                    // Profile photo: palitan lang kung may bagong in-upload.
                    if (model.ProfileImageFile != null && model.ProfileImageFile.Length > 0)
                    {
                        DeleteUpload(official.ProfileImagePath, "officials");
                        official.ProfileImagePath = await SaveUploadAsync(model.ProfileImageFile, "officials");
                    }

                    official.FullName = model.FullName;
                    official.Position = model.Position;
                    official.Committee = model.Committee ?? string.Empty;
                    official.IsActive = model.IsActive;

                    _context.BarangayOfficials.Update(official);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.BarangayOfficials.Any(e => e.BarangayOfficialId == model.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // 7. GET: Admin/BarangayOfficials/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var official = await _context.BarangayOfficials
                .FirstOrDefaultAsync(m => m.BarangayOfficialId == id);

            if (official == null) return NotFound();

            return View(ToViewModel(official));
        }

        // 8. POST: Admin/BarangayOfficials/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var official = await _context.BarangayOfficials.FindAsync(id);
            if (official != null)
            {
                // Burahin ang mga naka-upload na file (signature + profile photo).
                DeleteUpload(official.SignaturePath, "signatures");
                DeleteUpload(official.ProfileImagePath, "officials");

                // Ang service history ay cascade-delete na kasama ng opisyal.
                _context.BarangayOfficials.Remove(official);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // SERVICE HISTORY MANAGEMENT (per official)
        // ============================================================

        // 9. GET: Admin/BarangayOfficials/History/5
        public async Task<IActionResult> History(int? id)
        {
            if (id == null) return NotFound();

            var official = await _context.BarangayOfficials
                .Include(o => o.ServiceHistories)
                .FirstOrDefaultAsync(o => o.BarangayOfficialId == id);
            if (official == null) return NotFound();

            return View(BuildHistoryViewModel(official));
        }

        // 10. POST: Admin/BarangayOfficials/AddHistory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddHistory(OfficialHistoryViewModel model)
        {
            var official = await _context.BarangayOfficials
                .Include(o => o.ServiceHistories)
                .FirstOrDefaultAsync(o => o.BarangayOfficialId == model.OfficialId);
            if (official == null) return NotFound();

            // Manu-manong validation para sa magkatugmang taon.
            if (model.NewEndYear.HasValue && model.NewStartYear.HasValue && model.NewEndYear < model.NewStartYear)
            {
                ModelState.AddModelError("NewEndYear", "Ang taon ng pagtatapos ay hindi pwedeng mas maaga sa simula.");
            }

            if (!ModelState.IsValid)
            {
                var vm = BuildHistoryViewModel(official);
                // Panatilihin ang na-type ng user.
                vm.NewPosition = model.NewPosition;
                vm.NewCommittee = model.NewCommittee;
                vm.NewStartYear = model.NewStartYear;
                vm.NewEndYear = model.NewEndYear;
                return View("History", vm);
            }

            bool isCurrent = !model.NewEndYear.HasValue;

            _context.OfficialServiceHistories.Add(new OfficialServiceHistory
            {
                BarangayOfficialId = official.BarangayOfficialId,
                Position = model.NewPosition.Trim(),
                Committee = (model.NewCommittee ?? string.Empty).Trim(),
                StartDate = new DateTime(model.NewStartYear!.Value, 1, 1),
                EndDate = model.NewEndYear.HasValue ? new DateTime(model.NewEndYear.Value, 12, 31) : (DateTime?)null,
                Status = isCurrent ? "Current" : "Completed"
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Naidagdag ang termino sa service history.";
            return RedirectToAction(nameof(History), new { id = official.BarangayOfficialId });
        }

        // 11. POST: Admin/BarangayOfficials/DeleteHistory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteHistory(int historyId, int officialId)
        {
            var term = await _context.OfficialServiceHistories
                .FirstOrDefaultAsync(h => h.Id == historyId && h.BarangayOfficialId == officialId);
            if (term != null)
            {
                _context.OfficialServiceHistories.Remove(term);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Natanggal ang termino sa service history.";
            }
            return RedirectToAction(nameof(History), new { id = officialId });
        }

        // ============================================================
        // HELPERS
        // ============================================================

        private BarangayOfficialViewModel ToViewModel(BarangayOfficial o) => new BarangayOfficialViewModel
        {
            Id = o.BarangayOfficialId,
            FullName = o.FullName,
            Position = o.Position,
            Committee = o.Committee,
            SignaturePath = o.SignaturePath,
            ProfileImagePath = o.ProfileImagePath,
            IsActive = o.IsActive
        };

        private OfficialHistoryViewModel BuildHistoryViewModel(BarangayOfficial official) => new OfficialHistoryViewModel
        {
            OfficialId = official.BarangayOfficialId,
            OfficialName = official.FullName,
            CurrentPosition = official.Position,
            Terms = official.ServiceHistories
                .OrderByDescending(h => h.StartDate)
                .Select(h => new ServiceHistoryItem
                {
                    Id = h.Id,
                    Position = h.Position,
                    Committee = h.Committee,
                    StartDate = h.StartDate,
                    EndDate = h.EndDate,
                    Status = h.Status
                }).ToList()
        };

        // Ise-save ang isang naka-upload na file gamit ang natatanging pangalan; nagbabalik
        // ng filename (o "" kung walang file). Ginagamit para sa signatures at officials.
        private async Task<string> SaveUploadAsync(Microsoft.AspNetCore.Http.IFormFile? file, string subfolder)
        {
            if (file == null || file.Length == 0) return string.Empty;

            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", subfolder);
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            // Natatanging pangalan na hindi umaasa sa buong user-supplied filename (iwas traversal).
            var ext = Path.GetExtension(file.FileName);
            string uniqueFileName = Guid.NewGuid().ToString("N") + ext;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            return uniqueFileName;
        }

        private void DeleteUpload(string? fileName, string subfolder)
        {
            if (string.IsNullOrEmpty(fileName)) return;
            try
            {
                // Huling segment lamang — iwas path traversal.
                var safe = Path.GetFileName(fileName);
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", subfolder, safe);
                if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
            }
            catch { /* best-effort */ }
        }
    }
}
