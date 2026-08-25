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
    public class ComplaintService : IComplaintService
    {
        private readonly IComplaintRepository _complaintRepo;

        public ComplaintService(IComplaintRepository complaintRepo)
        {
            _complaintRepo = complaintRepo;
        }

        public async Task<ComplaintDTO?> GetComplaintByIdAsync(int id)
        {
            var complaint = await _complaintRepo.GetByIdAsync(id);
            if (complaint == null) return null;

            return new ComplaintDTO
            {
                Id = complaint.ComplaintId, // Mapping mula ComplaintId patungong Id ng DTO
                CaseNumber = complaint.CaseNumber,
                ComplainantResidentId = complaint.ResidentId,
                ComplainantName = complaint.ComplainantName,
                ComplainantContact = complaint.ComplainantContact,
                RespondentName = complaint.RespondentName,
                IncidentLocation = complaint.IncidentLocation,
                IncidentDate = complaint.IncidentDate,
                Details = complaint.Details,
                Status = complaint.Status,
                Remarks = complaint.Remarks,
                ActionTaken = complaint.ActionTaken,
                CreatedDate = complaint.DateSubmitted
            };
        }

        public async Task<IEnumerable<ComplaintDTO>> GetAllComplaintsAsync()
        {
            var complaints = await _complaintRepo.GetAllAsync();
            return complaints.Select(c => new ComplaintDTO
            {
                Id = c.ComplaintId,
                CaseNumber = c.CaseNumber,
                ComplainantName = c.ComplainantName,
                RespondentName = c.RespondentName,
                Status = c.Status,
                IncidentDate = c.IncidentDate
            });
        }

        public async Task<IEnumerable<ComplaintDTO>> GetActiveComplaintsAsync()
        {
            var complaints = await _complaintRepo.GetAllAsync();
            return complaints
                .Where(c => c.Status != "Resolved" && c.Status != "Dismissed")
                .Select(c => new ComplaintDTO
                {
                    Id = c.ComplaintId,
                    CaseNumber = c.CaseNumber,
                    ComplainantName = c.ComplainantName,
                    RespondentName = c.RespondentName,
                    Status = c.Status
                });
        }

        public async Task<bool> FileComplaintAsync(ComplaintDTO dto)
        {
            var complaint = new Complaint
            {
                CaseNumber = dto.CaseNumber,
                ResidentId = dto.ComplainantResidentId ?? 0, // Siguraduhing may valid ID
                ComplainantName = dto.ComplainantName,
                ComplainantContact = dto.ComplainantContact,
                RespondentName = dto.RespondentName,
                IncidentLocation = dto.IncidentLocation,
                IncidentDate = dto.IncidentDate,
                Details = dto.Details,
                Status = "Pending",
                DateSubmitted = DateTime.Now
            };

            await _complaintRepo.AddAsync(complaint);
            return await _complaintRepo.SaveChangesAsync();
        }

        public async Task<bool> UpdateComplaintStatusAsync(int id, string status, string remarks)
        {
            var complaint = await _complaintRepo.GetByIdAsync(id);
            if (complaint == null) return false;

            complaint.Status = status;
            complaint.Remarks = remarks;

            _complaintRepo.Update(complaint);
            return await _complaintRepo.SaveChangesAsync();
        }
    }
}