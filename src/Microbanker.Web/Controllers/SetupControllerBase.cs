// src/Microbanker.Web/Controllers/SetupControllerBase.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microbanker.Domain.Entities.Organization;
using Microbanker.Infrastructure.Persistence;
using Microbanker.Web.Models;
using Microbanker.Web.Services;

namespace Microbanker.Web.Controllers;

/// <summary>
/// Shared plumbing for the Setup / Reference Data controllers: organisation lookup from the
/// signed-in user's claim, safe decoding of encrypted route IDs, and dropdown loaders.
/// </summary>
public abstract class SetupControllerBase : BaseController
{
    protected readonly MicrobankerDbContext _db;
    protected readonly IEncryptionService _enc;

    protected SetupControllerBase(MicrobankerDbContext db, IEncryptionService enc)
    {
        _db = db;
        _enc = enc;
    }

    protected async Task<Organization?> GetOrgAsync()
    {
        var companyId = GetCompanyId();
        return await _db.Organizations.FirstOrDefaultAsync(o => o.Id == companyId && !o.IsDeleted);
    }

    /// <summary>Decrypts a route id; returns false (→ NotFound) for tampered or malformed values.</summary>
    protected bool TryDecodeId(string? id, out Guid guid)
    {
        guid = Guid.Empty;
        if (string.IsNullOrWhiteSpace(id)) return false;
        try { return Guid.TryParse(_enc.Decrypt(id), out guid); }
        catch (Exception) { return false; }
    }

    protected static string? NormCode(string? code)
        => string.IsNullOrWhiteSpace(code) ? null : code.Trim().ToUpperInvariant();

    protected static List<SelectListItem> Options(IEnumerable<string> values)
        => values.Select(v => new SelectListItem(v, v)).ToList();

    // ── Dropdown loaders (active, non-deleted, organisation-scoped) ───────────

    protected Task<List<SelectListItem>> RegionItemsAsync(Guid orgId)
        => _db.Regions.Where(x => x.OrganizationId == orgId && !x.IsDeleted && x.IsActive)
              .OrderBy(x => x.Name)
              .Select(x => new SelectListItem(x.Name, x.Id.ToString())).ToListAsync();

    protected Task<List<SelectListItem>> SectorItemsAsync(Guid orgId)
        => _db.Sectors.Where(x => x.OrganizationId == orgId && !x.IsDeleted && x.IsActive)
              .OrderBy(x => x.Name)
              .Select(x => new SelectListItem(x.Name, x.Id.ToString())).ToListAsync();

    protected Task<List<SelectListItem>> ProductTypeItemsAsync(Guid orgId)
        => _db.ProductTypes.Where(x => x.OrganizationId == orgId && !x.IsDeleted && x.IsActive)
              .OrderBy(x => x.Name)
              .Select(x => new SelectListItem(x.Name, x.Id.ToString())).ToListAsync();

    protected Task<List<DistrictOption>> DistrictOptionsAsync(Guid orgId)
        => _db.Districts.Where(x => x.OrganizationId == orgId && !x.IsDeleted && x.IsActive)
              .OrderBy(x => x.Name)
              .Select(x => new DistrictOption { Id = x.Id, RegionId = x.RegionId, Name = x.Name }).ToListAsync();
}
