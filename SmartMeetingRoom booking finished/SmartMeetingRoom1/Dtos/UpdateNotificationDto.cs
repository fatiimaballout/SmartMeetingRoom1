namespace SmartMeetingRoom1.Dtos
{
    public class UpdateNotificationDto
    {
        public string Type { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Link { get; set; }
        public bool IsRead { get; set; }
    }
}
