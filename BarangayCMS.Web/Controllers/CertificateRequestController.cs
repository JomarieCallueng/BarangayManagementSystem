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
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly BarangayCMS.DAL.Context.ApplicationDbContext _context;

        public CertificateRequestController(
            ICertificateService certificateService,
            IWebHostEnvironment webHostEnvironment,
            BarangayCMS.DAL.Context.ApplicationDbContext context)
        {
            _certificateService = certificateService;
            _webHostEnvironment = webHostEnvironment;
            _context = context;
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
        public async Task<IActionResult> Create(CertificateViewModel model, IFormFile? PaymentReceipt)
        {
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
                CertificateType = model.CertificateType,
                ResidentName = model.ResidentFullName,
                ResidentId = model.ResidentId ?? 0,
                Purpose = model.Purpose,
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