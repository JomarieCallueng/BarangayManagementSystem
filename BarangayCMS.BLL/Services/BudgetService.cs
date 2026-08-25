using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BarangayCMS.BLL.Interfaces;
using BarangayCMS.DAL.Repository.Interfaces;
using BarangayCMS.DTO;
using BarangayCMS.Entities;

namespace BarangayCMS.BLL.Services
{
    public class BudgetService : IBudgetService
    {
        private readonly IBudgetRepository _budgetRepo;

        public BudgetService(IBudgetRepository budgetRepo)
        {
            _budgetRepo = budgetRepo;
        }

        public async Task<BudgetDTO?> GetBudgetByIdAsync(int id)
        {
            var b = await _budgetRepo.GetByIdAsync(id);
            if (b == null) return null;

            return new BudgetDTO
            {
                Id = b.BudgetId,
                Year = b.Year,
                Category = b.Category,
                Description = b.Description,
                TotalAllocation = b.TotalAllocation,
                DisbursedAmount = b.DisbursedAmount,
                LoggedBy = b.LoggedBy,
                LastUpdated = b.LastUpdated
            };
        }

        public async Task<IEnumerable<BudgetDTO>> GetAllBudgetsAsync()
        {
            var budgets = await _budgetRepo.GetAllAsync();
            return budgets.Select(b => new BudgetDTO
            {
                Id = b.BudgetId,
                Year = b.Year,
                Category = b.Category,
                Description = b.Description,
                TotalAllocation = b.TotalAllocation,
                DisbursedAmount = b.DisbursedAmount,
                LoggedBy = b.LoggedBy,
                LastUpdated = b.LastUpdated
            });
        }

        public async Task<IEnumerable<BudgetDTO>> GetBudgetsByYearAsync(int year)
        {
            var budgets = await _budgetRepo.GetByYearAsync(year);
            return budgets.Select(b => new BudgetDTO
            {
                Id = b.BudgetId,
                Year = b.Year,
                Category = b.Category,
                Description = b.Description,
                TotalAllocation = b.TotalAllocation,
                DisbursedAmount = b.DisbursedAmount,
                LoggedBy = b.LoggedBy,
                LastUpdated = b.LastUpdated
            });
        }

        public async Task<bool> AllocateBudgetAsync(BudgetDTO dto)
        {
            var budget = new Budget
            {
                Year = dto.Year,
                Category = dto.Category,
                Description = dto.Description,
                TotalAllocation = dto.TotalAllocation,
                DisbursedAmount = 0, // 0 muna sa simula dahil bago pa lang na-allocate
                LoggedBy = dto.LoggedBy,
                LastUpdated = DateTime.Now
            };

            await _budgetRepo.AddAsync(budget);
            return await _budgetRepo.SaveChangesAsync();
        }

        public async Task<bool> DisburseAmountAsync(int id, decimal amount, string loggedBy)
        {
            var budget = await _budgetRepo.GetByIdAsync(id);
            if (budget == null) return false;

            // Check kung may sapat pa na pera bago mag-disburse
            if ((budget.TotalAllocation - budget.DisbursedAmount) < amount) return false;

            budget.DisbursedAmount += amount;
            budget.LoggedBy = loggedBy;
            budget.LastUpdated = DateTime.Now;

            _budgetRepo.Update(budget);
            return await _budgetRepo.SaveChangesAsync();
        }

        public async Task<bool> UpdateBudgetAsync(BudgetDTO dto)
        {
            var budget = await _budgetRepo.GetByIdAsync(dto.Id);
            if (budget == null) return false;

            budget.Year = dto.Year;
            budget.Category = dto.Category;
            budget.Description = dto.Description;
            budget.TotalAllocation = dto.TotalAllocation;
            budget.DisbursedAmount = dto.DisbursedAmount;
            budget.LoggedBy = dto.LoggedBy;
            budget.LastUpdated = DateTime.Now;

            _budgetRepo.Update(budget);
            return await _budgetRepo.SaveChangesAsync();
        }
    }
}