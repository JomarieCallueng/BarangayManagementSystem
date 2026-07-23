using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using BarangayCMS.Areas.Staff.ViewModels;
using BarangayCMS.BLL.Interfaces;
using BarangayCMS.DTO;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BarangayCMS.Areas.Staff.Controllers
{
    [Area("Staff")]
    public class ComplaintsController : Controller
    {
        private readonly IComplaintService _complaintService;
        private readonly IResidentService _residentService;

        public ComplaintsController(IComplaintService complaintService, IResidentService residentService)
        {
            _complaintService = complaintService;
            _residentService = residentService;
        }

        // GET: /Staff/Complaints/Index
        public async Task<IActionResult> Index()
        {
            var dtoList = await _complaintService.GetAllComplaintsAsync();

            // I-map ang DTO papuntang ViewModel base sa saktong properties ng DTO mo
            var viewModelList = dtoList.Select(c => new ComplaintViewModel
            {
                ComplaintId = c.Id,
                ResidentId = c.ComplainantResidentId ?? 0,
                ResidentName = string.IsNullOrEmpty(c.ComplainantName) ? "Walk-in Resident" : c.ComplainantName,
                Subject = c.CaseNumber, // Ginawang Case Number ang Identifier sa table
                Description = c.Details,
                DateSubmitted = c.CreatedDate,
                Status = c.Status
            }).ToList();

            return View(viewModelList);
        }

        // GET: /Staff/Complaints/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var c = await _complaintService.GetComplaintByIdAsync(id);
            if (c == null) return NotFound();

            var viewModel = new ComplaintViewModel
            {
                ComplaintId = c.Id,
                ResidentId = c.ComplainantResidentId ?? 0,
                ResidentName = c.ComplainantName,
                Subject = c.CaseNumber,
                Description = c.Details,
                DateSubmitted = c.CreatedDate,
                Status = c.Status
            };

            return View(viewModel);
        }

        // GET: /Staff/Complaints/Create
        public async Task<IActionResult> Create()
        {
            var residents = await _residentService.GetAllResidentsAsync();
            ViewBag.ResidentsList = new SelectList(residents.Select(r => new {
                Id = r.Id,
                FullName = $"{r.LastName}, {r.FirstName} {r.MiddleName}"
            }), "Id", "FullName");

            return View(new ComplaintViewModel());
        }

        // POST: /Staff/Complaints/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ComplaintViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kumuha ng pangalan ng Complainant kung may napiling Resident ID
                string complainantName = model.ResidentName ?? "";
                if (model.ResidentId > 0)
                {
                    var residents = await _residentService.GetAllResidentsAsync();
                    var resident = residents.FirstOrDefault(r => r.Id == model.ResidentId);
                    if (resident != null)
                    {
                        complainantName = $"{resident.LastName}, {resident.FirstName}";
                    }
                }

                // I-populate ang totoong ComplaintDTO
                var dto = new ComplaintDTO
                {
                    CaseNumber = $"BLOTTER-{DateTime.Now.ToString("yyyy")}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}",
                    ComplainantResidentId = model.ResidentId > 0 ? model.ResidentId : (int?)null,
                    ComplainantName = complainantName,
                    Details = model.Description ?? string.Empty,
                    IncidentDate = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    Status = "Pending"
                };

                // 🔑 Saktong tawag sa interface method mo
                bool isSaved = await _complaintService.FileComplaintAsync(dto);
                if (isSaved) return RedirectToAction(nameof(Index));

                ModelState.AddModelError(string.Empty, "Hindi mai-save ang reklamo sa database.");
            }

            var allResidents = await _residentService.GetAllResidentsAsync();
            ViewBag.ResidentsList = new SelectList(allResidents.Select(r => new {
                Id = r.Id,
                FullName = $"{r.LastName}, {r.FirstName}"
            }), "Id", "FullName", model.ResidentId);
            return View(model);
        }

        // GET: /Staff/Complaints/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var c = await _complaintService.GetComplaintByIdAsync(id);
            if (c == null) return NotFound();

            var viewModel = new ComplaintViewModel
            {
                ComplaintId = c.Id,
                ResidentName = c.ComplainantName,
                Subject = c.CaseNumber,
                Description = c.Details,
                Status = c.Status
            };

            return View(viewModel);
        }

        // POST: /Staff/Complaints/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ComplaintViewModel model)
        {
            // Dahil walang Update method para sa buong body, gagamitin natin ang UpdateComplaintStatusAsync mo
            bool isUpdated = await _complaintService.UpdateComplaintStatusAsync(id, model.Status, "Updated by Staff via Dashboard");

            if (isUpdated) return RedirectToAction(nameof(Index));

            ModelState.AddModelError(string.Empty, "Failed to update complaint status.");
            return View(model);
        }

        // 🔑 DISMISS/DISREGARD FLOW (Dahil walang Delete sa Repository)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Dismiss(int id)
        {
            // Imbis na burahin sa DB, binabago natin ang status nito gamit ang interface mo para maging "Dismissed"
            await _complaintService.UpdateComplaintStatusAsync(id, "Dismissed", "Cancelled/Dismissed by Staff");
            return RedirectToAction(nameof(Index));
        }
    }
}