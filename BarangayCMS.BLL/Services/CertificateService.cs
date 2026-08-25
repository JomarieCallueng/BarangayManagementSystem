using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BarangayCMS.BLL.Interfaces;
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
            string purokName = string.Empty;

            if (!string.IsNullOrEmpty(cert.ResidentName))
            {
                displayName = cert.ResidentName;
            }

            if (cert.Resident != null)
            {
                if (string.IsNullOrEmpty(cert.ResidentName))
                {
                    displayName = $"{cert.Resident.FirstName} {cert.Resident.LastName}".Trim();
                }
                // 🟢 Kunin ang Purok/Sitio mula sa Resident Entity
                purokName = cert.Resident.SitioPurok ?? string.Empty;
            }

            return new CertificateDTO
            {
                Id = cert.CertificateId,
                ResidentId = cert.ResidentId.GetValueOrDefault(),
                ResidentName = displayName,
                Purok = purokName, // 🟢 Naipasa na ang Purok
                CertificateType = cert.CertificateType ?? string.Empty,
                Purpose = cert.Purpose ?? string.Empty,
                ControlNumber = cert.ControlNumber ?? "N/A",
                FeePaid = cert.FeePaid,
                PaymentReceiptPath = cert.PaymentReceiptPath ?? string.Empty,
                OfficialReceiptNumber = cert.OfficialReceiptNumber ?? string.Empty,
                Status = cert.Status ?? "Pending",
                IssuedDate = cert.DateIssued ?? cert.DateRequested,
                IssuedBy = cert.IssuedBy ?? string.Empty
            };
        }

        public async Task<IEnumerable<CertificateDTO>> GetAllCertificatesAsync()
        {
            var certificates = await _certRepo.GetAllWithResidentAsync();

            return certificates.Select(c => {
                string displayName = "Unknown";
                string purokName = string.Empty;

                if (!string.IsNullOrEmpty(c.ResidentName))
                {
                    displayName = c.ResidentName;
                }

                if (c.Resident != null)
                {
                    if (string.IsNullOrEmpty(c.ResidentName))
                    {
                        displayName = $"{c.Resident.FirstName} {c.Resident.LastName}".Trim();
                    }
                    // 🟢 Kunin ang Purok/Sitio mula sa Resident Entity
                    purokName = c.Resident.SitioPurok ?? string.Empty;
                }

                return new CertificateDTO
                {
                    Id = c.CertificateId,
                    ResidentId = c.ResidentId.GetValueOrDefault(),
                    ResidentName = displayName,
                    Purok = purokName, // 🟢 Naipasa na ang Purok
                    CertificateType = c.CertificateType ?? string.Empty,
                    Purpose = c.Purpose ?? string.Empty,
                    ControlNumber = string.IsNullOrEmpty(c.ControlNumber) ? "N/A" : c.ControlNumber,
                    OfficialReceiptNumber = c.OfficialReceiptNumber ?? string.Empty,
                    Status = c.Status ?? "Pending",
                    IssuedDate = c.DateIssued ?? c.DateRequested,
                    IssuedBy = c.IssuedBy ?? string.Empty,
                    FeePaid = c.FeePaid,
                    PaymentReceiptPath = c.PaymentReceiptPath ?? string.Empty
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
                CertificateType = cert.CertificateType ?? string.Empty,
                Purpose = cert.Purpose ?? string.Empty,
                ControlNumber = cert.ControlNumber ?? "N/A",
                Status = cert.Status ?? "Pending",
                IssuedDate = cert.DateRequested,
                FeePaid = cert.FeePaid,
                PaymentReceiptPath = cert.PaymentReceiptPath ?? string.Empty,
                Purok = cert.Resident != null ? (cert.Resident.SitioPurok ?? string.Empty) : string.Empty
            });
        }

        // ⚡ FINAL FIX: REQUEST / CREATE CERTIFICATE METHOD
        public async Task<bool> RequestCertificateAsync(CertificateDTO dto)
        {
            try
            {
                // Siguraduhing may valid na Control Number
                string autoControlNo = string.IsNullOrEmpty(dto.ControlNumber) || dto.ControlNumber == "N/A"
                    ? $"BRGY-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}"
                    : dto.ControlNumber;

                // Siguraduhing naipapasa nang tama ang ResidentId
                int? validResidentId = dto.ResidentId > 0 ? dto.ResidentId : (int?)null;

                var entity = new Certificate
                {
                    CertificateType = !string.IsNullOrEmpty(dto.CertificateType) ? dto.CertificateType : "Barangay Clearance",
                    Purpose = !string.IsNullOrEmpty(dto.Purpose) ? dto.Purpose : "General Purpose",
                    Status = string.IsNullOrEmpty(dto.Status) ? "Pending" : dto.Status,
                    DateRequested = DateTime.Now,
                    DateIssued = dto.IssuedDate != default ? dto.IssuedDate : DateTime.Now,
                    ResidentName = !string.IsNullOrEmpty(dto.ResidentName) ? dto.ResidentName : "Unknown",
                    ResidentId = validResidentId,
                    PaymentReceiptPath = dto.PaymentReceiptPath ?? string.Empty,
                    FeePaid = dto.FeePaid,
                    ControlNumber = autoControlNo,
                    OfficialReceiptNumber = dto.OfficialReceiptNumber ?? string.Empty,
                    IssuedBy = !string.IsNullOrEmpty(dto.IssuedBy) ? dto.IssuedBy : "Staff Admin"
                };

                await _certRepo.AddAsync(entity);
                return await _certRepo.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                var innerMsg = dbEx.InnerException != null ? dbEx.InnerException.Message : dbEx.Message;
                Debug.WriteLine($"[SQL DATABASE ERROR]: {innerMsg}");
                throw new Exception($"Database Save Failure: {innerMsg}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GENERAL ERROR]: {ex.Message}");
                throw;
            }
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