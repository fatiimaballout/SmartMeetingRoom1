using SmartMeetingRoom1.Dtos;
using System.Threading.Tasks;

namespace SmartMeetingRoom1.Interfaces
{
    public interface IUser
    {
        Task<UserDto?> GetByIdAsync(int id);
        Task<UserDto> CreateAsync(CreateUserDto dto);
        Task<bool> UpdateAsync(int id, UpdateUserDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
