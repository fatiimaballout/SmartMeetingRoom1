namespace SmartMeetingRoom1.Dtos
{
    public class CreateMeetingAttendeeDto
    {
        public int MeetingId { get; set; }
        public int UserId { get; set; }
        public string? Status { get; set; }  
    }
}
