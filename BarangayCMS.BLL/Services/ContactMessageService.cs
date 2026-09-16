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
    public class ContactMessageService : IContactMessageService
    {
        private readonly IContactMessageRepository _repo;

        public ContactMessageService(IContactMessageRepository repo)
        {
            _repo = repo;
        }

        public async Task<bool> SubmitAsync(string contactNumber, string message)
        {
            if (string.IsNullOrWhiteSpace(contactNumber) || string.IsNullOrWhiteSpace(message))
                return false;

            var entity = new ContactMessage
            {
                ContactNumber = contactNumber.Trim(),
                Message = message.Trim(),
                Status = "Unread",
                CreatedAt = DateTime.Now
            };

            await _repo.AddAsync(entity);
            return await _repo.SaveChangesAsync();
        }

        public async Task<IEnumerable<ContactMessageDTO>> GetAllAsync()
        {
            var items = await _repo.GetAllAsync();
            return items.Select(MapToDto);
        }

        public async Task<ContactMessageDTO?> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            return entity == null ? null : MapToDto(entity);
        }

        public async Task<int> GetUnreadCountAsync()
        {
            return await _repo.CountUnreadAsync();
        }

        public async Task<bool> MarkAsReadAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return false;

            // Huwag i-downgrade ang "Replied" pabalik sa "Read".
            if (entity.Status == "Unread")
            {
                entity.Status = "Read";
                _repo.Update(entity);
                return await _repo.SaveChangesAsync();
            }
            return true;
        }

        public async Task<bool> ReplyAsync(int id, string reply)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null || string.IsNullOrWhiteSpace(reply)) return false;

            entity.AdminReply = reply.Trim();
            entity.RepliedAt = DateTime.Now;
            entity.Status = "Replied";
            _repo.Update(entity);
            return await _repo.SaveChangesAsync();
        }

        public async Task<bool> ArchiveAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return false;

            entity.Status = "Archived";
            _repo.Update(entity);
            return await _repo.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return false;

            _repo.Remove(entity);
            return await _repo.SaveChangesAsync();
        }

        private static ContactMessageDTO MapToDto(ContactMessage m) => new ContactMessageDTO
        {
            Id = m.Id,
            ContactNumber = m.ContactNumber,
            Message = m.Message,
            Status = m.Status,
            CreatedAt = m.CreatedAt,
            AdminReply = m.AdminReply,
            RepliedAt = m.RepliedAt
        };
    }
}
