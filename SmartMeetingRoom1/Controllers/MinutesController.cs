using Microsoft.AspNetCore.Mvc;
using SmartMeetingRoom1.Dtos;
using SmartMeetingRoom1.Interfaces;


    [ApiController]
    [Route("api/[controller]")]
    public class MinutesController : ControllerBase
    {
        private readonly IMinute _service;

        public MinutesController(IMinute service)
        {
            _service = service;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<MinuteDto>> Get(int id)
        {
            var minute = await _service.GetByIdAsync(id);
            if (minute == null) return NotFound();
            return Ok(minute);
        }

        [HttpPost]
        public async Task<ActionResult<MinuteDto>> Create([FromBody] CreateMinuteDto dto)
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
        public async Task<IActionResult> Update(int id, [FromBody] UpdateMinuteDto dto)
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

