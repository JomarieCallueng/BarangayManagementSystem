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
    public class EvacuationService : IEvacuationService
    {
        private readonly IEvacuationRepository _repo;

        public EvacuationService(IEvacuationRepository repo)
        {
            _repo = repo;
        }

        // ==========================================================
        // PUBLIC (read-only)
        // ==========================================================
        public async Task<PublicEvacuationDTO> GetPublicEvacuationInfoAsync()
        {
            var status = await _repo.GetStatusAsync();
            var centers = (await _repo.GetActiveCentersAsync()).ToList();

            var statusDto = status == null
                ? new EvacuationStatusDTO { OverallStatus = "Normal", DateUpdated = DateTime.Now }
                : MapStatus(status);

            var centerDtos = centers.Select(MapCenter).ToList();

            var result = new PublicEvacuationDTO
            {
                Status = statusDto,
                ActiveCenters = centerDtos,
                Instructions = ParseLines(statusDto.Instructions),
                EmergencyContacts = ParseContacts(statusDto.EmergencyContacts),
                TotalCapacity = centerDtos.Sum(c => c.Capacity),
                TotalOccupants = centerDtos.Sum(c => c.CurrentOccupants),
                TotalAvailableSlots = centerDtos.Sum(c => c.AvailableSlots)
            };

            // Last updated = pinakabagong petsa sa pagitan ng status at mga center.
            var timestamps = new List<DateTime> { statusDto.DateUpdated };
            timestamps.AddRange(centerDtos.Where(c => c.DateUpdated.HasValue).Select(c => c.DateUpdated!.Value));
            result.LastUpdated = timestamps.Max();

            return result;
        }

        // ==========================================================
        // ADMIN/STAFF — CENTERS
        // ==========================================================
        public async Task<IEnumerable<EvacuationCenterDTO>> GetAllCentersAsync()
        {
            var centers = await _repo.GetAllCentersAsync();
            return centers.Select(MapCenter);
        }

        public async Task<EvacuationCenterDTO?> GetCenterByIdAsync(int id)
        {
            var c = await _repo.GetCenterByIdAsync(id);
            return c == null ? null : MapCenter(c);
        }

        public async Task<bool> AddCenterAsync(EvacuationCenterDTO dto)
        {
            var center = new EvacuationCenter
            {
                Name = dto.Name?.Trim() ?? string.Empty,
                Address = dto.Address?.Trim() ?? string.Empty,
                SitioPurok = dto.SitioPurok?.Trim() ?? string.Empty,
                Capacity = Math.Max(0, dto.Capacity),
                CurrentOccupants = Math.Max(0, dto.CurrentOccupants),
                ContactNumber = dto.ContactNumber?.Trim() ?? string.Empty,
                Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
                IsActive = dto.IsActive,
                DateCreated = DateTime.Now
            };

            await _repo.AddCenterAsync(center);
            return await _repo.SaveChangesAsync();
        }

        public async Task<bool> UpdateCenterAsync(EvacuationCenterDTO dto)
        {
            var center = await _repo.GetCenterByIdAsync(dto.Id);
            if (center == null) return false;

            center.Name = dto.Name?.Trim() ?? string.Empty;
            center.Address = dto.Address?.Trim() ?? string.Empty;
            center.SitioPurok = dto.SitioPurok?.Trim() ?? string.Empty;
            center.Capacity = Math.Max(0, dto.Capacity);
            center.CurrentOccupants = Math.Max(0, dto.CurrentOccupants);
            center.ContactNumber = dto.ContactNumber?.Trim() ?? string.Empty;
            center.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();
            center.IsActive = dto.IsActive;
            center.DateUpdated = DateTime.Now;

            _repo.UpdateCenter(center);
            return await _repo.SaveChangesAsync();
        }

        public async Task<bool> DeleteCenterAsync(int id)
        {
            var center = await _repo.GetCenterByIdAsync(id);
            if (center == null) return false;

            _repo.DeleteCenter(center);
            return await _repo.SaveChangesAsync();
        }

        public async Task<bool> ToggleCenterActiveAsync(int id)
        {
            var center = await _repo.GetCenterByIdAsync(id);
            if (center == null) return false;

            center.IsActive = !center.IsActive;
            center.DateUpdated = DateTime.Now;

            _repo.UpdateCenter(center);
            return await _repo.SaveChangesAsync();
        }

        // ==========================================================
        // ADMIN/STAFF — STATUS
        // ==========================================================
        public async Task<EvacuationStatusDTO> GetStatusAsync()
        {
            var status = await _repo.GetStatusAsync();
            return status == null
                ? new EvacuationStatusDTO { OverallStatus = "Normal", DateUpdated = DateTime.Now }
                : MapStatus(status);
        }

        public async Task<bool> UpdateStatusAsync(EvacuationStatusDTO dto)
        {
            var status = await _repo.GetStatusAsync();
            if (status == null)
            {
                status = new EvacuationStatus();
                ApplyStatus(status, dto);
                await _repo.AddStatusAsync(status);
            }
            else
            {
                ApplyStatus(status, dto);
                _repo.UpdateStatus(status);
            }

            return await _repo.SaveChangesAsync();
        }

        private static void ApplyStatus(EvacuationStatus status, EvacuationStatusDTO dto)
        {
            // Normalize sa isa sa tatlong pinapayagang value.
            var s = (dto.OverallStatus ?? "Normal").Trim();
            status.OverallStatus = s.Equals("Active", StringComparison.OrdinalIgnoreCase) ? "Active"
                : s.Equals("Prepared", StringComparison.OrdinalIgnoreCase) ? "Prepared"
                : "Normal";
            status.StatusMessage = string.IsNullOrWhiteSpace(dto.StatusMessage) ? null : dto.StatusMessage.Trim();
            status.Instructions = dto.Instructions ?? string.Empty;
            status.EmergencyContacts = dto.EmergencyContacts ?? string.Empty;
            status.UpdatedBy = dto.UpdatedBy;
            status.DateUpdated = DateTime.Now;
        }

        // ==========================================================
        // Mappers + parsers
        // ==========================================================
        private static EvacuationCenterDTO MapCenter(EvacuationCenter c) => new EvacuationCenterDTO
        {
            Id = c.EvacuationCenterId,
            Name = c.Name,
            Address = c.Address,
            SitioPurok = c.SitioPurok,
            Capacity = c.Capacity,
            CurrentOccupants = c.CurrentOccupants,
            ContactNumber = c.ContactNumber,
            Notes = c.Notes,
            IsActive = c.IsActive,
            DateUpdated = c.DateUpdated ?? c.DateCreated
        };

        private static EvacuationStatusDTO MapStatus(EvacuationStatus s) => new EvacuationStatusDTO
        {
            Id = s.Id,
            OverallStatus = s.OverallStatus,
            StatusMessage = s.StatusMessage,
            Instructions = s.Instructions,
            EmergencyContacts = s.EmergencyContacts,
            UpdatedBy = s.UpdatedBy,
            DateUpdated = s.DateUpdated
        };

        // Hatiin ang multiline text sa listahan (aalisin ang blangkong linya).
        private static List<string> ParseLines(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return new List<string>();
            return raw
                .Replace("\r\n", "\n").Replace("\r", "\n")
                .Split('\n')
                .Select(l => l.Trim())
                .Where(l => l.Length > 0)
                .ToList();
        }

        // Parse ng "Label|Number" kada linya. Kung walang '|', ituturing na
        // parehong label at number ang buong linya.
        private static List<EmergencyContactDTO> ParseContacts(string? raw)
        {
            var contacts = new List<EmergencyContactDTO>();
            foreach (var line in ParseLines(raw))
            {
                var parts = line.Split('|');
                if (parts.Length >= 2)
                {
                    contacts.Add(new EmergencyContactDTO
                    {
                        Label = parts[0].Trim(),
                        Number = parts[1].Trim()
                    });
                }
                else
                {
                    contacts.Add(new EmergencyContactDTO { Label = line, Number = line });
                }
            }
            return contacts;
        }
    }
}
