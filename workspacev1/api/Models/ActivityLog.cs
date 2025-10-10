namespace api.Models;

public class ActivityLog
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; }
    public string Username { get; set; }
    public string ActivityType { get; set; } // Created, Updated, StatusChanged, Deleted, Commented, etc.
    public string Description { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
