using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartMeetingRoom1.Dtos;
using SmartMeetingRoom1.Interfaces;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotification _service;
    public NotificationsController(INotification service) => _service = service;

    [HttpGet("{id:int}")]
    public async Task<ActionResult<NotificationDto>> Get(int id)
    {
        var notification = await _service.GetByIdAsync(id);
        if (notification == null) return NotFound();
        return Ok(notification);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<ActionResult<NotificationDto>> Create([FromBody] CreateNotificationDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateNotificationDto dto)
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
}
