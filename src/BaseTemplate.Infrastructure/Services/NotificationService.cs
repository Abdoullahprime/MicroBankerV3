// src/BaseTemplate.Infrastructure/Services/NotificationService.cs
using System.Net;
using System.Text.Json;
using BaseTemplate.Domain.Entities.Notifications;
using BaseTemplate.Domain.Entities.Settings;
using BaseTemplate.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebPush;
using DomainPushSubscription = BaseTemplate.Domain.Entities.Notifications.PushSubscription;
using WebPushSubscription    = WebPush.PushSubscription;

namespace BaseTemplate.Infrastructure.Services;

public record VapidKeys(string Subject, string PublicKey, string PrivateKey);

public class NotificationService
{
    private const string PublicKeySetting  = "WebPush.VapidPublicKey";
    private const string PrivateKeySetting = "WebPush.VapidPrivateKey";
    private const string SubjectSetting    = "WebPush.VapidSubject";
    private const string DefaultSubject    = "mailto:admin@example.com";

    private static readonly SemaphoreSlim KeyLock = new(1, 1);
    private static VapidKeys? _cachedKeys;

    private readonly AppDbContext _db;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(AppDbContext db, ILogger<NotificationService> logger)
    {
        _db     = db;
        _logger = logger;
    }

    // Keys live in SystemSettings so every app instance signs with the same pair.
    public async Task<VapidKeys> GetVapidKeysAsync()
    {
        if (_cachedKeys != null) return _cachedKeys;

        await KeyLock.WaitAsync();
        try
        {
            if (_cachedKeys != null) return _cachedKeys;

            var settings = await _db.SystemSettings
                .Where(s => !s.IsDeleted &&
                            (s.Key == PublicKeySetting || s.Key == PrivateKeySetting || s.Key == SubjectSetting))
                .ToDictionaryAsync(s => s.Key, s => s.Value);

            settings.TryGetValue(PublicKeySetting,  out var pub);
            settings.TryGetValue(PrivateKeySetting, out var priv);
            settings.TryGetValue(SubjectSetting,    out var subject);

            if (string.IsNullOrWhiteSpace(pub) || string.IsNullOrWhiteSpace(priv))
            {
                var generated = VapidHelper.GenerateVapidKeys();
                pub  = generated.PublicKey;
                priv = generated.PrivateKey;
                _db.SystemSettings.Add(new SystemSetting(PublicKeySetting,  pub,  "Web Push VAPID public key (auto-generated)"));
                _db.SystemSettings.Add(new SystemSetting(PrivateKeySetting, priv, "Web Push VAPID private key (auto-generated) — keep secret"));
                if (string.IsNullOrWhiteSpace(subject))
                {
                    subject = DefaultSubject;
                    _db.SystemSettings.Add(new SystemSetting(SubjectSetting, subject, "Web Push contact (mailto: or https: URL)"));
                }
                await _db.SaveChangesAsync();
                _logger.LogInformation("Generated new Web Push VAPID keys.");
            }

            _cachedKeys = new VapidKeys(string.IsNullOrWhiteSpace(subject) ? DefaultSubject : subject, pub, priv);
            return _cachedKeys;
        }
        finally
        {
            KeyLock.Release();
        }
    }

    public async Task SubscribeAsync(NotificationAudience audience, Guid userId,
                                     string endpoint, string p256dh, string auth, string? userAgent)
    {
        var existing = await _db.PushSubscriptions.FirstOrDefaultAsync(s => s.Endpoint == endpoint);
        if (existing != null)
            existing.Refresh(audience, userId, p256dh, auth, Truncate(userAgent, 400));
        else
            _db.PushSubscriptions.Add(new DomainPushSubscription(audience, userId, endpoint, p256dh, auth, Truncate(userAgent, 400)));

        await _db.SaveChangesAsync();
    }

    public async Task UnsubscribeAsync(string endpoint)
    {
        var existing = await _db.PushSubscriptions.FirstOrDefaultAsync(s => s.Endpoint == endpoint && !s.IsDeleted);
        if (existing == null) return;
        existing.SoftDelete();
        await _db.SaveChangesAsync();
    }

    public Task NotifyAsync(NotificationAudience audience, Guid userId,
                            string title, string body, string? url = null, string category = "General")
        => NotifyManyAsync(audience, new[] { userId }, title, body, url, category);

    // Never throws: a failed notification must not break the business action that triggered it.
    public async Task NotifyManyAsync(NotificationAudience audience, IEnumerable<Guid> userIds,
                                      string title, string body, string? url = null, string category = "General")
    {
        try
        {
            var ids = userIds.Where(id => id != Guid.Empty).Distinct().ToList();
            if (ids.Count == 0) return;

            title = Truncate(title, 200)!;
            body  = Truncate(body, 1000)!;

            var notifications = ids
                .Select(id => new AppNotification(audience, id, title, body, url, category))
                .ToList();
            _db.AppNotifications.AddRange(notifications);
            await _db.SaveChangesAsync();

            var subs = await _db.PushSubscriptions
                .Where(s => !s.IsDeleted && s.Audience == audience && ids.Contains(s.UserId))
                .ToListAsync();
            if (subs.Count == 0) return;

            var keys   = await GetVapidKeysAsync();
            var vapid  = new VapidDetails(keys.Subject, keys.PublicKey, keys.PrivateKey);
            var client = new WebPushClient();
            var byUser = notifications.ToDictionary(n => n.UserId, n => n.Id);

            foreach (var sub in subs)
            {
                var payload = JsonSerializer.Serialize(new
                {
                    title,
                    body,
                    url      = url ?? "/",
                    category,
                    id       = byUser.TryGetValue(sub.UserId, out var nid) ? nid : Guid.Empty,
                    tag      = category
                });

                try
                {
                    await client.SendNotificationAsync(
                        new WebPushSubscription(sub.Endpoint, sub.P256dh, sub.Auth), payload, vapid);
                    sub.MarkUsed();
                }
                catch (WebPushException ex) when (ex.StatusCode is HttpStatusCode.Gone or HttpStatusCode.NotFound)
                {
                    sub.SoftDelete();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Web push failed for subscription {SubscriptionId}", sub.Id);
                }
            }

            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification '{Title}'", title);
        }
    }

    public Task<int> UnreadCountAsync(NotificationAudience audience, Guid userId)
        => _db.AppNotifications.CountAsync(n => !n.IsDeleted && !n.IsRead &&
                                                 n.Audience == audience && n.UserId == userId);

    public Task<List<AppNotification>> RecentAsync(NotificationAudience audience, Guid userId, int take)
        => _db.AppNotifications
              .Where(n => !n.IsDeleted && n.Audience == audience && n.UserId == userId)
              .OrderByDescending(n => n.CreatedAt)
              .Take(take)
              .ToListAsync();

    public Task<List<AppNotification>> PageAsync(NotificationAudience audience, Guid userId, int page, int pageSize)
        => _db.AppNotifications
              .Where(n => !n.IsDeleted && n.Audience == audience && n.UserId == userId)
              .OrderByDescending(n => n.CreatedAt)
              .Skip((page - 1) * pageSize)
              .Take(pageSize)
              .ToListAsync();

    public Task<int> CountAsync(NotificationAudience audience, Guid userId)
        => _db.AppNotifications.CountAsync(n => !n.IsDeleted && n.Audience == audience && n.UserId == userId);

    public async Task<AppNotification?> MarkReadAsync(NotificationAudience audience, Guid userId, Guid notificationId)
    {
        var n = await _db.AppNotifications.FirstOrDefaultAsync(x =>
            x.Id == notificationId && !x.IsDeleted && x.Audience == audience && x.UserId == userId);
        if (n == null) return null;
        n.MarkRead();
        await _db.SaveChangesAsync();
        return n;
    }

    public async Task MarkAllReadAsync(NotificationAudience audience, Guid userId)
    {
        var unread = await _db.AppNotifications
            .Where(n => !n.IsDeleted && !n.IsRead && n.Audience == audience && n.UserId == userId)
            .ToListAsync();
        foreach (var n in unread) n.MarkRead();
        await _db.SaveChangesAsync();
    }

    private static string? Truncate(string? value, int max)
        => value == null || value.Length <= max ? value : value[..max];
}
