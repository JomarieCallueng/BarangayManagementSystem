using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BarangayCMS.BLL.Interfaces;
using BarangayCMS.DTO;
using BarangayCMS.Web.Areas.Admin.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BarangayCMS.Web.Controllers
{
    // Public-facing resident flow: NOT part of the Admin area, so it uses the
    // public site layout and needs no staff/admin login.
    [Route("CertificateRequest")]
    public class CertificateRequestController : Controller
    {
        private readonly ICertificateService _certificateService;
        private readonly ICertificateRequirementService _requirementService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly BarangayCMS.DAL.Context.ApplicationDbContext _context;

        public CertificateRequestController(
            ICertificateService certificateService,
            ICertificateRequirementService requirementService,
            IWebHostEnvironment webHostEnvironment,
            BarangayCMS.DAL.Context.ApplicationDbContext context)
        {
            _certificateService = certificateService;
            _requirementService = requirementService;
            _webHostEnvironment = webHostEnvironment;
            _context = context;
        }

        // GET: /CertificateRequest/RequirementsFor?certificateType=Barangay%20Clearance
        // JSON endpoint na ginagamit ng dropdown (public at staff) para dynamic
        // na ma-load ang requirements ng napiling sertipiko nang walang refresh.
        [HttpGet("RequirementsFor")]
        public async Task<IActionResult> RequirementsFor(string? certificateType, int? certificateTypeId)
        {
            var items = certificateTypeId.HasValue && certificateTypeId.Value > 0
                ? await _requirementService.GetActiveByCertificateTypeIdAsync(certificateTypeId.Value)
                : await _requirementService.GetActiveByCertificateNameAsync(certificateType ?? string.Empty);

            var payload = items.Select(r => new
            {
                id = r.Id,
                name = r.RequirementName,
                description = r.Description,
                required = r.IsRequired
            });

            return Json(payload);
        }

        // GET: /CertificateRequest/Requirements
        [HttpGet("Requirements")]
        public IActionResult Requirements()
        {
            return View("~/Views/CertificateRequest/Requirements.cshtml");
        }

        // GET: /CertificateRequest/Create
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var model = new CertificateViewModel();
            ViewBag.CertificateTypes = await _context.CertificateTypes.ToListAsync();

            return View("~/Views/CertificateRequest/Create.cshtml", model);
        }

        // POST: /CertificateRequest/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CertificateViewModel model, IFormFile? PaymentReceipt, int[]? confirmedRequirements)
        {
            // 🔒 Server-side validation ng required documents.
            // Kunin ang required requirements ng napiling sertipiko at tiyaking
            // na-confirm lahat ng mga ito bago payagan ang pag-submit.
            var activeReqs = (await _requirementService
                .GetActiveByCertificateNameAsync(model.CertificateType ?? string.Empty)).ToList();

            var confirmed = confirmedRequirements ?? System.Array.Empty<int>();
            var missing = activeReqs
                .Where(r => r.IsRequired && !confirmed.Contains(r.Id))
                .Select(r => r.RequirementName)
                .ToList();

            if (missing.Any())
            {
                foreach (var name in missing)
                {
                    ModelState.AddModelError("", $"Please complete the following required document: {name}");
                }

                ViewBag.CertificateTypes = await _context.CertificateTypes.ToListAsync();
                return View("~/Views/CertificateRequest/Create.cshtml", model);
            }

            string? receiptPath = null;

            // Handle Payment Receipt Upload
            if (PaymentReceipt != null && PaymentReceipt.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "payments");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(PaymentReceipt.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await PaymentReceipt.CopyToAsync(fileStream);
                }

                receiptPath = "/uploads/payments/" + uniqueFileName;
            }

            // Get Price from Selected Certificate Type
            decimal feePaid = 0;
            var selectedCert = await _context.CertificateTypes
                .FirstOrDefaultAsync(c => c.CertificateName == model.CertificateType);

            if (selectedCert != null)
            {
                feePaid = selectedCert.Price;
            }

            var dto = new CertificateDTO
            {
                CertificateType = model.CertificateType ?? string.Empty,
                ResidentName = model.ResidentFullName ?? string.Empty,
                ResidentId = model.ResidentId ?? 0,
                Purpose = model.Purpose ?? string.Empty,
                PaymentReceiptPath = receiptPath,
                FeePaid = feePaid
            };

            bool isSuccess = await _certificateService.RequestCertificateAsync(dto);

            if (isSuccess)
            {
                return RedirectToAction(nameof(SuccessPage));
            }

            ModelState.AddModelError("", "May nagka-error sa pag-save ng iyong request.");
            ViewBag.CertificateTypes = await _context.CertificateTypes.ToListAsync();

            return View("~/Views/CertificateRequest/Create.cshtml", model);
        }

        // GET: /CertificateRequest/Fees
        [HttpGet("Fees")]
        public async Task<IActionResult> Fees()
        {
            var certificateTypes = await _context.CertificateTypes.ToListAsync();
            return View("~/Views/CertificateRequest/Fees.cshtml", certificateTypes);
        }

        // GET: /CertificateRequest/SuccessPage
        [HttpGet("SuccessPage")]
        public IActionResult SuccessPage()
        {
            return View("~/Views/CertificateRequest/SuccessPage.cshtml");
        }
    }
}