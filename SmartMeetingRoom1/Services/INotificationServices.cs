using System.Net;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartMeetingRoom1.Dtos;
using SmartMeetingRoom1.Interfaces;
using SmartMeetingRoom1.Models;

namespace SmartMeetingRoom1.Services
{
    /// <summary>
    /// Persists notifications and sends SMTP emails. No SignalR/hub usage.
    /// </summary>
    public sealed class INotificationService : INotification
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _cfg;
        private readonly ILogger<INotificationService> _log;

        public INotificationService(AppDbContext db, IConfiguration cfg, ILogger<INotificationService> log)
        {
            _db = db; _cfg = cfg; _log = log;
        }

        private static NotificationDto Map(Notification n) => new NotificationDto
        {
            Id = n.Id,
            UserId = n.UserId,
            Type = n.Type.ToString(),
            Title = n.Title,
            Message = n.Message,
            Link = n.Link,
            IsRead = n.IsRead,
            MeetingId = n.MeetingId,
            ActionItemId = n.ActionItemId,
            CreatedAt = n.CreatedAt
        };

        public async Task<NotificationDto?> GetByIdAsync(int id)
        {
            var n = await _db.Notifications.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return n is null ? null : Map(n);
        }

        public async Task<IReadOnlyList<NotificationDto>> ListAsync(int userId, bool unreadOnly = false, int take = 20)
        {
            var q = _db.Notifications.AsNoTracking().Where(n => n.UserId == userId);
            if (unreadOnly) q = q.Where(n => !n.IsRead);
            var rows = await q.OrderByDescending(n => n.CreatedAt).Take(Math.Clamp(take, 1, 100)).ToListAsync();
            return rows.Select(Map).ToList();
        }

        public async Task<NotificationDto> CreateAsync(CreateNotificationDto dto)
        {
            var userExists = await _db.Users.AnyAsync(u => u.Id == dto.UserId);
            if (!userExists)
                throw new ArgumentException($"User with ID {dto.UserId} does not exist.");

            // parse Type string -> enum safely
            if (!Enum.TryParse<NotificationType>(dto.Type, ignoreCase: true, out var typeEnum))
                throw new ArgumentException($"Unknown notification type '{dto.Type}'.");

            var n = new Notification
            {
                UserId = dto.UserId,
                Type = typeEnum,
                Title = dto.Title ?? string.Empty,
                Message = dto.Message ?? string.Empty,
                Link = dto.Link,
                MeetingId = dto.MeetingId,
                ActionItemId = dto.ActionItemId
            };

            _db.Notifications.Add(n);
            await _db.SaveChangesAsync();
            return Map(n);
        }

        public async Task<bool> UpdateAsync(int id, UpdateNotificationDto dto)
        {
            var n = await _db.Notifications.FindAsync(id);
            if (n is null) return false;

            if (!Enum.TryParse<NotificationType>(dto.Type, ignoreCase: true, out var typeEnum))
                throw new ArgumentException($"Unknown notification type '{dto.Type}'.");

            n.Type = typeEnum;
            n.Title = dto.Title ?? string.Empty;
            n.Message = dto.Message ?? string.Empty;
            n.Link = dto.Link;
            n.IsRead = dto.IsRead;
            n.MeetingId = dto.MeetingId;
            n.ActionItemId = dto.ActionItemId;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var n = await _db.Notifications.FindAsync(id);
            if (n is null) return false;
            _db.Notifications.Remove(n);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkReadAsync(int id, int userId)
        {
            var n = await _db.Notifications.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
            if (n is null) return false;
            if (!n.IsRead)
            {
                n.IsRead = true;
                await _db.SaveChangesAsync();
            }
            return true;
        }

        public async Task NotifyAsync(
            int userId, NotificationType type, string title, string message,
            string? link = null, int? meetingId = null, int? actionItemId = null)
        {
            // Save to DB
            var n = new Notification
            {
                UserId = userId,
                Type = type,
                Title = title ?? string.Empty,
                Message = message ?? string.Empty,
                Link = link,
                MeetingId = meetingId,
                ActionItemId = actionItemId
            };
            _db.Notifications.Add(n);
            await _db.SaveChangesAsync();

            // Email (best-effort; controlled via config flag)
            try { await SendEmailAsync(userId, title, message, link); }
            catch (Exception ex) { _log.LogWarning(ex, "Email send failed for user {UserId}", userId); }
        }

        // -------- SMTP helper (no hub) ----------
        private async Task SendEmailAsync(int userId, string subject, string message, string? link)
        {
            if (string.Equals(_cfg["Email:Enabled"], "false", StringComparison.OrdinalIgnoreCase))
                return;

            var to = await _db.Users.Where(u => u.Id == userId).Select(u => u.Email).FirstOrDefaultAsync();
            if (string.IsNullOrWhiteSpace(to)) return;

            using var client = new SmtpClient(_cfg["Smtp:Host"]!, int.Parse(_cfg["Smtp:Port"] ?? "587"))
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_cfg["Smtp:User"], _cfg["Smtp:Pass"])
            };

            var from = _cfg["Smtp:From"] ?? _cfg["Smtp:User"];
            var html = $"<p>{System.Net.WebUtility.HtmlEncode(message)}</p>" +
                       (link is not null ? $"<p><a href=\"{link}\">Open</a></p>" : string.Empty);

            using var mail = new MailMessage(from!, to!, subject, html) { IsBodyHtml = true };
            await client.SendMailAsync(mail);
        }
    }
}
