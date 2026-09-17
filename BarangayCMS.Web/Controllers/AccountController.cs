using Microsoft.AspNetCore.Mvc;
using BarangayCMS.Web.Models;
using Microsoft.AspNetCore.Identity;
using BarangayCMS.Entities; 
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BarangayCMS.Web.Controllers
{
    public class AccountController : Controller
    {
       
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

            
            if (model.Email == "admin@barangay.gov.ph" && model.Password == "Password123")
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };

                
                var claims = new List<System.Security.Claims.Claim> {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Admin")
                };

               
                await _signInManager.SignInWithClaimsAsync(user, isPersistent: model.RememberMe, claims);

                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }

            
            else if (model.Email == "staff@barangay.gov.ph" && model.Password == "StaffPassword123")
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };

                var claims = new List<System.Security.Claims.Claim> {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Staff")
                };

                await _signInManager.SignInWithClaimsAsync(user, isPersistent: model.RememberMe, claims);

                return RedirectToAction("Index", "Dashboard", new { area = "Staff" });
            }

            
            var realUser = await _signInManager.UserManager.FindByEmailAsync(model.Email)
                           ?? await _signInManager.UserManager.FindByNameAsync(model.Email);
            if (realUser != null)
            {
                
                if (!realUser.IsActive)
                {
                    ModelState.AddModelError(string.Empty, "Ang iyong account ay kasalukuyang hindi aktibo. Kontakin ang Admin.");
                    return View(model);
                }

                
                var result = await _signInManager.CheckPasswordSignInAsync(realUser, model.Password, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    
                    var roles = await _signInManager.UserManager.GetRolesAsync(realUser);
                    string userRole = roles.FirstOrDefault() ?? realUser.Role ?? "Staff";

                    
                    var claims = new List<System.Security.Claims.Claim> {
                        new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, userRole)
                    };

                    
                    await _signInManager.SignInWithClaimsAsync(realUser, isPersistent: model.RememberMe, claims);

                    
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
            return View(); 
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
           
            await _signInManager.SignOutAsync();

            
            

           
            return RedirectToAction("Login", "Account");
        }
    }
}