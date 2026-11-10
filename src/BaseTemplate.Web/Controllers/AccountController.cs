// src/BaseTemplate.Web/Controllers/AccountController.cs
using BaseTemplate.Infrastructure.Persistence;
using BaseTemplate.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BaseTemplate.Web.Controllers;

public class AccountController : BaseController
{
    private readonly AppDbContext      _db;
    private readonly IWebHostEnvironment _env;

    public AccountController(AppDbContext db, IWebHostEnvironment env)
    {
        _db  = db;
        _env = env;
    }

    // ── GET: /Account/Signature ───────────────────────────────────────────────

    public async Task<IActionResult> Signature()
    {
        var userId = GetUserId();
        var user   = await _db.AppUsers.FindAsync(userId);
        if (user == null) return NotFound();

        ViewData["ActivePage"]  = "Signature";
        ViewData["ActiveGroup"] = "Administration";
        ViewData["CurrentSignature"] = user.Signature;
        ViewData["FullName"]         = user.FullName;
        return View();
    }

    // ── POST: /Account/Signature ──────────────────────────────────────────────

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Signature(IFormFile? signatureFile)
    {
        var userId = GetUserId();
        var user   = await _db.AppUsers.FindAsync(userId);
        if (user == null) return NotFound();

        if (signatureFile == null || signatureFile.Length == 0)
        {
            TempData["Error"] = "Please select a signature image file.";
            return RedirectToAction(nameof(Signature));
        }

        var ext = Path.GetExtension(signatureFile.FileName).ToLowerInvariant();
        if (!new[] { ".png", ".jpg", ".jpeg", ".gif", ".webp" }.Contains(ext))
        {
            TempData["Error"] = "Only PNG, JPG, GIF or WebP image files are accepted.";
            return RedirectToAction(nameof(Signature));
        }

        if (signatureFile.Length > 2 * 1024 * 1024) // 2 MB
        {
            TempData["Error"] = "Signature file must be 2 MB or smaller.";
            return RedirectToAction(nameof(Signature));
        }

        // Save to wwwroot/uploads/signatures/{userId}{ext}
        var sigDir = Path.Combine(_env.WebRootPath, "uploads", "signatures");
        Directory.CreateDirectory(sigDir);

        // Remove old signature file(s) for this user
        foreach (var old in Directory.GetFiles(sigDir, $"{userId}.*"))
            System.IO.File.Delete(old);

        var fileName    = $"{userId}{ext}";
        var fullPath    = Path.Combine(sigDir, fileName);
        await using (var stream = new FileStream(fullPath, FileMode.Create))
            await signatureFile.CopyToAsync(stream);

        var relativePath = $"/uploads/signatures/{fileName}";
        user.SetSignature(relativePath);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Signature saved successfully.";
        return RedirectToAction(nameof(Signature));
    }

    // ── POST: /Account/DeleteSignature ────────────────────────────────────────

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSignature()
    {
        var userId = GetUserId();
        var user   = await _db.AppUsers.FindAsync(userId);
        if (user == null) return NotFound();

        if (!string.IsNullOrEmpty(user.Signature))
        {
            var fullPath = Path.Combine(_env.WebRootPath, user.Signature.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);
        }

        user.SetSignature(null);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Signature removed.";
        return RedirectToAction(nameof(Signature));
    }
}
