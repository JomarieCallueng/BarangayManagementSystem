using Microsoft.AspNetCore.Mvc;
using BarangayManagementSystem.Models;

namespace BarangayManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();

        // GET: Home/Contact
        public IActionResult Contact()
        {
            return View();
        }

        // POST: Home/Contact
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Contact(ContactFormModel model)
        {
            if (ModelState.IsValid)
            {
                // PRG Pattern: Process and save to DB or send email alert here
                TempData["SuccessMessage"] = "Thank you! Your message has been sent successfully.";
                return RedirectToAction(nameof(Contact));
            }
            return View(model);
        }
    }
}