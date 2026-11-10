namespace BaseTemplate.Domain.Entities.Notifications;

public enum NotificationAudience { Admin = 0, Portal = 1 }

public class PushSubscription : BaseEntity
{
    public NotificationAudience Audience { get; private set; }
    // AppUser.Id for Admin; a second user table can use Portal
    public Guid      UserId     { get; private set; }
    public string    Endpoint   { get; private set; } = string.Empty;
    public string    P256dh     { get; private set; } = string.Empty;
    public string    Auth       { get; private set; } = string.Empty;
    public string?   UserAgent  { get; private set; }
    public DateTime? LastUsedAt { get; private set; }

    private PushSubscription() { }

    public PushSubscription(NotificationAudience audience, Guid userId,
                            string endpoint, string p256dh, string auth, string? userAgent)
    {
        Audience  = audience;
        UserId    = userId;
        Endpoint  = endpoint;
        P256dh    = p256dh;
        Auth      = auth;
        UserAgent = userAgent;
    }

    public void Refresh(NotificationAudience audience, Guid userId,
                        string p256dh, string auth, string? userAgent)
    {
        Audience  = audience;
        UserId    = userId;
        P256dh    = p256dh;
        Auth      = auth;
        UserAgent = userAgent;
        IsDeleted = false;
        SetUpdatedAt();
    }

    public void MarkUsed()   { LastUsedAt = DateTime.UtcNow; }
    public void SoftDelete() => SetDeleted();
}
