using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartMeetingRoom1.Dtos;
using SmartMeetingRoom1.Interfaces;
using System.Security.Claims;

namespace SmartMeetingRoom1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MinutesController : ControllerBase
    {
        private readonly IMinute _service;

        public MinutesController(IMinute service) => _service = service;

        // GET /api/minutes/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<MinuteDto>> Get(int id)
        {
            var minute = await _service.GetByIdAsync(id);
            return minute is null ? NotFound() : Ok(minute);
        }

        // GET /api/minutes/by-meeting/123
        [HttpGet("by-meeting/{meetingId:int}")]
        public async Task<ActionResult<MinuteDto>> GetByMeeting(int meetingId)
        {
            var minute = await _service.GetByMeetingAsync(meetingId);
            return minute is null ? NotFound() : Ok(minute);
        }

        // POST /api/minutes
        [HttpPost]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<MinuteDto>> Create([FromBody] CreateMinuteDto dto)
        {
            if (dto is null) return BadRequest("Body is required.");
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (dto.MeetingId <= 0) return BadRequest("MeetingId is required.");

            // If CreatorId not provided by client, take it from JWT
            if (dto.CreatorId <= 0)
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdStr, out var uid)) dto.CreatorId = uid;
            }

            // Avoid duplicate minutes for the same meeting
            var existing = await _service.GetByMeetingAsync(dto.MeetingId);
            if (existing != null)
                return CreatedAtAction(nameof(Get), new { id = existing.Id }, existing);

            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        // PUT /api/minutes/5
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateMinuteDto dto)
        {
            if (dto is null) return BadRequest("Body is required.");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var ok = await _service.UpdateAsync(id, dto);
            return ok ? NoContent() : NotFound();
        }

        // DELETE /api/minutes/5
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);
            return ok ? NoContent() : NotFound();
        }

        // POST /api/minutes/5/finalize (optional)
        [HttpPost("{id:int}/finalize")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> FinalizeMinutes(int id)
        {
            // If you add a service method, call it here (e.g., await _service.FinalizeAsync(id))
            return NoContent();
        }
    }
}
