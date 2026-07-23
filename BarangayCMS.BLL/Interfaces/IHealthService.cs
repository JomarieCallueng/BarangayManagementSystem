using System.Collections.Generic;
using System.Threading.Tasks;
using BarangayCMS.DTO;

namespace BarangayCMS.BLL.Interfaces
{
    public interface IHealthService
    {
        Task<HealthDTO?> GetHealthRecordByIdAsync(int id);
        Task<IEnumerable<HealthDTO>> GetAllHealthRecordsAsync();
        Task<IEnumerable<HealthDTO>> GetHealthRecordsByResidentAsync(int residentId);
        Task<bool> LogHealthCheckupAsync(HealthDTO dto);
        Task<bool> UpdateHealthRecordAsync(HealthDTO dto);
        Task<bool> DeleteHealthRecordAsync(int id);
    }
}