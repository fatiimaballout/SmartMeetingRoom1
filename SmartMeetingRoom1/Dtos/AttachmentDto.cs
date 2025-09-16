using SmartMeetingRoom1.Models;
namespace SmartMeetingRoom1.Dtos
{
    public class AttachmentDto
    {
        public int Id { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? UploadedBy { get; set; }
        public int? MeetingId { get; set; }
        public int? MinuteId { get; set; }
        public int? UploaderId { get; set; }
        public long? SizeBytes { get; set; }

    }
}
