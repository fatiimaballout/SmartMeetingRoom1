using SmartMeetingRoom1.Dtos;
using System.Threading.Tasks;

namespace SmartMeetingRoom1.Interfaces
{
    public interface INotification
    {
        Task<NotificationDto?> GetByIdAsync(int id);
        Task<NotificationDto> CreateAsync(CreateNotificationDto dto);
        Task<bool> UpdateAsync(int id, UpdateNotificationDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
