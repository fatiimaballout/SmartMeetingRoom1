using SmartMeetingRoom1.Models;
namespace SmartMeetingRoom1.Dtos
{
    public class RoomDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Capacity { get; set; }
        public string Location { get; set; }
        public string Features { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
