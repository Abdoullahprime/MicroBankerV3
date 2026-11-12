// src/Microbanker.Web/Controllers/DistrictController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microbanker.Domain.Entities.Setup;
using Microbanker.Infrastructure.Persistence;
using Microbanker.Web.Services;
using Microbanker.Web.Models;
using Microbanker.Domain.Security;
using Microbanker.Web.Security;

namespace Microbanker.Web.Controllers;

[RequirePermission(Modules.Districts)]
public class DistrictController : SetupControllerBase
{
    public DistrictController(MicrobankerDbContext db, IEncryptionService enc) : base(db, enc) { }

    private void SetNav()
    {
        ViewData["ActiveGroup"] = "Setup";
        ViewData["ActivePage"]  = "Districts";
    }

    public async Task<IActionResult> Index()
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var rows = await _db.Districts.Include(x => x.Region)
            .Where(x => x.OrganizationId == org.Id && !x.IsDeleted)
            .OrderBy(x => x.Region.Name).ThenBy(x => x.Name).ToListAsync();

        var vm = rows.Select(x => new DistrictViewModel
        {
            EncryptedId = _enc.Encrypt(x.Id.ToString()),
            Name = x.Name, Code = x.Code, RegionName = x.Region.Name, IsActive = x.IsActive
        }).ToList();

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var model = new DistrictFormModel();
        model.Regions = await RegionItemsAsync(org.Id);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DistrictFormModel model)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var self = Guid.Empty;
        var code = NormCode(model.Code);
        if (code != null && await _db.Districts.AnyAsync(x => x.OrganizationId == org.Id && !x.IsDeleted && x.Id != self && x.Code == code))
            ModelState.AddModelError(nameof(model.Code), "Another record already uses this code.");
        if (model.RegionId != null && !await _db.Regions.AnyAsync(x => x.Id == model.RegionId && x.OrganizationId == org.Id && !x.IsDeleted && x.IsActive))
            ModelState.AddModelError(nameof(model.RegionId), "Please select a valid region.");

        if (!ModelState.IsValid)
        {
            model.Regions = await RegionItemsAsync(org.Id);
            return View(model);
        }

        var entity = new District(org.Id, model.RegionId!.Value, model.Name.Trim(), code);
        if (!model.IsActive) entity.Update(model.RegionId!.Value, model.Name.Trim(), code, false);
        _db.Districts.Add(entity);
        await _db.SaveChangesAsync();
        TempData["Success"] = "District created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        if (!TryDecodeId(id, out var self)) return NotFound();

        var entity = await _db.Districts.FirstOrDefaultAsync(x => x.Id == self && x.OrganizationId == org.Id && !x.IsDeleted);
        if (entity == null) return NotFound();

        var model = new DistrictFormModel
        {
            EncryptedId = id,
            RegionId = entity.RegionId, Name = entity.Name, Code = entity.Code, IsActive = entity.IsActive
        };
        model.Regions = await RegionItemsAsync(org.Id);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, DistrictFormModel model)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        if (!TryDecodeId(id, out var self)) return NotFound();

        var entity = await _db.Districts.FirstOrDefaultAsync(x => x.Id == self && x.OrganizationId == org.Id && !x.IsDeleted);
        if (entity == null) return NotFound();

        var code = NormCode(model.Code);
        if (code != null && await _db.Districts.AnyAsync(x => x.OrganizationId == org.Id && !x.IsDeleted && x.Id != self && x.Code == code))
            ModelState.AddModelError(nameof(model.Code), "Another record already uses this code.");
        if (model.RegionId != null && !await _db.Regions.AnyAsync(x => x.Id == model.RegionId && x.OrganizationId == org.Id && !x.IsDeleted && x.IsActive))
            ModelState.AddModelError(nameof(model.RegionId), "Please select a valid region.");

        if (!ModelState.IsValid)
        {
            model.EncryptedId = id;
            model.Regions = await RegionItemsAsync(org.Id);
            return View(model);
        }

        entity.Update(model.RegionId!.Value, model.Name.Trim(), code, model.IsActive);
        await _db.SaveChangesAsync();
        TempData["Success"] = "District updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        if (!TryDecodeId(id, out var self)) return NotFound();

        var entity = await _db.Districts.FirstOrDefaultAsync(x => x.Id == self && x.OrganizationId == org.Id && !x.IsDeleted);
        if (entity == null) return NotFound();

        if (await _db.Areas.AnyAsync(x => x.DistrictId == self && !x.IsDeleted))
        {
            TempData["Error"] = "This district has areas and cannot be deleted.";
            return RedirectToAction(nameof(Index));
        }

        entity.SoftDelete();
        await _db.SaveChangesAsync();
        TempData["Success"] = "District deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
