// src/BaseTemplate.Web/Controllers/NotificationsController.cs
using BaseTemplate.Domain.Entities.Notifications;
using BaseTemplate.Infrastructure.Services;
using BaseTemplate.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace BaseTemplate.Web.Controllers;

public class NotificationsController : BaseController
{
    private const NotificationAudience Audience = NotificationAudience.Admin;
    private const int PageSize = 20;

    private readonly NotificationService _notifications;

    public NotificationsController(NotificationService notifications)
    {
        _notifications = notifications;
    }

    // GET /Notifications
    public async Task<IActionResult> Index(int page = 1)
    {
        ViewData["Title"]      = "Notifications";
        ViewData["ActivePage"] = "Notifications";

        var userId = GetUserId();
        page = Math.Max(1, page);
        var total = await _notifications.CountAsync(Audience, userId);
        var items = await _notifications.PageAsync(Audience, userId, page, PageSize);

        return View(new NotificationListViewModel
        {
            Page       = page,
            TotalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize)),
            Items      = items.Select(ToRow).ToList()
        });
    }

    // GET /Notifications/Open/{id} — marks read, then follows the notification link
    public async Task<IActionResult> Open(Guid id)
    {
        var n = await _notifications.MarkReadAsync(Audience, GetUserId(), id);
        if (n?.Url is { Length: > 0 } url && Url.IsLocalUrl(url))
            return LocalRedirect(url);
        return RedirectToAction(nameof(Index));
    }

    // GET /Notifications/Feed — bell dropdown data
    [HttpGet]
    public async Task<IActionResult> Feed()
    {
        var userId = GetUserId();
        var unread = await _notifications.UnreadCountAsync(Audience, userId);
        var recent = await _notifications.RecentAsync(Audience, userId, 8);
        return Json(new
        {
            unread,
            items = recent.Select(ToRow).Select(r => new
            {
                id = r.Id, r.Title, r.Body, r.Category, r.IsRead, r.TimeAgo,
                openUrl = Url.Action(nameof(Open), new { id = r.Id })
            })
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead()
    {
        await _notifications.MarkAllReadAsync(Audience, GetUserId());
        if (Request.Headers.XRequestedWith == "XMLHttpRequest" || Request.Headers.Accept.ToString().Contains("application/json"))
            return Ok();
        return RedirectToAction(nameof(Index));
    }

    // GET /Notifications/VapidPublicKey
    [HttpGet]
    public async Task<IActionResult> VapidPublicKey()
        => Json(new { publicKey = (await _notifications.GetVapidKeysAsync()).PublicKey });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Subscribe([FromBody] PushSubscriptionDto dto)
    {
        if (!dto.IsValid()) return BadRequest();
        await _notifications.SubscribeAsync(Audience, GetUserId(), dto.Endpoint!, dto.Keys!.P256dh!, dto.Keys.Auth!,
                                            Request.Headers.UserAgent.ToString());
        return Ok();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Unsubscribe([FromBody] PushSubscriptionDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Endpoint)) return BadRequest();
        await _notifications.UnsubscribeAsync(dto.Endpoint);
        return Ok();
    }

    // POST /Notifications/Test — sends a test notification to the current user's devices
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Test()
    {
        await _notifications.NotifyAsync(Audience, GetUserId(),
            "Test notification", "Push notifications are working on this device.",
            Url.Action(nameof(Index)), "System");
        return Ok();
    }

    private static NotificationRowModel ToRow(AppNotification n) => new()
    {
        Id        = n.Id,
        Title     = n.Title,
        Body      = n.Body,
        Category  = n.Category,
        IsRead    = n.IsRead,
        CreatedAt = n.CreatedAt,
        TimeAgo   = NotificationRowModel.Ago(n.CreatedAt)
    };
}
