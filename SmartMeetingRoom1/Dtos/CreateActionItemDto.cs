namespace SmartMeetingRoom1.Dtos
{
    public class CreateActionItemDto
    {
        public int MinutesId { get; set; }
        public string? AssignedTo { get; set; }       // <- nullable
        public string? Description { get; set; }
        public string? Status { get; set; }
        public DateTime? DueDate { get; set; }     // <- nullable
    }
}
