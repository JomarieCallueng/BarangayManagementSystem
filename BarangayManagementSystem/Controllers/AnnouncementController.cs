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
            var announcements = await _context.Announcements
                .Where(a => a.PublishDate <= now && (a.ExpiryDate == null || a.ExpiryDate >= now))
                .OrderByDescending(a => a.IsPinned) // (Optional) Unahing i-display ang naka-pin
                .ThenByDescending(a => a.PublishDate)
                .ToListAsync();

            return View(announcements);
        }
    }
}