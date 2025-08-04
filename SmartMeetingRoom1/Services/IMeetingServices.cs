using Microsoft.EntityFrameworkCore;
using SmartMeetingRoom1.Dtos;
using SmartMeetingRoom1.Interfaces;
using SmartMeetingRoom1.Models;


namespace SmartMeetingRoom1.Services
{
    public class IMeetingServices: IMeeting
    {
        private readonly AppDbContext _db;

        public IMeetingServices(AppDbContext db)
        {
            _db = db;
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
                .Include(m => m.Organizer)
                .Include(m => m.Room)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (m == null) return null;
            return MapToDto(m);
        }

        public async Task<MeetingDto> CreateAsync(CreateMeetingDto dto)
        {
            if (!await _db.Rooms.AnyAsync(r => r.Id == dto.RoomId))
                throw new ArgumentException($"Room {dto.RoomId} does not exist.");
            if (!await _db.Users.AnyAsync(u => u.Id == dto.OrganizerId))
                throw new ArgumentException($"Organizer {dto.OrganizerId} does not exist.");

            var meeting = new Meeting
            {
                RoomId = dto.RoomId,
                OrganizerId = dto.OrganizerId,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Status = dto.Status,
                Title = dto.Title,
                Agenda = dto.Agenda
            };

            _db.Meetings.Add(meeting);
            await _db.SaveChangesAsync();

            return MapToDto(meeting);
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
            meeting.StartTime = dto.StartTime;
            meeting.EndTime = dto.EndTime;
            meeting.Status = dto.Status;
            meeting.Title = dto.Title;
            meeting.Agenda = dto.Agenda;

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
