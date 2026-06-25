using Microsoft.AspNetCore.Mvc;

namespace BarangayManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        // URL: /Admin/Dashboard
        public IActionResult Index()
        {
            // Dito mapupunta ang admin pagkatapos mag-login
            return View();
        }
    }
}