using Microsoft.AspNetCore.Mvc;
using BarangayCMS.Areas.Staff.ViewModels;
using BarangayCMS.BLL.Interfaces; // Para mabasa ang IResidentService
using BarangayCMS.DTO;            // Para mabasa ang ResidentDTO class mo
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BarangayCMS.Areas.Staff.Controllers
{
    [Area("Staff")]
    public class ResidentsController : Controller
    {
        private readonly IResidentService _residentService;

        public ResidentsController(IResidentService residentService)
        {
            _residentService = residentService;
        }

        // GET: /Staff/Residents/Index
        public async Task<IActionResult> Index()
        {
            // 1. Kuhanin ang listahan ng ResidentDTO mula sa BLL Service
            var dtoList = await _residentService.GetAllResidentsAsync();

            // 2. I-map ang DTOs papuntang ResidentViewModel para sa iyong UI
            var viewModelList = dtoList.Select(r => new ResidentViewModel
            {
                ResidentId = r.Id,
                FirstName = r.FirstName,
                LastName = r.LastName,
                MiddleName = r.MiddleName,
                Gender = r.Gender,
                CivilStatus = r.CivilStatus,
                ContactNumber = r.ContactNumber,
                IsVoter = r.IsVoter,
                Address = r.FullAddress
            }).OrderBy(r => r.LastName).ToList();

            return View(viewModelList);
        }

        // GET: /Staff/Residents/Details/5
        public async Task<IActionResult> Details(int id)
        {
            // Kuhanin ang partikular na residente gamit ang ID mula sa BLL Service
            var residentDto = await _residentService.GetResidentByIdAsync(id);

            if (residentDto == null)
            {
                return NotFound();
            }

            // I-map ang DTO papuntang ViewModel para sa Details View
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
            if (ModelState.IsValid)
            {
                // 3. I-convert ang Form ViewModel papuntang ResidentDTO base sa hiningi ng iyong Interface
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
                    IsResident = true, // Active Resident status
                    CreatedAt = DateTime.Now,

                    // Dahil isang string lang ang 'Address' sa iyong kasalukuyang Form, 
                    // isaksak muna natin ito sa 'Street' para ligtas ang data papuntang DB
                    Street = model.Address ?? string.Empty
                };

                // 4. Tinawag ang tamang interface method gamit ang DTO
                bool isSaved = await _residentService.RegisterResidentAsync(newResidentDto);

                if (isSaved)
                {
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, "Nagkaroon ng problema sa pag-save sa database. Subukan muli.");
            }
            return View(model);
        }

        // GET: /Staff/Residents/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            // Kuhanin ang kasalukuyang data ng residente mula sa database
            var residentDto = await _residentService.GetResidentByIdAsync(id);

            if (residentDto == null)
            {
                return NotFound();
            }

            // I-map papunta sa ViewModel para mai-load sa mga input fields ng Form
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
                Address = residentDto.Street, // o kung anong string field ang gamit mo
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
                // I-convert ang binagong Form data pabalik sa DTO
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
                    // Maaari mong idagdag ang ibang fields depende sa iyong DTO architecture
                };

                // I-save ang pagbabago gamit ang iyong BLL Service method (Suriin kung ano ang eksaktong pangalan nito sa iyong interface, hal. UpdateResidentAsync)
                // Patakbuhin natin ito:
                bool isUpdated = await _residentService.UpdateResidentInfoAsync(updatedDto);

                if (isUpdated)
                {
                    // Kapag tagumpay, babalik sa Details page ng residenteng binago
                    return RedirectToAction(nameof(Details), new { id = model.ResidentId });
                }

                ModelState.AddModelError(string.Empty, "Nagkaroon ng problema sa pag-update. Subukan muli.");
            }

            return View(model);
        }
    }
}