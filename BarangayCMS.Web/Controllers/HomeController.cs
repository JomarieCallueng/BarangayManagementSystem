using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BarangayCMS.Web.Models;
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
            ViewData["TotalResidents"] = await _context.Residents.CountAsync(r => r.IsResident);
            ViewData["ActiveEvacuationCenters"] = (await _evacuationService.GetPublicEvacuationInfoAsync()).ActiveCenters.Count;
            return View();
        }

        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(string ContactNumber, string Message)
        {
            if (string.IsNullOrWhiteSpace(ContactNumber) || string.IsNullOrWhiteSpace(Message))
            {
                ModelState.AddModelError(string.Empty, "Punan po ang Contact Number at Message.");
                return View();
            }

            // 🔒 Backend enforcement: PH mobile must be exactly 11 digits (09XXXXXXXXX).
            if (!BarangayCMS.Web.Validation.PhilippineMobileAttribute.IsValidMobile(ContactNumber))
            {
                var loc = HttpContext.RequestServices
                    .GetService(typeof(Microsoft.Extensions.Localization.IStringLocalizer<SharedResource>))
                    as Microsoft.Extensions.Localization.IStringLocalizer<SharedResource>;
                ModelState.AddModelError(string.Empty,
                    loc?["Val.Mobile.Format"].Value ?? "Mobile number must be exactly 11 digits (e.g., 09XXXXXXXXX).");
                return View();
            }

            var ok = await _contactMessages.SubmitAsync(ContactNumber.Trim(), Message);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty, "Nagkaroon ng problema sa pagpapadala. Subukan muli.");
                return View();
            }

            TempData["SuccessMessage"] = "Your message has been sent successfully.";
            return RedirectToAction(nameof(Contact));
        }

        public IActionResult About() => RedirectToAction(nameof(Officials));

        // 🏛️ GET: /Home/Officials — Ipinapasa ang LAHAT ng active officials (kasama ang SK Chairperson)
        // para mahanap ng _BarangayCommittees.cshtml ang SK Chairperson sa Youth & Sports Development (Ex-Officio).
        public async Task<IActionResult> Officials()
        {
            var officials = await GetActiveOfficialsAsync();

            // Kina-capture ang SK Chairperson para sa Ex-Officio display badge
            ViewData["ExOfficio"] = officials
                .FirstOrDefault(o => IsSk(o.Position) && (o.Position ?? string.Empty).ToLowerInvariant().Contains("chair"));

            return View(officials);
        }

        // 🧑‍🤝‍🧑 GET: /Home/SK — ACTIVE Sangguniang Kabataan (SK) officials LAMANG.
        public async Task<IActionResult> SK()
        {
            var officials = await GetActiveOfficialsAsync();
            var sk = officials.Where(o => IsSk(o.Position)).ToList();
            return View(sk);
        }

        public IActionResult History() => View();

        public async Task<IActionResult> Faq()
        {
            var certificates = await _context.CertificateTypes
                .AsNoTracking()
                .Include(c => c.Requirements)
                .OrderBy(c => c.CertificateName)
                .ToListAsync();
            return View(certificates);
        }

        private async Task<List<BarangayOfficial>> GetActiveOfficialsAsync()
        {
            var active = await _context.BarangayOfficials
                .AsNoTracking()
                .Include(o => o.ServiceHistories)
                .Where(o => o.IsActive)
                .ToListAsync();

            return active
                .OrderBy(o => PositionRank(o.Position))
                .ThenBy(o => o.FullName)
                .ToList();
        }

        private static int PositionRank(string? position)
        {
            var p = (position ?? string.Empty).ToLowerInvariant();
            if (IsSk(position))
            {
                if (p.Contains("chair") || p.Contains("chairperson") || p.Contains("chairman")) return 30;
                return 31;
            }
            if (p.Contains("captain") || p.Contains("punong")) return 0;
            if (p.Contains("secretary") || p.Contains("kalihim")) return 1;
            if (p.Contains("treasurer") || p.Contains("ingat-yaman")) return 2;
            if (p.Contains("kagawad") || p.Contains("councilor") || p.Contains("council") || p.Contains("konsehal")) return 4;
            return 5;
        }

        private static bool IsSk(string? position)
        {
            var p = (position ?? string.Empty).ToLowerInvariant();
            return p.Contains("sk ") || p.StartsWith("sk") || p.Contains("kabataan") || p.Contains("youth");
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