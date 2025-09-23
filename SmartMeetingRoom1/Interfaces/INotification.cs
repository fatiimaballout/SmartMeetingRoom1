using SmartMeetingRoom1.Dtos;
using SmartMeetingRoom1.Models;

namespace SmartMeetingRoom1.Interfaces
{
    public interface INotification
    {
        Task<NotificationDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<NotificationDto>> ListAsync(int userId, bool unreadOnly = false, int take = 20);
        Task<NotificationDto> CreateAsync(CreateNotificationDto dto);
        Task<bool> UpdateAsync(int id, UpdateNotificationDto dto);
        Task<bool> DeleteAsync(int id);
        Task<bool> MarkReadAsync(int id, int userId);

        // High-level helper: store + email (NO realtime push)
        Task NotifyAsync(int userId, NotificationType type, string title, string message,
                         string? link = null, int? meetingId = null, int? actionItemId = null);
    }
}
