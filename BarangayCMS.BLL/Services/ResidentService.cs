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
    public class ResidentService : IResidentService
    {
        private readonly IResidentRepository _residentRepo;

        public ResidentService(IResidentRepository residentRepo)
        {
            _residentRepo = residentRepo;
        }

        public async Task<ResidentDTO?> GetResidentByIdAsync(int id)
        {
            var r = await _residentRepo.GetByIdAsync(id);
            if (r == null) return null;

            return new ResidentDTO
            {
                Id = r.ResidentId,
                FirstName = r.FirstName,
                MiddleName = r.MiddleName,
                LastName = r.LastName,
                Suffix = r.Suffix,
                BirthDate = r.BirthDate,
                Gender = r.Gender,
                CivilStatus = r.CivilStatus,
                ContactNumber = r.ContactNumber,
                Email = r.Email,
                HouseNumber = r.HouseNumber,
                Street = r.Street,
                SitioPurok = r.SitioPurok,
                IsVoter = r.IsVoter,
                IsResident = r.IsResident,
                CreatedAt = r.CreatedAt
            };
        }

        public async Task<IEnumerable<ResidentDTO>> GetAllResidentsAsync()
        {
            var residents = await _residentRepo.GetAllAsync();
            return residents.Select(r => new ResidentDTO
            {
                Id = r.ResidentId,
                FirstName = r.FirstName,
                MiddleName = r.MiddleName,
                LastName = r.LastName,
                Suffix = r.Suffix,
                BirthDate = r.BirthDate,
                Gender = r.Gender,
                CivilStatus = r.CivilStatus,
                ContactNumber = r.ContactNumber,
                Email = r.Email,
                HouseNumber = r.HouseNumber,
                Street = r.Street,
                SitioPurok = r.SitioPurok,
                IsVoter = r.IsVoter,
                IsResident = r.IsResident,
                CreatedAt = r.CreatedAt
            });
        }

        public async Task<IEnumerable<ResidentDTO>> GetRegisteredVotersAsync()
        {
            var voters = await _residentRepo.GetVotersAsync();
            return voters.Select(r => new ResidentDTO
            {
                Id = r.ResidentId,
                FirstName = r.FirstName,
                MiddleName = r.MiddleName,
                LastName = r.LastName,
                Suffix = r.Suffix,
                BirthDate = r.BirthDate,
                Gender = r.Gender,
                CivilStatus = r.CivilStatus,
                HouseNumber = r.HouseNumber,
                Street = r.Street,
                SitioPurok = r.SitioPurok,
                IsVoter = r.IsVoter,
                IsResident = r.IsResident
            });
        }

        public async Task<bool> RegisterResidentAsync(ResidentDTO dto)
        {
            var resident = new Resident
            {
                FirstName = dto.FirstName,
                MiddleName = dto.MiddleName,
                LastName = dto.LastName,
                Suffix = dto.Suffix,
                BirthDate = dto.BirthDate,
                Gender = dto.Gender,
                CivilStatus = dto.CivilStatus,
                ContactNumber = dto.ContactNumber,
                Email = dto.Email,
                HouseNumber = dto.HouseNumber,
                Street = dto.Street,
                SitioPurok = dto.SitioPurok,
                IsVoter = dto.IsVoter,
                IsResident = true, // Bagong rehistro ay automatic active resident
                CreatedAt = DateTime.Now
            };

            await _residentRepo.AddAsync(resident);
            return await _residentRepo.SaveChangesAsync();
        }

        public async Task<bool> UpdateResidentInfoAsync(ResidentDTO dto)
        {
            var resident = await _residentRepo.GetByIdAsync(dto.Id);
            if (resident == null) return false;

            resident.FirstName = dto.FirstName;
            resident.MiddleName = dto.MiddleName;
            resident.LastName = dto.LastName;
            resident.Suffix = dto.Suffix;
            resident.BirthDate = dto.BirthDate;
            resident.Gender = dto.Gender;
            resident.CivilStatus = dto.CivilStatus;
            resident.ContactNumber = dto.ContactNumber;
            resident.Email = dto.Email;
            resident.HouseNumber = dto.HouseNumber;
            resident.Street = dto.Street;
            resident.SitioPurok = dto.SitioPurok;
            resident.IsVoter = dto.IsVoter;
            resident.IsResident = dto.IsResident;

            _residentRepo.Update(resident);
            return await _residentRepo.SaveChangesAsync();
        }

        public async Task<bool> ChangeResidentStatusAsync(int id, bool isActive)
        {
            var resident = await _residentRepo.GetByIdAsync(id);
            if (resident == null) return false;

            resident.IsResident = isActive;

            _residentRepo.Update(resident);
            return await _residentRepo.SaveChangesAsync();
        }
    }
}