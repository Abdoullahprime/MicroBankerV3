// src/BaseTemplate.Web/Controllers/SystemSettingsController.cs
using System.ComponentModel.DataAnnotations;
using BaseTemplate.Domain.Entities.Settings;
using BaseTemplate.Infrastructure.Persistence;
using BaseTemplate.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BaseTemplate.Web.Controllers;

/// <summary>Generic key/value application settings. Internal keys (prefix "WebPush.") are hidden.</summary>
[RequirePermission(Modules.SystemSettings)]
public class SystemSettingsController : BaseController
{
    private const string HiddenPrefix = "WebPush.";

    private readonly AppDbContext _db;
    private readonly IEncryptionService _enc;

    public SystemSettingsController(AppDbContext db, IEncryptionService enc)
    {
        _db  = db;
        _enc = enc;
    }

    public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
    {
        base.OnActionExecuting(context);
        ViewData["ActivePage"]  = "SystemSettings";
        ViewData["ActiveGroup"] = "Administration";
    }

    // ── GET /SystemSettings ───────────────────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        var rows = await _db.SystemSettings
            .Where(s => !s.IsDeleted && !s.Key.StartsWith(HiddenPrefix))
            .OrderBy(s => s.Key)
            .ToListAsync();

        return View(new SystemSettingsViewModel
        {
            Settings = rows.Select(s => new SystemSettingRow
            {
                EncryptedId = _enc.Encrypt(s.Id.ToString()),
                Key         = s.Key,
                Value       = s.Value,
                Description = s.Description,
            }).ToList()
        });
    }

    // ── POST /SystemSettings/Save — add or update by key ──────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(SystemSettingForm form)
    {
        var key = form.Key?.Trim() ?? "";
        if (!ModelState.IsValid || key.StartsWith(HiddenPrefix, StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "A valid key and value are required.";
            return RedirectToAction(nameof(Index));
        }

        var existing = await _db.SystemSettings.FirstOrDefaultAsync(s => s.Key == key && !s.IsDeleted);
        if (existing == null)
            _db.SystemSettings.Add(new SystemSetting(key, form.Value ?? "", form.Description));
        else
            existing.Update(form.Value ?? "", form.Description);

        await _db.SaveChangesAsync();
        TempData["Success"] = $"Setting '{key}' saved.";
        return RedirectToAction(nameof(Index));
    }

    // ── POST /SystemSettings/Delete ───────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var settingId = Guid.Parse(_enc.Decrypt(id));
        var setting = await _db.SystemSettings.FirstOrDefaultAsync(s => s.Id == settingId && !s.IsDeleted);
        if (setting != null && !setting.Key.StartsWith(HiddenPrefix))
        {
            setting.SoftDelete();
            await _db.SaveChangesAsync();
            TempData["Success"] = "Setting deleted.";
        }
        return RedirectToAction(nameof(Index));
    }
}

public class SystemSettingsViewModel
{
    public List<SystemSettingRow> Settings { get; set; } = new();
}

public class SystemSettingRow
{
    public string EncryptedId { get; set; } = "";
    public string Key         { get; set; } = "";
    public string Value       { get; set; } = "";
    public string? Description { get; set; }
}

public class SystemSettingForm
{
    [Required, StringLength(200)]
    public string Key { get; set; } = "";

    [StringLength(2000)]
    public string? Value { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }
}
