using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BarangayCMS.BLL.Interfaces;
using BarangayCMS.DAL.Context; // Namespace ng iyong DBContext
using BarangayCMS.DTO;
using BarangayCMS.Entities;   // Namespace ng iyong Entities

namespace BarangayManagementSystem.Controllers.Admin
{
    [Area("Admin")] // 🌟 NAPAKAHALAGA: Ito ang lulutas sa iyong 404 Error!
    // [Authorize(Roles = "Admin,Captain")] // (Opsyonal) Siguraduhing admin lang ang makakapasok
    public class AdminCertificateTypesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICertificateRequirementService _requirementService;

        public AdminCertificateTypesController(
            ApplicationDbContext context,
            ICertificateRequirementService requirementService)
        {
            _context = context;
            _requirementService = requirementService;
        }

        // ==========================================
        // 1. LISTAHAN (INDEX - GET)
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var certificateTypes = await _context.CertificateTypes.ToListAsync();
            return View(certificateTypes);
        }

        // ==========================================
        // 2. PAG-ADD NG BAGO (CREATE - GET)
        // ==========================================
        public IActionResult Create()
        {
            return View();
        }

        // ==========================================
        // 3. PAG-SAVE NG BAGO (CREATE - POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
     [Bind("CertificateTypeId,CertificateName,Price")] CertificateType certificateType,
     IFormFile? TemplateFile) // 🌟 Tinatanggap na dito ang uploaded Word file
        {
            // Burahin muna ang tracking/navigation validation errors
            ModelState.Clear();

            if (certificateType.CertificateName != null)
            {
                // 🌟 CODE PARA SA PAG-SAVE NG FILE SA SERVER
                if (TemplateFile != null && TemplateFile.Length > 0)
                {
                    // Gumawa ng folder na 'templates' sa loob ng wwwroot kung wala pa ito
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Gagawa ng unique file name gamit ang Guid para walang maging kapareho
                    var fileExtension = Path.GetExtension(TemplateFile.FileName);
                    var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Isusulat at isasave ang file sa wwwroot/templates folder
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await TemplateFile.CopyToAsync(fileStream);
                    }

                    // Isasave ang file name sa DB model para madaling mahanap mamaya kapag mag-pi-print
                    certificateType.TemplateFileName = uniqueFileName;
                }

                _context.Add(certificateType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(certificateType);
        }

        // ==========================================
        // 4. PAG-EDIT (EDIT - GET)
        // ==========================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var certificateType = await _context.CertificateTypes.FindAsync(id);
            if (certificateType == null)
            {
                return NotFound();
            }
            return View(certificateType);
        }

        // ==========================================
        // 5. PAG-SAVE NG BINAGO (EDIT - POST)
        // ==========================================
        // 5. PAG-SAVE NG BINAGO (EDIT - POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("CertificateTypeId,CertificateName,Price,TemplateFileName")] CertificateType certificateType,
            IFormFile? TemplateFile) // 🌟 Tinatanggap na rin ang uploaded file rito
        {
            // The form posts the PK as "CertificateTypeId" (not a route "id"), so id can be 0.
            if (id == 0) id = certificateType.CertificateTypeId;

            if (id != certificateType.CertificateTypeId)
            {
                return NotFound();
            }

            // Burahin ang tracking validation errors para sa kaligtasan
            ModelState.Clear();

            if (certificateType.CertificateName != null)
            {
                try
                {
                    // 🌟 CODE PARA SA PAG-UPDATE NG FILE SA SERVER
                    if (TemplateFile != null && TemplateFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        // A. Burahin ang lumang file kung meron man para iwas kalat sa server
                        if (!string.IsNullOrEmpty(certificateType.TemplateFileName))
                        {
                            var oldFilePath = Path.Combine(uploadsFolder, certificateType.TemplateFileName);
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        // B. I-save ang bagong file
                        var fileExtension = Path.GetExtension(TemplateFile.FileName);
                        var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await TemplateFile.CopyToAsync(fileStream);
                        }

                        // C. Ituro ang bagong file name sa database
                        certificateType.TemplateFileName = uniqueFileName;
                    }

                    _context.Update(certificateType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CertificateTypeExists(certificateType.CertificateTypeId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(certificateType);
        }

        // ==========================================
        // 6. PAG-BURA (DELETE - GET / PROMPT PAGE)
        // ==========================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var certificateType = await _context.CertificateTypes
                .FirstOrDefaultAsync(m => m.CertificateTypeId == id);
            if (certificateType == null)
            {
                return NotFound();
            }

            return View(certificateType);
        }

        // ==========================================
        // 7. PAG-CONFIRM NG PAGBURA (DELETE - POST)
        // ==========================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var certificateType = await _context.CertificateTypes.FindAsync(id);
            if (certificateType != null)
            {
                _context.CertificateTypes.Remove(certificateType);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // REQUIREMENTS MANAGEMENT (per certificate type)
        // ==========================================

        // GET: /Admin/AdminCertificateTypes/Requirements/5
        public async Task<IActionResult> Requirements(int? id)
        {
            if (id == null) return NotFound();

            var certType = await _context.CertificateTypes.FindAsync(id);
            if (certType == null) return NotFound();

            ViewBag.CertificateType = certType;
            var requirements = await _requirementService.GetAllByCertificateTypeIdAsync(id.Value);
            return View(requirements);
        }

        // POST: /Admin/AdminCertificateTypes/AddRequirement
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRequirement(CertificateRequirementDTO model)
        {
            if (string.IsNullOrWhiteSpace(model.RequirementName))
            {
                TempData["Error"] = "Requirement name is required.";
            }
            else
            {
                bool ok = await _requirementService.AddAsync(model);
                TempData[ok ? "Success" : "Error"] = ok
                    ? "Requirement added."
                    : "That requirement already exists for this certificate.";
            }

            return RedirectToAction(nameof(Requirements), new { id = model.CertificateTypeId });
        }

        // POST: /Admin/AdminCertificateTypes/EditRequirement
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRequirement(CertificateRequirementDTO model)
        {
            bool ok = await _requirementService.UpdateAsync(model);
            TempData[ok ? "Success" : "Error"] = ok
                ? "Requirement updated."
                : "Unable to update requirement (name may be blank or duplicate).";

            return RedirectToAction(nameof(Requirements), new { id = model.CertificateTypeId });
        }

        // POST: /Admin/AdminCertificateTypes/DeleteRequirement
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRequirement(int id, int certificateTypeId)
        {
            bool ok = await _requirementService.DeleteAsync(id);
            TempData[ok ? "Success" : "Error"] = ok ? "Requirement removed." : "Requirement not found.";
            return RedirectToAction(nameof(Requirements), new { id = certificateTypeId });
        }

        // ==========================================
        // PRIVATE HELPER METHOD
        // ==========================================
        private bool CertificateTypeExists(int id)
        {
            return _context.CertificateTypes.Any(e => e.CertificateTypeId == id);
        }
    }
}