using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BarangayCMS.DAL.Context;
using BarangayCMS.Entities;
using BarangayCMS.Web.Areas.Admin.Models;
using System;
using System.Collections.Generic;
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

        // 1. GET: Admin/Budget  (optional ?projectId= para sa project-filtered financial view)
        public async Task<IActionResult> Index(int? year, int? projectId)
        {
            int selectedYear = year ?? DateTime.Now.Year;
            ViewBag.SelectedYear = selectedYear;

            var vm = new BudgetTrackerViewModel
            {
                SelectedYear = selectedYear,
                SelectedProjectId = projectId,
                // Dropdown ng lahat ng proyekto para sa project filter.
                ProjectOptions = await _context.Projects
                    .OrderBy(p => p.Title)
                    .Select(p => new ProjectOption { Id = p.ProjectId, Name = p.Title, Status = p.Status })
                    .ToListAsync()
            };

            if (projectId.HasValue)
            {
                // 🔎 Project-filtered view — tanging financial records lang ng piniling proyekto.
                var project = await _context.Projects.FindAsync(projectId.Value);
                if (project == null) return NotFound();

                var expenses = await _context.ProjectExpenses
                    .Where(e => e.ProjectId == projectId.Value && !e.IsVoided)
                    .ToListAsync();

                vm.ProjectBudget = ProjectFinancialBuilder.BuildSummary(project, expenses);
            }
            else
            {
                // Umiiral na fund-source annual planner (hindi ginalaw ang logic).
                vm.FundBudgets = await _context.Budgets
                    .Where(b => b.Year == selectedYear)
                    .OrderBy(b => b.Category)
                    .Select(b => new BudgetViewModel
                    {
                        Id = b.BudgetId,
                        MainFundSource = b.MainFundSource,
                        Category = b.Category,
                        AllocatedAmount = b.TotalAllocation,
                        UsedAmount = b.DisbursedAmount,
                        FiscalYear = b.Year,
                        Remarks = b.Description
                    }).ToListAsync();
            }

            return View(vm);
        }

        // ── 💸 PROJECT EXPENSES (itemized, naka-ugnay sa real ProjectId) ──────────────

        // GET: Admin/Budget/AddExpense?projectId=5
        public async Task<IActionResult> AddExpense(int? projectId)
        {
            var model = new ProjectExpenseViewModel
            {
                ExpenseDate = DateTime.Now,
                ProjectOptions = await GetProjectOptionsAsync(),
                ReturnProjectId = projectId
            };
            if (projectId.HasValue) model.ProjectId = projectId.Value;

            return View(model);
        }

        // POST: Admin/Budget/AddExpense
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddExpense(ProjectExpenseViewModel model)
        {
            bool projectExists = await _context.Projects.AnyAsync(p => p.ProjectId == model.ProjectId);
            if (!projectExists) ModelState.AddModelError(nameof(model.ProjectId), "Pumili ng tamang proyekto.");

            if (!ModelState.IsValid)
            {
                model.ProjectOptions = await GetProjectOptionsAsync();
                return View(model);
            }

            var expense = new ProjectExpense
            {
                ProjectId = model.ProjectId,
                Category = string.IsNullOrWhiteSpace(model.Category) ? "Others" : model.Category,
                Amount = model.Amount,
                ExpenseDate = model.ExpenseDate,
                Description = model.Description ?? string.Empty,
                LoggedBy = User.Identity?.Name ?? "Admin",
                DateLogged = DateTime.Now,
                IsVoided = false
            };

            _context.ProjectExpenses.Add(expense);
            await _context.SaveChangesAsync();

            await RecalculateProjectExpensesAsync(model.ProjectId);

            TempData["ExpenseSuccess"] = "Naitala ang gastos at na-update ang buod ng pondo.";
            return RedirectToAction(nameof(Index), new { projectId = model.ProjectId });
        }

        // GET: Admin/Budget/EditExpense/5
        public async Task<IActionResult> EditExpense(int? id)
        {
            if (id == null) return NotFound();

            var expense = await _context.ProjectExpenses.FindAsync(id.Value);
            if (expense == null) return NotFound();

            var model = new ProjectExpenseViewModel
            {
                ExpenseId = expense.ExpenseId,
                ProjectId = expense.ProjectId,
                Category = expense.Category,
                Amount = expense.Amount,
                ExpenseDate = expense.ExpenseDate,
                Description = expense.Description,
                LoggedBy = expense.LoggedBy,
                IsVoided = expense.IsVoided,
                ProjectOptions = await GetProjectOptionsAsync(),
                ReturnProjectId = expense.ProjectId
            };

            return View(model);
        }

        // POST: Admin/Budget/EditExpense/5  — ina-update ang PAREHONG record.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditExpense(int id, ProjectExpenseViewModel model)
        {
            if (id != model.ExpenseId) return NotFound();

            bool projectExists = await _context.Projects.AnyAsync(p => p.ProjectId == model.ProjectId);
            if (!projectExists) ModelState.AddModelError(nameof(model.ProjectId), "Pumili ng tamang proyekto.");

            if (!ModelState.IsValid)
            {
                model.ProjectOptions = await GetProjectOptionsAsync();
                return View(model);
            }

            var expense = await _context.ProjectExpenses.FindAsync(id);
            if (expense == null) return NotFound();

            int oldProjectId = expense.ProjectId;

            expense.ProjectId = model.ProjectId;
            expense.Category = string.IsNullOrWhiteSpace(model.Category) ? "Others" : model.Category;
            expense.Amount = model.Amount;
            expense.ExpenseDate = model.ExpenseDate;
            expense.Description = model.Description ?? string.Empty;

            _context.ProjectExpenses.Update(expense);
            await _context.SaveChangesAsync();

            // I-recompute ang luma at bagong proyekto kung nailipat.
            if (oldProjectId != model.ProjectId)
                await RecalculateProjectExpensesAsync(oldProjectId);
            await RecalculateProjectExpensesAsync(model.ProjectId);

            TempData["ExpenseSuccess"] = "Na-update ang gastos at ang buod ng pondo.";
            return RedirectToAction(nameof(Index), new { projectId = model.ProjectId });
        }

        // POST: Admin/Budget/VoidExpense/5  — soft delete (void), hindi buo na burado.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VoidExpense(int id)
        {
            var expense = await _context.ProjectExpenses.FindAsync(id);
            if (expense == null) return NotFound();

            int projectId = expense.ProjectId;
            expense.IsVoided = true;
            _context.ProjectExpenses.Update(expense);
            await _context.SaveChangesAsync();

            await RecalculateProjectExpensesAsync(projectId);

            TempData["ExpenseSuccess"] = "Na-void ang gastos at na-update ang buod ng pondo.";
            return RedirectToAction(nameof(Index), new { projectId });
        }

        // Muling kinukwenta ang Project.TotalExpenses mula sa aktibong itemized na gastos.
        private async Task RecalculateProjectExpensesAsync(int projectId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return;

            decimal total = await _context.ProjectExpenses
                .Where(e => e.ProjectId == projectId && !e.IsVoided)
                .SumAsync(e => (decimal?)e.Amount) ?? 0m;

            project.TotalExpenses = total;
            project.LastUpdated = DateTime.Now;
            _context.Projects.Update(project);
            await _context.SaveChangesAsync();
        }

        private async Task<List<ProjectOption>> GetProjectOptionsAsync()
        {
            return await _context.Projects
                .OrderBy(p => p.Title)
                .Select(p => new ProjectOption { Id = p.ProjectId, Name = p.Title, Status = p.Status })
                .ToListAsync();
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
                MainFundSource = budget.MainFundSource,
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
                    MainFundSource = model.MainFundSource,
                    Category = model.Category,
                    TotalAllocation = model.AllocatedAmount,
                    DisbursedAmount = model.UsedAmount,
                    Year = model.FiscalYear,
                    Description = model.Remarks ?? string.Empty,
                    LastUpdated = DateTime.Now,
                    LoggedBy = User.Identity?.Name ?? "Admin"
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
                MainFundSource = budget.MainFundSource,
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

                    budget.MainFundSource = model.MainFundSource;
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
                MainFundSource = budget.MainFundSource,
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

            // Magdadagdag sa DisbursedAmount
            budget.DisbursedAmount += expenseAmount;
            budget.LastUpdated = DateTime.Now;
            budget.LoggedBy = User.Identity?.Name ?? "Admin";

            // I-format ang logs
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
                MainFundSource = budget.MainFundSource,
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
                MainFundSource = budget.MainFundSource,
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

        // 13. POST: Admin/Budget/SetAnnualBudget
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetAnnualBudget(decimal annualBudget, int year)
        {
            // Ginawang string para suportado ng DefaultTempDataSerializer at hindi mag-error
            TempData[$"AnnualBudget_{year}"] = annualBudget.ToString();

            return RedirectToAction(nameof(Index), new { year = year });
        }
    }
}