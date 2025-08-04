using System;
using System.Collections.Generic;
using System.Net.Mail;

namespace SmartMeetingRoom1.Models
{
    public class User
    {
        public int Id { get; set; }            
        public string Name { get; set; }
        public string Email { get; set; }             
        public string Password { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }              
        public DateTime CreatedAt { get; set; }

        public ICollection<Meeting> OrganizedMeetings { get; set; }=new List<Meeting>();
        public ICollection<MeetingAttendee> MeetingAttendees { get; set; }
        public ICollection<Minute> MinutesCreated { get; set; }= new List<Minute>();
        public ICollection<ActionItem> AssignedTasks { get; set; }
        public ICollection<Attachment> UploadedFiles { get; set; }
        public ICollection<Notification> Notifications { get; set; }
        public ICollection<Minute> CreatedMinutes { get; set; }
        public ICollection<ActionItem> AssignedActionItems { get; set; }



    }
}
