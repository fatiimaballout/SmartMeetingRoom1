using Microsoft.AspNetCore.Mvc;
using SmartMeetingRoom1.Dtos;
using SmartMeetingRoom1.Interfaces;


    [ApiController]
    [Route("api/[controller]")]
    public class ActionItemsController : ControllerBase
    {
        private readonly IActionItem _service;

        public ActionItemsController(IActionItem service)
        {
            _service = service;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ActionItemDto>> Get(int id)
        {
            var actionItem = await _service.GetByIdAsync(id);
            if (actionItem == null) return NotFound();
            return Ok(actionItem);
        }

        [HttpPost]
        public async Task<ActionResult<ActionItemDto>> Create([FromBody] CreateActionItemDto dto)
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
        public async Task<IActionResult> Update(int id, [FromBody] UpdateActionItemDto dto)
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

