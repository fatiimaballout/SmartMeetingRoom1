public class ActionItem
{
    public int Id { get; set; }
    public int MinutesId { get; set; }
    public Minute Minute { get; set; } = null!;

    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";

    public int? AssignedTo { get; set; }            // domain user id
    public User? Assignee { get; set; }             // optional
    public DateTime? DueDate { get; set; }
}
