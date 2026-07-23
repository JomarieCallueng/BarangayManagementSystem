using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// 🔑 NAMESPACES
using BarangayCMS.DAL.Context;
using BarangayCMS.Entities;
using BarangayCMS.Web.Areas.Admin.Models;

namespace BarangayCMS.Web.Areas.Admin.Controllers
{
    [Area("Admin")] // 👈 NATITIYAK NA HINDI MAG-404 ERROR ANG URL /Admin/Residents
    public class ResidentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ResidentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. GET: Admin/Residents (List View)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var residents = await _context.Residents
                .Where(r => r.IsResident) // Kunin lang ang mga active residents
                .ToListAsync();

            var modelList = residents.Select(r => new ResidentViewModel
            {
                Id = r.ResidentId, // 🔑 Ginamit ang ResidentId mula sa Entity
                FirstName = r.FirstName,
                LastName = r.LastName,
                MiddleName = r.MiddleName,
                Gender = r.Gender,
                BirthDate = r.BirthDate,
                CivilStatus = r.CivilStatus,
                ContactNumber = r.ContactNumber,
                Address = r.HouseNumber,
                Purok = r.SitioPurok,
                IsVoter = r.IsVoter // 🗳️ Voting Status
            }).ToList();

            return View(modelList);
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
                Address = resident.HouseNumber,
                Purok = resident.SitioPurok,
                IsVoter = resident.IsVoter
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
                    IsVoter = model.IsVoter, // 🗳️ Isave ang Voter status
                    IsResident = true,
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
                IsVoter = resident.IsVoter // 🔑 Kukunin ang kasalukuyang value mula sa DB
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

                // 🔄 MAP VIEWMODEL TO ENTITY
                resident.FirstName = model.FirstName;
                resident.LastName = model.LastName;
                resident.MiddleName = model.MiddleName ?? string.Empty;
                resident.Gender = model.Gender;
                resident.BirthDate = model.BirthDate;
                resident.CivilStatus = model.CivilStatus;
                resident.ContactNumber = model.ContactNumber ?? string.Empty;
                resident.HouseNumber = model.Address ?? string.Empty;
                resident.SitioPurok = model.Purok ?? string.Empty;

                // 🔑 ITO ANG MAGSE-SAVE NG VOTER STATUS
                resident.IsVoter = model.IsVoter;

                _context.Update(resident);
                await _context.SaveChangesAsync(); // 💾 I-save sa SQL Database

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
                // Soft delete (pwedeng palitan ng _context.Residents.Remove(resident) kung totoong delete)
                resident.IsResident = false;
                _context.Update(resident);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}