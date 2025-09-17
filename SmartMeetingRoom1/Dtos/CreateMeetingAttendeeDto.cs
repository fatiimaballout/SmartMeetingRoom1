namespace SmartMeetingRoom1.Dtos
{
    public class CreateMeetingAttendeeDto
    {
        public int MeetingId { get; set; }
        public required string UserEmail { get; set; }
        public string? Status { get; set; }
    }
}
