using SmartMeetingRoom1.Dtos;
using System.Threading.Tasks;

namespace SmartMeetingRoom1.Interfaces
{
    public interface IAttachment
    {
        Task<AttachmentDto?> GetByIdAsync(int id);
        Task<AttachmentDto> CreateAsync(CreateAttachmentDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
