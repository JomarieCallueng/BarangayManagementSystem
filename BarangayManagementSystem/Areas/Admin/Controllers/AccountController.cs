using Microsoft.AspNetCore.Mvc;

namespace BarangayManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccountController : Controller
    {
        // GET: /Admin/Account/Login o kaya /Admin
        [HttpGet]
        public IActionResult Login()
        {
            // Kung naka-login na (halimbawa gamit ang Session), pwede mo silang i-redirect direkta sa Dashboard.
            // Para sa ngayon, ibabalik nito ang malinis na Login Page na walang global layout navbar.
            return View();
        }

        // POST: /Admin/Account/Login
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            // Simple at secure credential check para sa prototyping/development phase ninyo
            if (username == "admin" && password == "admin123")
            {
                // TODO: Sa susunod, maaari mo itong palitan ng Cookie Authentication o ASP.NET Core Identity.
                // HttpContext.SignInAsync(...);

                // Matagumpay ang login, i-redirect sa Dashboard Index ng Admin Area
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }

            // Kapag mali ang credentials, magpapakita ng error message sa View nga walang crash
            ModelState.AddModelError("", "Maling username o password. Pakisubukang muli.");
            return View();
        }

        // GET/POST: /Admin/Account/Logout
        public IActionResult Logout()
        {
            // TODO: Kung gagamit ka na ng authentication sa susunod, i-clear ang session o cookies dito:
            // HttpContext.SignOutAsync();

            // Pagka-logout, ibabalik ang user sa login screen ng Admin portal
            return RedirectToAction("Login", "Account", new { area = "Admin" });
        }
    }
}