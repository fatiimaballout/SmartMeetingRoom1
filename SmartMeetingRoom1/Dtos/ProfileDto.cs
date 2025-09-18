namespace SmartMeetingRoom1.Dtos
{
    public class ProfileDto
    {
        public int Id { get; set; }
        public string? FullName { get; set; }
        public string? UserName { get; set; } // read-only on page
        public string? Email { get; set; }
        public string[] Roles { get; set; } = Array.Empty<string>();
        public string? AvatarUrl { get; set; }
        public DateTime? LastLoginUtc { get; set; }
        public DateTime? CreatedUtc { get; set; }
    }
}
