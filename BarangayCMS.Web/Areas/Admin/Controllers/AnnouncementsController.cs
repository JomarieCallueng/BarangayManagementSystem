using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BarangayCMS.DAL.Context;
using BarangayCMS.Entities;
using BarangayCMS.Web.Areas.Admin.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BarangayCMS.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AnnouncementsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        // Folder (relative sa wwwroot) kung saan iniimbak ang mga larawan ng patalastas.
        private const string UploadFolder = "uploads/announcements";
        private const long MaxImageBytes = 5 * 1024 * 1024; // 5 MB

        // Pinapayagang mga extension at MIME type (JPG, JPEG, PNG, WEBP lamang).
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private static readonly string[] AllowedContentTypes = { "image/jpeg", "image/png", "image/webp" };

        public AnnouncementsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // 1. GET: Admin/Announcements
        public async Task<IActionResult> Index()
        {
            var announcements = await _context.Announcements
                .OrderByDescending(a => a.IsPinned)
                .ThenByDescending(a => a.PublishDate)
                .Select(a => new AnnouncementViewModel
                {
                    Id = a.AnnouncementId,
                    Title = a.Title,
                    Content = a.Content,
                    Category = a.Category,
                    ImageUrl = a.ImageUrl,
                    HasImage = a.ImageContentType != null,
                    IsPinned = a.IsPinned,
                    AuthorName = a.AuthorName,
                    DatePosted = a.PublishDate,
                    ExpiryDate = a.ExpiryDate
                }).ToListAsync();

            return View(announcements);
        }

        // 2. GET: Admin/Announcements/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var announcement = await _context.Announcements
                .FirstOrDefaultAsync(m => m.AnnouncementId == id);

            if (announcement == null) return NotFound();

            return View(ToViewModel(announcement));
        }

        // 3. GET: Admin/Announcements/Create
        public IActionResult Create()
        {
            var viewModel = new AnnouncementViewModel
            {
                DatePosted = DateTime.Now,
                Category = "General"
            };
            return View(viewModel);
        }

        // 4. POST: Admin/Announcements/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AnnouncementViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Alamin ang huling image path (galing sa AJAX upload, o fallback na direct file post).
            var (ok, imagePath) = await ResolveImagePathAsync(model, existingPath: null);
            if (!ok) return View(model);

            var announcement = new Announcement
            {
                Title = model.Title,
                Content = model.Content,
                PublishDate = model.DatePosted,
                ExpiryDate = model.ExpiryDate,
                Category = string.IsNullOrWhiteSpace(model.Category) ? "General" : model.Category,
                ImageUrl = imagePath ?? string.Empty,
                IsPinned = model.IsPinned,
                AuthorName = User.Identity?.Name ?? "Admin"
            };

            _context.Announcements.Add(announcement);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                // Kung nabigo ang DB save, linisin ang bagong na-upload na file (iwas orphan).
                TryDeleteLocalUpload(imagePath);
                ModelState.AddModelError(string.Empty, "Hindi na-save ang patalastas. Subukan muli.");
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        // 5. GET: Admin/Announcements/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var announcement = await _context.Announcements.FindAsync(id.Value);
            if (announcement == null) return NotFound();

            return View(ToViewModel(announcement));
        }

        // 6. POST: Admin/Announcements/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AnnouncementViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                var current = await _context.Announcements.AsNoTracking().FirstOrDefaultAsync(a => a.AnnouncementId == id);
                model.HasImage = current?.ImageContentType != null;
                return View(model);
            }

            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null) return NotFound();

            var oldPath = announcement.ImageUrl;

            // Alamin ang huling image path. Kung walang bagong larawan, mananatili ang luma.
            var (ok, imagePath) = await ResolveImagePathAsync(model, existingPath: oldPath);
            if (!ok)
            {
                model.HasImage = announcement.ImageContentType != null;
                return View(model);
            }

            announcement.Title = model.Title;
            announcement.Content = model.Content;
            announcement.Category = string.IsNullOrWhiteSpace(model.Category) ? "General" : model.Category;
            announcement.IsPinned = model.IsPinned;
            announcement.PublishDate = model.DatePosted;
            announcement.ExpiryDate = model.ExpiryDate;
            announcement.AuthorName = User.Identity?.Name ?? "Admin";

            bool imageChanged = imagePath != oldPath;
            if (imageChanged)
            {
                announcement.ImageUrl = imagePath ?? string.Empty;

                // May bagong file-based na larawan → linisin ang lumang DB-stored binary
                // para gamitin ng display ang bagong ImageUrl.
                if (!string.IsNullOrEmpty(imagePath))
                {
                    announcement.ImageData = null;
                    announcement.ImageContentType = null;
                }
            }

            try
            {
                _context.Announcements.Update(announcement);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Announcements.Any(e => e.AnnouncementId == model.Id)) return NotFound();
                throw;
            }
            catch
            {
                // Kung nabigo ang DB save at may bagong na-upload na file, linisin ito.
                if (imageChanged) TryDeleteLocalUpload(imagePath);
                ModelState.AddModelError(string.Empty, "Hindi na-save ang pagbabago. Subukan muli.");
                model.HasImage = announcement.ImageContentType != null;
                return View(model);
            }

            // Matagumpay na na-save. I-delete na ang lumang file (iwas orphan) kung napalitan.
            if (imageChanged) TryDeleteLocalUpload(oldPath);

            return RedirectToAction(nameof(Index));
        }

        // 7. GET: Admin/Announcements/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var announcement = await _context.Announcements
                .FirstOrDefaultAsync(m => m.AnnouncementId == id);

            if (announcement == null) return NotFound();

            var viewModel = new AnnouncementViewModel
            {
                Id = announcement.AnnouncementId,
                Title = announcement.Title,
                Content = announcement.Content,
                DatePosted = announcement.PublishDate,
                ExpiryDate = announcement.ExpiryDate
            };

            return View(viewModel);
        }

        // 8. POST: Admin/Announcements/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement != null)
            {
                var path = announcement.ImageUrl;
                _context.Announcements.Remove(announcement);
                await _context.SaveChangesAsync();

                // Linisin ang naka-store na larawan sa disk (iwas orphan files).
                TryDeleteLocalUpload(path);
            }
            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // 9. POST: Admin/Announcements/UploadImage  (AJAX)
        // Tumatanggap ng file, vine-validate, gumagawa ng natatanging
        // filename, ise-save sa wwwroot/uploads/announcements, at
        // ibinabalik ang app-relative na URL bilang JSON.
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(6 * 1024 * 1024)] // hard cap para pigilan ang sobrang laking upload
        public async Task<IActionResult> UploadImage(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, error = "Walang napiling larawan." });
            }

            var (valid, error) = ValidateImage(file);
            if (!valid)
            {
                return Json(new { success = false, error });
            }

            try
            {
                var relativeUrl = await SaveImageAsync(file);
                return Json(new { success = true, url = relativeUrl });
            }
            catch
            {
                return Json(new { success = false, error = "Nabigo ang pag-upload. Subukan muli." });
            }
        }

        // ============================================================
        // HELPERS
        // ============================================================

        private AnnouncementViewModel ToViewModel(Announcement a) => new AnnouncementViewModel
        {
            Id = a.AnnouncementId,
            Title = a.Title,
            Content = a.Content,
            Category = a.Category,
            ImageUrl = a.ImageUrl,
            HasImage = a.ImageData != null && a.ImageData.Length > 0,
            IsPinned = a.IsPinned,
            AuthorName = a.AuthorName,
            DatePosted = a.PublishDate,
            ExpiryDate = a.ExpiryDate
        };

        /// <summary>
        /// Tinutukoy ang huling image path para sa isang save:
        /// 1) Kung may na-populate nang ImageUrl (mula sa AJAX upload) → gamitin iyon.
        /// 2) Kung walang ImageUrl pero may direct na na-post na file (JS off) → i-save ito ngayon.
        /// 3) Kung wala kahit alin → panatilihin ang <paramref name="existingPath"/>.
        /// Nagbabalik ng (false, null) at nag-a-add ng ModelState error kapag di-wasto ang file.
        /// </summary>
        private async Task<(bool ok, string? path)> ResolveImagePathAsync(AnnouncementViewModel model, string? existingPath)
        {
            // 1) May halaga ang ImageUrl (mula sa AJAX upload o dating naka-store na value).
            if (!string.IsNullOrWhiteSpace(model.ImageUrl))
            {
                var url = model.ImageUrl.Trim();

                // Hindi nagbago mula sa naka-store na value → panatilihin ito (kabilang ang
                // mga legacy na external URL). Ang readonly na field ay nagbabago LAMANG sa
                // pamamagitan ng matagumpay na AJAX upload.
                if (existingPath != null && string.Equals(url, existingPath.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return (true, existingPath);
                }

                // Bagong halaga → seguridad: dapat app-relative na nasa loob ng upload folder.
                // Pinipigilan nito ang pag-store ng lokal na Windows path (C:\...) o external na URL.
                if (!IsAllowedStoredPath(url))
                {
                    ModelState.AddModelError("ImageUrl",
                        "Di-wastong image path. Gamitin ang Upload Image para awtomatikong makuha ang tamang path.");
                    return (false, null);
                }
                return (true, url);
            }

            // 2) Fallback (JS naka-off): may direktang na-post na file — i-validate at i-save ngayon.
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var (valid, error) = ValidateImage(model.ImageFile);
                if (!valid)
                {
                    ModelState.AddModelError("ImageFile", error ?? "Di-wastong larawan.");
                    return (false, null);
                }

                var saved = await SaveImageAsync(model.ImageFile);
                return (true, saved);
            }

            // 3) Walang bagong larawan — panatilihin ang dati.
            return (true, existingPath);
        }

        /// <summary>Vine-validate ang laki, extension, MIME type, at signature ng larawan.</summary>
        private static (bool ok, string? error) ValidateImage(IFormFile file)
        {
            if (file.Length > MaxImageBytes)
                return (false, "Ang larawan ay hindi dapat lumagpas sa 5 MB.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
                return (false, "Pinapayagan lamang ang .jpg, .jpeg, .png, at .webp na larawan.");

            if (!AllowedContentTypes.Contains(file.ContentType))
                return (false, "Di-wastong uri ng file. Dapat JPG, PNG, o WEBP na larawan.");

            // Karagdagang seguridad: suriin ang magic bytes (signature) ng file.
            if (!HasValidImageSignature(file))
                return (false, "Ang file ay hindi tunay na larawan.");

            return (true, null);
        }

        /// <summary>
        /// Ise-save ang larawan gamit ang natatanging server-generated na filename
        /// (hindi kailanman ginagamit ang orihinal na pangalan mula sa browser).
        /// Nagbabalik ng app-relative na URL, hal. /uploads/announcements/abc123.png.
        /// </summary>
        private async Task<string> SaveImageAsync(IFormFile file)
        {
            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsFolder = Path.Combine(webRoot, "uploads", "announcements");
            Directory.CreateDirectory(uploadsFolder);

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext)) ext = ".png"; // safety net

            // Natatanging pangalan — walang bahagi mula sa user-supplied filename (iwas path traversal).
            var uniqueName = Guid.NewGuid().ToString("N") + ext;
            var fullPath = Path.Combine(uploadsFolder, uniqueName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return "/" + UploadFolder + "/" + uniqueName;
        }

        /// <summary>Tinatanggal ang lokal na naka-upload na file kung ito ay nasa loob ng upload folder.</summary>
        private void TryDeleteLocalUpload(string? relativePath)
        {
            if (!IsAllowedStoredPath(relativePath)) return; // huwag galawin ang external URL o walang laman

            try
            {
                var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var fileName = Path.GetFileName(relativePath!); // huling segment lamang — iwas traversal
                var fullPath = Path.Combine(webRoot, "uploads", "announcements", fileName);

                // Siguraduhing nasa loob talaga ng inaasahang folder bago mag-delete.
                var expectedFolder = Path.GetFullPath(Path.Combine(webRoot, "uploads", "announcements"));
                var resolved = Path.GetFullPath(fullPath);
                if (resolved.StartsWith(expectedFolder, StringComparison.OrdinalIgnoreCase) && System.IO.File.Exists(resolved))
                {
                    System.IO.File.Delete(resolved);
                }
            }
            catch
            {
                // Best-effort lamang — huwag ipatigil ang daloy kung hindi ma-delete.
            }
        }

        /// <summary>
        /// Totoo kung ang path ay app-relative na nasa loob ng announcements upload folder,
        /// hal. /uploads/announcements/abc123.png. Pinipigilan ang lokal na Windows path,
        /// external na URL, at path traversal.
        /// </summary>
        private static bool IsAllowedStoredPath(string? path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;
            var p = path.Trim().Replace("\\", "/");

            if (!p.StartsWith("/" + UploadFolder + "/", StringComparison.OrdinalIgnoreCase)) return false;
            if (p.Contains("..")) return false;

            var fileName = p.Substring(("/" + UploadFolder + "/").Length);
            if (string.IsNullOrEmpty(fileName) || fileName.Contains('/')) return false;

            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            return AllowedExtensions.Contains(ext);
        }

        /// <summary>Sinisuri ang magic bytes ng file para tiyaking tunay itong JPG/PNG/WEBP.</summary>
        private static bool HasValidImageSignature(IFormFile file)
        {
            try
            {
                using var stream = file.OpenReadStream();
                Span<byte> header = stackalloc byte[12];
                var read = stream.Read(header);
                if (read < 12) return false;

                // JPEG: FF D8 FF
                if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF) return true;

                // PNG: 89 50 4E 47 0D 0A 1A 0A
                if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
                    header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A) return true;

                // WEBP: "RIFF"...."WEBP"
                if (header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 &&
                    header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50) return true;

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
