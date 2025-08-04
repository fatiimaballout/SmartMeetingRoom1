using System;

namespace SmartMeetingRoom1.Models
{
    public class ActionItem
    {
        public int Id { get; set; }

        public int MinutesId { get; set; }
        public Minute Minute { get; set; }

        public int AssignedTo { get; set; }
        public User Assignee { get; set; }

        public string Description { get; set; }
        public string Status { get; set; } 
        public DateTime DueDate { get; set; }
    }
}
