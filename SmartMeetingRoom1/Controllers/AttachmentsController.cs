using Microsoft.AspNetCore.Mvc;
using SmartMeetingRoom1.Dtos;
using SmartMeetingRoom1.Interfaces;


    [ApiController]
    [Route("api/[controller]")]
    public class AttachmentsController : ControllerBase
    {
        private readonly IAttachment _service;

        public AttachmentsController(IAttachment service)
        {
            _service = service;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<AttachmentDto>> Get(int id)
        {
            var attachment = await _service.GetByIdAsync(id);
            if (attachment == null) return NotFound();
            return Ok(attachment);
        }

        [HttpPost]
        public async Task<ActionResult<AttachmentDto>> Create([FromBody] CreateAttachmentDto dto)
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

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }

