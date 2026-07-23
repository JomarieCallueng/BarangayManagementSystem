using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BarangayCMS.Entities;
using BarangayCMS.Web.Areas.Admin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BarangayCMS.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // 1. GET: Admin/Users
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userList = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                string firstName = user.FullName;
                string lastName = string.Empty;
                int lastSpaceIndex = user.FullName.LastIndexOf(' ');

                if (lastSpaceIndex >= 0)
                {
                    firstName = user.FullName.Substring(0, lastSpaceIndex);
                    lastName = user.FullName.Substring(lastSpaceIndex + 1);
                }

                userList.Add(new UserViewModel
                {
                    Id = user.Id,
                    FirstName = firstName,
                    LastName = lastName,
                    Email = user.Email ?? "",
                    Username = user.UserName ?? "",
                    Role = roles.FirstOrDefault() ?? user.Role ?? "No Role"
                });
            }

            return View(userList);
        }

        // 2. GET: Admin/Users/Create
        public IActionResult Create()
        {
            return View(new UserViewModel());
        }

        // 3. POST: Admin/Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Username,
                    Email = model.Email,
                    FullName = $"{model.FirstName} {model.LastName}".Trim(),
                    Role = model.Role ?? "Staff",
                    IsActive = true,
                    DateCreated = DateTime.Now
                };

                string password = string.IsNullOrEmpty(model.Password) ? "Barangay123!" : model.Password;
                var result = await _userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.Role))
                    {
                        if (!await _roleManager.RoleExistsAsync(model.Role))
                        {
                            await _roleManager.CreateAsync(new IdentityRole(model.Role));
                        }
                        await _userManager.AddToRoleAsync(user, model.Role);
                    }

                    TempData["SuccessMessage"] = "Account successfully registered!";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        // 🔑 4. GET: Admin/Users/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            string firstName = user.FullName;
            string lastName = string.Empty;
            int lastSpaceIndex = user.FullName.LastIndexOf(' ');

            if (lastSpaceIndex >= 0)
            {
                firstName = user.FullName.Substring(0, lastSpaceIndex);
                lastName = user.FullName.Substring(lastSpaceIndex + 1);
            }

            var model = new UserViewModel
            {
                Id = user.Id,
                Username = user.UserName ?? "",
                Email = user.Email ?? "",
                FirstName = firstName,
                LastName = lastName,
                Role = roles.FirstOrDefault() ?? user.Role ?? "Staff"
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserViewModel model)
        {
            // 1. Kapag walang nilagay sa password, tanggalin sa validation para hindi mag-fail ang ModelState
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.Remove("Password");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return NotFound();
            }

            // 2. Pagdugtungin ang FirstName at LastName para sa FullName property ng ApplicationUser
            user.FullName = $"{model.FirstName} {model.LastName}".Trim();
            user.Email = model.Email;

            // Save ang user profile details
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }

            // 3. Update System Role (RBAC)
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (!string.IsNullOrEmpty(model.Role))
            {
                if (!await _roleManager.RoleExistsAsync(model.Role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(model.Role));
                }
                await _userManager.AddToRoleAsync(user, model.Role);
                user.Role = model.Role;
            }

            // 4. Admin Direct Password Override (No Token Required)
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                await _userManager.RemovePasswordAsync(user);
                var passwordResult = await _userManager.AddPasswordAsync(user, model.Password);

                if (!passwordResult.Succeeded)
                {
                    foreach (var error in passwordResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }
            }

            TempData["SuccessMessage"] = "User account updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        // 🔑 6. POST: Admin/Users/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
                TempData["SuccessMessage"] = "User account deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        // 7. GET: Admin/Users/Roles
        public async Task<IActionResult> Roles()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return View(roles);
        }

        // 8. GET: Admin/Users/Permissions
        public IActionResult Permissions()
        {
            return View();
        }
    }
}