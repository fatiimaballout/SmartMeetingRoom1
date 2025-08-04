using System;

namespace SmartMeetingRoom1.Models
{
    public class MeetingAttendee
    {
        public int Id { get; set; }

        public int MeetingId { get; set; }
        public Meeting Meeting { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public string Status { get; set; } 
        public DateTime CreatedAt { get; set; }
    }
}
