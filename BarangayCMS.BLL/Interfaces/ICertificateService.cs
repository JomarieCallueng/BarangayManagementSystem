using System.Collections.Generic;
using System.Threading.Tasks;
using BarangayCMS.DTO;

namespace BarangayCMS.BLL.Interfaces
{
    public interface ICertificateService
    {
        Task<CertificateDTO?> GetCertificateByIdAsync(int id);
        Task<IEnumerable<CertificateDTO>> GetAllCertificatesAsync();
        Task<IEnumerable<CertificateDTO>> GetCertificatesByResidentAsync(int residentId);
        Task<bool> RequestCertificateAsync(CertificateDTO dto);
        Task<bool> IssueCertificateAsync(int id, string controlNumber, string issuedBy);
        Task<bool> UpdateStatusAsync(int id, string status);
    }
}