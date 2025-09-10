namespace SmartMeetingRoom1.Dtos
{
    public class CreateAttachmentDto
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public int UploadedBy { get; set; }
        public int? MeetingId { get; set; }
        public int? MinuteId { get; set; }
    }
}
