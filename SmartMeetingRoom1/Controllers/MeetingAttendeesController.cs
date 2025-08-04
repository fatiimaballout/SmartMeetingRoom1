using Microsoft.AspNetCore.Mvc;
using SmartMeetingRoom1.Dtos;
using SmartMeetingRoom1.Interfaces;


    [ApiController]
    [Route("api/[controller]")]
    public class MeetingAttendeesController : ControllerBase
    {
        private readonly IMeetingAttendee _service;

        public MeetingAttendeesController(IMeetingAttendee service)
        {
            _service = service;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<MeetingAttendeeDto>> Get(int id)
        {
            var attendee = await _service.GetByIdAsync(id);
            if (attendee == null) return NotFound();
            return Ok(attendee);
        }

        [HttpPost]
        public async Task<ActionResult<MeetingAttendeeDto>> Create([FromBody] CreateMeetingAttendeeDto dto)
        {
            try
            {
                var created = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateMeetingAttendeeDto dto)
        {
            var ok = await _service.UpdateAsync(id, dto);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }

