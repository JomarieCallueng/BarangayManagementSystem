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

        public AnnouncementsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. GET: Admin/Announcements
        public async Task<IActionResult> Index()
        {
            var announcements = await _context.Announcements
                .OrderByDescending(a => a.IsPinned) // Unahing i-display ang pinned announcements
                .ThenByDescending(a => a.PublishDate) // Inayos mula DatePosted -> PublishDate
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

            var viewModel = new AnnouncementViewModel
            {
                Id = announcement.AnnouncementId,
                Title = announcement.Title,
                Content = announcement.Content,
                Category = announcement.Category,
                ImageUrl = announcement.ImageUrl,
                HasImage = announcement.ImageData != null && announcement.ImageData.Length > 0,
                IsPinned = announcement.IsPinned,
                AuthorName = announcement.AuthorName,
                DatePosted = announcement.PublishDate,
                ExpiryDate = announcement.ExpiryDate
            };

            return View(viewModel);
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
            if (ModelState.IsValid)
            {
                var announcement = new Announcement
                {
                    Title = model.Title,
                    Content = model.Content,
                    PublishDate = model.DatePosted,
                    ExpiryDate = model.ExpiryDate, // Kukunin mula sa user input o magiging null kung walang pinili

                    Category = string.IsNullOrWhiteSpace(model.Category) ? "General" : model.Category,
                    ImageUrl = model.ImageUrl ?? string.Empty,
                    IsPinned = model.IsPinned,
                    AuthorName = User.Identity?.Name ?? "Admin"
                };

                // Kunin ang in-upload na larawan at itago nang diretso sa database.
                if (!await ApplyUploadedImageAsync(model.ImageFile, announcement))
                {
                    return View(model);
                }

                _context.Announcements.Add(announcement);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // 5. GET: Admin/Announcements/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var announcement = await _context.Announcements.FindAsync(id.Value);
            if (announcement == null) return NotFound();

            var viewModel = new AnnouncementViewModel
            {
                Id = announcement.AnnouncementId,
                Title = announcement.Title,
                Content = announcement.Content,
                Category = announcement.Category,
                ImageUrl = announcement.ImageUrl,
                HasImage = announcement.ImageData != null && announcement.ImageData.Length > 0,
                IsPinned = announcement.IsPinned,
                AuthorName = announcement.AuthorName,
                DatePosted = announcement.PublishDate,
                ExpiryDate = announcement.ExpiryDate
            };

            return View(viewModel);
        }

        // 6. POST: Admin/Announcements/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AnnouncementViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var announcement = await _context.Announcements.FindAsync(id);
                    if (announcement == null) return NotFound();

                    announcement.Title = model.Title;
                    announcement.Content = model.Content;
                    announcement.Category = string.IsNullOrWhiteSpace(model.Category) ? "General" : model.Category;
                    announcement.ImageUrl = model.ImageUrl ?? string.Empty;
                    announcement.IsPinned = model.IsPinned;
                    announcement.PublishDate = model.DatePosted;
                    announcement.ExpiryDate = model.ExpiryDate;

                    // Kung may bagong in-upload na larawan, palitan ang naka-store sa database.
                    if (!await ApplyUploadedImageAsync(model.ImageFile, announcement))
                    {
                        model.HasImage = announcement.ImageData != null && announcement.ImageData.Length > 0;
                        return View(model);
                    }

                    // Panatilihing updated ang pangalan ng huling nag-edit kung kinakailangan
                    announcement.AuthorName = User.Identity?.Name ?? "Admin";

                    _context.Announcements.Update(announcement);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Announcements.Any(e => e.AnnouncementId == model.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
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
                _context.Announcements.Remove(announcement);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Helper: i-validate at i-store ang in-upload na larawan nang diretso sa database.
        // Nagbabalik ng false (at nag-a-add ng ModelState error) kapag hindi wasto ang file.
        private async Task<bool> ApplyUploadedImageAsync(Microsoft.AspNetCore.Http.IFormFile? file, Announcement announcement)
        {
            if (file == null || file.Length == 0)
            {
                return true; // Walang bagong larawan — walang babaguhin.
            }

            const long maxBytes = 5 * 1024 * 1024; // 5 MB
            if (file.Length > maxBytes)
            {
                ModelState.AddModelError("ImageFile", "Ang larawan ay hindi dapat lumagpas sa 5 MB.");
                return false;
            }

            var allowed = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            if (!allowed.Contains(file.ContentType))
            {
                ModelState.AddModelError("ImageFile", "Ang larawan ay dapat .jpg, .png, .gif, o .webp lamang.");
                return false;
            }

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            announcement.ImageData = ms.ToArray();
            announcement.ImageContentType = file.ContentType;
            return true;
        }
    }
}