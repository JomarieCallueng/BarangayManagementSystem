using System.Collections.Generic;
using System.Threading.Tasks;
using BarangayCMS.DTO;

namespace BarangayCMS.BLL.Interfaces
{
    public interface ICertificateRequirementService
    {
        // Para sa request form: active requirements lang, naka-sort by DisplayOrder.
        Task<IEnumerable<CertificateRequirementDTO>> GetActiveByCertificateTypeIdAsync(int certificateTypeId);
        Task<IEnumerable<CertificateRequirementDTO>> GetActiveByCertificateNameAsync(string certificateName);

        // Para sa admin management: kasama ang inactive.
        Task<IEnumerable<CertificateRequirementDTO>> GetAllByCertificateTypeIdAsync(int certificateTypeId);

        Task<CertificateRequirementDTO?> GetByIdAsync(int id);
        Task<bool> AddAsync(CertificateRequirementDTO dto);
        Task<bool> UpdateAsync(CertificateRequirementDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}
