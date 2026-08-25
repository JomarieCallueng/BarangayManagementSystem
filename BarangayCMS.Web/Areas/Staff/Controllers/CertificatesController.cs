using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using BarangayCMS.Web.Areas.Staff.ViewModels;
using BarangayCMS.BLL.Interfaces;
using BarangayCMS.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BarangayCMS.DAL.Context;

namespace BarangayCMS.Areas.Staff.Controllers
{
    [Area("Staff")]
    public class CertificatesController : Controller
    {
        private readonly ICertificateService _certificateService;
        private readonly IResidentService _residentService;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public CertificatesController(
            ICertificateService certificateService,
            IResidentService residentService,
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _certificateService = certificateService;
            _residentService = residentService;
            _context = context;
            _environment = environment;
        }

        // GET: /Staff/Certificates/Index
        public async Task<IActionResult> Index()
        {
            var dtoList = await _certificateService.GetAllCertificatesAsync();
            var viewModelList = dtoList.Select(c => new CertificateViewModel
            {
                CertificateId = c.Id,
                ResidentId = c.ResidentId,
                ResidentName = c.ResidentName,
                CertificateType = c.CertificateType,
                Purpose = c.Purpose,
                ControlNumber = string.IsNullOrEmpty(c.ControlNumber) ? "N/A" : c.ControlNumber,
                FeePaid = c.FeePaid,
                PaymentReceiptPath = c.PaymentReceiptPath,
                Status = c.Status,
                DateIssued = c.IssuedDate != default ? c.IssuedDate : (DateTime?)null,
                IssuedBy = c.IssuedBy
            }).ToList();

            return View(viewModelList);
        }

        // POST: /Staff/Certificates/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            string controlNumber = $"BRGY-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";
            string currentStaff = User.Identity?.Name ?? "Staff Admin";

            bool isSuccess = await _certificateService.IssueCertificateAsync(id, controlNumber, currentStaff);

            if (!isSuccess)
            {
                isSuccess = await _certificateService.UpdateStatusAsync(id, "Approved");
            }

            if (isSuccess)
            {
                return RedirectToAction(nameof(Print), new { id = id });
            }

            TempData["Error"] = "Hindi ma-aprubahan ang sertipiko.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Staff/Certificates/Print/5
        [HttpGet]
        public async Task<IActionResult> Print(int id)
        {
            var cert = await _context.Certificates
                .Include(c => c.Resident)
                .FirstOrDefaultAsync(c => c.CertificateId == id);

            if (cert == null) return NotFound();

            // Automatic na palitan ang Status sa "Success" kapag ginamit ang Print
            cert.Status = "Success";
            cert.DateIssued ??= DateTime.Now;
            _context.Certificates.Update(cert);
            await _context.SaveChangesAsync();

            var certType = await _context.CertificateTypes
                .FirstOrDefaultAsync(ct => ct.CertificateName.ToLower() == cert.CertificateType.ToLower());

            if (certType != null && !string.IsNullOrEmpty(certType.TemplateFileName))
            {
                string fileName = certType.TemplateFileName;

                string[] possiblePaths = new[]
                {
                    Path.Combine(_environment.WebRootPath, "templates", fileName),
                    Path.Combine(_environment.WebRootPath, "uploads", fileName),
                    Path.Combine(_environment.WebRootPath, "uploads", "templates", fileName),
                    Path.Combine(_environment.WebRootPath, fileName)
                };

                string? foundPath = possiblePaths.FirstOrDefault(p => System.IO.File.Exists(p));

                if (foundPath == null && Directory.Exists(_environment.WebRootPath))
                {
                    foundPath = Directory.GetFiles(_environment.WebRootPath, fileName, SearchOption.AllDirectories).FirstOrDefault();
                }

                if (foundPath != null && System.IO.File.Exists(foundPath))
                {
                    byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(foundPath);
                    string contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

                    return File(fileBytes, contentType, $"{cert.CertificateType}_{cert.ResidentName ?? "Document"}.docx");
                }
            }

            string displayName = !string.IsNullOrEmpty(cert.ResidentName)
                ? cert.ResidentName
                : (cert.Resident != null ? $"{cert.Resident.FirstName} {cert.Resident.LastName}" : "Unknown Resident");

            int day = DateTime.Now.Day;
            string suffix = (day % 10 == 1 && day != 11) ? "st" :
                           (day % 10 == 2 && day != 12) ? "nd" :
                           (day % 10 == 3 && day != 13) ? "rd" : "th";

            ViewBag.FormattedDate = $"{day}{suffix} day of {DateTime.Now:MMMM, yyyy}";

            var viewModel = new CertificateViewModel
            {
                CertificateId = cert.CertificateId,
                ResidentId = cert.ResidentId ?? 0,
                ResidentName = displayName,
                CertificateType = string.IsNullOrEmpty(cert.CertificateType) ? "BARANGAY CERTIFICATE" : cert.CertificateType.ToUpper(),
                Purpose = cert.Purpose,
                ControlNumber = cert.ControlNumber,
                DateRequested = cert.DateRequested,
                DateIssued = cert.DateIssued ?? DateTime.Now,
                Status = cert.Status,
                FeePaid = cert.FeePaid,
                PaymentReceiptPath = cert.PaymentReceiptPath,
                IssuedBy = string.IsNullOrEmpty(cert.IssuedBy) ? "PUNONG BARANGAY" : cert.IssuedBy
            };

            return View("PrintTemplate", viewModel);
        }
        // GET: /Staff/Certificates/Create
        public async Task<IActionResult> Create()
        {
            var residents = await _residentService.GetAllResidentsAsync();
            ViewBag.ResidentsList = new SelectList(residents.Select(r => new {
                Id = r.Id,
                FullName = $"{r.LastName}, {r.FirstName} {r.MiddleName}".Trim()
            }), "Id", "FullName");

            return View(new CertificateViewModel());
        }

        // POST: /Staff/Certificates/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CertificateViewModel model)
        {
            // Opsyonal ang mga field na ito para hindi ma-block ng ModelState
            ModelState.Remove("ControlNumber");
            ModelState.Remove("ResidentName");
            ModelState.Remove("IssuedBy");
            ModelState.Remove("OfficialReceiptNumber");

            if (ModelState.IsValid)
            {
                var resident = await _residentService.GetResidentByIdAsync(model.ResidentId);
                string buongPangalan = "Unknown";

                if (resident != null)
                {
                    string middleInit = !string.IsNullOrEmpty(resident.MiddleName) ? $"{resident.MiddleName[0]}." : "";
                    string suffix = !string.IsNullOrEmpty(resident.Suffix) ? $" {resident.Suffix}" : "";
                    buongPangalan = $"{resident.FirstName} {middleInit} {resident.LastName}{suffix}".Trim();
                }

                string generatedControlNo = $"BRGY-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";

                var newCertificateDto = new CertificateDTO
                {
                    ResidentId = model.ResidentId,
                    ResidentName = buongPangalan,
                    CertificateType = model.CertificateType ?? string.Empty,
                    Purpose = model.Purpose ?? string.Empty,
                    FeePaid = model.FeePaid,
                    PaymentReceiptPath = model.PaymentReceiptPath ?? string.Empty,
                    Status = string.IsNullOrEmpty(model.Status) ? "Pending" : model.Status,
                    ControlNumber = generatedControlNo,
                    IssuedDate = DateTime.Now,
                    IssuedBy = User.Identity?.Name ?? "Staff Admin"
                };

                bool isSaved = await _certificateService.RequestCertificateAsync(newCertificateDto);
                if (isSaved)
                {
                    TempData["Success"] = "Matagumpay na na-generate at na-save sa database!";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, "Nagkaroon ng problema sa pag-save sa database.");
            }

            TempData["Error"] = "Hindi na-save ang sertipiko. Pakisuri ang mga patlang sa pormularyo.";

            var residents = await _residentService.GetAllResidentsAsync();
            ViewBag.ResidentsList = new SelectList(residents.Select(r => new {
                Id = r.Id,
                FullName = $"{r.LastName}, {r.FirstName} {r.MiddleName}".Trim()
            }), "Id", "FullName", model.ResidentId);

            return View(model);
        }

        // GET: /Staff/Certificates/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var certificateDto = await _certificateService.GetCertificateByIdAsync(id);
            if (certificateDto == null) return NotFound();

            var viewModel = new CertificateViewModel
            {
                CertificateId = certificateDto.Id,
                ResidentId = certificateDto.ResidentId,
                ResidentName = certificateDto.ResidentName,
                CertificateType = certificateDto.CertificateType,
                Purpose = certificateDto.Purpose,
                ControlNumber = certificateDto.ControlNumber,
                FeePaid = certificateDto.FeePaid,
                PaymentReceiptPath = certificateDto.PaymentReceiptPath,
                Status = certificateDto.Status,
                DateIssued = certificateDto.IssuedDate != default ? certificateDto.IssuedDate : (DateTime?)null,
                IssuedBy = certificateDto.IssuedBy
            };

            return View(viewModel);
        }
    }
}