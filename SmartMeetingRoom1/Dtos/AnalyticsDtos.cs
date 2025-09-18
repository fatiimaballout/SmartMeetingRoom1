public sealed class RoomUsagePointDto
{
    public string Room { get; set; } = "";
    public DateTime Date { get; set; }          // UTC day bucket
    public int Meetings { get; set; }
    public int Minutes { get; set; }            // total occupied minutes that day
    public int RoomId { get; internal set; }
    public double HoursBooked { get; internal set; }
}
