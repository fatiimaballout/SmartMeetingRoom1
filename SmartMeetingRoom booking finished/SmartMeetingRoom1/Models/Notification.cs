using System;
namespace SmartMeetingRoom1.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Link { get; set; }
        public bool IsRead { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
