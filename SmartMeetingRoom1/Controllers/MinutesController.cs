using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartMeetingRoom1.Dtos;
using SmartMeetingRoom1.Interfaces;
using SmartMeetingRoom1.Models;
using System.Security.Claims;
using System.IO;

namespace SmartMeetingRoom1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MinutesController : ControllerBase
    {
        private readonly IMinute _service;
        private readonly AppDbContext _db;

        public MinutesController(IMinute service, AppDbContext db)
        {
            _service = service;
            _db = db;
        }

        // ===== Minutes CRUD =====

        [HttpGet("{id:int}")]
        public async Task<ActionResult<MinuteDto>> Get(int id)
        {
            var minute = await _service.GetByIdAsync(id);
            return minute is null ? NotFound() : Ok(minute);
        }

        [HttpGet("by-meeting/{meetingId:int}")]
        public async Task<ActionResult<MinuteDto>> GetByMeeting(int meetingId)
        {
            var minute = await _service.GetByMeetingAsync(meetingId);
            return minute is null ? NotFound() : Ok(minute);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<MinuteDto>> Create([FromBody] CreateMinuteDto dto)
        {
            if (dto is null) return BadRequest("Body is required.");
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (dto.MeetingId <= 0) return BadRequest("MeetingId is required.");

            // Always take the creator from the JWT
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var uid)) return Unauthorized();
            dto.CreatorId = uid;

            var existing = await _service.GetByMeetingAsync(dto.MeetingId);
            if (existing != null)
                return CreatedAtAction(nameof(Get), new { id = existing.Id }, existing);

            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }


        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateMinuteDto dto)
        {
            if (dto is null) return BadRequest("Body is required.");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var ok = await _service.UpdateAsync(id, dto);
            return ok ? NoContent() : NotFound();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);
            return ok ? NoContent() : NotFound();
        }

        [HttpPost("{id:int}/finalize")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> FinalizeMinutes(int id)
        {
            return NoContent();
        }

        // ===== Attachments (bytes in DB, no ContentType) =====

        // GET /api/minutes/{id}/attachments
        [HttpGet("{id:int}/attachments")]
        public async Task<IActionResult> ListAttachments(int id)
        {
            var rows = await _db.Attachments
                .Where(a => a.MinuteId == id)
                .Select(a => new { a.Id, fileName = a.FileName, a.CreatedAt, a.SizeBytes })
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return Ok(rows);
        }


[HttpPost("{id:int}/attachments")]
    [Consumes("multipart/form-data")]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> UploadAttachments(int id, [FromForm] List<IFormFile> file)
    {
        var minutes = await _db.Minutes.FindAsync(id);
        if (minutes is null) return NotFound("Minutes not found.");
        if (file is null || file.Count == 0) return BadRequest("No files were sent.");

        // handle nameidentifier or sub (OIDC)
        var uidRaw = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        int? uploaderId = int.TryParse(uidRaw, out var uid) ? uid : (int?)null;

        var created = new List<object>();

        foreach (var f in file.Where(f => f?.Length > 0))
        {
            byte[] bytes;
            using (var ms = new MemoryStream()) { await f.CopyToAsync(ms); bytes = ms.ToArray(); }

            var att = new Attachment
            {
                MinuteId = id,
                MeetingId = minutes.MeetingId,
                FileName = Path.GetFileName(f.FileName),
                SizeBytes = f.Length,
                FileContent = bytes,                 // VARBINARY(MAX)
                CreatedAt = DateTime.UtcNow,
                UploaderId = uploaderId,            // NULL is OK after step 1
                UploadedBy = uploaderId,            // (or remove one column entirely)
                FilePath = null                   // we store in DB, not disk
            };

            _db.Attachments.Add(att);
            await _db.SaveChangesAsync();
            created.Add(new { att.Id, att.FileName, att.SizeBytes });
        }

        return Created(string.Empty, created);
    }




    // GET /api/attachments/{attId}  (download)
    [HttpGet("/api/attachments/{attId:int}")]
        public async Task<IActionResult> DownloadAttachment(int attId)
        {
            var a = await _db.Attachments.FindAsync(attId);
            if (a is null || a.FileContent == null) return NotFound();

            return File(a.FileContent, "application/octet-stream", a.FileName ?? $"file-{attId}");
        }

        // DELETE /api/attachments/{attId}
        [HttpDelete("/api/attachments/{attId:int}")]
        public async Task<IActionResult> DeleteAttachment(int attId)
        {
            var a = await _db.Attachments.FindAsync(attId);
            if (a is null) return NotFound();

            _db.Attachments.Remove(a);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // GET /api/minutes/{id}/action-items
        [HttpGet("{id:int}/action-items")]
        public async Task<IActionResult> ListItems(int id, [FromServices] AppDbContext db)
        {
            var rows = await db.ActionItems
                .Where(a => a.MinutesId == id)
                .OrderBy(a => a.Status).ThenBy(a => a.DueDate)
                .Select(a => new ActionItemDto
                {
                    Id = a.Id,
                    Description = a.Description,
                    AssignedTo = a.AssignedTo,
                    DueDate = a.DueDate,
                    Status = a.Status
                })
                .ToListAsync();

            return Ok(rows);
        }

        // POST /api/minutes/{id}/action-items
        [HttpPost("{id:int}/action-items")]
        public async Task<IActionResult> AddItem(int id, [FromBody] CreateActionItemDto dto, [FromServices] AppDbContext db)
        {
            if (string.IsNullOrWhiteSpace(dto.Description))
                return BadRequest("Description required.");

            var exists = await db.Minutes.AnyAsync(m => m.Id == id);
            if (!exists) return NotFound("Minutes not found.");

            var item = new ActionItem
            {
                MinutesId = id,
                Description = dto.Description.Trim(),
                AssignedTo = dto.AssignedTo,
                DueDate = dto.DueDate,
                Status = "Pending"
            };

            db.ActionItems.Add(item);
            await db.SaveChangesAsync();

            return Created(string.Empty, new { item.Id });
        }

        public record UpdateAgendaDto(string? Agenda);

        [HttpPatch("api/meetings/{id}/agenda")]
        public async Task<IActionResult> UpdateAgenda(int id, [FromBody] UpdateAgendaDto dto)
        {
            var m = await _db.Meetings.FindAsync(id);
            if (m is null) return NotFound();
            m.Agenda = dto.Agenda ?? "";
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // PATCH /api/action-items/{id}/status
        [HttpPatch("/api/action-items/{id:int}/status")]
        public async Task<IActionResult> SetStatus(int id, [FromBody] SetStatusDto dto, [FromServices] AppDbContext db)
        {
            var ai = await db.ActionItems.FindAsync(id);
            if (ai == null) return NotFound();

            ai.Status = dto.Status;
            await db.SaveChangesAsync();
            return NoContent();
        }

        // DELETE /api/action-items/{id}
        [HttpDelete("/api/action-items/{id:int}")]
        public async Task<IActionResult> DeleteItem(int id, [FromServices] AppDbContext db)
        {
            var ai = await db.ActionItems.FindAsync(id);
            if (ai == null) return NotFound();

            db.ActionItems.Remove(ai);
            await db.SaveChangesAsync();
            return NoContent();
        }

    }
}
