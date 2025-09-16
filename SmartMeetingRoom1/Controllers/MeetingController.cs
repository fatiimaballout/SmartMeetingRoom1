using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartMeetingRoom1.Dtos;
using SmartMeetingRoom1.Interfaces;
using SmartMeetingRoom1.Models;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MeetingsController : ControllerBase
{
    private readonly IMeeting _service;
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public MeetingsController(IMeeting service, AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _service = service;
        _db = db;
        _userManager = userManager;
    }

    // ----------------- CRUD -----------------
    [HttpGet("{id:int}")]
    public async Task<ActionResult<MeetingDto>> Get(int id)
    {
        var m = await _service.GetByIdAsync(id);
        if (m == null) return NotFound();
        return Ok(m);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<ActionResult<MeetingDto>> Create([FromBody] CreateMeetingDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Organizer comes from JWT
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
        dto.OrganizerId = int.Parse(userIdStr);

        try
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMeetingDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var ok = await _service.UpdateAsync(id, dto);
            if (!ok) return NotFound();
            return NoContent();
        }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _service.DeleteAsync(id);
        if (!ok) return NotFound();
        return NoContent();
    }

    // ----------------- Availability -----------------
    // GET /api/meetings/availability?fromUtc=...&toUtc=...&roomId=3
    [HttpGet("availability")]
    [AllowAnonymous] // remove if you want auth
    public async Task<ActionResult<IEnumerable<RoomAvailabilityDto>>> Availability(
        [FromQuery] DateTime fromUtc,
        [FromQuery] DateTime toUtc,
        [FromQuery] int? roomId = null)
    {
        if (toUtc <= fromUtc) return BadRequest("toUtc must be after fromUtc");

        var roomsQuery = _db.Rooms.AsNoTracking().AsQueryable();
        if (roomId.HasValue) roomsQuery = roomsQuery.Where(r => r.Id == roomId.Value);
        var rooms = await roomsQuery.ToListAsync();

        var overlappingRoomIds = await _db.Meetings
            .AsNoTracking()
            .Where(m => (!roomId.HasValue || m.RoomId == roomId.Value) &&
                        m.StartTime < toUtc &&
                        m.EndTime > fromUtc)
            .Select(m => m.RoomId)
            .ToListAsync();

        var busy = overlappingRoomIds.ToHashSet();

        var list = rooms.Select(r => new RoomAvailabilityDto
        {
            RoomId = r.Id,
            RoomName = r.Name,
            FromUtc = fromUtc,
            ToUtc = toUtc,
            Status = busy.Contains(r.Id) ? "Busy" : "Free"
        });

        return Ok(list);
    }

    // ----------------- Search (for "Find meeting" button) -----------------
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<MeetingListItemDto>>> Search(
        [FromQuery] DateTime? date = null,
        [FromQuery] string? q = null,
        [FromQuery] bool mine = true)
    {
        var day = (date ?? DateTime.UtcNow).Date;
        var fromUtc = day;
        var toUtc = day.AddDays(1);

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int.TryParse(userIdStr, out var userId);

        // current user's email (from Identity)
        string? currentEmail = null;
        if (userId > 0)
        {
            currentEmail = await _userManager.Users
                .Where(u => u.Id == userId)
                .Select(u => u.Email)
                .FirstOrDefaultAsync();
        }
        currentEmail ??= User.FindFirstValue(ClaimTypes.Email); // fallback

        var query = _db.Meetings
            .AsNoTracking()
            .Include(m => m.Room)
            .Include(m => m.Organizer)
            .Where(m => m.StartTime < toUtc && m.EndTime >= fromUtc);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var ql = q.Trim().ToLower();
            query = query.Where(m => m.Title.ToLower().Contains(ql));
        }

        if (mine && (userId > 0 || !string.IsNullOrEmpty(currentEmail)))
        {
            if (!string.IsNullOrEmpty(currentEmail))
            {
                query = query.Where(m =>
                    m.OrganizerId == userId ||
                    _db.MeetingAttendees.Any(a => a.MeetingId == m.Id && a.Email == currentEmail));
            }
            else
            {
                query = query.Where(m => m.OrganizerId == userId);
            }
        }

        var items = await query
            .OrderBy(m => m.StartTime)
            .Select(m => new MeetingListItemDto
            {
                Id = m.Id,
                Title = m.Title,
                StartTime = m.StartTime,
                EndTime = m.EndTime,
                Status = m.Status,
                Room = m.Room.Name,
                Organizer = m.Organizer.UserName ?? m.Organizer.Email ?? m.Organizer.Id.ToString(),
                AttendeeCount = _db.MeetingAttendees.Count(a => a.MeetingId == m.Id)
            })
            .ToListAsync();

        return Ok(items);
    }

    // ----------------- Start / End -----------------
    [HttpPost("{id:int}/start")]
    public async Task<IActionResult> Start(int id)
    {
        var m = await _db.Meetings.FindAsync(id);
        if (m == null) return NotFound();
        m.Status = "Started";
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/end")]
    public async Task<IActionResult> End(int id)
    {
        var m = await _db.Meetings.FindAsync(id);
        if (m == null) return NotFound();
        m.Status = "Ended";
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ----------------- Attendees (email-based) -----------------
    public record AttendeeEmailDto(string Email);
    public record AddAttendeeDto(string Email);
    public record RemoveAttendeeDto(string Email);

    // GET /api/meetings/{id}/attendees  -> [{ email }]
    [HttpGet("{id:int}/attendees")]
    public async Task<ActionResult<IEnumerable<AttendeeEmailDto>>> Attendees(int id)
    {
        var exists = await _db.Meetings.AsNoTracking().AnyAsync(m => m.Id == id);
        if (!exists) return NotFound();

        var rows = await _db.MeetingAttendees
            .AsNoTracking()
            .Where(a => a.MeetingId == id)
            .Select(a => new AttendeeEmailDto(a.Email))
            .ToListAsync();

        return Ok(rows);
    }

    // POST /api/meetings/{id}/attendees  { email }
    [HttpPost("{id:int}/attendees")]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<IActionResult> AddAttendee(int id, [FromBody] AddAttendeeDto dto)
    {
        var mtg = await _db.Meetings.FindAsync(id);
        if (mtg == null) return NotFound("Meeting not found.");

        var email = (dto.Email ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email)) return BadRequest("Email is required.");

        var exists = await _db.MeetingAttendees.AnyAsync(a => a.MeetingId == id && a.Email == email);
        if (exists) return NoContent(); // idempotent

        _db.MeetingAttendees.Add(new MeetingAttendee { MeetingId = id, Email = email });
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE /api/meetings/{id}/attendees   { email }
    [HttpDelete("{id:int}/attendees")]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<IActionResult> RemoveAttendee(int id, [FromBody] RemoveAttendeeDto dto)
    {
        var email = (dto.Email ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email)) return BadRequest("Email is required.");

        var row = await _db.MeetingAttendees.FirstOrDefaultAsync(a => a.MeetingId == id && a.Email == email);
        if (row == null) return NotFound();
        _db.MeetingAttendees.Remove(row);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ----------------- Agenda (simple text stored on Meeting) -----------------
    public record AgendaDto(string? Agenda);

    [HttpGet("{id:int}/agenda")]
    public async Task<ActionResult<AgendaDto>> GetAgenda(int id)
    {
        var m = await _db.Meetings.AsNoTracking()
            .Select(x => new { x.Id, x.Agenda })
            .FirstOrDefaultAsync(x => x.Id == id);
        if (m == null) return NotFound();
        return Ok(new AgendaDto(m.Agenda));
    }

    [HttpPut("{id:int}/agenda")]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<IActionResult> UpdateAgenda(int id, [FromBody] AgendaDto dto)
    {
        var m = await _db.Meetings.FindAsync(id);
        if (m == null) return NotFound();
        m.Agenda = dto.Agenda ?? string.Empty;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ----------------- Optional no-ops -----------------
    public record ToggleDto(bool Enabled);
    [HttpPost("{id:int}/transcription")]
    public IActionResult Transcription(int id, [FromBody] ToggleDto dto) => NoContent();

    public record InviteDto(List<string> Emails);
    [HttpPost("{id:int}/invite")]
    public IActionResult Invite(int id, [FromBody] InviteDto dto) => NoContent();

    // GET /api/meetings/upcoming?days=7&mine=true&take=10
    [HttpGet("upcoming")]
    public async Task<ActionResult<IEnumerable<object>>> Upcoming(
        [FromQuery] int days = 7,
        [FromQuery] bool mine = true,
        [FromQuery] int take = 10)
    {
        var now = DateTime.UtcNow;
        var toUtc = now.AddDays(days);

        // who is the current user?
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int.TryParse(userIdStr, out var userId);

        string? currentEmail = null;
        if (userId > 0)
        {
            currentEmail = await _userManager.Users
                .Where(u => u.Id == userId)
                .Select(u => u.Email)
                .FirstOrDefaultAsync();
        }
        currentEmail ??= User.FindFirstValue(ClaimTypes.Email);

        // base query: future meetings in window
        var query = _db.Meetings
            .AsNoTracking()
            .Include(m => m.Room)
            .Include(m => m.Organizer)
            .Where(m => m.StartTime >= now && m.StartTime < toUtc);

        // "mine" = organizer or attendee (by email)
        if (mine && (userId > 0 || !string.IsNullOrEmpty(currentEmail)))
        {
            if (!string.IsNullOrEmpty(currentEmail))
            {
                query = query.Where(m =>
                    m.OrganizerId == userId ||
                    _db.MeetingAttendees.Any(a => a.MeetingId == m.Id && a.Email == currentEmail));
            }
            else
            {
                query = query.Where(m => m.OrganizerId == userId);
            }
        }

        var items = await query
            .OrderBy(m => m.StartTime)
            .Take(Math.Clamp(take, 1, 50))
            .Select(m => new
            {
                id = m.Id,
                title = m.Title,
                startTime = m.StartTime,
                endTime = m.EndTime,
                status = m.Status,
                // Dashboard JS supports either room.name or plain roomName
                room = new { name = m.Room.Name },
                roomName = m.Room.Name,
                organizer = m.Organizer.UserName ?? m.Organizer.Email ?? m.Organizer.Id.ToString()
            })
            .ToListAsync();

        return Ok(items);
    }

}
