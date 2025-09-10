namespace SmartMeetingRoom1.Dtos
{
    public class RoomAvailabilityDto
    {
        public int RoomId { get; set; }
        public string RoomName { get; set; } = "";
        public DateTime FromUtc { get; set; }
        public DateTime ToUtc { get; set; }
        public string Status { get; set; } = "Free";
    }
}
