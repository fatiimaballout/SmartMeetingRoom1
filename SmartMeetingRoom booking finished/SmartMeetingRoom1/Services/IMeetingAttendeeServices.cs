using Microsoft.EntityFrameworkCore;
using SmartMeetingRoom1.Dtos;
using SmartMeetingRoom1.Interfaces;
using SmartMeetingRoom1.Models;

namespace SmartMeetingRoom1.Services
{
    public class IMeetingAttendeeServices : IMeetingAttendee
    {
        private readonly AppDbContext _db;

        public IMeetingAttendeeServices(AppDbContext db)
        {
            _db = db;
        }

        private MeetingAttendeeDto MapToDto(MeetingAttendee attendee) => new()
        {
            Id = attendee.Id,
            MeetingId = attendee.MeetingId,
            UserId = attendee.UserId,
            Status = attendee.Status,
            CreatedAt = attendee.CreatedAt
        };

        public async Task<MeetingAttendeeDto?> GetByIdAsync(int id)
        {
            var attendee = await _db.MeetingAttendees
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);

            return attendee == null ? null : MapToDto(attendee);
        }

        public async Task<MeetingAttendeeDto> CreateAsync(CreateMeetingAttendeeDto dto)
        {
            if (!await _db.Meetings.AnyAsync(m => m.Id == dto.MeetingId))
                throw new ArgumentException($"Meeting ID {dto.MeetingId} does not exist.");
            if (!await _db.Users.AnyAsync(u => u.Id == dto.UserId))
                throw new ArgumentException($"User ID {dto.UserId} does not exist.");

            var attendee = new MeetingAttendee
            {
                MeetingId = dto.MeetingId,
                UserId = dto.UserId,
                Status = dto.Status ?? "invited",
                CreatedAt = DateTime.UtcNow
            };

            _db.MeetingAttendees.Add(attendee);
            await _db.SaveChangesAsync();

            return MapToDto(attendee);
        }

        public async Task<bool> UpdateAsync(int id, UpdateMeetingAttendeeDto dto)
        {
            var attendee = await _db.MeetingAttendees.FindAsync(id);
            if (attendee == null) return false;

            attendee.Status = dto.Status;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var attendee = await _db.MeetingAttendees.FindAsync(id);
            if (attendee == null) return false;

            _db.MeetingAttendees.Remove(attendee);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
