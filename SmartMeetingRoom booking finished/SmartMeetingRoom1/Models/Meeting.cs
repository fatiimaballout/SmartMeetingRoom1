using System;
using System.Collections.Generic;
using System.Net.Mail;

namespace SmartMeetingRoom1.Models
{
    public class Meeting
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public int OrganizerId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public string Status { get; set; } = "Scheduled";
        public string Title { get; set; } = string.Empty;
        public string Agenda { get; set; } = string.Empty;

        public ApplicationUser Organizer { get; set; } = null!;
        public Room Room { get; set; } = null!;

       
        public ICollection<MeetingAttendee> Attendees { get; set; } = new List<MeetingAttendee>();
        
        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
        public ICollection<Minute> Minutes { get; set; } = new List<Minute>();


    }
}
