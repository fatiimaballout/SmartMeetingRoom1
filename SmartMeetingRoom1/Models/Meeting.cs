using SmartMeetingRoom1.Models;

public class Meeting
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Agenda { get; set; }

    public int RoomId { get; set; }
    public Room Room { get; set; } = null!;

    public int OrganizerId { get; set; }                 // Identity user id
    public ApplicationUser Organizer { get; set; } = null!;

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = "Scheduled";

    public ICollection<MeetingAttendee> Attendees { get; set; } = new List<MeetingAttendee>();
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
}
