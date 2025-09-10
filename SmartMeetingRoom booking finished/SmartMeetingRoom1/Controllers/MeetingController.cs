using Microsoft.AspNetCore.Authorization;
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

    public MeetingsController(IMeeting service, AppDbContext db)
    {
        _service = service;
        _db = db;
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


}
