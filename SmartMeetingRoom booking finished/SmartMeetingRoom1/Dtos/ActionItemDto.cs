using SmartMeetingRoom1.Models;
namespace SmartMeetingRoom1.Dtos
{
    public class ActionItemDto
    {
        public int Id { get; set; }
        public int MinutesId { get; set; }
        public int AssignedTo { get; set; }
        public string Description { get; set; }
        public string Status { get; set; } 
        public DateTime DueDate { get; set; }
    }
}
