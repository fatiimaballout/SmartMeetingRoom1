using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartMeetingRoom1.Dtos;
using SmartMeetingRoom1.Interfaces;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoomsController : ControllerBase
{
    private readonly IRoom _service;
    public RoomsController(IRoom service) => _service = service;

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RoomDto>> Get(int id)
    {
        var room = await _service.GetByIdAsync(id);
        if (room == null) return NotFound();
        return Ok(room);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<RoomDto>> Create([FromBody] CreateRoomDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRoomDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var ok = await _service.UpdateAsync(id, dto);
        if (!ok) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, [FromServices] AppDbContext db)
    {
        await using var tx = await db.Database.BeginTransactionAsync();

        // delete dependents of meetings in this room
        var meetingIds = await db.Meetings
            .Where(m => m.RoomId == id)
            .Select(m => m.Id)
            .ToListAsync();

        if (meetingIds.Count > 0)
        {
            await db.MeetingAttendees.Where(a => meetingIds.Contains(a.MeetingId)).ExecuteDeleteAsync();
            await db.Minutes.Where(x => meetingIds.Contains(x.MeetingId)).ExecuteDeleteAsync();


            await db.Meetings.Where(m => m.RoomId == id).ExecuteDeleteAsync();
        }

        var room = await db.Rooms.FindAsync(id);
        if (room is null) return NotFound();

        db.Rooms.Remove(room);
        await db.SaveChangesAsync();
        await tx.CommitAsync();

        return NoContent();
    }



    [HttpGet] // GET /api/rooms
    public async Task<ActionResult<IEnumerable<RoomDto>>> List()
    {
        var rooms = await _service.GetAllAsync(); // implement in IRoom service
        return Ok(rooms);
    }

}
