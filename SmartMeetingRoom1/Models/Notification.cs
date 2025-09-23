namespace SmartMeetingRoom1.Models
{
    public enum NotificationType
    {
        MeetingInvitation = 1,
        BookingConfirmation = 2,
        ActionItemAssigned = 3,
        MeetingUpdated = 4,
        MeetingCancelled = 5
    }

    public class Notification
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        public NotificationType Type { get; set; }
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public string? Link { get; set; }

        public int? MeetingId { get; set; }
        public int? ActionItemId { get; set; }

        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
