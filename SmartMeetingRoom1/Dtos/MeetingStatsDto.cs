namespace SmartMeetingRoom1.Dtos
{
    public class MeetingStatsDto
    {
        public int BookedToday { get; set; }
        public int ActiveNow { get; set; }
        public double UtilizationRate { get; set; }   // 0..100
        public double Delta { get; set; }             // today vs. yesterday (percentage points)
        public int TotalRooms { get; set; }
        public int NewRoomsThisMonth { get; set; }
    }
}
