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
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepo;
        private readonly IComplaintRepository _complaintRepo;
        private readonly IBudgetRepository _budgetRepo;
        private readonly IProjectRepository _projectRepo;
        private readonly IDisasterRepository _disasterRepo;

        public ReportService(
            IReportRepository reportRepo,
            IComplaintRepository complaintRepo,
            IBudgetRepository budgetRepo,
            IProjectRepository projectRepo,
            IDisasterRepository disasterRepo)
        {
            _reportRepo = reportRepo;
            _complaintRepo = complaintRepo;
            _budgetRepo = budgetRepo;
            _projectRepo = projectRepo;
            _disasterRepo = disasterRepo;
        }

        public async Task<ReportDTO> GenerateDashboardMetricsAsync(string reportType)
        {
            // 1. Hilaan ng data mula sa Complaints module
            var complaints = await _complaintRepo.GetAllAsync();
            int activeComplaints = complaints.Count(c => c.Status != "Resolved" && c.Status != "Dismissed");
            int resolvedComplaints = complaints.Count(c => c.Status == "Resolved");

            // 2. Hilaan ng data mula sa Financial Budget module
            var budgets = await _budgetRepo.GetAllAsync();
            decimal totalBudget = budgets.Sum(b => b.TotalAllocation);

            // 3. Hilaan ng data mula sa Infrastructure Projects
            var projects = await _projectRepo.GetAllAsync();
            decimal totalExpenses = projects.Sum(p => p.TotalExpenses);

            // 4. Hilaan ng data mula sa Disaster module
            var disasters = await _disasterRepo.GetAllAsync();
            int totalIncidents = disasters.Count();
            int displacedFamilies = disasters.Sum(d => d.AffectedHouseholdsCount);

            // 5. I-map at ibalik lahat gamit ang ReportDTO
            return new ReportDTO
            {
                TargetReportType = reportType,
                GeneratedAt = DateTime.Now,

                ActiveComplaintsCount = activeComplaints,
                ResolvedComplaintsCount = resolvedComplaints,

                TotalAllocatedBudget = totalBudget,
                TotalProjectExpenses = totalExpenses,

                TotalRecordedIncidents = totalIncidents,
                DisplacedFamiliesCount = displacedFamilies,

                // TODO: Kung may Resident Repository ka na, i-count mo rito:
                TotalPopulation = 0,
                TotalRegisteredVoters = 0,
                TotalHouseholds = 0
            };
        }

        public async Task<bool> LogGeneratedReportAsync(ReportDTO dto, string generatedBy)
        {
            var log = new ReportLog
            {
                TargetReportType = dto.TargetReportType,
                GeneratedAt = dto.GeneratedAt,
                TotalPopulation = dto.TotalPopulation,
                TotalRegisteredVoters = dto.TotalRegisteredVoters,
                TotalHouseholds = dto.TotalHouseholds,
                ActiveComplaintsCount = dto.ActiveComplaintsCount,
                ResolvedComplaintsCount = dto.ResolvedComplaintsCount,
                TotalAllocatedBudget = dto.TotalAllocatedBudget,
                TotalProjectExpenses = dto.TotalProjectExpenses,
                TotalRecordedIncidents = dto.TotalRecordedIncidents,
                DisplacedFamiliesCount = dto.DisplacedFamiliesCount,
                GeneratedBy = generatedBy
            };

            await _reportRepo.AddLogAsync(log);
            return await _reportRepo.SaveChangesAsync();
        }

        public async Task<IEnumerable<ReportDTO>> GetReportHistoryAsync()
        {
            var logs = await _reportRepo.GetAllLogsAsync();
            return logs.Select(l => new ReportDTO
            {
                TargetReportType = l.TargetReportType,
                GeneratedAt = l.GeneratedAt,
                TotalPopulation = l.TotalPopulation,
                TotalRegisteredVoters = l.TotalRegisteredVoters,
                TotalHouseholds = l.TotalHouseholds,
                ActiveComplaintsCount = l.ActiveComplaintsCount,
                ResolvedComplaintsCount = l.ResolvedComplaintsCount,
                TotalAllocatedBudget = l.TotalAllocatedBudget,
                TotalProjectExpenses = l.TotalProjectExpenses,
                TotalRecordedIncidents = l.TotalRecordedIncidents,
                DisplacedFamiliesCount = l.DisplacedFamiliesCount
            });
        }
    }
}