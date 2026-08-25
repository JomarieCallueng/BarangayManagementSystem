using System.Collections.Generic;
using System.Threading.Tasks;
using BarangayCMS.Entities;

namespace BarangayCMS.DAL.Repository.Interfaces
{
    public interface IReportRepository
    {
        Task<ReportLog?> GetByIdAsync(int id);
        Task<IEnumerable<ReportLog>> GetAllLogsAsync();
        Task AddLogAsync(ReportLog log);
        Task<bool> SaveChangesAsync();
    }
}