using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BarangayCMS.DAL.Context;
using BarangayCMS.Entities;
using BarangayCMS.Web.Areas.Admin.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BarangayCMS.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProjectsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. GET: Admin/Projects
        public async Task<IActionResult> Index()
        {
            var projects = await _context.Projects
                .OrderByDescending(p => p.StartDate)
                .Select(p => new ProjectViewModel
                {
                    Id = p.ProjectId,
                    ProjectName = p.Title, // Inayos mula ProjectName -> Title
                    Description = p.Description,
                    Budget = p.BudgetAllocated, // Inayos mula Budget -> BudgetAllocated
                    StartDate = p.StartDate,
                    EndDate = p.EndDate ?? DateTime.MinValue, // Nullable handling para sa ViewModel mo
                    Status = p.Status
                }).ToListAsync();

            return View(projects);
        }

        // 2. GET: Admin/Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects.FindAsync(id.Value);
            if (project == null) return NotFound();

            var viewModel = new ProjectViewModel
            {
                Id = project.ProjectId,
                ProjectName = project.Title,
                Description = project.Description,
                Budget = project.BudgetAllocated,
                StartDate = project.StartDate,
                EndDate = project.EndDate ?? DateTime.MinValue,
                Status = project.Status
            };

            return View(viewModel);
        }

        // 3. GET: Admin/Projects/Create
        public IActionResult Create()
        {
            return View(new ProjectViewModel { StartDate = DateTime.Now });
        }

        // 4. POST: Admin/Projects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProjectViewModel model)
        {
            if (ModelState.IsValid)
            {
                var project = new Project
                {
                    Title = model.ProjectName,
                    Description = model.Description ?? string.Empty,
                    BudgetAllocated = model.Budget,
                    TotalExpenses = 0, // Default panimula para sa bagong project property
                    StartDate = model.StartDate,
                    EndDate = model.EndDate == DateTime.MinValue ? null : model.EndDate, // Gawing null kung hindi sinagutan
                    Status = model.Status ?? "Planning",
                    Location = string.Empty,   // Default/Fallback para sa bagong db field
                    Contractor = string.Empty, // Default/Fallback para sa bagong db field
                    DateLogged = DateTime.Now
                };

                _context.Projects.Add(project);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // 5. GET: Admin/Projects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects.FindAsync(id.Value);
            if (project == null) return NotFound();

            var viewModel = new ProjectViewModel
            {
                Id = project.ProjectId,
                ProjectName = project.Title,
                Description = project.Description,
                Budget = project.BudgetAllocated,
                StartDate = project.StartDate,
                EndDate = project.EndDate ?? DateTime.MinValue,
                Status = project.Status
            };

            return View(viewModel);
        }

        // 6. POST: Admin/Projects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProjectViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var project = await _context.Projects.FindAsync(id);
                    if (project == null) return NotFound();

                    project.Title = model.ProjectName;
                    project.Description = model.Description ?? string.Empty;
                    project.BudgetAllocated = model.Budget;
                    project.StartDate = model.StartDate;
                    project.EndDate = model.EndDate == DateTime.MinValue ? null : model.EndDate;
                    project.Status = model.Status;
                    project.LastUpdated = DateTime.Now; // Selyo ng pag-update sa entity

                    _context.Projects.Update(project);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Projects.Any(p => p.ProjectId == model.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // 7. GET: Admin/Projects/Progress
        public async Task<IActionResult> Progress()
        {
            var ongoingProjects = await _context.Projects
                .Where(p => p.Status == "Ongoing")
                .OrderBy(p => p.EndDate)
                .Select(p => new ProjectViewModel
                {
                    Id = p.ProjectId,
                    ProjectName = p.Title,
                    Description = p.Description,
                    Budget = p.BudgetAllocated,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate ?? DateTime.MinValue,
                    Status = p.Status
                }).ToListAsync();

            return View(ongoingProjects);
        }

        // 8. GET: Admin/Projects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects.FindAsync(id.Value);
            if (project == null) return NotFound();

            var viewModel = new ProjectViewModel
            {
                Id = project.ProjectId,
                ProjectName = project.Title,
                Budget = project.BudgetAllocated
            };

            return View(viewModel);
        }

        // 9. POST: Admin/Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}