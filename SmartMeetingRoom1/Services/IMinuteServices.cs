using Microsoft.EntityFrameworkCore;
using SmartMeetingRoom1.Dtos;
using SmartMeetingRoom1.Interfaces;
using SmartMeetingRoom1.Models;

namespace SmartMeetingRoom1.Services
{
    public class IMinuteServices : IMinute
    {
        private readonly AppDbContext _db;
        public IMinuteServices(AppDbContext db) => _db = db;

        // Map entity -> DTO (UI expects lastSavedUtc + notes)
        private static MinuteDto MapToDto(Minute m) => new()
        {
            Id = m.Id,
            MeetingId = m.MeetingId,
            CreatorId = m.CreatorId,
            Notes = m.Notes,           // <-- added
            Discussion = m.Discussion,
            Decisions = m.Decisions,
            LastSavedUtc = m.CreatedUtc       // UI reads minutes.lastSavedUtc
        };

        public async Task<MinuteDto?> GetByIdAsync(int id)
        {
            var minute = await _db.Minutes.AsNoTracking()
                               .FirstOrDefaultAsync(m => m.Id == id);
            return minute is null ? null : MapToDto(minute);
        }

        public async Task<MinuteDto?> GetByMeetingAsync(int meetingId)
        {
            var minute = await _db.Minutes.AsNoTracking()
                               .FirstOrDefaultAsync(m => m.MeetingId == meetingId);
            return minute is null ? null : MapToDto(minute);
        }

        public async Task<MinuteDto> CreateAsync(CreateMinuteDto dto)
        {
            // Ensure meeting exists
            var meetingExists = await _db.Meetings
                                         .AnyAsync(meeting => meeting.Id == dto.MeetingId);
            if (!meetingExists)
                throw new ArgumentException($"Meeting ID {dto.MeetingId} does not exist.");

            // Optional: CreatorId may be null/omitted
            if (dto.CreatorId.HasValue && dto.CreatorId.Value > 0)
            {
                var userExists = await _db.Users.AnyAsync(u => u.Id == dto.CreatorId.Value);
                if (!userExists)
                    throw new ArgumentException($"Creator ID {dto.CreatorId} does not exist.");
            }
            else
            {
                dto.CreatorId = null; // tolerate missing creator
            }

            // One minutes row per meeting: if it exists, just return it
            var existing = await _db.Minutes.FirstOrDefaultAsync(x => x.MeetingId == dto.MeetingId);
            if (existing is not null)
                return MapToDto(existing);

            var minute = new Minute
            {
                MeetingId = dto.MeetingId,
                CreatorId = dto.CreatorId ?? 0,
                Notes = dto.Notes ?? string.Empty,   // <-- added
                Discussion = dto.Discussion,
                Decisions = dto.Decisions,
                CreatedUtc = DateTime.UtcNow
            };

            _db.Minutes.Add(minute);
            await _db.SaveChangesAsync();

            return MapToDto(minute);
        }

        public async Task<bool> UpdateAsync(int id, UpdateMinuteDto dto)
        {
            var minute = await _db.Minutes.FirstOrDefaultAsync(m => m.Id == id);
            if (minute is null) return false;

            // PATCH semantics: only overwrite when provided (null means "leave as is")
            if (dto.Notes is not null) minute.Notes = dto.Notes;      // <-- added
            if (dto.Discussion is not null) minute.Discussion = dto.Discussion;
            if (dto.Decisions is not null) minute.Decisions = dto.Decisions;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var minute = await _db.Minutes.FindAsync(id);
            if (minute is null) return false;

            _db.Minutes.Remove(minute);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
