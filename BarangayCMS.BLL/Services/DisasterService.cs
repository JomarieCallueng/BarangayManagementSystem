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
    public class DisasterService : IDisasterService
    {
        private readonly IDisasterRepository _disasterRepo;

        public DisasterService(IDisasterRepository disasterRepo)
        {
            _disasterRepo = disasterRepo;
        }

        public async Task<DisasterDTO?> GetDisasterByIdAsync(int id)
        {
            var d = await _disasterRepo.GetByIdAsync(id);
            if (d == null) return null;

            return new DisasterDTO
            {
                Id = d.DisasterId,
                IncidentName = d.IncidentName,
                DisasterType = d.DisasterType,
                OccurrenceDate = d.OccurrenceDate,
                AffectedHouseholdsCount = d.AffectedHouseholdsCount,
                DisplacedIndividualsCount = d.DisplacedIndividualsCount,
                CasualtiesCount = d.CasualtiesCount,
                EvacuationCenterStatus = d.EvacuationCenterStatus,
                ReliefDistributionStatus = d.ReliefDistributionStatus,
                LoggedBy = d.LoggedBy
            };
        }

        public async Task<IEnumerable<DisasterDTO>> GetAllDisastersAsync()
        {
            var disasters = await _disasterRepo.GetAllAsync();
            return disasters.Select(d => new DisasterDTO
            {
                Id = d.DisasterId,
                IncidentName = d.IncidentName,
                DisasterType = d.DisasterType,
                OccurrenceDate = d.OccurrenceDate,
                AffectedHouseholdsCount = d.AffectedHouseholdsCount,
                DisplacedIndividualsCount = d.DisplacedIndividualsCount,
                CasualtiesCount = d.CasualtiesCount,
                EvacuationCenterStatus = d.EvacuationCenterStatus,
                ReliefDistributionStatus = d.ReliefDistributionStatus,
                LoggedBy = d.LoggedBy
            });
        }

        public async Task<IEnumerable<DisasterDTO>> GetActiveDisastersAsync()
        {
            var disasters = await _disasterRepo.GetActiveIncidentsAsync();
            return disasters.Select(d => new DisasterDTO
            {
                Id = d.DisasterId,
                IncidentName = d.IncidentName,
                DisasterType = d.DisasterType,
                OccurrenceDate = d.OccurrenceDate,
                AffectedHouseholdsCount = d.AffectedHouseholdsCount,
                DisplacedIndividualsCount = d.DisplacedIndividualsCount,
                CasualtiesCount = d.CasualtiesCount,
                EvacuationCenterStatus = d.EvacuationCenterStatus,
                ReliefDistributionStatus = d.ReliefDistributionStatus,
                LoggedBy = d.LoggedBy
            });
        }

        public async Task<bool> LogDisasterAsync(DisasterDTO dto)
        {
            var disaster = new Disaster
            {
                IncidentName = dto.IncidentName,
                DisasterType = dto.DisasterType,
                OccurrenceDate = dto.OccurrenceDate,
                AffectedHouseholdsCount = dto.AffectedHouseholdsCount,
                DisplacedIndividualsCount = dto.DisplacedIndividualsCount,
                CasualtiesCount = dto.CasualtiesCount,
                EvacuationCenterStatus = dto.EvacuationCenterStatus,
                ReliefDistributionStatus = dto.ReliefDistributionStatus,
                LoggedBy = dto.LoggedBy,
                DateCreated = DateTime.Now
            };

            await _disasterRepo.AddAsync(disaster);
            return await _disasterRepo.SaveChangesAsync();
        }

        public async Task<bool> UpdateDisasterImpactAsync(DisasterDTO dto)
        {
            var disaster = await _disasterRepo.GetByIdAsync(dto.Id);
            if (disaster == null) return false;

            disaster.IncidentName = dto.IncidentName;
            disaster.DisasterType = dto.DisasterType;
            disaster.AffectedHouseholdsCount = dto.AffectedHouseholdsCount;
            disaster.DisplacedIndividualsCount = dto.DisplacedIndividualsCount;
            disaster.CasualtiesCount = dto.CasualtiesCount;
            disaster.EvacuationCenterStatus = dto.EvacuationCenterStatus;
            disaster.ReliefDistributionStatus = dto.ReliefDistributionStatus;
            disaster.LoggedBy = dto.LoggedBy;
            disaster.DateUpdated = DateTime.Now;

            _disasterRepo.Update(disaster);
            return await _disasterRepo.SaveChangesAsync();
        }

        public async Task<bool> CloseEvacuationCenterAsync(int id, string loggedBy)
        {
            var disaster = await _disasterRepo.GetByIdAsync(id);
            if (disaster == null) return false;

            disaster.EvacuationCenterStatus = "Closed";
            disaster.ReliefDistributionStatus = "Completed";
            disaster.LoggedBy = loggedBy;
            disaster.DateUpdated = DateTime.Now;

            _disasterRepo.Update(disaster);
            return await _disasterRepo.SaveChangesAsync();
        }
    }
}