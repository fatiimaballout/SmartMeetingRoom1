using Microsoft.EntityFrameworkCore;
using SmartMeetingRoom1.Dtos;
using SmartMeetingRoom1.Interfaces;
using SmartMeetingRoom1.Models;

namespace SmartMeetingRoom1.Services
{
    public class IAttachmentServices : IAttachment
    {
        private readonly AppDbContext _db;

        public IAttachmentServices(AppDbContext db)
        {
            _db = db;
        }

        private AttachmentDto MapToDto(Attachment a) => new()
        {
            Id = a.Id,
            FilePath = a.FilePath,
            FileName = a.FileName,
            CreatedAt = a.CreatedAt,
            UploadedBy = a.UploadedBy,
            MeetingId = a.MeetingId,
            MinuteId = a.MinuteId
        };

        public async Task<AttachmentDto?> GetByIdAsync(int id)
        {
            var attachment = await _db.Attachments
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);

            return attachment == null ? null : MapToDto(attachment);
        }

        public async Task<AttachmentDto> CreateAsync(CreateAttachmentDto dto)
        {

            if (!await _db.Users.AnyAsync(u => u.Id == dto.UploadedBy))
                throw new ArgumentException($"User ID {dto.UploadedBy} does not exist.");

            if (dto.MeetingId.HasValue && !await _db.Meetings.AnyAsync(m => m.Id == dto.MeetingId.Value))
                throw new ArgumentException($"Meeting ID {dto.MeetingId.Value} does not exist.");

            if (dto.MinuteId.HasValue && !await _db.Minutes.AnyAsync(m => m.Id == dto.MinuteId.Value))
                throw new ArgumentException($"Minute ID {dto.MinuteId.Value} does not exist.");

            var attachment = new Attachment
            {
                FilePath = dto.FilePath,
                FileName = dto.FileName,
                CreatedAt = DateTime.UtcNow,
                UploadedBy = dto.UploadedBy,
                MeetingId = dto.MeetingId,
                MinuteId = dto.MinuteId
            };

            _db.Attachments.Add(attachment);
            await _db.SaveChangesAsync();

            return MapToDto(attachment);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var attachment = await _db.Attachments.FindAsync(id);
            if (attachment == null) return false;

            _db.Attachments.Remove(attachment);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
