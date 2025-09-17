using SmartMeetingRoom1.Dtos;
using System.Threading.Tasks;

namespace SmartMeetingRoom1.Interfaces
{
    public interface IMeetingAttendee
    {
        Task<MeetingAttendeeDto> CreateAsync(CreateMeetingAttendeeDto dto);
        Task<List<MeetingAttendeeDto>> GetByMeetingIdAsync(int meetingId);
        Task<bool> UpdateAsync(int id, UpdateMeetingAttendeeDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
