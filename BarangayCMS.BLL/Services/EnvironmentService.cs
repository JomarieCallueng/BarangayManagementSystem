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
    public class EnvironmentService : IEnvironmentService
    {
        private readonly IEnvironmentRepository _envRepo;

        public EnvironmentService(IEnvironmentRepository envRepo)
        {
            _envRepo = envRepo;
        }

        public async Task<EnvironmentDTO?> GetRecordByIdAsync(int id)
        {
            var e = await _envRepo.GetByIdAsync(id);
            if (e == null) return null;

            return new EnvironmentDTO
            {
                Id = e.EnvironmentRecordId,
                ActivityName = e.ActivityName,
                LocationArea = e.LocationArea,
                InspectionOrActivityDate = e.InspectionOrActivityDate,
                WasteManagementStatus = e.WasteManagementStatus,
                ViolationsCount = e.ViolationsCount,
                Remarks = e.Remarks,
                InspectorName = e.InspectorName
            };
        }

        public async Task<IEnumerable<EnvironmentDTO>> GetAllRecordsAsync()
        {
            var records = await _envRepo.GetAllAsync();
            return records.Select(e => new EnvironmentDTO
            {
                Id = e.EnvironmentRecordId,
                ActivityName = e.ActivityName,
                LocationArea = e.LocationArea,
                InspectionOrActivityDate = e.InspectionOrActivityDate,
                WasteManagementStatus = e.WasteManagementStatus,
                ViolationsCount = e.ViolationsCount,
                Remarks = e.Remarks,
                InspectorName = e.InspectorName
            });
        }

        public async Task<bool> LogEnvironmentActivityAsync(EnvironmentDTO dto)
        {
            var record = new EnvironmentRecord
            {
                ActivityName = dto.ActivityName,
                LocationArea = dto.LocationArea,
                InspectionOrActivityDate = dto.InspectionOrActivityDate,
                WasteManagementStatus = dto.WasteManagementStatus,
                ViolationsCount = dto.ViolationsCount,
                Remarks = dto.Remarks,
                InspectorName = dto.InspectorName,
                DateLogged = DateTime.Now
            };

            await _envRepo.AddAsync(record);
            return await _envRepo.SaveChangesAsync();
        }

        public async Task<bool> UpdateEnvironmentRecordAsync(EnvironmentDTO dto)
        {
            var record = await _envRepo.GetByIdAsync(dto.Id);
            if (record == null) return false;

            record.ActivityName = dto.ActivityName;
            record.LocationArea = dto.LocationArea;
            record.InspectionOrActivityDate = dto.InspectionOrActivityDate;
            record.WasteManagementStatus = dto.WasteManagementStatus;
            record.ViolationsCount = dto.ViolationsCount;
            record.Remarks = dto.Remarks;
            record.InspectorName = dto.InspectorName;

            _envRepo.Update(record);
            return await _envRepo.SaveChangesAsync();
        }

        public async Task<bool> DeleteRecordAsync(int id)
        {
            var record = await _envRepo.GetByIdAsync(id);
            if (record == null) return false;

            _envRepo.Delete(record);
            return await _envRepo.SaveChangesAsync();
        }
    }
}