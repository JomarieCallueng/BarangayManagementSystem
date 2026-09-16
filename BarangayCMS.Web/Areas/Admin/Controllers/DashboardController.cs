using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BarangayCMS.DAL.Context;
using BarangayCMS.BLL.Interfaces;
using BarangayManagementSystem.Areas.Admin.Models;

namespace BarangayManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IContactMessageService _contactMessages;

        public DashboardController(ApplicationDbContext context, IContactMessageService contactMessages)
        {
            _context = context;
            _contactMessages = contactMessages;
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

                // Bilang ng hindi pa nababasang mensahe mula sa Contact Us form
                UnreadMessages = await _contactMessages.GetUnreadCountAsync(),

                SystemStatus = "Operational"
            };

            return View(model);
        }

        public IActionResult Residents()
        {
            // The view file is named "Resident.cshtml"; render it explicitly so the
            // action name ("Residents") doesn't cause a "view not found" error.
            return View("Resident");
        }

        public IActionResult StaffProfile()
        {
            return View();
        }
    }
}