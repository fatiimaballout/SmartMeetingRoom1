using System;

namespace SmartMeetingRoom1.Models
{
    public class Attachment
    {
        public int Id { get; set; }

        public string FilePath { get; set; }       
        public string FileName { get; set; }       
        public DateTime CreatedAt { get; set; }

        public int UploadedBy { get; set; }
        public User Uploader { get; set; }

        public int? MeetingId { get; set; }       
        public Meeting Meeting { get; set; }

        public int? MinuteId { get; set; }         
        public Minute Minute { get; set; }
    }
}
