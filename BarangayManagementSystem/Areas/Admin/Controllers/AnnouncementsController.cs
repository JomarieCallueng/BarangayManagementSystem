using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BarangayCMS.DAL.Context;
using BarangayCMS.Entities;
using BarangayCMS.Web.Areas.Admin.Models;
using System;
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
                DatePosted = DateTime.Now
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

                    // Default fallbacks para sa mga bagong entity properties
                    Category = "General",
                    ImageUrl = string.Empty,
                    IsPinned = false,
                    AuthorName = User.Identity?.Name ?? "Admin"
                };

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
                    announcement.PublishDate = model.DatePosted;
                    announcement.ExpiryDate = model.ExpiryDate;

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
    }
}