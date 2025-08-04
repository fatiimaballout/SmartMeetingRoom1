namespace SmartMeetingRoom1.Dtos
{
    public class CreateActionItemDto
    {
        public int MinutesId { get; set; }
        public int AssignedTo { get; set; }
        public string Description { get; set; }
        public string? Status { get; set; } 
        public DateTime DueDate { get; set; }
    }
}
