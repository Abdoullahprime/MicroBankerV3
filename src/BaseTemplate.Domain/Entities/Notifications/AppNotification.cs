namespace BaseTemplate.Domain.Entities.Notifications;

public class AppNotification : BaseEntity
{
    public NotificationAudience Audience { get; private set; }
    public Guid      UserId   { get; private set; }
    public string    Title    { get; private set; } = string.Empty;
    public string    Body     { get; private set; } = string.Empty;
    public string?   Url      { get; private set; }
    public string    Category { get; private set; } = "General";
    public bool      IsRead   { get; private set; }
    public DateTime? ReadAt   { get; private set; }

    private AppNotification() { }

    public AppNotification(NotificationAudience audience, Guid userId,
                           string title, string body, string? url, string category)
    {
        Audience = audience;
        UserId   = userId;
        Title    = title;
        Body     = body;
        Url      = url;
        Category = string.IsNullOrWhiteSpace(category) ? "General" : category;
    }

    public void MarkRead()
    {
        if (IsRead) return;
        IsRead = true;
        ReadAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    public void SoftDelete() => SetDeleted();
}
