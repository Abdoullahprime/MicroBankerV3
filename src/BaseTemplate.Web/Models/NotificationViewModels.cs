// src/BaseTemplate.Web/Models/NotificationViewModels.cs
namespace BaseTemplate.Web.Models;

public class NotificationListViewModel
{
    public int Page       { get; set; }
    public int TotalPages { get; set; }
    public List<NotificationRowModel> Items { get; set; } = new();
}

public class NotificationRowModel
{
    public Guid     Id        { get; set; }
    public string   Title     { get; set; } = string.Empty;
    public string   Body      { get; set; } = string.Empty;
    public string   Category  { get; set; } = string.Empty;
    public bool     IsRead    { get; set; }
    public DateTime CreatedAt { get; set; }
    public string   TimeAgo   { get; set; } = string.Empty;

    public static string Ago(DateTime utc)
    {
        var span = DateTime.UtcNow - utc;
        if (span.TotalMinutes < 1) return "just now";
        if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes}m ago";
        if (span.TotalHours   < 24) return $"{(int)span.TotalHours}h ago";
        if (span.TotalDays    < 7)  return $"{(int)span.TotalDays}d ago";
        return utc.ToLocalTime().ToString("dd MMM yyyy");
    }
}

public class PushSubscriptionDto
{
    public string? Endpoint { get; set; }
    public PushKeysDto? Keys { get; set; }

    public bool IsValid() =>
        !string.IsNullOrWhiteSpace(Endpoint) && Endpoint.StartsWith("https://") && Endpoint.Length <= 800 &&
        !string.IsNullOrWhiteSpace(Keys?.P256dh) && Keys.P256dh.Length <= 200 &&
        !string.IsNullOrWhiteSpace(Keys?.Auth)   && Keys.Auth.Length   <= 100;
}

public class PushKeysDto
{
    public string? P256dh { get; set; }
    public string? Auth   { get; set; }
}
