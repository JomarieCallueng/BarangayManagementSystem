using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BarangayCMS.DAL.Context; // Siguraduhing tama ang namespace ng iyong ApplicationDbContext

namespace BarangayManagementSystem.Controllers
{
    public class AnnouncementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AnnouncementController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Announcement
        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;

            // Kukunin LAMANG ang mga announcement na:
            // 1. Nakalipas na ang PublishDate (a.PublishDate <= now)
            // 2. AT WALANG ExpiryDate (null) O HINDI pa nakakalipas ang ExpiryDate (a.ExpiryDate >= now)
            // Huwag i-load ang buong ImageData (bytes) sa listahan para hindi bumigat ang page.
            // Ang aktuwal na larawan ay kinukuha isa-isa sa pamamagitan ng /Announcement/Image/{id}.
            // Ang ImageContentType (maliit na string) ang senyas kung may naka-store na larawan.
            var announcements = await _context.Announcements
                .Where(a => a.PublishDate <= now && (a.ExpiryDate == null || a.ExpiryDate >= now))
                .OrderByDescending(a => a.IsPinned) // (Optional) Unahing i-display ang naka-pin
                .ThenByDescending(a => a.PublishDate)
                .Select(a => new BarangayCMS.Entities.Announcement
                {
                    AnnouncementId = a.AnnouncementId,
                    Title = a.Title,
                    Content = a.Content,
                    Category = a.Category,
                    ImageUrl = a.ImageUrl,
                    ImageContentType = a.ImageContentType,
                    IsPinned = a.IsPinned,
                    PublishDate = a.PublishDate,
                    ExpiryDate = a.ExpiryDate,
                    AuthorName = a.AuthorName
                })
                .ToListAsync();

            return View(announcements);
        }

        // GET: /Announcement/Image/5
        // Ibinabalik ang larawang naka-store sa database para mabuksan sa kahit anong device.
        [HttpGet]
        public async Task<IActionResult> Image(int id)
        {
            var img = await _context.Announcements
                .Where(a => a.AnnouncementId == id)
                .Select(a => new { a.ImageData, a.ImageContentType })
                .FirstOrDefaultAsync();

            if (img?.ImageData == null || img.ImageData.Length == 0)
            {
                return NotFound();
            }

            return File(img.ImageData, string.IsNullOrEmpty(img.ImageContentType) ? "image/jpeg" : img.ImageContentType);
        }
    }
}