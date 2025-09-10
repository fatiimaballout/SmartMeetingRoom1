using SmartMeetingRoom1.Models;

public class MeetingAttendee
{
    public int Id { get; set; }                      // NEW (surrogate PK)

    public int MeetingId { get; set; }
    public Meeting Meeting { get; set; } = null!;

    public int UserId { get; set; }                  // domain user id
    public User User { get; set; } = null!;          // domain user nav

    public string Status { get; set; } = "Invited";  // NEW (default)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // NEW
}

