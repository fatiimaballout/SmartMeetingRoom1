using SmartMeetingRoom1.Models;
using SmartMeetingRoom1.Dtos;

namespace SmartMeetingRoom1.Interfaces
{
    public interface IRoom
    {
     
            Task<RoomDto?> GetByIdAsync(int id);
            Task<RoomDto> CreateAsync(CreateRoomDto dto);
            Task<bool> UpdateAsync(int id, UpdateRoomDto dto);
            Task<bool> DeleteAsync(int id);
            Task<List<RoomDto>> GetAllAsync();

    }
}
