using System;
using System.Linq;
using System.Threading.Tasks;
using BarangayCMS.BLL.Interfaces;
using BarangayCMS.DAL;
using BarangayCMS.DAL.Context;
using BarangayCMS.Entities;
using BarangayCMS.Web.Areas.Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BarangayCMS.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CertificatesController : Controller
    {
        private readonly ICertificateService _certificateService;
        private readonly ApplicationDbContext _context;

        public CertificatesController(ICertificateService certificateService, ApplicationDbContext context)
        {
            _certificateService = certificateService;
            _context = context;
        }

        // GET: /Admin/Certificates
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

                return new CertificateViewModel
                {
                    Id = c.CertificateId,
                    ResidentId = c.ResidentId,
                    ResidentFullName = displayName,
                    CertificateType = c.CertificateType ?? "Barangay Document",
                    DateRequested = c.DateRequested,
                    Status = c.Status,
                    FeePaid = c.FeePaid,
                    PaymentReceiptPath = c.PaymentReceiptPath
                };
            }).ToList();

            return View(viewModel);
        }

        // GET: /Admin/Certificates/Generate/5
        [HttpGet]
        public async Task<IActionResult> Generate(int id)
        {
            var cert = await _context.Certificates
                .Include(c => c.Resident)
                .FirstOrDefaultAsync(c => c.CertificateId == id);

            if (cert == null)
            {
                return NotFound();
            }

            string displayName = "Unknown Resident";
            if (!string.IsNullOrEmpty(cert.ResidentName))
            {
                displayName = cert.ResidentName;
            }
            else if (cert.Resident != null)
            {
                displayName = $"{cert.Resident.FirstName} {cert.Resident.LastName}";
            }

            var viewModel = new CertificateViewModel
            {
                Id = cert.CertificateId,
                ResidentId = cert.ResidentId,
                ResidentFullName = displayName,
                CertificateType = cert.CertificateType,
                DateRequested = cert.DateRequested,
                Status = cert.Status,
                FeePaid = cert.FeePaid,
                PaymentReceiptPath = cert.PaymentReceiptPath
            };

            return View(viewModel);
        }

        // POST: /Admin/Certificates/Generate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generate(CertificateViewModel model)
        {
            var cert = await _context.Certificates.FindAsync(model.Id);
            if (cert != null)
            {
                cert.Status = !string.IsNullOrEmpty(model.Status) ? model.Status : "Issued";
                cert.DateIssued = DateTime.Now;
                cert.FeePaid = model.FeePaid;

                _context.Certificates.Update(cert);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Hindi mahanap ang certificate sa database.");
            return View(model);
        }

        // GET: /Admin/Certificates/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var cert = await _context.Certificates
                .Include(c => c.Resident)
                .FirstOrDefaultAsync(c => c.CertificateId == id);

            if (cert == null)
            {
                return NotFound();
            }

            string displayName = "Unknown Resident";
            if (!string.IsNullOrEmpty(cert.ResidentName))
            {
                displayName = cert.ResidentName;
            }
            else if (cert.Resident != null)
            {
                displayName = $"{cert.Resident.FirstName} {cert.Resident.LastName}";
            }

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
                PaymentReceiptPath = cert.PaymentReceiptPath
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

            if (cert == null)
            {
                return NotFound();
            }

            string displayName = "Unknown Resident";
            if (!string.IsNullOrEmpty(cert.ResidentName))
            {
                displayName = cert.ResidentName;
            }
            else if (cert.Resident != null)
            {
                displayName = $"{cert.Resident.FirstName} {cert.Resident.LastName}";
            }

            var viewModel = new CertificateViewModel
            {
                Id = cert.CertificateId,
                ResidentId = cert.ResidentId,
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
            if (cert == null)
            {
                return NotFound();
            }

            _context.Certificates.Remove(cert);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}