using System.Collections.Generic;
using System.Threading.Tasks;
using BarangayCMS.Entities;

namespace BarangayCMS.DAL.Repository.Interfaces
{
    public interface IEvacuationRepository
    {
        // ---- Evacuation Centers ----
        Task<EvacuationCenter?> GetCenterByIdAsync(int id);
        Task<IEnumerable<EvacuationCenter>> GetAllCentersAsync();
        Task<IEnumerable<EvacuationCenter>> GetActiveCentersAsync(); // publiko: active lang
        Task AddCenterAsync(EvacuationCenter center);
        void UpdateCenter(EvacuationCenter center);
        void DeleteCenter(EvacuationCenter center);

        // ---- Barangay-wide Evacuation Status (singleton) ----
        Task<EvacuationStatus?> GetStatusAsync();
        Task AddStatusAsync(EvacuationStatus status);
        void UpdateStatus(EvacuationStatus status);

        Task<bool> SaveChangesAsync();
    }
}
