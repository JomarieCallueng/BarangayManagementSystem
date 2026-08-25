using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BarangayCMS.DAL.Context;
using BarangayCMS.Entities;
using BarangayCMS.DAL.Repository.Interfaces;

namespace BarangayCMS.DAL.Repository
{
    public class BudgetRepository : IBudgetRepository
    {
        private readonly ApplicationDbContext _context;

        public BudgetRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Budget?> GetByIdAsync(int id) =>
            await _context.Budgets.FindAsync(id);

        public async Task<IEnumerable<Budget>> GetAllAsync() =>
            await _context.Budgets.OrderByDescending(b => b.Year).ThenBy(b => b.Category).ToListAsync();

        public async Task<IEnumerable<Budget>> GetByYearAsync(int year) =>
            await _context.Budgets.Where(b => b.Year == year).ToListAsync();

        public async Task AddAsync(Budget budget) =>
            await _context.Budgets.AddAsync(budget);

        public void Update(Budget budget) =>
            _context.Budgets.Update(budget);

        public void Delete(Budget budget) =>
            _context.Budgets.Remove(budget);

        public async Task<bool> SaveChangesAsync() =>
            await _context.SaveChangesAsync() > 0;
    }
}