using Microsoft.EntityFrameworkCore;
using SmartMeetingRoom1.Dtos;
using SmartMeetingRoom1.Interfaces;
using SmartMeetingRoom1.Models;

namespace SmartMeetingRoom1.Services
{
    public class IMinuteServices : IMinute
    {
        private readonly AppDbContext _db;

        public IMinuteServices(AppDbContext db)
        {
            _db = db;
        }

        private MinuteDto MapToDto(Minute m) => new()
        {
            Id = m.Id,
            MeetingId = m.MeetingId,
            CreatorId = m.CreatorId,
            Discussion = m.Discussion,
            Decisions = m.Decisions,
            CreatedAt = m.CreatedAt
        };

        public async Task<MinuteDto?> GetByIdAsync(int id)
        {
            var minute = await _db.Minutes
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            return minute == null ? null : MapToDto(minute);
        }

        public async Task<MinuteDto> CreateAsync(CreateMinuteDto dto)
        {
            if (!await _db.Meetings.AnyAsync(meeting => meeting.Id == dto.MeetingId))
                throw new ArgumentException($"Meeting ID {dto.MeetingId} does not exist.");

            if (!await _db.Users.AnyAsync(user => user.Id == dto.CreatorId))
                throw new ArgumentException($"Creator ID {dto.CreatorId} does not exist.");

            var minute = new Minute
            {
                MeetingId = dto.MeetingId,
                CreatorId = dto.CreatorId,
                Discussion = dto.Discussion,
                Decisions = dto.Decisions,
                CreatedAt = DateTime.UtcNow
            };

            _db.Minutes.Add(minute);
            await _db.SaveChangesAsync();

            return MapToDto(minute);
        }

        public async Task<bool> UpdateAsync(int id, UpdateMinuteDto dto)
        {
            var minute = await _db.Minutes.FindAsync(id);
            if (minute == null) return false;

            minute.Discussion = dto.Discussion;
            minute.Decisions = dto.Decisions;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var minute = await _db.Minutes.FindAsync(id);
            if (minute == null) return false;

            _db.Minutes.Remove(minute);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
