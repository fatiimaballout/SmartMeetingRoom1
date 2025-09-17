// Models/User.cs  (domain user, NOT ApplicationUser)
using SmartMeetingRoom1.Models;

public class User
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;   // if you keep it
    public string Phone { get; set; } = string.Empty;
    public string Role { get; set; } = "Employee";
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public ICollection<ActionItem> AssignedActionItems { get; set; } = new List<ActionItem>();
    public ICollection<Attachment> UploadedFiles { get; set; } = new List<Attachment>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<Minute> CreatedMinutes { get; set; } = new List<Minute>();
}
