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
    public class HealthService : IHealthService
    {
        private readonly IHealthRepository _healthRepo;

        public HealthService(IHealthRepository healthRepo)
        {
            _healthRepo = healthRepo;
        }

        public async Task<HealthDTO?> GetHealthRecordByIdAsync(int id)
        {
            var h = await _healthRepo.GetByIdAsync(id);
            if (h == null) return null;

            return new HealthDTO
            {
                Id = h.HealthRecordId,
                ResidentId = h.ResidentId,
                ResidentName = h.Resident != null ? $"{h.Resident.FirstName} {h.Resident.LastName}" : "Unknown Resident",
                WeightKg = h.WeightKg,
                HeightCm = h.HeightCm,
                BloodType = h.BloodType,
                IsVaccinated = h.IsVaccinated,
                MedicalCondition = h.MedicalCondition,
                HealthClassification = h.HealthClassification,
                LastCheckupDate = h.LastCheckupDate,
                AttendingHealthWorker = h.AttendingHealthWorker
            };
        }

        public async Task<IEnumerable<HealthDTO>> GetAllHealthRecordsAsync()
        {
            var records = await _healthRepo.GetAllWithResidentAsync();
            return records.Select(h => new HealthDTO
            {
                Id = h.HealthRecordId,
                ResidentId = h.ResidentId,
                ResidentName = h.Resident != null ? $"{h.Resident.FirstName} {h.Resident.LastName}" : "Unknown Resident",
                WeightKg = h.WeightKg,
                HeightCm = h.HeightCm,
                BloodType = h.BloodType,
                IsVaccinated = h.IsVaccinated,
                MedicalCondition = h.MedicalCondition,
                HealthClassification = h.HealthClassification,
                LastCheckupDate = h.LastCheckupDate,
                AttendingHealthWorker = h.AttendingHealthWorker
            });
        }

        public async Task<IEnumerable<HealthDTO>> GetHealthRecordsByResidentAsync(int residentId)
        {
            var records = await _healthRepo.GetByResidentIdAsync(residentId);
            return records.Select(h => new HealthDTO
            {
                Id = h.HealthRecordId,
                ResidentId = h.ResidentId,
                WeightKg = h.WeightKg,
                HeightCm = h.HeightCm,
                BloodType = h.BloodType,
                IsVaccinated = h.IsVaccinated,
                MedicalCondition = h.MedicalCondition,
                HealthClassification = h.HealthClassification,
                LastCheckupDate = h.LastCheckupDate,
                AttendingHealthWorker = h.AttendingHealthWorker
            });
        }

        public async Task<bool> LogHealthCheckupAsync(HealthDTO dto)
        {
            var record = new HealthRecord
            {
                ResidentId = dto.ResidentId,
                WeightKg = dto.WeightKg,
                HeightCm = dto.HeightCm,
                BloodType = dto.BloodType,
                IsVaccinated = dto.IsVaccinated,
                MedicalCondition = dto.MedicalCondition,
                HealthClassification = dto.HealthClassification,
                LastCheckupDate = dto.LastCheckupDate,
                AttendingHealthWorker = dto.AttendingHealthWorker,
                DateLogged = DateTime.Now
            };

            await _healthRepo.AddAsync(record);
            return await _healthRepo.SaveChangesAsync();
        }

        public async Task<bool> UpdateHealthRecordAsync(HealthDTO dto)
        {
            var record = await _healthRepo.GetByIdAsync(dto.Id);
            if (record == null) return false;

            record.ResidentId = dto.ResidentId;
            record.WeightKg = dto.WeightKg;
            record.HeightCm = dto.HeightCm;
            record.BloodType = dto.BloodType;
            record.IsVaccinated = dto.IsVaccinated;
            record.MedicalCondition = dto.MedicalCondition;
            record.HealthClassification = dto.HealthClassification;
            record.LastCheckupDate = dto.LastCheckupDate;
            record.AttendingHealthWorker = dto.AttendingHealthWorker;

            _healthRepo.Update(record);
            return await _healthRepo.SaveChangesAsync();
        }

        public async Task<bool> DeleteHealthRecordAsync(int id)
        {
            var record = await _healthRepo.GetByIdAsync(id);
            if (record == null) return false;

            _healthRepo.Delete(record);
            return await _healthRepo.SaveChangesAsync();
        }
    }
}