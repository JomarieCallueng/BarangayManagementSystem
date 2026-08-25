using System.Collections.Generic;
using System.Threading.Tasks;
using BarangayCMS.DTO;

namespace BarangayCMS.BLL.Interfaces
{
    public interface IReportService
    {
        // Ito ang kukuha ng real-time computation para sa Dashboard
        Task<ReportDTO> GenerateDashboardMetricsAsync(string reportType);

        // Para sa pag-save at pag-view ng mga lumang analytics generation
        Task<bool> LogGeneratedReportAsync(ReportDTO dto, string generatedBy);
        Task<IEnumerable<ReportDTO>> GetReportHistoryAsync();
    }
}