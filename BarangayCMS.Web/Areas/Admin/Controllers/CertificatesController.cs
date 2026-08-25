using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BarangayCMS.BLL.Interfaces;
using BarangayCMS.DAL.Context;
using BarangayCMS.Entities;
using BarangayCMS.Web.Areas.Admin.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BarangayCMS.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CertificatesController : Controller
    {
        private readonly ICertificateService _certificateService;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public CertificatesController(
            ICertificateService certificateService,
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _certificateService = certificateService;
            _context = context;
            _environment = environment;
        }

        // GET: /Admin/Certificates
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var certificates = await _context.Certificates
                .Include(c => c.Resident)
                .OrderByDescending(c => c.DateRequested)
                .ToListAsync();

            var viewModel = certificates.Select(c => {
                string displayName = "Unknown Resident";
                if (!string.IsNullOrEmpty(c.ResidentName))
                {
                    displayName = c.ResidentName;
                }
                else if (c.Resident != null)
                {
                    displayName = $"{c.Resident.FirstName} {c.Resident.LastName}";
                }

                string receiptUrl = !string.IsNullOrEmpty(c.PaymentReceiptPath)
                    ? $"/Admin/Certificates/GetPaymentReceipt/{c.CertificateId}"
                    : null;

                return new CertificateViewModel
                {
                    Id = c.CertificateId,
                    ResidentId = c.ResidentId,
                    ResidentFullName = displayName,
                    CertificateType = c.CertificateType ?? "Barangay Document",
                    DateRequested = c.DateRequested,
                    Status = c.Status,
                    FeePaid = c.FeePaid,
                    PaymentReceiptPath = receiptUrl
                };
            }).ToList();

            return View(viewModel);
        }

        // GET: /Admin/Certificates/Approve/1027
        [HttpGet]
        [Route("Admin/Certificates/Approve/{id:int}")]
        public async Task<IActionResult> Approve(int id)
        {
            var cert = await _context.Certificates.FindAsync(id);
            if (cert == null) return NotFound();

            cert.Status = "Approved";
            cert.DateIssued = DateTime.Now;

            _context.Certificates.Update(cert);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Certificates/Print/1027
        [HttpGet]
        [Route("Admin/Certificates/Print/{id:int}")]
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
                Id = cert.CertificateId,
                ResidentId = cert.ResidentId,
                ResidentFullName = displayName,
                CertificateType = string.IsNullOrEmpty(cert.CertificateType) ? "BARANGAY CERTIFICATE" : cert.CertificateType.ToUpper(),
                DateRequested = cert.DateRequested,
                DateIssued = cert.DateIssued ?? DateTime.Now,
                Status = cert.Status,
                FeePaid = cert.FeePaid,
                PaymentReceiptPath = cert.PaymentReceiptPath
            };

            return View("PrintTemplate", viewModel);
        }

        // GET: /Admin/Certificates/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var residents = await _context.Residents
                .Select(r => new { r.ResidentId, FullName = r.FirstName + " " + r.LastName })
                .ToListAsync();

            var viewModel = new CertificateViewModel
            {
                DateRequested = DateTime.Now,
                Status = "Pending",
                ResidentList = new SelectList(residents, "ResidentId", "FullName")
            };

            return View(viewModel);
        }

        // POST: /Admin/Certificates/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CertificateViewModel model, IFormFile? receiptFile)
        {
            if (ModelState.IsValid)
            {
                var resident = await _context.Residents.FindAsync(model.ResidentId);
                string? storedRelativePath = null;

                if (receiptFile != null && receiptFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "receipts");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(receiptFile.FileName);
                    string fullPath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await receiptFile.CopyToAsync(stream);
                    }

                    storedRelativePath = "uploads/receipts/" + uniqueFileName;
                }

                var cert = new Certificate
                {
                    ResidentId = model.ResidentId,
                    ResidentName = resident != null ? $"{resident.FirstName} {resident.LastName}" : model.ResidentFullName,
                    CertificateType = model.CertificateType,
                    DateRequested = model.DateRequested != default ? model.DateRequested : DateTime.Now,
                    DateIssued = model.DateIssued,
                    Status = string.IsNullOrEmpty(model.Status) ? "Pending" : model.Status,
                    FeePaid = model.FeePaid,
                    PaymentReceiptPath = storedRelativePath
                };

                _context.Certificates.Add(cert);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            var residents = await _context.Residents
                .Select(r => new { r.ResidentId, FullName = r.FirstName + " " + r.LastName })
                .ToListAsync();

            model.ResidentList = new SelectList(residents, "ResidentId", "FullName", model.ResidentId);
            return View(model);
        }

        // GET: /Admin/Certificates/GetPaymentReceipt/1027
        [HttpGet]
        public async Task<IActionResult> GetPaymentReceipt(int id)
        {
            var cert = await _context.Certificates.FindAsync(id);
            if (cert == null || string.IsNullOrEmpty(cert.PaymentReceiptPath))
            {
                return NotFound("Receipt not found.");
            }

            string cleanPath = cert.PaymentReceiptPath.TrimStart('~', '/').Replace('/', Path.DirectorySeparatorChar);
            string fullPath = Path.Combine(_environment.WebRootPath, cleanPath);

            if (!System.IO.File.Exists(fullPath))
            {
                string fileName = Path.GetFileName(cleanPath);
                var match = Directory.GetFiles(_environment.WebRootPath, fileName, SearchOption.AllDirectories).FirstOrDefault();
                if (match != null) fullPath = match;
            }

            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound("Physical receipt image file missing from server storage.");
            }

            byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
            string extension = Path.GetExtension(fullPath).ToLower();
            string contentType = extension switch
            {
                ".png" => "image/png",
                ".pdf" => "application/pdf",
                _ => "image/jpeg"
            };

            return File(fileBytes, contentType);
        }

        // GET: /Admin/Certificates/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var cert = await _context.Certificates
                .Include(c => c.Resident)
                .FirstOrDefaultAsync(c => c.CertificateId == id);

            if (cert == null) return NotFound();

            string displayName = string.IsNullOrEmpty(cert.ResidentName)
                ? (cert.Resident != null ? $"{cert.Resident.FirstName} {cert.Resident.LastName}" : "Unknown Resident")
                : cert.ResidentName;

            var viewModel = new CertificateViewModel
            {
                Id = cert.CertificateId,
                ResidentId = cert.ResidentId,
                ResidentFullName = displayName,
                CertificateType = cert.CertificateType,
                DateRequested = cert.DateRequested,
                DateIssued = cert.DateIssued,
                Status = cert.Status,
                FeePaid = cert.FeePaid,
                PaymentReceiptPath = !string.IsNullOrEmpty(cert.PaymentReceiptPath)
                    ? $"/Admin/Certificates/GetPaymentReceipt/{cert.CertificateId}"
                    : null
            };

            return View(viewModel);
        }

        // GET: /Admin/Certificates/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var cert = await _context.Certificates
                .Include(c => c.Resident)
                .FirstOrDefaultAsync(c => c.CertificateId == id);

            if (cert == null) return NotFound();

            string displayName = string.IsNullOrEmpty(cert.ResidentName)
                ? (cert.Resident != null ? $"{cert.Resident.FirstName} {cert.Resident.LastName}" : "Unknown Resident")
                : cert.ResidentName;

            var viewModel = new CertificateViewModel
            {
                Id = cert.CertificateId,
                ResidentFullName = displayName,
                CertificateType = cert.CertificateType,
                DateRequested = cert.DateRequested,
                Status = cert.Status,
                FeePaid = cert.FeePaid
            };

            return View(viewModel);
        }

        // POST: /Admin/Certificates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cert = await _context.Certificates.FindAsync(id);
            if (cert == null) return NotFound();

            _context.Certificates.Remove(cert);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}