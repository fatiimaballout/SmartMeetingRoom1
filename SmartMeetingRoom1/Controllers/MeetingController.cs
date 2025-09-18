using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartMeetingRoom1.Dtos;
using SmartMeetingRoom1.Interfaces;
using SmartMeetingRoom1.Models; // AppDbContext
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MeetingsController : ControllerBase
{
    private readonly IMeeting _service;
    private readonly IMeetingAttendee _attendeeService;
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public MeetingsController(
        IMeeting service,
        AppDbContext db,
        UserManager<ApplicationUser> userManager,
        IMeetingAttendee attendeeService)
    {
        _service = service;
        _db = db;
        _userManager = userManager;
        _attendeeService = attendeeService;
    }

    // ===== Basic CRUD =====

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

            // Add attendees by email (if provided)
            if (dto.Attendees != null)
            {
                foreach (var email in dto.Attendees)
                {
                    var attendeeDto = new CreateMeetingAttendeeDto
                    {
                        MeetingId = created.Id,
                        UserEmail = email,
                        Status = "invited"
                    };

                    try { await _attendeeService.CreateAsync(attendeeDto); }
                    catch (ArgumentException ex)
                    {
                        // Non-fatal: keep creating the meeting even if one attendee fails
                        Console.WriteLine($"Failed to add attendee {email}: {ex.Message}");
                    }
                }
            }

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
            return ok ? NoContent() : NotFound();
        }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _service.DeleteAsync(id);
        return ok ? NoContent() : NotFound();
    }

    // ===== Availability =====
    // GET /api/meetings/availability?fromUtc=...&toUtc=...&roomId=3
    [HttpGet("availability")]
    [AllowAnonymous] // keep as in your version (remove if you require auth)
    public async Task<ActionResult<IEnumerable<RoomAvailabilityDto>>> Availability(
        [FromQuery] DateTime fromUtc,
        [FromQuery] DateTime toUtc,
        [FromQuery] int? roomId = null)
    {
        if (toUtc <= fromUtc) return BadRequest("toUtc must be after fromUtc");

        var roomsQuery = _db.Rooms.AsNoTracking().AsQueryable();
        if (roomId.HasValue) roomsQuery = roomsQuery.Where(r => r.Id == roomId.Value);
        var rooms = await roomsQuery.ToListAsync();

        // overlap: start < to && end > from
        var overlappingRoomIds = await _db.Meetings
            .AsNoTracking()
            .Where(m =>
                (!roomId.HasValue || m.RoomId == roomId.Value) &&
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

    // ===== Search (includes Agenda in projection) =====
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

        if (mine && userId > 0)
        {
            query = query.Where(m =>
                m.OrganizerId == userId ||
                _db.MeetingAttendees.Any(a => a.MeetingId == m.Id && a.UserId == userId));
        }

        var items = await query
            .OrderBy(m => m.StartTime)
            .Select(m => new MeetingListItemDto
            {
                Id = m.Id,
                Title = m.Title,
                Agenda = m.Agenda,
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

    // ===== Agenda (get / patch) =====
    public record UpdateAgendaDto(string? Agenda);

    // GET /api/meetings/{id}/agenda
    [HttpGet("{id:int}/agenda")]
    public async Task<IActionResult> GetAgenda(int id)
    {
        var m = await _db.Meetings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (m is null) return NotFound();
        return Ok(new { agenda = m.Agenda ?? "" });
    }

    // PATCH /api/meetings/{id}/agenda
    [HttpPatch("{id:int}/agenda")]
    public async Task<IActionResult> UpdateAgenda(int id, [FromBody] UpdateAgendaDto dto)
    {
        var m = await _db.Meetings.FindAsync(id);
        if (m is null) return NotFound();

        m.Agenda = dto?.Agenda ?? "";
        await _db.SaveChangesAsync();
        return NoContent();
    }
    // GET: /api/meetings/upcoming?days=7&take=5
    [HttpGet("upcoming")]
    public async Task<ActionResult<IEnumerable<MeetingListItemDto>>> Upcoming(
        [FromQuery] int days = 7,
        [FromQuery] int take = 5)
    {
        // who am I?
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

        var now = DateTime.UtcNow;
        var to = now.AddDays(Math.Max(1, days));
        take = Math.Clamp(take, 1, 50);

        var items = await _db.Meetings
            .AsNoTracking()
            .Include(m => m.Room)
            .Include(m => m.Organizer)
            .Where(m =>
                m.StartTime >= now && m.StartTime <= to &&
                // organizer or invited attendee
                (m.OrganizerId == userId ||
                 _db.MeetingAttendees.Any(a => a.MeetingId == m.Id && a.UserId == userId)))
            .OrderBy(m => m.StartTime)
            .Take(take)
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

    // ===== Start / End =====
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

    // ===== Attendees (minimal list for UI) =====
    public record AttendeeDto(int UserId, string? Name, bool IsHost);

    [HttpGet("{id:int}/attendees")]
    public async Task<ActionResult<IEnumerable<AttendeeDto>>> Attendees(int id)
    {
        var meeting = await _db.Meetings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (meeting == null) return NotFound();

        var rows = await _db.MeetingAttendees
            .AsNoTracking()
            .Where(a => a.MeetingId == id)
            .Join(_db.Users,
                  attendee => attendee.UserId,
                  user => user.Id,
                  (attendee, user) => new AttendeeDto(
                      attendee.UserId,
                      user.Email, // show email as name (as in your code)
                      attendee.UserId == meeting.OrganizerId))
            .ToListAsync();

        return Ok(rows);
    }

    // ===== Optional no-ops kept from your version =====
    public record ToggleDto(bool Enabled);

    [HttpPost("{id:int}/transcription")]
    public IActionResult Transcription(int id, [FromBody] ToggleDto dto) => NoContent();

    public record InviteDto(List<string> Emails);

    [HttpPost("{id:int}/invite")]
    public IActionResult Invite(int id, [FromBody] InviteDto dto) => NoContent();
}
