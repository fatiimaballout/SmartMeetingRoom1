using SmartMeetingRoom1.Models;
namespace SmartMeetingRoom1.Dtos
{
    public class MeetingAttendeeDto
    {
        public int Id { get; set; }
        public int MeetingId { get; set; }
        public Meeting Meeting { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public string Status { get; set; } = "Invited";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
