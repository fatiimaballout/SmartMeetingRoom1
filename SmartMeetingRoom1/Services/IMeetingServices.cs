using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartMeetingRoom1.Dtos;
using SmartMeetingRoom1.Interfaces;
using SmartMeetingRoom1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartMeetingRoom1.Services
{
    public class IMeetingServices : IMeeting
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public IMeetingServices(AppDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // ------- mapping -------
        private static MeetingDto MapToDto(Meeting m) => new()
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

        private static string NormalizeEmail(string e) => (e ?? string.Empty).Trim().ToLowerInvariant();

        // ------- queries -------
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
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            // Validate organizer (ASP.NET Identity user)
            var organizer = await _userManager.FindByIdAsync(dto.OrganizerId.ToString());
            if (organizer == null)
                throw new ArgumentException($"Organizer {dto.OrganizerId} does not exist.");

            // Validate room (domain Room)
            var room = await _db.Rooms.FindAsync(dto.RoomId);
            if (room == null)
                throw new ArgumentException("Room not found.");

            // Times -> UTC (compute EndTime if missing)
            var startUtc = dto.StartTime.Kind == DateTimeKind.Utc ? dto.StartTime : dto.StartTime.ToUniversalTime();

            DateTime endUtc;
            if (dto.EndTime != default && dto.EndTime > dto.StartTime)
            {
                endUtc = dto.EndTime.Kind == DateTimeKind.Utc ? dto.EndTime : dto.EndTime.ToUniversalTime();
            }
            else
            {
                var minutes = (dto.DurationMinutes.HasValue && dto.DurationMinutes.Value > 0) ? dto.DurationMinutes.Value : 60;
                endUtc = startUtc.AddMinutes(minutes);
            }

            // Prevent double booking
            var overlap = await _db.Meetings
                .AsNoTracking()
                .AnyAsync(m => m.RoomId == dto.RoomId &&
                               m.StartTime < endUtc &&
                               m.EndTime > startUtc);
            if (overlap)
                throw new ArgumentException("This room is already booked for the selected time window.");

            // Create meeting
            var meeting = new Meeting
            {
                Title = dto.Title ?? string.Empty,
                Agenda = dto.Agenda ?? string.Empty,
                Status = string.IsNullOrWhiteSpace(dto.Status) ? "Scheduled" : dto.Status,
                RoomId = dto.RoomId,
                OrganizerId = organizer.Id,
                StartTime = startUtc,
                EndTime = endUtc
            };

            await using var tx = await _db.Database.BeginTransactionAsync();

            _db.Meetings.Add(meeting);
            await _db.SaveChangesAsync(); // meeting.Id set

            // --- Save attendees as plain emails ---
            // Prefer dto.Attendees; fall back to dto.AttendeeEmails (for old clients)
            IEnumerable<string>? rawEmails = dto.Attendees;
            if (rawEmails == null)
            {
                var p = dto.GetType().GetProperty("AttendeeEmails");
                if (p != null) rawEmails = p.GetValue(dto) as IEnumerable<string>;
            }

            var emails = (rawEmails ?? Enumerable.Empty<string>())
                .Select(NormalizeEmail)
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Distinct()
                .ToList();

            if (emails.Count > 0)
            {
                var existing = await _db.MeetingAttendees
                    .AsNoTracking()
                    .Where(a => a.MeetingId == meeting.Id)
                    .Select(a => a.Email)
                    .ToListAsync();

                var toAdd = emails
                    .Where(e => !existing.Contains(e))
                    .Select(e => new MeetingAttendee
                    {
                        MeetingId = meeting.Id,
                        Email = e
                    })
                    .ToList();

                if (toAdd.Count > 0)
                {
                    _db.MeetingAttendees.AddRange(toAdd);
                    await _db.SaveChangesAsync();
                }
            }

            await tx.CommitAsync();

            return MapToDto(meeting);
        }

        public async Task<bool> UpdateAsync(int id, UpdateMeetingDto dto)
        {
            var meeting = await _db.Meetings.FindAsync(id);
            if (meeting == null) return false;

            if (!await _db.Rooms.AnyAsync(r => r.Id == dto.RoomId))
                throw new ArgumentException($"Room {dto.RoomId} does not exist.");

            // Organizer is an Identity user
            var orgExists = await _userManager.Users.AnyAsync(u => u.Id == dto.OrganizerId);
            if (!orgExists)
                throw new ArgumentException($"Organizer {dto.OrganizerId} does not exist.");

            var startUtc = dto.StartTime.Kind == DateTimeKind.Utc ? dto.StartTime : dto.StartTime.ToUniversalTime();
            var endUtc = dto.EndTime.Kind == DateTimeKind.Utc ? dto.EndTime : dto.EndTime.ToUniversalTime();

            // Only re-check overlap if time or room changed
            if (meeting.RoomId != dto.RoomId || meeting.StartTime != startUtc || meeting.EndTime != endUtc)
            {
                var overlap = await _db.Meetings
                    .AsNoTracking()
                    .AnyAsync(m => m.Id != id &&
                                   m.RoomId == dto.RoomId &&
                                   m.StartTime < endUtc &&
                                   m.EndTime > startUtc);
                if (overlap)
                    throw new ArgumentException("This room is already booked for the selected time window.");
            }

            meeting.RoomId = dto.RoomId;
            meeting.OrganizerId = dto.OrganizerId;
            meeting.StartTime = startUtc;
            meeting.EndTime = endUtc;
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

        // (Optional) if your interface includes it
        private static MinuteDto Map(Minute m) => new()
        {
            Id = m.Id,
            MeetingId = m.MeetingId,
            CreatorId = m.CreatorId,
            Notes = m.Notes,
            Discussion = m.Discussion,
            Decisions = m.Decisions,
            CreatedUtc = m.CreatedUtc
        };

        public async Task<MinuteDto?> GetByMeetingAsync(int meetingId)
        {
            var m = await _db.Minutes.AsNoTracking().FirstOrDefaultAsync(x => x.MeetingId == meetingId);
            return m is null ? null : Map(m);
        }
    }
}
