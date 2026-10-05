// src/BaseTemplate.Web/Controllers/EmailConfigController.cs
using BaseTemplate.Domain.Entities.Organization;
using BaseTemplate.Infrastructure.Persistence;
using BaseTemplate.Infrastructure.Services;
using BaseTemplate.Web.Models;
using BaseTemplate.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BaseTemplate.Web.Controllers;

// One SMTP configuration per channel: Core (system), Portal (optional second audience), External (optional third audience).
[RequirePermission(Modules.EmailConfig)]
public class EmailConfigController : BaseController
{
    private readonly AppDbContext _db;
    private readonly IEmailService _email;

    public EmailConfigController(AppDbContext db, IEmailService email)
    { _db = db; _email = email; }

    private void SetNav()
    {
        ViewData["ActivePage"]  = "EmailConfig";
        ViewData["ActiveGroup"] = "Administration";
    }

    private async Task<Organization?> GetOrgAsync()
        => await _db.Organizations.Where(o => !o.IsDeleted).FirstOrDefaultAsync();

    private static EmailChannel ParseChannel(string? channel)
        => Enum.TryParse<EmailChannel>(channel, true, out var c) ? c : EmailChannel.Core;

    private Task<EmailConfiguration?> ConfigAsync(Guid orgId, EmailChannel channel)
        => _db.EmailConfigurations
              .Where(c => c.OrganizationId == orgId && !c.IsDeleted && c.Channel == channel)
              .OrderByDescending(c => c.CreatedAt)
              .FirstOrDefaultAsync();

    private async Task FillChannelsAsync(EmailConfigFormModel model, Guid orgId)
    {
        var rows = await _db.EmailConfigurations
            .Where(c => c.OrganizationId == orgId && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
        model.Channels = Enum.GetValues<EmailChannel>().Select(ch =>
        {
            var c = rows.FirstOrDefault(r => r.Channel == ch);
            return new EmailChannelTab(ch.ToString(), EmailConfigFormModel.Label(ch), EmailConfigFormModel.Purpose(ch), c != null,
                                       c?.SmtpHost, c?.SmtpPort, c?.FromEmail, c?.FromName, c?.UpdatedAt ?? c?.CreatedAt);
        }).ToList();
    }

    // ── GET /EmailConfig/Settings?channel=Core|Portal|External ───────────────
    public async Task<IActionResult> Settings(string? channel)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var ch = ParseChannel(channel);
        var config = await ConfigAsync(org.Id, ch);

        var model = new EmailConfigFormModel
        {
            Channel           = ch.ToString(),
            HasExistingConfig = config != null,
            SmtpHost  = config?.SmtpHost  ?? "",
            SmtpPort  = config?.SmtpPort  ?? 587,
            EnableSsl = config?.EnableSsl ?? true,
            FromEmail = config?.FromEmail ?? "",
            FromName  = config?.FromName  ?? org.Name,
            Username  = config?.Username  ?? "",
        };
        await FillChannelsAsync(model, org.Id);
        return View(model);
    }

    // ── POST /EmailConfig/Settings ────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Settings(EmailConfigFormModel model)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var ch = ParseChannel(model.Channel);
        model.Channel = ch.ToString();
        var existing = await ConfigAsync(org.Id, ch);

        // Password required only on first save
        if (existing == null && string.IsNullOrWhiteSpace(model.Password))
            ModelState.AddModelError("Password", "Password is required for a new configuration.");

        if (!ModelState.IsValid)
        {
            model.HasExistingConfig = existing != null;
            await FillChannelsAsync(model, org.Id);
            return View(model);
        }

        if (existing == null)
        {
            _db.EmailConfigurations.Add(new EmailConfiguration(
                org.Id, model.SmtpHost, model.SmtpPort, model.EnableSsl,
                model.FromEmail, model.FromName, model.Username, model.Password!, ch));
        }
        else
        {
            existing.Update(model.SmtpHost, model.SmtpPort, model.EnableSsl,
                model.FromEmail, model.FromName, model.Username,
                string.IsNullOrWhiteSpace(model.Password) ? null : model.Password);
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = $"{EmailConfigFormModel.Label(ch)} email configuration saved.";
        return RedirectToAction(nameof(Settings), new { channel = ch.ToString() });
    }

    // ── POST /EmailConfig/Remove — channel goes back to using Core ───────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(string channel)
    {
        var ch = ParseChannel(channel);
        if (ch == EmailChannel.Core)
        {
            TempData["Error"] = "The Core configuration can't be removed; the other channels fall back to it.";
            return RedirectToAction(nameof(Settings));
        }
        var org = await GetOrgAsync();
        var existing = org == null ? null : await ConfigAsync(org.Id, ch);
        if (existing != null)
        {
            existing.SoftDelete();
            await _db.SaveChangesAsync();
            TempData["Success"] = $"{EmailConfigFormModel.Label(ch)} now sends through the Core mail server.";
        }
        return RedirectToAction(nameof(Settings), new { channel = ch.ToString() });
    }

    // ── POST /EmailConfig/SendTest ────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SendTest(string testEmail, string? channel)
    {
        var ch = ParseChannel(channel);
        if (string.IsNullOrWhiteSpace(testEmail))
        {
            TempData["Error"] = "Please enter a test email address.";
            return RedirectToAction(nameof(Settings), new { channel = ch.ToString() });
        }

        var (ok, err) = await _email.SendAsync(
            testEmail, testEmail,
            $"Email Configuration Test ({EmailConfigFormModel.Label(ch)})",
            $"<p>This is a test email from the <strong>{EmailConfigFormModel.Label(ch)}</strong> mail settings of your <strong>BaseTemplate</strong> system.</p>" +
            "<p>If you received this, the configuration is working correctly.</p>", ch);

        TempData[ok ? "Success" : "Error"] = ok
            ? $"Test email sent to {testEmail} using the {EmailConfigFormModel.Label(ch)} settings."
            : $"Failed to send test email: {err}";

        return RedirectToAction(nameof(Settings), new { channel = ch.ToString() });
    }
}
