using Microsoft.AspNetCore.Mvc;

namespace BarangayManagementSystem.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
