namespace SmartMeetingRoom1.Dtos
{
    public class MeetingListItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; } = "";
        public string Room { get; set; } = "";
        public string Organizer { get; set; } = "";
        public int AttendeeCount { get; set; }
        public string Agenda { get; set; } = "";
    }
}
