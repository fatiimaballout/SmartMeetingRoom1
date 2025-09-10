using SmartMeetingRoom1.Models;
using SmartMeetingRoom1.Dtos;

namespace SmartMeetingRoom1.Interfaces
{
    public interface IMeeting
    {
        Task<MeetingDto?> GetByIdAsync(int id);
        Task<MeetingDto> CreateAsync(CreateMeetingDto dto);
        Task<bool> UpdateAsync(int id, UpdateMeetingDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
