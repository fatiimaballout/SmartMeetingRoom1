namespace SmartMeetingRoom1.Dtos
{
    public class RoomUsagePointDto
    {
        public int RoomId { get; set; }
        public string Room { get; set; } = string.Empty;
        public double HoursBooked { get; set; }
    }
}
