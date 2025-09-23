using SmartMeetingRoom1.Models;
using System.ComponentModel.DataAnnotations;

namespace SmartMeetingRoom1.Dtos
{
    public class CreateNotificationDto
    {

        [Required] public int UserId { get; set; }
        [Required, MaxLength(64)] public string Type { get; set; } = string.Empty;
        [Required, MaxLength(200)] public string Title { get; set; } = string.Empty;
        [Required, MaxLength(4000)] public string Message { get; set; } = string.Empty;
        [MaxLength(1024)] public string? Link { get; set; }
        public int? MeetingId { get; set; }
        public int? ActionItemId { get; set; }
    }
}