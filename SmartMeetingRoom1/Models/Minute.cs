// Minute.cs  ✅
using SmartMeetingRoom1.Models;

public class Minute
{
    public int Id { get; set; }
    public int MeetingId { get; set; }
    public Meeting Meeting { get; set; } = null!;

    public int CreatorId { get; set; }
    public ApplicationUser Creator { get; set; } = null!;

    public string Notes { get; set; } = string.Empty;
    public string Discussion { get; set; } = string.Empty;
    public string Decisions { get; set; } = string.Empty;

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;  // <— keep this
    // … any navigation collections you have
    public ICollection<ActionItem> ActionItems { get; set; } = new List<ActionItem>();
}
