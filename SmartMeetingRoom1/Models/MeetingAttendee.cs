using SmartMeetingRoom1.Models;
using System.ComponentModel.DataAnnotations;

public class MeetingAttendee
{
    public int Id { get; set; }                      // NEW (surrogate PK)

    public int MeetingId { get; set; }
    public Meeting Meeting { get; set; }

    [Required, MaxLength(256)]
    public string Email { get; set; } = string.Empty;   // <-- store email directly

    public int UserId { get; set; }                  // domain user id
    public User User { get; set; } = null!;          // domain user nav

    public string Status { get; set; } = "Invited";  // NEW (default)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // NEW
}

