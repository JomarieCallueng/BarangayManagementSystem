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
    public class AnnouncementService : IAnnouncementService
    {
        private readonly IAnnouncementRepository _announceRepo;

        public AnnouncementService(IAnnouncementRepository announceRepo)
        {
            _announceRepo = announceRepo;
        }

        public async Task<AnnouncementDTO?> GetAnnouncementByIdAsync(int id)
        {
            var item = await _announceRepo.GetByIdAsync(id);
            if (item == null) return null;

            return new AnnouncementDTO
            {
                Id = item.AnnouncementId,
                Title = item.Title,
                Content = item.Content,
                Category = item.Category,
                ImageUrl = item.ImageUrl,
                IsPinned = item.IsPinned,
                PublishDate = item.PublishDate,
                ExpiryDate = item.ExpiryDate,
                AuthorName = item.AuthorName
            };
        }

        public async Task<IEnumerable<AnnouncementDTO>> GetAllAnnouncementsAsync()
        {
            var items = await _announceRepo.GetAllAsync();
            return items.Select(item => new AnnouncementDTO
            {
                Id = item.AnnouncementId,
                Title = item.Title,
                Content = item.Content,
                Category = item.Category,
                ImageUrl = item.ImageUrl,
                IsPinned = item.IsPinned,
                PublishDate = item.PublishDate,
                ExpiryDate = item.ExpiryDate,
                AuthorName = item.AuthorName
            });
        }

        public async Task<IEnumerable<AnnouncementDTO>> GetHeroAnnouncementsAsync()
        {
            var items = await _announceRepo.GetPinnedAnnouncementsAsync();
            return items.Select(item => new AnnouncementDTO
            {
                Id = item.AnnouncementId,
                Title = item.Title,
                Content = item.Content,
                Category = item.Category,
                ImageUrl = item.ImageUrl,
                IsPinned = item.IsPinned,
                PublishDate = item.PublishDate,
                ExpiryDate = item.ExpiryDate,
                AuthorName = item.AuthorName
            });
        }

        public async Task<bool> CreateAnnouncementAsync(AnnouncementDTO dto)
        {
            var item = new Announcement
            {
                Title = dto.Title,
                Content = dto.Content,
                Category = dto.Category,
                ImageUrl = dto.ImageUrl,
                IsPinned = dto.IsPinned,
                PublishDate = DateTime.Now, // Awtomatikong ngayon ilalathala
                ExpiryDate = dto.ExpiryDate,
                AuthorName = dto.AuthorName
            };

            await _announceRepo.AddAsync(item);
            return await _announceRepo.SaveChangesAsync();
        }

        public async Task<bool> UpdateAnnouncementAsync(AnnouncementDTO dto)
        {
            var item = await _announceRepo.GetByIdAsync(dto.Id);
            if (item == null) return false;

            item.Title = dto.Title;
            item.Content = dto.Content;
            item.Category = dto.Category;
            item.ImageUrl = dto.ImageUrl;
            item.IsPinned = dto.IsPinned;
            item.ExpiryDate = dto.ExpiryDate;
            item.AuthorName = dto.AuthorName;

            _announceRepo.Update(item);
            return await _announceRepo.SaveChangesAsync();
        }

        public async Task<bool> DeleteAnnouncementAsync(int id)
        {
            var item = await _announceRepo.GetByIdAsync(id);
            if (item == null) return false;

            _announceRepo.Delete(item);
            return await _announceRepo.SaveChangesAsync();
        }
    }
}
