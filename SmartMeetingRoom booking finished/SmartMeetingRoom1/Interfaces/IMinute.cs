using SmartMeetingRoom1.Dtos;
using System.Threading.Tasks;

namespace SmartMeetingRoom1.Interfaces
{
    public interface IMinute
    {
        Task<MinuteDto?> GetByIdAsync(int id);
        Task<MinuteDto> CreateAsync(CreateMinuteDto dto);
        Task<bool> UpdateAsync(int id, UpdateMinuteDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
