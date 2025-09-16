// Models/ActionItem.cs
public class ActionItem
{
    public int Id { get; set; }

    // FK to minutes
    public int MinutesId { get; set; }

    // REQUIRED
    public string Description { get; set; } = string.Empty;

    // “Pending”, “Done”, etc.
    public string Status { get; set; } = "Pending";

    // Email or user id as TEXT (matches what the UI sends)
    public string? AssignedTo { get; set; }

    public DateTime? DueDate { get; set; }

    public Minute? Minute { get; set; }
}
