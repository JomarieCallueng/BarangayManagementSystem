using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BarangayCMS.Areas.Staff.ViewModels;
using BarangayCMS.BLL.Interfaces;
using BarangayCMS.DTO;
using Mammoth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BarangayCMS.DAL.Context; // Galing sa inyong DAL project

namespace BarangayCMS.Areas.Staff.Controllers
{
    [Area("Staff")]
    public class CertificatesController : Controller
    {
        private readonly ICertificateService _certificateService;
        private readonly IResidentService _residentService;
        private readonly ApplicationDbContext _context;

        public CertificatesController(
            ICertificateService certificateService,
            IResidentService residentService,
            ApplicationDbContext context)
        {
            _certificateService = certificateService;
            _residentService = residentService;
            _context = context;
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
                ControlNumber = c.ControlNumber,
                FeePaid = c.FeePaid,
                PaymentReceiptPath = c.PaymentReceiptPath,
                OfficialReceiptNumber = c.OfficialReceiptNumber,
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
            string controlNumber = $"BRGY-{DateTime.Now.ToString("yyyyMMdd")}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";
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
        public async Task<IActionResult> Print(int id)
        {
            var certificateDto = await _certificateService.GetCertificateByIdAsync(id);
            if (certificateDto == null) return NotFound();

            var controlNo = string.IsNullOrEmpty(certificateDto.ControlNumber)
                ? $"BRGY-{DateTime.Now.ToString("yyyyMMdd")}-{certificateDto.Id}"
                : certificateDto.ControlNumber;

            var resident = await _residentService.GetResidentByIdAsync(certificateDto.ResidentId);

            string htmlContent = "";

            if (resident != null)
            {
                // Kunin ang record mula sa database
                var certificateTypeEntity = await _context.CertificateTypes
                    .FirstOrDefaultAsync(ct => ct.CertificateName == certificateDto.CertificateType);

                // Gagamit ng 'TemplateData' na nahanap natin sa CertificateType.cs ninyo
                byte[]? templateBytes = certificateTypeEntity?.TemplateData;

                if (templateBytes != null && templateBytes.Length > 0)
                {
                    try
                    {
                        using (var stream = new MemoryStream(templateBytes))
                        {
                            var converter = new Mammoth.DocumentConverter();
                            var result = converter.ConvertToHtml(stream);
                            htmlContent = result.Value;
                        }

                        // Pagpapalit ng placeholders
                        string middleInit = !string.IsNullOrEmpty(resident.MiddleName) ? $"{resident.MiddleName[0]}." : "";
                        string suffix = !string.IsNullOrEmpty(resident.Suffix) ? $" {resident.Suffix}" : "";
                        string buongPangalan = $"{resident.FirstName} {middleInit} {resident.LastName}{suffix}".Trim();

                        string addressPart = $"{resident.HouseNumber} {resident.Street}".Trim();
                        string sitioPart = !string.IsNullOrEmpty(resident.SitioPurok) ? $", {resident.SitioPurok}" : "";
                        string kumpletongAddress = $"{addressPart}{sitioPart}".Trim();

                        int edad = DateTime.Now.Year - resident.BirthDate.Year;
                        if (resident.BirthDate.Date > DateTime.Now.AddYears(-edad)) edad--;

                        htmlContent = htmlContent.Replace("{{pangalan}}", $"<strong>{buongPangalan}</strong>");
                        htmlContent = htmlContent.Replace("{{tirahan}}", $"<strong>{kumpletongAddress}</strong>");
                        htmlContent = htmlContent.Replace("{{edad}}", $"<strong>{edad}</strong>");
                        htmlContent = htmlContent.Replace("{{purpose}}", $"<strong>{certificateDto.Purpose}</strong>");
                        htmlContent = htmlContent.Replace("{{control_no}}", $"<strong>{controlNo}</strong>");
                        htmlContent = htmlContent.Replace("{{petsa}}", $"<strong>{DateTime.Now.ToString("dd MMMM yyyy")}</strong>");
                    }
                    catch (Exception ex)
                    {
                        htmlContent = $"<p class='text-danger'>Error sa pag-convert ng Word template mula sa database: {ex.Message}</p>";
                    }
                }
                else
                {
                    htmlContent = "<p class='text-danger text-center p-4'>May record sa database pero walang laman ang naka-upload na file (0 bytes).</p>";
                }
            }
            else
            {
                htmlContent = "<p class='text-danger text-center p-4'>Hindi nahanap ang record ng residente.</p>";
            }

            ViewBag.HtmlContent = htmlContent;

            var viewModel = new CertificateViewModel
            {
                CertificateId = certificateDto.Id,
                ResidentId = certificateDto.ResidentId,
                ResidentName = certificateDto.ResidentName,
                CertificateType = certificateDto.CertificateType,
                Purpose = certificateDto.Purpose,
                ControlNumber = controlNo,
                FeePaid = certificateDto.FeePaid,
                PaymentReceiptPath = certificateDto.PaymentReceiptPath,
                OfficialReceiptNumber = certificateDto.OfficialReceiptNumber,
                Status = certificateDto.Status,
                DateIssued = certificateDto.IssuedDate != default ? certificateDto.IssuedDate : DateTime.Now,
                IssuedBy = string.IsNullOrEmpty(certificateDto.IssuedBy) ? "PUNONG BARANGAY" : certificateDto.IssuedBy
            };

            return View(viewModel);
        }

        // GET: /Staff/Certificates/Create
        public async Task<IActionResult> Create()
        {
            var residents = await _residentService.GetAllResidentsAsync();
            ViewBag.ResidentsList = new SelectList(residents.Select(r => new {
                Id = r.Id,
                FullName = $"{r.LastName}, {r.FirstName} {r.MiddleName}"
            }), "Id", "FullName");

            return View(new CertificateViewModel());
        }

        // POST: /Staff/Certificates/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CertificateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var newCertificateDto = new CertificateDTO
                {
                    ResidentId = model.ResidentId,
                    CertificateType = model.CertificateType ?? string.Empty,
                    Purpose = model.Purpose ?? string.Empty,
                    FeePaid = model.FeePaid,
                    PaymentReceiptPath = model.PaymentReceiptPath,
                    OfficialReceiptNumber = model.OfficialReceiptNumber ?? string.Empty,
                    Status = model.Status,
                    ControlNumber = model.Status == "Issued" || model.Status == "Approved" ? $"BRGY-{DateTime.Now.ToString("yyyyMMdd")}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}" : string.Empty,
                    IssuedDate = model.Status == "Issued" || model.Status == "Approved" ? DateTime.Now : default,
                    IssuedBy = User.Identity?.Name ?? "Staff Admin"
                };

                bool isSaved = await _certificateService.RequestCertificateAsync(newCertificateDto);
                if (isSaved) return RedirectToAction(nameof(Index));

                ModelState.AddModelError(string.Empty, "Nagkaroon ng problema sa pag-save.");
            }

            var residents = await _residentService.GetAllResidentsAsync();
            ViewBag.ResidentsList = new SelectList(residents.Select(r => new {
                Id = r.Id,
                FullName = $"{r.LastName}, {r.FirstName} {r.MiddleName}"
            }), "Id", "FullName");

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
                OfficialReceiptNumber = certificateDto.OfficialReceiptNumber,
                Status = certificateDto.Status,
                DateIssued = certificateDto.IssuedDate != default ? certificateDto.IssuedDate : (DateTime?)null,
                IssuedBy = certificateDto.IssuedBy
            };

            return View(viewModel);
        }
    }
}