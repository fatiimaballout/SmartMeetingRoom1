namespace SmartMeetingRoom1.Dtos
{
    public class CreateMinuteDto
    {
        public int MeetingId { get; set; }
        public int CreatorId { get; set; }
        public string Discussion { get; set; }
        public string Decisions { get; set; }
    }
}
