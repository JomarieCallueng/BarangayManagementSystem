using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BarangayCMS.DAL.Context;
using BarangayCMS.Entities;
using BarangayCMS.Web.Areas.Admin.Models;
using System;
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

            var viewModel = new BarangayOfficialViewModel
            {
                Id = official.BarangayOfficialId,
                FullName = official.FullName,
                Position = official.Position,
                Committee = official.Committee,
                SignaturePath = official.SignaturePath,
                IsActive = official.IsActive
            };

            return View(viewModel);
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
                string uniqueFileName = string.Empty;

                if (model.SignatureFile != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "signatures");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    uniqueFileName = Guid.NewGuid().ToString() + "_" + model.SignatureFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.SignatureFile.CopyToAsync(fileStream);
                    }
                }

                var official = new BarangayOfficial
                {
                    FullName = model.FullName,
                    Position = model.Position,
                    Committee = model.Committee ?? string.Empty,
                    SignaturePath = uniqueFileName,
                    IsActive = model.IsActive
                };

                _context.BarangayOfficials.Add(official); // Tahasang tinukoy ang DbSet (.BarangayOfficials)
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

            var viewModel = new BarangayOfficialViewModel
            {
                Id = official.BarangayOfficialId,
                FullName = official.FullName,
                Position = official.Position,
                Committee = official.Committee,
                SignaturePath = official.SignaturePath,
                IsActive = official.IsActive
            };

            return View(viewModel);
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

                    if (model.SignatureFile != null)
                    {
                        // I-delete ang lumang file kung may bago para hindi magsikip ang storage
                        if (!string.IsNullOrEmpty(official.SignaturePath))
                        {
                            string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "signatures", official.SignaturePath);
                            if (System.IO.File.Exists(oldFilePath)) System.IO.File.Delete(oldFilePath);
                        }

                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "signatures");
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.SignatureFile.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.SignatureFile.CopyToAsync(fileStream);
                        }
                        official.SignaturePath = uniqueFileName;
                    }

                    official.FullName = model.FullName;
                    official.Position = model.Position;
                    official.Committee = model.Committee ?? string.Empty;
                    official.IsActive = model.IsActive;

                    _context.BarangayOfficials.Update(official); // Tahasang tinukoy ang DbSet (.BarangayOfficials)
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

            var viewModel = new BarangayOfficialViewModel
            {
                Id = official.BarangayOfficialId,
                FullName = official.FullName,
                Position = official.Position,
                IsActive = official.IsActive
            };

            return View(viewModel);
        }

        // 8. POST: Admin/BarangayOfficials/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var official = await _context.BarangayOfficials.FindAsync(id);
            if (official != null)
            {
                // Burahin din ang signature image sa physical folder bago tanggalin ang record sa DB
                if (!string.IsNullOrEmpty(official.SignaturePath))
                {
                    string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "signatures", official.SignaturePath);
                    if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                }

                _context.BarangayOfficials.Remove(official);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}