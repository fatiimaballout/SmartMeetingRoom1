using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartMeetingRoom1.Dtos;
using SmartMeetingRoom1.Interfaces;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MeetingAttendeesController : ControllerBase
{
    private readonly IMeetingAttendee _service;
    private readonly ILogger<MeetingAttendeesController> _logger;
    public MeetingAttendeesController(IMeetingAttendee service, ILogger<MeetingAttendeesController> logger)
    {
        _logger = logger;
        _service = service;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MeetingAttendeeDto>> Get(int id)
    {
        _logger.LogWarning("Fetching attendee");
        var attendees = await _service.GetByMeetingIdAsync(id);
        if (attendees == null) return NotFound();
        return Ok(attendees);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<ActionResult<MeetingAttendeeDto>> Create([FromBody] CreateMeetingAttendeeDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMeetingAttendeeDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var ok = await _service.UpdateAsync(id, dto);
        if (!ok) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _service.DeleteAsync(id);
        if (!ok) return NotFound();
        return NoContent();
    }
}
