using SmartMeetingRoom1.Dtos;
using System.Threading.Tasks;

namespace SmartMeetingRoom1.Interfaces
{
    public interface IActionItem
    {
        Task<ActionItemDto?> GetByIdAsync(int id);
        Task<ActionItemDto> CreateAsync(CreateActionItemDto dto);
        Task<bool> UpdateAsync(int id, UpdateActionItemDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
