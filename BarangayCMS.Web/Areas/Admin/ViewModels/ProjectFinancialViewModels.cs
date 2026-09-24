using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using BarangayCMS.Entities;

namespace BarangayCMS.Web.Areas.Admin.Models
{
    // 🧮 Iisang lugar kung saan binubuo ang financial summary — pare-pareho ang kalkulasyon
    // sa Project Details at sa Budget Tracker (walang duplicate na logic).
    public static class ProjectFinancialBuilder
    {
        public static ProjectBudgetSummaryViewModel BuildSummary(Project project, IEnumerable<ProjectExpense> activeExpenses)
        {
            var list = (activeExpenses ?? Enumerable.Empty<ProjectExpense>())
                .OrderByDescending(e => e.ExpenseDate)
                .ThenByDescending(e => e.ExpenseId)
                .ToList();

            var vm = new ProjectBudgetSummaryViewModel
            {
                ProjectId = project.ProjectId,
                ProjectName = project.Title,
                Status = project.Status,
                ApprovedBudget = project.BudgetAllocated,
                TotalExpenses = list.Sum(e => e.Amount),
                Breakdown = list
                    .GroupBy(e => string.IsNullOrWhiteSpace(e.Category) ? "Others" : e.Category)
                    .Select(g => new ExpenseCategoryBreakdown
                    {
                        Category = g.Key,
                        Amount = g.Sum(x => x.Amount),
                        Count = g.Count()
                    })
                    .OrderByDescending(b => b.Amount)
                    .ToList(),
                Expenses = list.Select(e => new ProjectExpenseViewModel
                {
                    ExpenseId = e.ExpenseId,
                    ProjectId = e.ProjectId,
                    ProjectName = project.Title,
                    Category = e.Category,
                    Amount = e.Amount,
                    ExpenseDate = e.ExpenseDate,
                    Description = e.Description,
                    LoggedBy = e.LoggedBy,
                    IsVoided = e.IsVoided
                }).ToList()
            };

            return vm;
        }
    }

    // 💸 Isang itemized na gastos — ginagamit sa listahan at sa Add/Edit form.
    public class ProjectExpenseViewModel
    {
        // Karaniwang mga kategorya ng gastos sa proyekto.
        public static readonly string[] Categories =
            { "Materials", "Labor", "Equipment", "Permits", "Professional Fees", "Others" };

        public int ExpenseId { get; set; }

        [Required(ErrorMessage = "Pumili ng proyekto.")]
        [Display(Name = "Proyekto")]
        public int ProjectId { get; set; }

        public string ProjectName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Pumili ng kategorya.")]
        [Display(Name = "Kategorya")]
        public string Category { get; set; } = "Materials";

        [Range(0.01, 999999999.99, ErrorMessage = "Maglagay ng tamang halaga (mas malaki sa 0).")]
        [Display(Name = "Halaga (₱)")]
        public decimal Amount { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Petsa ng Gastos")]
        public DateTime ExpenseDate { get; set; } = DateTime.Now;

        [MaxLength(500, ErrorMessage = "Hindi pwedeng lumagpas sa 500 characters.")]
        [Display(Name = "Deskripsyon")]
        public string Description { get; set; } = string.Empty;

        public string LoggedBy { get; set; } = string.Empty;
        public bool IsVoided { get; set; }

        // Para sa dropdown ng proyekto sa form.
        public List<ProjectOption> ProjectOptions { get; set; } = new();

        // Para sa "Bumalik" navigation na hindi nawawala ang konteksto.
        public int? ReturnProjectId { get; set; }
    }

    public class ProjectOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        // "Name — Status" para sa dropdown; walang status filter — lahat ng proyekto kasama.
        public string DisplayLabel =>
            string.IsNullOrWhiteSpace(Status) ? Name : $"{Name} — {Status}";
    }

    public class ExpenseCategoryBreakdown
    {
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int Count { get; set; }
    }

    // 💰 Buod ng pananalapi ng iisang proyekto — approved / used / remaining + status + expenses.
    public class ProjectBudgetSummaryViewModel
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        public decimal ApprovedBudget { get; set; }   // Project.BudgetAllocated
        public decimal TotalExpenses { get; set; }     // SUM(non-voided expenses)

        public bool HasBudget => ApprovedBudget > 0;
        public bool HasExpenses => TotalExpenses > 0 || (Expenses?.Any() ?? false);

        public decimal Remaining => ApprovedBudget - TotalExpenses;

        // Naka-clamp para walang NaN / Infinity / negatibong bar.
        public double PercentUsed =>
            ApprovedBudget > 0
                ? Math.Round(Math.Min((double)(TotalExpenses / ApprovedBudget) * 100, 100), 1)
                : 0;

        // Tunay na porsyento (pwedeng lumampas sa 100 kapag over budget) — para sa label.
        public double RawPercentUsed =>
            ApprovedBudget > 0 ? Math.Round((double)(TotalExpenses / ApprovedBudget) * 100, 1) : 0;

        public List<ExpenseCategoryBreakdown> Breakdown { get; set; } = new();
        public List<ProjectExpenseViewModel> Expenses { get; set; } = new();

        // Status label na hango sa tunay na datos.
        public string BudgetStatusLabel
        {
            get
            {
                if (!HasBudget) return "No Budget Set";
                if (Remaining < 0) return "Over Budget";
                if (Remaining == 0) return "Budget Exhausted";
                if (RawPercentUsed >= 90) return "Near Budget Limit";
                return "Within Budget";
            }
        }

        // Bootstrap contextual color para sa badge/text.
        public string BudgetStatusColor
        {
            get
            {
                if (!HasBudget) return "secondary";
                if (Remaining < 0) return "danger";
                if (Remaining == 0) return "dark";
                if (RawPercentUsed >= 90) return "warning";
                return "success";
            }
        }

        public string ProgressBarColor
        {
            get
            {
                if (Remaining < 0) return "bg-danger";
                if (RawPercentUsed >= 90) return "bg-warning";
                return "bg-success";
            }
        }
    }

    // 🧭 Composite VM para sa Budget Tracker Index — pinagsasama ang fund-source planner
    // at ang optional na project-filtered financial view. Iisang page pa rin.
    public class BudgetTrackerViewModel
    {
        public int SelectedYear { get; set; } = DateTime.Now.Year;
        public int? SelectedProjectId { get; set; }

        // Umiiral na fund-source annual planner (hindi ginagalaw).
        public List<BudgetViewModel> FundBudgets { get; set; } = new();

        // Para sa project filter dropdown.
        public List<ProjectOption> ProjectOptions { get; set; } = new();

        // Naka-set lamang kapag may piniling proyekto.
        public ProjectBudgetSummaryViewModel? ProjectBudget { get; set; }
    }
}
