using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartMeetingRoom1.Dtos;
using SmartMeetingRoom1.Interfaces;
using SmartMeetingRoom1.Models;

namespace SmartMeetingRoom1.Services
{
    public class IMeetingServices : IMeeting
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        // ✅ inject UserManager here
        public IMeetingServices(AppDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        private MeetingDto MapToDto(Meeting m) => new()
        {
            Id = m.Id,
            RoomId = m.RoomId,
            OrganizerId = m.OrganizerId,
            StartTime = m.StartTime,
            EndTime = m.EndTime,
            Status = m.Status,
            Title = m.Title,
            Agenda = m.Agenda
        };

        public async Task<MeetingDto?> GetByIdAsync(int id)
        {
            var m = await _db.Meetings
                .AsNoTracking()
                .Include(x => x.Organizer)
                .Include(x => x.Room)
                .FirstOrDefaultAsync(x => x.Id == id);

            return m == null ? null : MapToDto(m);
        }

        public async Task<MeetingDto> CreateAsync(CreateMeetingDto dto)
        {
            // compute end time
            var end = dto.StartTime.AddMinutes(dto.DurationMinutes);

            var entity = new Meeting
            {
                Title = dto.Title,
                RoomId = dto.RoomId,
                StartTime = dto.StartTime,
                EndTime = end,
                OrganizerId = dto.OrganizerId,
                Status = "Scheduled",
                Agenda = dto.Agenda ?? ""   // <-- PERSIST IT HERE
            };

            _db.Meetings.Add(entity);
            await _db.SaveChangesAsync();

            // map back to DTO (RoomName optional)
            return new MeetingDto
            {
                Id = entity.Id,
                Title = entity.Title,
                StartTime = entity.StartTime,
                EndTime = entity.EndTime,
                Status = entity.Status,
                RoomId = entity.RoomId,
                Agenda = entity.Agenda
            };
        }

        private static MinuteDto Map(Minute m) => new()
        {
            Id = m.Id,
            MeetingId = m.MeetingId,
            CreatorId = m.CreatorId,
            Notes = m.Notes,
            Discussion = m.Discussion,
            Decisions = m.Decisions,
            CreatedUtc = m.CreatedUtc      // see #2 below
        };

        public async Task<MinuteDto?> GetByMeetingAsync(int meetingId)
        {
            var m = await _db.Minutes.AsNoTracking()
                                     .FirstOrDefaultAsync(x => x.MeetingId == meetingId);
            return m is null ? null : Map(m);
        }


        public async Task<bool> UpdateAsync(int id, UpdateMeetingDto dto)
        {
            var meeting = await _db.Meetings.FindAsync(id);
            if (meeting == null) return false;

            if (!await _db.Rooms.AnyAsync(r => r.Id == dto.RoomId))
                throw new ArgumentException($"Room {dto.RoomId} does not exist.");
            if (!await _db.Users.AnyAsync(u => u.Id == dto.OrganizerId))
                throw new ArgumentException($"Organizer {dto.OrganizerId} does not exist.");

            meeting.RoomId = dto.RoomId;
            meeting.OrganizerId = dto.OrganizerId;
            meeting.StartTime = dto.StartTime.ToUniversalTime();
            meeting.EndTime = dto.EndTime.ToUniversalTime();
            meeting.Status = string.IsNullOrWhiteSpace(dto.Status) ? meeting.Status : dto.Status;
            meeting.Title = dto.Title ?? meeting.Title;
            meeting.Agenda = dto.Agenda ?? meeting.Agenda;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _db.Meetings.FindAsync(id);
            if (existing == null) return false;
            _db.Meetings.Remove(existing);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
