using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BarangayCMS.Web.Models; // Tiyaking mayroon kang ErrorViewModel dito
using BarangayCMS.BLL.Interfaces;
using BarangayCMS.DAL.Context;
using BarangayCMS.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BarangayCMS.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IContactMessageService _contactMessages;
        private readonly ApplicationDbContext _context;
        private readonly IEvacuationService _evacuationService;

        public HomeController(IContactMessageService contactMessages, ApplicationDbContext context, IEvacuationService evacuationService)
        {
            _contactMessages = contactMessages;
            _context = context;
            _evacuationService = evacuationService;
        }

        public async Task<IActionResult> Index()
        {
            // Dynamic na bilang para sa hero/stats (galing sa database, hindi hardcoded).
            ViewData["TotalResidents"] = await _context.Residents.CountAsync(r => r.IsResident);
            ViewData["ActiveEvacuationCenters"] = (await _evacuationService.GetPublicEvacuationInfoAsync()).ActiveCenters.Count;
            return View();
        }

        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }

        // 💬 POST: /Home/Contact — sine-save ang "Send a Message" form sa database.
        // Walang account — Contact Number at Message lang ang kailangan.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(string ContactNumber, string Message)
        {
            if (string.IsNullOrWhiteSpace(ContactNumber) || string.IsNullOrWhiteSpace(Message))
            {
                ModelState.AddModelError(string.Empty, "Punan po ang Contact Number at Message.");
                return View();
            }

            var ok = await _contactMessages.SubmitAsync(ContactNumber, Message);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty, "Nagkaroon ng problema sa pagpapadala. Subukan muli.");
                return View();
            }

            TempData["SuccessMessage"] = "Your message has been sent successfully.";
            return RedirectToAction(nameof(Contact));
        }

        // 🏛️ GET: /Home/About — dynamically loads ACTIVE barangay officials mula sa
        // database (single source of truth). Ang Admin Barangay Officials module ang
        // kumokontrol sa listahang ito — walang hardcoded na pangalan dito.
        public async Task<IActionResult> About()
        {
            // Kunin LAMANG ang mga aktibong opisyal KASAMA ang kani-kanilang personal
            // na service history (naka-ugnay sa BarangayOfficialId), tapos i-order nang
            // lohikal ayon sa posisyon.
            var active = await _context.BarangayOfficials
                .AsNoTracking()
                .Include(o => o.ServiceHistories)
                .Where(o => o.IsActive)
                .ToListAsync();

            var ordered = active
                .OrderBy(o => PositionRank(o.Position))
                .ThenBy(o => o.FullName)
                .ToList();

            return View(ordered);
        }

        // Nagbibigay ng lohikal na pagkakasunod-sunod base sa posisyon.
        // Ginagamit lamang kapag walang tahasang display/order field ang entity.
        private static int PositionRank(string? position)
        {
            var p = (position ?? string.Empty).ToLowerInvariant();
            if (p.Contains("captain") || p.Contains("punong")) return 0;
            if (p.Contains("secretary") || p.Contains("kalihim")) return 1;
            if (p.Contains("treasurer") || p.Contains("ingat-yaman")) return 2;
            if (p.Contains("sk ") || p.StartsWith("sk") || p.Contains("kabataan") || p.Contains("youth")) return 3;
            if (p.Contains("kagawad") || p.Contains("councilor") || p.Contains("council") || p.Contains("konsehal")) return 4;
            return 5; // iba pang miyembro
        }

        public IActionResult Announcements()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}