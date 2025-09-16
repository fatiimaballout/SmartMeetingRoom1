using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartMeetingRoom1.Models;

namespace SmartMeetingRoom1.Controllers
{
    /// <summary>
    /// Provides lightweight stats for the admin dashboard cards.
    /// Route matches /api/meetings/stats as expected by admin.html.
    /// </summary>
    [ApiController]
    [Route("api/meetings")]
    [Authorize] // keep protected (your admin.html sends JWT)
    public class MeetingsStatsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public MeetingsStatsController(AppDbContext db) => _db = db;

        public record StatsDto(
            int bookedToday,
            int activeNow,
            int utilizationRate,  // percent (0-100)
            int delta              // percent difference vs yesterday
        );

        [HttpGet("stats")]
        public async Task<ActionResult<StatsDto>> Get([FromQuery] string? range = "today")
        {
            // We compute in UTC to avoid TZ drift; your UI only shows numbers.
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);
            var now = DateTime.UtcNow;

            // basic queryables
            var meetings = _db.Meetings.AsNoTracking();
            var roomsCount = await _db.Rooms.AsNoTracking().CountAsync();
            if (roomsCount <= 0)
            {
                // No rooms yet -> return zeros (avoid divide by zero).
                return Ok(new StatsDto(0, 0, 0, 0));
            }

            // ---- Booked Today ----
            // count by StartTime falling on "today"
            var bookedToday = await meetings
                .Where(m => m.StartTime >= today && m.StartTime < tomorrow)
                .CountAsync();

            // ---- Active Now ----
            var activeNow = await meetings
                .Where(m => m.StartTime <= now && m.EndTime > now && m.Status != "Cancelled")
                .CountAsync();

            // ---- Utilization for today & yesterday ----
            static double OverlapMinutes(DateTime s, DateTime e, DateTime winStart, DateTime winEnd)
            {
                var start = s > winStart ? s : winStart;
                var end = e < winEnd ? e : winEnd;
                var mins = (end - start).TotalMinutes;
                return mins > 0 ? mins : 0;
            }

            // total booked minutes today across ALL meetings (clipped to today window)
            var todaysMeetings = await meetings
                .Where(m => m.EndTime > today && m.StartTime < tomorrow) // overlaps today
                .Select(m => new { m.StartTime, m.EndTime })
                .ToListAsync();

            var todayBookedMinutes = todaysMeetings
                .Sum(m => OverlapMinutes(m.StartTime, m.EndTime, today, tomorrow));

            // capacity minutes (rooms * minutes in the day)
            var minutesInDay = 24 * 60.0;
            var capacityToday = roomsCount * minutesInDay;
            var utilTodayPct = capacityToday > 0
                ? (int)Math.Round((todayBookedMinutes / capacityToday) * 100.0)
                : 0;

            // yesterday (for delta)
            var y0 = today.AddDays(-1);
            var y1 = today;

            var yMeetings = await meetings
                .Where(m => m.EndTime > y0 && m.StartTime < y1) // overlaps yesterday
                .Select(m => new { m.StartTime, m.EndTime })
                .ToListAsync();

            var yBookedMinutes = yMeetings
                .Sum(m => OverlapMinutes(m.StartTime, m.EndTime, y0, y1));

            var capacityYesterday = roomsCount * minutesInDay;
            var utilYesterdayPct = capacityYesterday > 0
                ? (int)Math.Round((yBookedMinutes / capacityYesterday) * 100.0)
                : 0;

            var delta = utilTodayPct - utilYesterdayPct;

            return Ok(new StatsDto(
                bookedToday: bookedToday,
                activeNow: activeNow,
                utilizationRate: Math.Clamp(utilTodayPct, 0, 100),
                delta: delta
            ));
        }
    }
}
