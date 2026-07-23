using Microsoft.AspNetCore.Mvc;
using BarangayCMS.Web.Models;
using Microsoft.AspNetCore.Identity;
using BarangayCMS.Entities; // Siguraduhing kasama ito para mabasa ang ApplicationUser
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BarangayCMS.Web.Controllers
{
    public class AccountController : Controller
    {
        // Gagamit tayo ng SignInManager base sa iyong ApplicationUser
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // ========================================================
            // BACKUP: 1. HARDCODED ADMIN LOGIN WITH CLAIMS
            // ========================================================
            if (model.Email == "admin@barangay.gov.ph" && model.Password == "Password123")
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };

                // Gumawa ng claims para sa Role
                var claims = new List<System.Security.Claims.Claim> {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Admin")
                };

                // I-sign in kasama ang Role Claim para basahin ng [Authorize(Roles = "Admin")]
                await _signInManager.SignInWithClaimsAsync(user, isPersistent: model.RememberMe, claims);

                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }

            // ========================================================
            // BACKUP: 2. HARDCODED STAFF LOGIN WITH CLAIMS
            // ========================================================
            else if (model.Email == "staff@barangay.gov.ph" && model.Password == "StaffPassword123")
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };

                var claims = new List<System.Security.Claims.Claim> {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Staff")
                };

                await _signInManager.SignInWithClaimsAsync(user, isPersistent: model.RememberMe, claims);

                return RedirectToAction("Index", "Dashboard", new { area = "Staff" });
            }

            // ========================================================
            // DYNAMIC: 3. TOTOONG LOGIN MULA SA DATABASE (WITH FIX FOR 403)
            // ========================================================
            // 1. Hanapin ang user gamit ang kanyang Email Address
            var realUser = await _signInManager.UserManager.FindByEmailAsync(model.Email);
            if (realUser != null)
            {
                // 2. I-verify kung aktibo ang account
                if (!realUser.IsActive)
                {
                    ModelState.AddModelError(string.Empty, "Ang iyong account ay kasalukuyang hindi aktibo. Kontakin ang Admin.");
                    return View(model);
                }

                // 3. I-authenticate muna ang password ng user
                var result = await _signInManager.CheckPasswordSignInAsync(realUser, model.Password, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    // 4. Alamin ang totoong Role ng user mula sa Identity o sa Custom Column property
                    var roles = await _signInManager.UserManager.GetRolesAsync(realUser);
                    string userRole = roles.FirstOrDefault() ?? realUser.Role ?? "Staff";

                    // 5. 🔑 LUNAS SA 403: Puwersahang gawan ng Role Claim Cookie para kilalanin ng [Authorize(Roles = "...")]
                    var claims = new List<System.Security.Claims.Claim> {
                        new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, userRole)
                    };

                    // I-sign in gamit ang binuong claims cookie
                    await _signInManager.SignInWithClaimsAsync(realUser, isPersistent: model.RememberMe, claims);

                    // 6. Redirect sa tamang Area base sa nakuhang Role string
                    if (userRole == "SuperAdmin" || userRole == "Admin")
                    {
                        return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                    }
                    else
                    {
                        return RedirectToAction("Index", "Dashboard", new { area = "Staff" });
                    }
                }
            }

            ModelState.AddModelError(string.Empty, "Maling email address o password. Subukan muli.");
            return View(model);
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View(); // Hahanapin nito ang AccessDenied.cshtml
        }

        // ========================================================
        // 🚀 INAYOS: GINAWANG POST AT DIRETSONG REDIRECT SA LOGIN
        // ========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // 1. I-sign out ang user mula sa Identity authentication cookie context
            await _signInManager.SignOutAsync();

            // 2. Siguraduhing malinis ang local memory sessions ng server
            

            // 3. Diretsong balik sa Login screen ng controller na ito
            return RedirectToAction("Login", "Account");
        }
    }
}