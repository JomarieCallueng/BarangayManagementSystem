using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BarangayCMS.BLL.Interfaces;
using BarangayCMS.DAL.Context;
using BarangayCMS.DTO;
using BarangayCMS.Entities;
using Microsoft.EntityFrameworkCore;

namespace BarangayCMS.BLL.Services
{
    public class CertificateRequirementService : ICertificateRequirementService
    {
        private readonly ApplicationDbContext _context;

        public CertificateRequirementService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CertificateRequirementDTO>> GetActiveByCertificateTypeIdAsync(int certificateTypeId)
        {
            var items = await _context.CertificateRequirements
                .Where(r => r.CertificateTypeId == certificateTypeId && r.IsActive)
                .OrderByDescending(r => r.IsRequired)
                .ThenBy(r => r.DisplayOrder)
                .ThenBy(r => r.CertificateRequirementId)
                .ToListAsync();

            return items.Select(MapToDto);
        }

        public async Task<IEnumerable<CertificateRequirementDTO>> GetActiveByCertificateNameAsync(string certificateName)
        {
            if (string.IsNullOrWhiteSpace(certificateName))
                return Enumerable.Empty<CertificateRequirementDTO>();

            var type = await _context.CertificateTypes
                .FirstOrDefaultAsync(t => t.CertificateName == certificateName);

            if (type == null)
                return Enumerable.Empty<CertificateRequirementDTO>();

            return await GetActiveByCertificateTypeIdAsync(type.CertificateTypeId);
        }

        public async Task<IEnumerable<CertificateRequirementDTO>> GetAllByCertificateTypeIdAsync(int certificateTypeId)
        {
            var items = await _context.CertificateRequirements
                .Where(r => r.CertificateTypeId == certificateTypeId)
                .OrderBy(r => r.DisplayOrder)
                .ThenBy(r => r.CertificateRequirementId)
                .ToListAsync();

            return items.Select(MapToDto);
        }

        public async Task<CertificateRequirementDTO?> GetByIdAsync(int id)
        {
            var entity = await _context.CertificateRequirements.FindAsync(id);
            return entity == null ? null : MapToDto(entity);
        }

        public async Task<bool> AddAsync(CertificateRequirementDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.RequirementName))
                return false;

            // Iwas duplicate: parehong pangalan sa parehong certificate type.
            bool exists = await _context.CertificateRequirements.AnyAsync(r =>
                r.CertificateTypeId == dto.CertificateTypeId &&
                r.RequirementName == dto.RequirementName.Trim());

            if (exists) return false;

            var entity = new CertificateRequirement
            {
                CertificateTypeId = dto.CertificateTypeId,
                RequirementName = dto.RequirementName.Trim(),
                Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
                IsRequired = dto.IsRequired,
                DisplayOrder = dto.DisplayOrder,
                IsActive = dto.IsActive
            };

            _context.CertificateRequirements.Add(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(CertificateRequirementDTO dto)
        {
            var entity = await _context.CertificateRequirements.FindAsync(dto.Id);
            if (entity == null || string.IsNullOrWhiteSpace(dto.RequirementName))
                return false;

            // Iwas duplicate laban sa ibang record.
            bool clash = await _context.CertificateRequirements.AnyAsync(r =>
                r.CertificateTypeId == entity.CertificateTypeId &&
                r.RequirementName == dto.RequirementName.Trim() &&
                r.CertificateRequirementId != dto.Id);

            if (clash) return false;

            entity.RequirementName = dto.RequirementName.Trim();
            entity.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
            entity.IsRequired = dto.IsRequired;
            entity.DisplayOrder = dto.DisplayOrder;
            entity.IsActive = dto.IsActive;

            _context.CertificateRequirements.Update(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.CertificateRequirements.FindAsync(id);
            if (entity == null) return false;

            _context.CertificateRequirements.Remove(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        private static CertificateRequirementDTO MapToDto(CertificateRequirement r) => new CertificateRequirementDTO
        {
            Id = r.CertificateRequirementId,
            CertificateTypeId = r.CertificateTypeId,
            RequirementName = r.RequirementName,
            Description = r.Description,
            IsRequired = r.IsRequired,
            DisplayOrder = r.DisplayOrder,
            IsActive = r.IsActive
        };
    }
}
