using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BarangayCMS.DAL.Context;
using BarangayCMS.Entities;
using BarangayCMS.DAL.Repository.Interfaces;

namespace BarangayCMS.DAL.Repository
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly ApplicationDbContext _context;

        public ProjectRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Project?> GetByIdAsync(int id) =>
            await _context.Projects.FindAsync(id);

        public async Task<IEnumerable<Project>> GetAllAsync() =>
            await _context.Projects.OrderByDescending(p => p.StartDate).ToListAsync();

        public async Task<IEnumerable<Project>> GetProjectsByStatusAsync(string status) =>
            await _context.Projects
                .Where(p => p.Status == status)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();

        public async Task AddAsync(Project project) =>
            await _context.Projects.AddAsync(project);

        public void Update(Project project) =>
            _context.Projects.Update(project);

        public void Delete(Project project) =>
            _context.Projects.Remove(project);

        public async Task<bool> SaveChangesAsync() =>
            await _context.SaveChangesAsync() > 0;
    }
}