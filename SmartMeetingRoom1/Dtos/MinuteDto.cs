// MinuteDto.cs  ✅ final
public class MinuteDto
{
    public int Id { get; set; }
    public int MeetingId { get; set; }
    public int CreatorId { get; set; }

    // Make strings non-nullable and give safe defaults to kill the warnings
    public string Notes { get; set; } = string.Empty;
    public string Discussion { get; set; } = string.Empty;
    public string Decisions { get; set; } = string.Empty;

    public DateTime CreatedUtc { get; set; }   // single source of truth
}


