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

namespace BarangayCMS.Web.Areas.Admin.Controllers
{
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

        // 🌟 GET: /CertificateRequest/Requirements
        [HttpGet("Requirements")]
        public IActionResult Requirements()
        {
            return View(); // Babasahin nito ang "Requirements.cshtml" automatically!
        }

        // 🌟 GET: /CertificateRequest/Create
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var model = new CertificateViewModel();
            var certificateTypes = await _context.CertificateTypes.ToListAsync();
            ViewBag.CertificateTypes = certificateTypes;

            return View(model);
        }

        // 🌟 POST: /CertificateRequest/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CertificateViewModel model, IFormFile? PaymentReceipt)
        {
            string? receiptPath = null;

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

            return View(model);
        }

        // 🌟 GET: /CertificateRequest/Fees
        [HttpGet("Fees")]
        public async Task<IActionResult> Fees()
        {
            var certificateTypes = await _context.CertificateTypes.ToListAsync();
            return View(certificateTypes);
        }

        // 🌟 GET: /CertificateRequest/SuccessPage
        [HttpGet("SuccessPage")]
        public IActionResult SuccessPage()
        {
            return View();
        }
    }
}