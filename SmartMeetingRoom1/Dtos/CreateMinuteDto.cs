namespace SmartMeetingRoom1.Dtos
{
    public class CreateMinuteDto
    {
        public int MeetingId { get; set; }
        public int CreatorId { get; set; }      // or take this from the token server-side
        public string Notes { get; set; } = string.Empty;
        public string Discussion { get; set; } = string.Empty;
        public string Decisions { get; set; } = string.Empty;
    }
}
