using Microsoft.AspNetCore.Identity;
namespace SmartMeetingRoom1.Models
{
    public class ApplicationUser : IdentityUser<int> {
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; } // e.g. "/uploads/avatars/u-2-638...png"
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginUtc { get; set; }
    }
    
}
