using System.Collections.Generic;
using System.Threading.Tasks;
using BarangayCMS.DTO;

namespace BarangayCMS.BLL.Interfaces
{
    public interface IBudgetService
    {
        Task<BudgetDTO?> GetBudgetByIdAsync(int id);
        Task<IEnumerable<BudgetDTO>> GetAllBudgetsAsync();
        Task<IEnumerable<BudgetDTO>> GetBudgetsByYearAsync(int year);
        Task<bool> AllocateBudgetAsync(BudgetDTO dto);
        Task<bool> DisburseAmountAsync(int id, decimal amount, string loggedBy);
        Task<bool> UpdateBudgetAsync(BudgetDTO dto);
    }
}