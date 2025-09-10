using SmartMeetingRoom1.Models;
namespace SmartMeetingRoom1.Dtos
{
    public class MeetingAttendeeDto
    {
        public int Id { get; set; }
        public int MeetingId { get; set; }
        public int UserId { get; set; }
        public string Status { get; set; } 
        public DateTime CreatedAt { get; set; }
    }
}
