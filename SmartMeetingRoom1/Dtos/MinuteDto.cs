using SmartMeetingRoom1.Models;
namespace SmartMeetingRoom1.Dtos
{
    public class MinuteDto
    {
        public int Id { get; set; }
        public int MeetingId { get; set; }
        public int CreatorId { get; set; }
        public string Discussion { get; set; }
        public string Decisions { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
