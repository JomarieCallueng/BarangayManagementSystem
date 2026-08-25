using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BarangayCMS.Areas.Staff.ViewModels;
using BarangayCMS.BLL.Interfaces;
using BarangayCMS.DAL.Context;
using BarangayCMS.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BarangayCMS.Areas.Staff.Controllers
{
    [Area("Staff")]
    public class ResidentsController : Controller
    {
        private readonly IResidentService _residentService;
        private readonly ApplicationDbContext _context;

        public ResidentsController(IResidentService residentService, ApplicationDbContext context)
        {
            _residentService = residentService;
            _context = context;
        }

        // GET: /Staff/Residents/Index
        public async Task<IActionResult> Index(string searchTerm, string purokFilter)
        {
            var dtoList = await _residentService.GetAllResidentsAsync();

            var query = dtoList.Select(r => new ResidentViewModel
            {
                ResidentId = r.Id,
                FirstName = r.FirstName,
                LastName = r.LastName,
                MiddleName = r.MiddleName,
                Gender = r.Gender,
                CivilStatus = r.CivilStatus,
                ContactNumber = r.ContactNumber,
                IsVoter = r.IsVoter,
                Address = r.FullAddress ?? r.Street ?? string.Empty
            }).AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(r =>
                    (!string.IsNullOrEmpty(r.FirstName) && r.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(r.LastName) && r.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(r.MiddleName) && r.MiddleName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                );
            }

            if (!string.IsNullOrWhiteSpace(purokFilter))
            {
                query = query.Where(r => r.Address != null && r.Address.Contains(purokFilter, StringComparison.OrdinalIgnoreCase));
            }

            var viewModelList = query.OrderBy(r => r.LastName).ToList();

            var purokOptions = new List<string>
            {
                "Chicago / Ohio Area",
                "Kalasag Area",
                "Kubo Area",
                "Tagalog Area"
            };

            ViewBag.PurokList = new SelectList(purokOptions, purokFilter);
            ViewBag.CurrentSearch = searchTerm;
            ViewBag.CurrentPurok = purokFilter;

            return View(viewModelList);
        }

        // GET: /Staff/Residents/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var residentDto = await _residentService.GetResidentByIdAsync(id);

            if (residentDto == null)
            {
                return NotFound();
            }

            var viewModel = new ResidentViewModel
            {
                ResidentId = residentDto.Id,
                FirstName = residentDto.FirstName,
                LastName = residentDto.LastName,
                MiddleName = residentDto.MiddleName,
                Gender = residentDto.Gender,
                CivilStatus = residentDto.CivilStatus,
                ContactNumber = residentDto.ContactNumber,
                IsVoter = residentDto.IsVoter,
                Address = residentDto.FullAddress,
                BirthDate = residentDto.BirthDate
            };

            return View(viewModel);
        }

        // GET: /Staff/Residents/Create
        public IActionResult Create()
        {
            return View(new ResidentViewModel());
        }

        // POST: /Staff/Residents/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ResidentViewModel model)
        {
            // Alisin ang validation para sa ResidentId dahil bago pa lang ito (wala pang ID)
            ModelState.Remove("ResidentId");

            if (!ModelState.IsValid)
            {
                // IPAPASOK NITO SA CONSOLE/OUTPUT WINDOW KUNG ANO ANG NAGPA-FAIL SA VALIDATION
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine($"VALIDATION ERROR ON '{state.Key}': {error.ErrorMessage}");
                    }
                }
                return View(model);
            }

            try
            {
                string fullStreetAddress = string.IsNullOrWhiteSpace(model.Purok)
                    ? (model.Address ?? string.Empty)
                    : $"{model.Address}, {model.Purok}";

                var newResidentDto = new ResidentDTO
                {
                    FirstName = model.FirstName ?? string.Empty,
                    LastName = model.LastName ?? string.Empty,
                    MiddleName = model.MiddleName ?? string.Empty,
                    BirthDate = model.BirthDate,
                    Gender = model.Gender ?? string.Empty,
                    CivilStatus = model.CivilStatus ?? string.Empty,
                    ContactNumber = model.ContactNumber ?? string.Empty,
                    IsVoter = model.IsVoter,
                    IsResident = true,
                    CreatedAt = DateTime.Now,
                    Street = fullStreetAddress
                };

                bool isSaved = await _residentService.RegisterResidentAsync(newResidentDto);

                if (isSaved)
                {
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, "Nagkaroon ng problema sa pag-save sa database. Subukan muli.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Database/Server Error: " + ex.Message);
            }

            return View(model);
        }

        // GET: /Staff/Residents/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var residentDto = await _residentService.GetResidentByIdAsync(id);

            if (residentDto == null)
            {
                return NotFound();
            }

            var viewModel = new ResidentViewModel
            {
                ResidentId = residentDto.Id,
                FirstName = residentDto.FirstName,
                LastName = residentDto.LastName,
                MiddleName = residentDto.MiddleName,
                Gender = residentDto.Gender,
                CivilStatus = residentDto.CivilStatus,
                ContactNumber = residentDto.ContactNumber,
                IsVoter = residentDto.IsVoter,
                Address = residentDto.Street,
                BirthDate = residentDto.BirthDate
            };

            return View(viewModel);
        }

        // POST: /Staff/Residents/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ResidentViewModel model)
        {
            if (id != model.ResidentId)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                var updatedDto = new ResidentDTO
                {
                    Id = model.ResidentId,
                    FirstName = model.FirstName ?? string.Empty,
                    LastName = model.LastName ?? string.Empty,
                    MiddleName = model.MiddleName ?? string.Empty,
                    BirthDate = model.BirthDate,
                    Gender = model.Gender ?? string.Empty,
                    CivilStatus = model.CivilStatus ?? string.Empty,
                    ContactNumber = model.ContactNumber ?? string.Empty,
                    IsVoter = model.IsVoter,
                    Street = model.Address ?? string.Empty
                };

                bool isUpdated = await _residentService.UpdateResidentInfoAsync(updatedDto);

                if (isUpdated)
                {
                    return RedirectToAction(nameof(Details), new { id = model.ResidentId });
                }

                ModelState.AddModelError(string.Empty, "Nagkaroon ng problema sa pag-update. Subukan muli.");
            }

            return View(model);
        }
    }
}