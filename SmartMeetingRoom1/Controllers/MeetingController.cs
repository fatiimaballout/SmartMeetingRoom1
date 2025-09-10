using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartMeetingRoom1.Dtos;
using SmartMeetingRoom1.Interfaces;
using SmartMeetingRoom1.Models;    // AppDbContext
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MeetingsController : ControllerBase
{
    private readonly IMeeting _service;  // your existing service
    private readonly AppDbContext _db;           // <-- add EF Core context for availability
    private readonly UserManager<ApplicationUser> _userManager;

    public MeetingsController(IMeeting service, AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _service = service;
        _db = db;
        _userManager = userManager;
    }

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

    // take organizer from JWT, not from the client
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

    // ===== Availability =====
    // GET /api/meetings/availability?fromUtc=2025-09-10T08:00:00Z&toUtc=2025-09-10T10:00:00Z&roomId=3
    [HttpGet("availability")]
    [AllowAnonymous] // optional; remove if you want auth
    public async Task<ActionResult<IEnumerable<RoomAvailabilityDto>>> Availability(
        [FromQuery] DateTime fromUtc,
        [FromQuery] DateTime toUtc,
        [FromQuery] int? roomId = null)
    {
        if (toUtc <= fromUtc) return BadRequest("toUtc must be after fromUtc");

        // rooms to consider
        var roomsQuery = _db.Rooms.AsNoTracking().AsQueryable();
        if (roomId.HasValue) roomsQuery = roomsQuery.Where(r => r.Id == roomId.Value);
        var rooms = await roomsQuery.ToListAsync();

        // find meetings that overlap the window
        // Overlap when: start < to && end > from
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

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<MeetingListItemDto>>> Search(
    [FromQuery] DateTime? date = null,
    [FromQuery] string? q = null,
    [FromQuery] bool mine = true)
    {
        // Default to "today" if no date
        var day = (date ?? DateTime.UtcNow).Date;
        var fromUtc = day;
        var toUtc = day.AddDays(1);

        // current user id (int) from JWT
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int.TryParse(userIdStr, out var userId);

        var query = _db.Meetings
            .AsNoTracking()
            .Include(m => m.Room)
            .Include(m => m.Organizer)
            .Where(m => m.StartTime < toUtc && m.EndTime >= fromUtc); // in this day

        if (!string.IsNullOrWhiteSpace(q))
        {
            var ql = q.Trim().ToLower();
            query = query.Where(m => m.Title.ToLower().Contains(ql));
        }

        if (mine && userId > 0)
        {
            // organizer or attendee
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

    // --- Start / End ---
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

    // --- Attendees (minimal) ---
    public record AttendeeDto(int UserId, string? Name, bool IsHost);

    [HttpGet("{id:int}/attendees")]
    public async Task<ActionResult<IEnumerable<AttendeeDto>>> Attendees(int id)
    {
        var meeting = await _db.Meetings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (meeting == null) return NotFound();

        var rows = await _db.MeetingAttendees
            .AsNoTracking()
            .Where(a => a.MeetingId == id)
            .Select(a => new AttendeeDto(
                a.UserId,
                null,                       // fill with a name later if you want
                a.UserId == meeting.OrganizerId))
            .ToListAsync();

        return Ok(rows);
    }

    // --- Optional no-ops so buttons don't error ---
    public record ToggleDto(bool Enabled);
    [HttpPost("{id:int}/transcription")]
    public IActionResult Transcription(int id, [FromBody] ToggleDto dto) => NoContent();

    public record InviteDto(List<string> Emails);
    [HttpPost("{id:int}/invite")]
    public IActionResult Invite(int id, [FromBody] InviteDto dto) => NoContent();

}
