using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BarangayCMS.DAL.Context;
using BarangayCMS.Entities;
using BarangayManagementSystem.Areas.Staff.ViewModels;

namespace BarangayCMS.Web.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Authorize(Roles = "Staff,Staff / Encoder")]
    [Route("Staff/[controller]/[action]")]
    public class StaffDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StaffDashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Route("~/Staff")]
        [Route("~/Staff/StaffDashboard")]
        public async Task<IActionResult> Index()
        {
            // Kukunin ang kasalukuyang naka-login na Staff
            var currentUser = await _userManager.GetUserAsync(User);
            ViewData["UserFullName"] = currentUser?.FullName ?? "Staff / Encoder";

            // Populate dashboard data mula sa Database
            var model = new DashboardViewModel
            {
                TotalResidents = await _context.Residents.CountAsync(),
                ActiveBlotters = await _context.Complaints.CountAsync(c => c.Status == "Pending"),
                PendingCertificates = await _context.Certificates.CountAsync(),
                RecentAnnouncementsCount = await _context.Announcements.CountAsync()
            };

            return View("~/Areas/Staff/Views/StaffDashboard/Index.cshtml", model);
        }
    }
}