using SmartMeetingRoom1.Dtos;

public interface IMinute
{
    Task<MinuteDto?> GetByIdAsync(int id);
    Task<MinuteDto?> GetByMeetingAsync(int meetingId);   // <-- you declared this
    Task<MinuteDto> CreateAsync(CreateMinuteDto dto);
    Task<bool> UpdateAsync(int id, UpdateMinuteDto dto);
    Task<bool> DeleteAsync(int id);
}
