using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartMeetingRoom1.Models; // AppDbContext

[ApiController]
[Route("api/admin/analytics")]
[Authorize(Roles = "Admin")]
public class AdminAnalyticsController : ControllerBase
{
    private readonly AppDbContext _db;
    public AdminAnalyticsController(AppDbContext db) => _db = db;

    // GET /api/admin/analytics/room-usage?fromUtc=2025-08-20T00:00:00Z&toUtc=2025-09-18T00:00:00Z
    [HttpGet("room-usage")]
    public async Task<ActionResult<IEnumerable<RoomUsagePointDto>>> GetRoomUsage(
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc)
    {
        // defaults: last 30 days (UTC)
        var to = (toUtc ?? DateTime.UtcNow).Date.AddDays(1);     // exclusive
        var from = (fromUtc ?? to.AddDays(-30)).Date;

        // Guard
        if (to <= from) return BadRequest("toUtc must be after fromUtc.");

        // Build query: meetings that overlap the window
        var q = _db.Meetings.AsNoTracking()
            .Include(m => m.Room)
            .Where(m => m.StartTime < to && m.EndTime >= from);

        // Group by Room + Day; compute minutes safely with DateDiffMinute
        // EF translates DateDiffMinute to DATEDIFF(MINUTE, ...) on SQL Server (int)
        // and we cast the SUM to int to avoid overflow (range is limited by filter).
        var data = await q
            .GroupBy(m => new
            {
                RoomId = m.RoomId,
                RoomName = m.Room.Name,
                Day = EF.Functions.DateDiffDay(from, m.StartTime) // bucket index
            })
            .Select(g => new RoomUsagePointDto
            {
                Room = g.Key.RoomName,
                Date = from.AddDays(g.Key.Day), // re-materialize day
                Meetings = g.Count(),
                Minutes = g.Sum(m => (int?)EF.Functions.DateDiffMinute(m.StartTime, m.EndTime)) ?? 0
            })
            .OrderBy(x => x.Date)
            .ThenBy(x => x.Room)
            .ToListAsync();

        return Ok(data);
    }
}
