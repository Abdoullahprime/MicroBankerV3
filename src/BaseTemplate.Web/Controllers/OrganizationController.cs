// src/BaseTemplate.Web/Controllers/OrganizationController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BaseTemplate.Domain.Entities.Organization;
using BaseTemplate.Infrastructure.Persistence;
using BaseTemplate.Web.Services;
using BaseTemplate.Web.Models;
using BaseTemplate.Web.ViewComponents;
using Microsoft.Extensions.Caching.Memory;

namespace BaseTemplate.Web.Controllers;

[RequirePermission(Modules.Organization)]
public class OrganizationController : BaseController
{
    private readonly AppDbContext        _db;
    private readonly IEncryptionService   _enc;
    private readonly IWebHostEnvironment  _env;
    private readonly IMemoryCache         _cache;

    private static readonly string[] AllowedLogoExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg"];
    private const long MaxLogoSize = 2 * 1024 * 1024; // 2 MB

    public OrganizationController(AppDbContext db, IEncryptionService enc, IWebHostEnvironment env, IMemoryCache cache)
    {
        _cache = cache;
        _db  = db;
        _enc = enc;
        _env = env;
    }

    public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
    {
        base.OnActionExecuting(context);
        ViewData["ActiveGroup"] = "Org";
        ViewData["ActivePage"]  = "Organization";
    }


    public async Task<IActionResult> Index()
    {
        var org = await _db.Organizations
            .Where(o => !o.IsDeleted)
            .FirstOrDefaultAsync();

        var vm = new OrganizationViewModel
        {
            HasOrganization    = org != null,
            EncryptedId        = org != null ? _enc.Encrypt(org.Id.ToString()) : null,
            Name               = org?.Name,
            ShortName          = org?.ShortName,
            Address            = org?.Address,
            Phone              = org?.Phone,
            Email              = org?.Email,
            Website            = org?.Website,
            RegistrationNumber = org?.RegistrationNumber,
            TaxNumber          = org?.TaxNumber,
            Industry           = org?.Industry,
            EstablishedDate    = org?.EstablishedDate,
            Logo               = org?.Logo,
            PostalCode         = org?.PostalCode,
            GoogleCode         = org?.GoogleCode,
            MissionStatement   = org?.MissionStatement,
            VisionStatement    = org?.VisionStatement,
        };
        return View(vm);
    }

    public IActionResult Create() => View(new OrganizationFormModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OrganizationFormModel model)
    {
        if (!ModelState.IsValid) return View(model);

        if (await _db.Organizations.AnyAsync(o => !o.IsDeleted))
        {
            ModelState.AddModelError("", "An organization already exists.");
            return View(model);
        }

        if (model.LogoFile != null && !ValidateLogo(model.LogoFile, out var logoErr))
        {
            ModelState.AddModelError("LogoFile", logoErr);
            return View(model);
        }

        var code = (!string.IsNullOrEmpty(model.ShortName)
            ? model.ShortName
            : model.Name.Length >= 6 ? model.Name[..6] : model.Name)
            .ToUpper().Replace(" ", "");

        var org = new Organization(model.Name, code);
        org.Update(model.Name, code, model.ShortName, model.Address,
            model.Phone, model.Phone, model.Email, model.Website,
            model.RegistrationNumber, model.TaxNumber, model.Industry,
            model.EstablishedDate, true,
            model.PostalCode, model.GoogleCode,
            model.MissionStatement, model.VisionStatement);

        _db.Organizations.Add(org);
        await _db.SaveChangesAsync();

        if (model.LogoFile != null)
        {
            var logoName = await SaveLogoAsync(model.LogoFile, org.Id);
            org.UpdateLogo(logoName);
            await _db.SaveChangesAsync();
        }

        _cache.Remove(OrgLogoViewComponent.CacheKey);
        TempData["Success"] = "Organization created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string id)
    {
        var orgId = Guid.Parse(_enc.Decrypt(id));
        var org   = await _db.Organizations.FindAsync(orgId);
        if (org == null || org.IsDeleted) return NotFound();

        var model = new OrganizationFormModel
        {
            Name               = org.Name,
            ShortName          = org.ShortName,
            Address            = org.Address,
            Phone              = org.Phone,
            Email              = org.Email,
            Website            = org.Website,
            RegistrationNumber = org.RegistrationNumber,
            TaxNumber          = org.TaxNumber,
            Industry           = org.Industry,
            EstablishedDate    = org.EstablishedDate,
            PostalCode         = org.PostalCode,
            GoogleCode         = org.GoogleCode,
            MissionStatement   = org.MissionStatement,
            VisionStatement    = org.VisionStatement,
            ExistingLogo       = org.Logo,
            EncryptedId        = id
        };
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, OrganizationFormModel model)
    {
        if (!ModelState.IsValid) { model.EncryptedId = id; return View(model); }

        if (model.LogoFile != null && !ValidateLogo(model.LogoFile, out var logoErr))
        {
            ModelState.AddModelError("LogoFile", logoErr);
            model.EncryptedId = id;
            return View(model);
        }

        var orgId = Guid.Parse(_enc.Decrypt(id));
        var org   = await _db.Organizations.FindAsync(orgId);
        if (org == null || org.IsDeleted) return NotFound();

        var code = !string.IsNullOrEmpty(model.ShortName) ? model.ShortName : org.Code;

        org.Update(model.Name, code, model.ShortName, model.Address,
            model.Phone, model.Phone, model.Email, model.Website,
            model.RegistrationNumber, model.TaxNumber, model.Industry,
            model.EstablishedDate, true,
            model.PostalCode, model.GoogleCode,
            model.MissionStatement, model.VisionStatement);

        if (model.LogoFile != null)
        {
            // Delete old logo file if it exists
            if (!string.IsNullOrEmpty(org.Logo))
                DeleteLogoFile(org.Logo);

            var logoName = await SaveLogoAsync(model.LogoFile, org.Id);
            org.UpdateLogo(logoName);
        }

        await _db.SaveChangesAsync();
        _cache.Remove(OrgLogoViewComponent.CacheKey);
        TempData["Success"] = "Organization updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static bool ValidateLogo(IFormFile file, out string error)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedLogoExtensions.Contains(ext))
        {
            error = "Allowed formats: JPG, PNG, GIF, WEBP, SVG.";
            return false;
        }
        if (file.Length > MaxLogoSize)
        {
            error = "Logo must be 2 MB or smaller.";
            return false;
        }
        error = "";
        return true;
    }

    private async Task<string> SaveLogoAsync(IFormFile file, Guid orgId)
    {
        var folder = Path.Combine(_env.WebRootPath, "uploads", "org-logos");
        Directory.CreateDirectory(folder);

        var ext      = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{orgId}{ext}";
        var path     = Path.Combine(folder, fileName);

        using var stream = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(stream);

        return fileName;
    }

    private void DeleteLogoFile(string fileName)
    {
        try
        {
            var path = Path.Combine(_env.WebRootPath, "uploads", "org-logos", fileName);
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);
        }
        catch { /* swallow — non-critical */ }
    }
}
