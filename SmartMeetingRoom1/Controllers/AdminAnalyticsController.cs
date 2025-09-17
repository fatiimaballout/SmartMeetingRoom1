using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartMeetingRoom1.Dtos;

[ApiController]
[Route("api/admin/analytics")]
[Authorize(Roles = "Admin")] // or remove if you don't use roles yet
public class AdminAnalyticsController : ControllerBase
{
    private readonly AppDbContext _db;
    public AdminAnalyticsController(AppDbContext db) => _db = db;

    // GET /api/admin/analytics/room-usage?fromUtc=...&toUtc=...
    [HttpGet("room-usage")]
    public async Task<IActionResult> GetRoomUsage([FromQuery] DateTime? fromUtc, [FromQuery] DateTime? toUtc)
    {
        var to = toUtc ?? DateTime.UtcNow;
        var from = fromUtc ?? to.AddDays(-30);

        var rows = await _db.Meetings
            .AsNoTracking()
            .Where(m => m.StartTime >= from && m.EndTime <= to && m.Status != "Cancelled")
            .GroupBy(m => new { m.RoomId, Name = m.Room.Name })   // uses navigation Room
            .Select(g => new RoomUsagePointDto
            {
                RoomId = g.Key.RoomId,
                Room = g.Key.Name,
                HoursBooked = g.Sum(m => EF.Functions.DateDiffMinute(m.StartTime, m.EndTime)) / 60.0
            })
            .OrderByDescending(x => x.HoursBooked)
            .ToListAsync();

        return Ok(new { fromUtc = from, toUtc = to, points = rows });
    }
}
