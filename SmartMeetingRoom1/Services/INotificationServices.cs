using Microsoft.EntityFrameworkCore;
using SmartMeetingRoom1.Dtos;
using SmartMeetingRoom1.Interfaces;
using SmartMeetingRoom1.Models;

namespace SmartMeetingRoom1.Services
{
    public class INotificationServices : INotification
    {
        private readonly AppDbContext _db;

        public INotificationServices(AppDbContext db)
        {
            _db = db;
        }

        private NotificationDto MapToDto(Notification n) => new()
        {
            Id = n.Id,
            Type = n.Type,
            Title = n.Title,
            Message = n.Message,
            Link = n.Link,
            IsRead = n.IsRead,
            UserId = n.UserId
        };

        public async Task<NotificationDto?> GetByIdAsync(int id)
        {
            var notification = await _db.Notifications
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.Id == id);

            return notification == null ? null : MapToDto(notification);
        }

        public async Task<NotificationDto> CreateAsync(CreateNotificationDto dto)
        {
            if (!await _db.Users.AnyAsync(u => u.Id == dto.UserId))
                throw new ArgumentException($"User with ID {dto.UserId} does not exist.");

            var notification = new Notification
            {
                Type = dto.Type,
                Title = dto.Title,
                Message = dto.Message,
                Link = dto.Link,
                IsRead = false,
                UserId = dto.UserId
            };

            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync();

            return MapToDto(notification);
        }

        public async Task<bool> UpdateAsync(int id, UpdateNotificationDto dto)
        {
            var notification = await _db.Notifications.FindAsync(id);
            if (notification == null) return false;

            notification.Type = dto.Type;
            notification.Title = dto.Title;
            notification.Message = dto.Message;
            notification.Link = dto.Link;
            notification.IsRead = dto.IsRead;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var notification = await _db.Notifications.FindAsync(id);
            if (notification == null) return false;

            _db.Notifications.Remove(notification);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
