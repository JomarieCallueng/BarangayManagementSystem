using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BarangayCMS.DAL.Context; // Namespace ng iyong DBContext
using BarangayCMS.Entities;   // Namespace ng iyong Entities

namespace BarangayManagementSystem.Controllers.Admin
{
    [Area("Admin")] // 🌟 NAPAKAHALAGA: Ito ang lulutas sa iyong 404 Error!
    // [Authorize(Roles = "Admin,Captain")] // (Opsyonal) Siguraduhing admin lang ang makakapasok
    public class AdminCertificateTypesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminCertificateTypesController(ApplicationDbContext context)
        {
            _context = context;
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
        // PRIVATE HELPER METHOD
        // ==========================================
        private bool CertificateTypeExists(int id)
        {
            return _context.CertificateTypes.Any(e => e.CertificateTypeId == id);
        }
    }
}