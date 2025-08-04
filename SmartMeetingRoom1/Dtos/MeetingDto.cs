using SmartMeetingRoom1.Models;

namespace SmartMeetingRoom1.Dtos
{
    public class MeetingDto
    {
 

        public int Id { get; set; }
        public int RoomId { get; set; }
        public int OrganizerId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; } = "";
        public string Title { get; set; } = "";
        public string Agenda { get; set; } = "";
    
}
}
