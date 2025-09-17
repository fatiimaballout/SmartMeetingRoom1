using Microsoft.EntityFrameworkCore;
using SmartMeetingRoom1.Dtos;
using SmartMeetingRoom1.Interfaces;
using SmartMeetingRoom1.Models;

namespace SmartMeetingRoom1.Services
{
    public class IMeetingAttendeeServices : IMeetingAttendee
    {
        private readonly AppDbContext _db;
        private readonly ILogger<IMeetingAttendeeServices> _logger;

        public IMeetingAttendeeServices(AppDbContext db, ILogger<IMeetingAttendeeServices> logger)
        {
            _db = db;
            _logger = logger;
            _logger.LogDebug("MeetingAttendeeService initialized");
        }

        private MeetingAttendeeDto MapToDto(MeetingAttendee attendee) => new()
        {
            Id = attendee.Id,
            MeetingId = attendee.MeetingId,
            UserId = attendee.UserId,
            Status = attendee.Status,
            CreatedAt = attendee.CreatedAt
        };

        public async Task<List<MeetingAttendeeDto>> GetByMeetingIdAsync(int meetingId)
        {
            var attendees = await _db.MeetingAttendees
                .AsNoTracking()
                .Where(a => a.MeetingId == meetingId)
                .ToListAsync();

            return attendees.Select(MapToDto).ToList();
        }

        public async Task<MeetingAttendeeDto> CreateAsync(CreateMeetingAttendeeDto dto)
        {
            _logger.LogDebug($"Creating attendee for Meeting ID {dto.MeetingId} and User Email {dto.UserEmail}");
            if (!await _db.Meetings.AnyAsync(m => m.Id == dto.MeetingId))
                throw new ArgumentException($"Meeting ID {dto.MeetingId} does not exist.");

            if (!await _db.Users.AnyAsync(u => u.Email!.Equals(dto.UserEmail)))
                throw new ArgumentException($"User {dto.UserEmail} does not exist.");

            var userId = await _db.Users
                .Where(u => u.Email!.Equals(dto.UserEmail))
                .Select(u => u.Id)
                .FirstOrDefaultAsync();

            _logger.LogDebug($"Found User ID {userId} for Email {dto.UserEmail}");

            var attendee = new MeetingAttendee
            {
                MeetingId = dto.MeetingId,
                UserId = userId,
                Status = dto.Status ?? "invited",
                CreatedAt = DateTime.UtcNow
            };

            _logger.LogDebug($"Adding attendee: MeetingId={attendee.MeetingId}, UserId={attendee.UserId}, Status={attendee.Status}");
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
