using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BarangayCMS.DAL.Context;
using BarangayManagementSystem.Areas.Admin.Models;

namespace BarangayManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🌟 REAL-TIME DATABASE COUNTING
        public async Task<IActionResult> Index()
        {
            var model = new AdminDashboardViewModel
            {
                // Bibilangin ang totoong records sa Residents table
                TotalResidents = await _context.Residents.CountAsync(),

                // Bibilangin ang complaints na may status na "Pending"
                PendingComplaints = await _context.Complaints
                    .CountAsync(c => c.Status == "Pending"),

                // 🌟 INAYOS DITO: 'Certificates' na ang ginamit mula sa iyong ApplicationDbContext
                CertificatesHandled = await _context.Certificates.CountAsync(),

                SystemStatus = "Operational"
            };

            return View(model);
        }

        public IActionResult Residents()
        {
            return View();
        }

        public IActionResult StaffProfile()
        {
            return View();
        }
    }
}