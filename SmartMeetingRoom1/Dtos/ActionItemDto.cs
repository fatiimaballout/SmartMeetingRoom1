// Dtos/ActionItemDto.cs
public class ActionItemDto
{
    public int Id { get; set; }
    public int MinutesId { get; set; }
    public int? AssignedTo { get; set; }       // <- nullable
    public string? Description { get; set; }
    public string? Status { get; set; }
    public DateTime? DueDate { get; set; }     // <- nullable
}
