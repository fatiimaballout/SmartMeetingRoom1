using System;
using System.Collections.Generic;
using System.Net.Mail;

namespace SmartMeetingRoom1.Models
{
    public class Minute
    {
        public int Id { get; set; }

        public int MeetingId { get; set; }
        public Meeting Meeting { get; set; }

        public int CreatorId { get; set; }
        public User Creator { get; set; }

        public string Discussion { get; set; }
        public string Decisions { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<ActionItem> ActionItems { get; set; }
        public ICollection<Attachment> Attachments { get; set; }

    }
}
