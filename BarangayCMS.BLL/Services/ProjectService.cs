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
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepo;

        public ProjectService(IProjectRepository projectRepo)
        {
            _projectRepo = projectRepo;
        }

        public async Task<ProjectDTO?> GetProjectByIdAsync(int id)
        {
            var p = await _projectRepo.GetByIdAsync(id);
            if (p == null) return null;

            return new ProjectDTO
            {
                Id = p.ProjectId,
                Title = p.Title,
                Description = p.Description,
                Location = p.Location,
                BudgetAllocated = p.BudgetAllocated,
                TotalExpenses = p.TotalExpenses,
                Contractor = p.Contractor,
                Status = p.Status,
                StartDate = p.StartDate,
                EndDate = p.EndDate
            };
        }

        public async Task<IEnumerable<ProjectDTO>> GetAllProjectsAsync()
        {
            var projects = await _projectRepo.GetAllAsync();
            return projects.Select(p => new ProjectDTO
            {
                Id = p.ProjectId,
                Title = p.Title,
                Description = p.Description,
                Location = p.Location,
                BudgetAllocated = p.BudgetAllocated,
                TotalExpenses = p.TotalExpenses,
                Contractor = p.Contractor,
                Status = p.Status,
                StartDate = p.StartDate,
                EndDate = p.EndDate
            });
        }

        public async Task<IEnumerable<ProjectDTO>> GetProjectsByStatusAsync(string status)
        {
            var projects = await _projectRepo.GetProjectsByStatusAsync(status);
            return projects.Select(p => new ProjectDTO
            {
                Id = p.ProjectId,
                Title = p.Title,
                Description = p.Description,
                Location = p.Location,
                BudgetAllocated = p.BudgetAllocated,
                TotalExpenses = p.TotalExpenses,
                Contractor = p.Contractor,
                Status = p.Status,
                StartDate = p.StartDate,
                EndDate = p.EndDate
            });
        }

        public async Task<bool> ProposeProjectAsync(ProjectDTO dto)
        {
            var project = new Project
            {
                Title = dto.Title,
                Description = dto.Description,
                Location = dto.Location,
                BudgetAllocated = dto.BudgetAllocated,
                TotalExpenses = 0, // 0 muna sa umpisa ng Planning
                Contractor = dto.Contractor,
                Status = "Planning",
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                DateLogged = DateTime.Now
            };

            await _projectRepo.AddAsync(project);
            return await _projectRepo.SaveChangesAsync();
        }

        public async Task<bool> UpdateProjectDetailsAsync(ProjectDTO dto)
        {
            var project = await _projectRepo.GetByIdAsync(dto.Id);
            if (project == null) return false;

            project.Title = dto.Title;
            project.Description = dto.Description;
            project.Location = dto.Location;
            project.BudgetAllocated = dto.BudgetAllocated;
            project.TotalExpenses = dto.TotalExpenses;
            project.Contractor = dto.Contractor;
            project.Status = dto.Status;
            project.StartDate = dto.StartDate;
            project.EndDate = dto.EndDate;
            project.LastUpdated = DateTime.Now;

            _projectRepo.Update(project);
            return await _projectRepo.SaveChangesAsync();
        }

        public async Task<bool> UpdateExpensesAsync(int id, decimal additionalExpense)
        {
            var project = await _projectRepo.GetByIdAsync(id);
            if (project == null) return false;

            // Opsyonal: Lagyan ng validation kung lumalampas sa budget allocation
            project.TotalExpenses += additionalExpense;
            project.LastUpdated = DateTime.Now;

            _projectRepo.Update(project);
            return await _projectRepo.SaveChangesAsync();
        }

        public async Task<bool> DeleteProjectAsync(int id)
        {
            var project = await _projectRepo.GetByIdAsync(id);
            if (project == null) return false;

            _projectRepo.Delete(project);
            return await _projectRepo.SaveChangesAsync();
        }
    }
}