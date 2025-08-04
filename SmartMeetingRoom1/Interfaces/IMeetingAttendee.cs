using SmartMeetingRoom1.Dtos;
using System.Threading.Tasks;

namespace SmartMeetingRoom1.Interfaces
{
    public interface IMeetingAttendee
    {
        Task<MeetingAttendeeDto?> GetByIdAsync(int id);
        Task<MeetingAttendeeDto> CreateAsync(CreateMeetingAttendeeDto dto);
        Task<bool> UpdateAsync(int id, UpdateMeetingAttendeeDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
