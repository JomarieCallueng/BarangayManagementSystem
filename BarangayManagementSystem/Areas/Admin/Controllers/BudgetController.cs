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
    public class BudgetController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BudgetController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. GET: Admin/Budget
        public async Task<IActionResult> Index(int? year)
        {
            int selectedYear = year ?? DateTime.Now.Year;
            ViewBag.SelectedYear = selectedYear;

            var budgets = await _context.Budgets
                .Where(b => b.Year == selectedYear) // Inayos mula FiscalYear -> Year
                .OrderBy(b => b.Category)
                .Select(b => new BudgetViewModel
                {
                    Id = b.BudgetId,
                    Category = b.Category,
                    AllocatedAmount = b.TotalAllocation,
                    UsedAmount = b.DisbursedAmount, // Inayos mula UsedAmount -> DisbursedAmount
                    FiscalYear = b.Year,           // Map sa ViewModel property para sa View mo
                    Remarks = b.Description        // Map sa ViewModel property para sa View mo
                }).ToListAsync();

            return View(budgets);
        }

        // 2. GET: Admin/Budget/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var budget = await _context.Budgets.FindAsync(id.Value);
            if (budget == null) return NotFound();

            var viewModel = new BudgetViewModel
            {
                Id = budget.BudgetId,
                Category = budget.Category,
                AllocatedAmount = budget.TotalAllocation,
                UsedAmount = budget.DisbursedAmount,
                FiscalYear = budget.Year,
                Remarks = budget.Description
            };

            return View(viewModel);
        }

        // 3. GET: Admin/Budget/Create
        public IActionResult Create()
        {
            return View(new BudgetViewModel { FiscalYear = DateTime.Now.Year });
        }

        // 4. POST: Admin/Budget/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BudgetViewModel model)
        {
            if (ModelState.IsValid)
            {
                var budget = new Budget
                {
                    Category = model.Category,
                    TotalAllocation = model.AllocatedAmount,
                    DisbursedAmount = model.UsedAmount,
                    Year = model.FiscalYear,
                    Description = model.Remarks ?? string.Empty,
                    LastUpdated = DateTime.Now,
                    LoggedBy = User.Identity?.Name ?? "Admin" // Auto-fill para sa required metadata fields mo
                };

                _context.Budgets.Add(budget);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { year = budget.Year });
            }
            return View(model);
        }

        // 5. GET: Admin/Budget/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var budget = await _context.Budgets.FindAsync(id.Value);
            if (budget == null) return NotFound();

            var viewModel = new BudgetViewModel
            {
                Id = budget.BudgetId,
                Category = budget.Category,
                AllocatedAmount = budget.TotalAllocation,
                UsedAmount = budget.DisbursedAmount,
                FiscalYear = budget.Year,
                Remarks = budget.Description
            };

            return View(viewModel);
        }

        // 6. POST: Admin/Budget/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BudgetViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var budget = await _context.Budgets.FindAsync(id);
                    if (budget == null) return NotFound();

                    budget.Category = model.Category;
                    budget.TotalAllocation = model.AllocatedAmount;
                    budget.DisbursedAmount = model.UsedAmount;
                    budget.Year = model.FiscalYear;
                    budget.Description = model.Remarks ?? string.Empty;
                    budget.LastUpdated = DateTime.Now;
                    budget.LoggedBy = User.Identity?.Name ?? "Admin";

                    _context.Budgets.Update(budget);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Budgets.Any(e => e.BudgetId == model.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index), new { year = model.FiscalYear });
            }
            return View(model);
        }

        // 7. GET: Admin/Budget/Expenses/5
        public async Task<IActionResult> Expenses(int? id)
        {
            if (id == null) return NotFound();

            var budget = await _context.Budgets.FindAsync(id.Value);
            if (budget == null) return NotFound();

            var viewModel = new BudgetViewModel
            {
                Id = budget.BudgetId,
                Category = budget.Category,
                AllocatedAmount = budget.TotalAllocation,
                UsedAmount = budget.DisbursedAmount,
                FiscalYear = budget.Year
            };

            return View(viewModel);
        }

        // 8. POST: Admin/Budget/Expenses/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Expenses(int id, decimal expenseAmount, string expenseRemarks)
        {
            var budget = await _context.Budgets.FindAsync(id);
            if (budget == null) return NotFound();

            budget.DisbursedAmount += expenseAmount;
            budget.LastUpdated = DateTime.Now;
            budget.LoggedBy = User.Identity?.Name ?? "Admin";

            string cleanRemarks = string.IsNullOrEmpty(expenseRemarks) ? "No remarks provided" : expenseRemarks;
            budget.Description = $"[{DateTime.Now:yyyy-MM-dd}] Spent ₱{expenseAmount:N2}: {cleanRemarks} | " + (budget.Description ?? string.Empty);

            _context.Budgets.Update(budget);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { year = budget.Year });
        }

        // 9. GET: Admin/Budget/Income/5
        public async Task<IActionResult> Income(int? id)
        {
            if (id == null) return NotFound();

            var budget = await _context.Budgets.FindAsync(id.Value);
            if (budget == null) return NotFound();

            var viewModel = new BudgetViewModel
            {
                Id = budget.BudgetId,
                Category = budget.Category,
                AllocatedAmount = budget.TotalAllocation,
                UsedAmount = budget.DisbursedAmount,
                FiscalYear = budget.Year
            };

            return View(viewModel);
        }

        // 10. POST: Admin/Budget/Income/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Income(int id, decimal incomeAmount, string incomeRemarks)
        {
            var budget = await _context.Budgets.FindAsync(id);
            if (budget == null) return NotFound();

            budget.TotalAllocation += incomeAmount;
            budget.LastUpdated = DateTime.Now;
            budget.LoggedBy = User.Identity?.Name ?? "Admin";

            string cleanRemarks = string.IsNullOrEmpty(incomeRemarks) ? "No remarks provided" : incomeRemarks;
            budget.Description = $"[{DateTime.Now:yyyy-MM-dd}] Added ₱{incomeAmount:N2}: {cleanRemarks} | " + (budget.Description ?? string.Empty);

            _context.Budgets.Update(budget);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { year = budget.Year });
        }

        // 11. GET: Admin/Budget/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var budget = await _context.Budgets.FindAsync(id.Value);
            if (budget == null) return NotFound();

            var viewModel = new BudgetViewModel
            {
                Id = budget.BudgetId,
                Category = budget.Category,
                AllocatedAmount = budget.TotalAllocation,
                FiscalYear = budget.Year
            };

            return View(viewModel);
        }

        // 12. POST: Admin/Budget/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var budget = await _context.Budgets.FindAsync(id);
            int year = DateTime.Now.Year;

            if (budget != null)
            {
                year = budget.Year;
                _context.Budgets.Remove(budget);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index), new { year = year });
        }
    }
}