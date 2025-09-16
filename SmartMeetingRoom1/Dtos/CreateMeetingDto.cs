using System.Text.Json.Serialization;

namespace SmartMeetingRoom1.Dtos
{
    public class CreateMeetingDto
    {
        public int RoomId { get; set; }
        public int OrganizerId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; } = "";
        public string Title { get; set; } = "";
        public string Agenda { get; set; } = "";
        
        // Duration sent from booking page; optional. If 0/empty we’ll default to 60.
        [JsonPropertyName("durationMinutes")]
        public int? DurationMinutes { get; set; }

        public List<string>? Attendees { get; set; }
    }
}
