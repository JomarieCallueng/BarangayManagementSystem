using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BarangayCMS.DAL.Context;
using BarangayCMS.Entities;
using BarangayCMS.Web.Areas.Admin.Models;

namespace BarangayCMS.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ResidentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ResidentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. GET: Admin/Residents (List View with Search & Purok Filter)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Index(string searchTerm, string purokFilter)
        {
            // 💡 1. Kunin ang LAHAT ng residente mula sa database (WALA NANG .Where(r => r.IsResident))
            var residentsList = await _context.Residents.ToListAsync();

            // 💡 2. Dynamic list ng mga Purok/Sitio mula sa totoong data sa database para sa Dropdown
            var purokOptions = residentsList
                .Where(r => !string.IsNullOrWhiteSpace(r.SitioPurok))
                .Select(r => r.SitioPurok.Trim())
                .Distinct()
                .OrderBy(p => p)
                .ToList();

            // 💡 3. I-map ang entities papunta sa ResidentViewModel
            var query = residentsList.Select(r => new ResidentViewModel
            {
                Id = r.ResidentId,
                FirstName = r.FirstName ?? string.Empty,
                LastName = r.LastName ?? string.Empty,
                MiddleName = r.MiddleName,
                Gender = r.Gender ?? string.Empty,
                BirthDate = r.BirthDate,
                CivilStatus = string.IsNullOrWhiteSpace(r.CivilStatus) ? "Single" : r.CivilStatus,
                ContactNumber = r.ContactNumber,
                Address = string.IsNullOrEmpty(r.HouseNumber)
                    ? (r.Street ?? string.Empty)
                    : $"{r.HouseNumber} {r.Street}".Trim(),
                Purok = string.IsNullOrWhiteSpace(r.SitioPurok) ? "N/A" : r.SitioPurok,
                IsVoter = r.IsVoter,
                DateRegistered = r.CreatedAt
            }).AsQueryable();

            // 💡 4. Search Filter (First Name, Last Name, Middle Name)
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(r =>
                    (!string.IsNullOrEmpty(r.FirstName) && r.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(r.LastName) && r.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(r.MiddleName) && r.MiddleName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                );
            }

            // 💡 5. Purok Filter
            if (!string.IsNullOrWhiteSpace(purokFilter))
            {
                query = query.Where(r =>
                    r.Purok != null && r.Purok.Equals(purokFilter, StringComparison.OrdinalIgnoreCase));
            }

            // 💡 6. I-pass ang Filter options at Current Filters sa ViewBag
            ViewBag.PurokList = new SelectList(purokOptions, purokFilter);
            ViewBag.CurrentSearch = searchTerm;
            ViewBag.CurrentPurok = purokFilter;

            return View(query.OrderBy(r => r.LastName).ToList());
        }

        // ==========================================
        // 2. GET: Admin/Residents/Details/5
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var resident = await _context.Residents
                .FirstOrDefaultAsync(m => m.ResidentId == id);

            if (resident == null) return NotFound();

            var viewModel = new ResidentViewModel
            {
                Id = resident.ResidentId,
                FirstName = resident.FirstName,
                LastName = resident.LastName,
                MiddleName = resident.MiddleName,
                Gender = resident.Gender,
                BirthDate = resident.BirthDate,
                CivilStatus = resident.CivilStatus,
                ContactNumber = resident.ContactNumber,
                Address = string.IsNullOrEmpty(resident.HouseNumber) ? resident.Street : $"{resident.HouseNumber} {resident.Street}".Trim(),
                Purok = string.IsNullOrWhiteSpace(resident.SitioPurok) ? "N/A" : resident.SitioPurok,
                IsVoter = resident.IsVoter,
                DateRegistered = resident.CreatedAt
            };

            return View(viewModel);
        }

        // ==========================================
        // 3. GET: Admin/Residents/Create
        // ==========================================
        [HttpGet]
        public IActionResult Create()
        {
            return View(new ResidentViewModel());
        }

        // ==========================================
        // 4. POST: Admin/Residents/Create
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ResidentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var resident = new Resident
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    MiddleName = model.MiddleName ?? string.Empty,
                    Gender = model.Gender,
                    BirthDate = model.BirthDate,
                    CivilStatus = model.CivilStatus,
                    ContactNumber = model.ContactNumber ?? string.Empty,
                    HouseNumber = model.Address ?? string.Empty,
                    SitioPurok = model.Purok ?? string.Empty,
                    IsVoter = model.IsVoter,
                    IsResident = true, // Automatic active
                    CreatedAt = DateTime.Now
                };

                _context.Residents.Add(resident);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // ==========================================
        // 5. GET: Admin/Residents/Edit/5
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var resident = await _context.Residents.FindAsync(id);
            if (resident == null) return NotFound();

            var viewModel = new ResidentViewModel
            {
                Id = resident.ResidentId,
                FirstName = resident.FirstName,
                LastName = resident.LastName,
                MiddleName = resident.MiddleName,
                Gender = resident.Gender,
                BirthDate = resident.BirthDate,
                CivilStatus = resident.CivilStatus,
                ContactNumber = resident.ContactNumber,
                Address = resident.HouseNumber,
                Purok = resident.SitioPurok,
                IsVoter = resident.IsVoter
            };

            return View(viewModel);
        }

        // ==========================================
        // 6. POST: Admin/Residents/Edit/5
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ResidentViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var resident = await _context.Residents.FindAsync(id);
                if (resident == null) return NotFound();

                resident.FirstName = model.FirstName;
                resident.LastName = model.LastName;
                resident.MiddleName = model.MiddleName ?? string.Empty;
                resident.Gender = model.Gender;
                resident.BirthDate = model.BirthDate;
                resident.CivilStatus = model.CivilStatus;
                resident.ContactNumber = model.ContactNumber ?? string.Empty;
                resident.HouseNumber = model.Address ?? string.Empty;
                resident.SitioPurok = model.Purok ?? string.Empty;
                resident.IsVoter = model.IsVoter;

                _context.Update(resident);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // ==========================================
        // 7. GET: Admin/Residents/Delete/5
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var resident = await _context.Residents
                .FirstOrDefaultAsync(m => m.ResidentId == id);

            if (resident == null) return NotFound();

            var viewModel = new ResidentViewModel
            {
                Id = resident.ResidentId,
                FirstName = resident.FirstName,
                LastName = resident.LastName,
                MiddleName = resident.MiddleName,
                Address = resident.HouseNumber,
                Purok = resident.SitioPurok
            };

            return View(viewModel);
        }

        // ==========================================
        // 8. POST: Admin/Residents/Delete/5
        // ==========================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var resident = await _context.Residents.FindAsync(id);
            if (resident != null)
            {
                // Hard delete para tuluyang maalis sa database
                _context.Residents.Remove(resident);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}