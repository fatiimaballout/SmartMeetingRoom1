public sealed class CalendarEventDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public int RoomId { get; set; }
    public string Room { get; set; } = "";
    public DateTime Start { get; set; }   // UTC
    public DateTime End { get; set; }     // UTC
    public string Status { get; set; } = "";
}
