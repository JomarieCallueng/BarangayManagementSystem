using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BarangayCMS.BLL.Interfaces;
using BarangayCMS.DAL.Repository;
using BarangayCMS.DAL.Repository.Interfaces;
using BarangayCMS.DTO;
using BarangayCMS.Entities;
using Microsoft.EntityFrameworkCore;

namespace BarangayCMS.BLL.Services
{
    public class CertificateService : ICertificateService
    {
        private readonly ICertificateRepository _certRepo;

        public CertificateService(ICertificateRepository certRepo)
        {
            _certRepo = certRepo;
        }

        public async Task<CertificateDTO?> GetCertificateByIdAsync(int id)
        {
            var cert = await _certRepo.GetByIdAsync(id);
            if (cert == null) return null;

            string displayName = "Unknown";
            if (!string.IsNullOrEmpty(cert.ResidentName))
            {
                displayName = cert.ResidentName;
            }
            else if (cert.Resident != null)
            {
                displayName = $"{cert.Resident.FirstName} {cert.Resident.LastName}";
            }

            return new CertificateDTO
            {
                Id = cert.CertificateId,
                ResidentId = cert.ResidentId.GetValueOrDefault(),
                ResidentName = displayName,
                CertificateType = cert.CertificateType,
                Purpose = cert.Purpose,
                ControlNumber = cert.ControlNumber,
                FeePaid = cert.FeePaid,
                PaymentReceiptPath = cert.PaymentReceiptPath,
                OfficialReceiptNumber = cert.OfficialReceiptNumber,
                Status = cert.Status,
                IssuedDate = cert.DateRequested,
                IssuedBy = cert.IssuedBy
            };
        }

        public async Task<IEnumerable<CertificateDTO>> GetAllCertificatesAsync()
        {
            // 🌟 ITINAMA: Ginamit ang 'GetAllWithResidentAsync()' mula sa iyong repository interface
            var certificates = await _certRepo.GetAllWithResidentAsync();

            return certificates.Select(c => {
                string displayName = "Unknown";
                if (!string.IsNullOrEmpty(c.ResidentName))
                {
                    displayName = c.ResidentName;
                }
                else if (c.Resident != null)
                {
                    displayName = $"{c.Resident.FirstName} {c.Resident.LastName}";
                }

                return new CertificateDTO
                {
                    Id = c.CertificateId,
                    ResidentId = c.ResidentId.GetValueOrDefault(),
                    ResidentName = displayName,
                    CertificateType = c.CertificateType,
                    Purpose = c.Purpose,
                    ControlNumber = c.ControlNumber,
                    OfficialReceiptNumber = c.OfficialReceiptNumber,
                    Status = c.Status,
                    IssuedDate = c.DateIssued ?? default,
                    IssuedBy = c.IssuedBy,
                    FeePaid = c.FeePaid,
                    PaymentReceiptPath = c.PaymentReceiptPath
                };
            }).ToList();
        }

        public async Task<IEnumerable<CertificateDTO>> GetCertificatesByResidentAsync(int residentId)
        {
            var certs = await _certRepo.GetByResidentIdAsync(residentId);
            return certs.Select(cert => new CertificateDTO
            {
                Id = cert.CertificateId,
                ResidentId = cert.ResidentId.GetValueOrDefault(),
                CertificateType = cert.CertificateType,
                Purpose = cert.Purpose,
                ControlNumber = cert.ControlNumber,
                Status = cert.Status,
                IssuedDate = cert.DateRequested,
                FeePaid = cert.FeePaid,
                PaymentReceiptPath = cert.PaymentReceiptPath
            });
        }

        public async Task<bool> RequestCertificateAsync(CertificateDTO dto)
        {
            var entity = new Certificate
            {
                CertificateType = dto.CertificateType,
                Purpose = dto.Purpose,
                Status = "Pending",
                DateRequested = DateTime.Now,
                ResidentName = dto.ResidentName,
                ResidentId = dto.ResidentId > 0 ? dto.ResidentId : null,
                PaymentReceiptPath = dto.PaymentReceiptPath,
                FeePaid = dto.FeePaid
            };

            await _certRepo.AddAsync(entity);
            return await _certRepo.SaveChangesAsync();
        }

        public async Task<bool> IssueCertificateAsync(int id, string controlNumber, string issuedBy)
        {
            var cert = await _certRepo.GetByIdAsync(id);
            if (cert == null) return false;

            cert.ControlNumber = controlNumber;
            cert.IssuedBy = issuedBy;
            cert.Status = "Issued";
            cert.DateIssued = DateTime.Now;

            _certRepo.Update(cert);
            return await _certRepo.SaveChangesAsync();
        }

        public async Task<bool> UpdateStatusAsync(int id, string status)
        {
            var cert = await _certRepo.GetByIdAsync(id);
            if (cert == null) return false;

            cert.Status = status;
            _certRepo.Update(cert);
            return await _certRepo.SaveChangesAsync();
        }
    }
}