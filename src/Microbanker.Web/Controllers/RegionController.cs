// src/Microbanker.Web/Controllers/RegionController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microbanker.Domain.Entities.Setup;
using Microbanker.Infrastructure.Persistence;
using Microbanker.Web.Services;
using Microbanker.Web.Models;
using Microbanker.Domain.Security;
using Microbanker.Web.Security;

namespace Microbanker.Web.Controllers;

[RequirePermission(Modules.Regions)]
public class RegionController : SetupControllerBase
{
    public RegionController(MicrobankerDbContext db, IEncryptionService enc) : base(db, enc) { }

    private void SetNav()
    {
        ViewData["ActiveGroup"] = "Setup";
        ViewData["ActivePage"]  = "Regions";
    }

    public async Task<IActionResult> Index()
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var rows = await _db.Regions
            .Where(x => x.OrganizationId == org.Id && !x.IsDeleted)
            .OrderBy(x => x.Name).ToListAsync();

        var vm = rows.Select(x => new RegionViewModel
        {
            EncryptedId = _enc.Encrypt(x.Id.ToString()),
            Name = x.Name, Code = x.Code, IsActive = x.IsActive
        }).ToList();

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var model = new RegionFormModel();
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RegionFormModel model)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var self = Guid.Empty;
        var code = NormCode(model.Code);
        if (code != null && await _db.Regions.AnyAsync(x => x.OrganizationId == org.Id && !x.IsDeleted && x.Id != self && x.Code == code))
            ModelState.AddModelError(nameof(model.Code), "Another record already uses this code.");

        if (!ModelState.IsValid)
        {

            return View(model);
        }

        var entity = new Region(org.Id, model.Name.Trim(), code);
        if (!model.IsActive) entity.Update(model.Name.Trim(), code, false);
        _db.Regions.Add(entity);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Region created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        if (!TryDecodeId(id, out var self)) return NotFound();

        var entity = await _db.Regions.FirstOrDefaultAsync(x => x.Id == self && x.OrganizationId == org.Id && !x.IsDeleted);
        if (entity == null) return NotFound();

        var model = new RegionFormModel
        {
            EncryptedId = id,
            Name = entity.Name, Code = entity.Code, IsActive = entity.IsActive
        };

        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, RegionFormModel model)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        if (!TryDecodeId(id, out var self)) return NotFound();

        var entity = await _db.Regions.FirstOrDefaultAsync(x => x.Id == self && x.OrganizationId == org.Id && !x.IsDeleted);
        if (entity == null) return NotFound();

        var code = NormCode(model.Code);
        if (code != null && await _db.Regions.AnyAsync(x => x.OrganizationId == org.Id && !x.IsDeleted && x.Id != self && x.Code == code))
            ModelState.AddModelError(nameof(model.Code), "Another record already uses this code.");

        if (!ModelState.IsValid)
        {
            model.EncryptedId = id;

            return View(model);
        }

        entity.Update(model.Name.Trim(), code, model.IsActive);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Region updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        if (!TryDecodeId(id, out var self)) return NotFound();

        var entity = await _db.Regions.FirstOrDefaultAsync(x => x.Id == self && x.OrganizationId == org.Id && !x.IsDeleted);
        if (entity == null) return NotFound();

        if (await _db.Districts.AnyAsync(x => x.RegionId == self && !x.IsDeleted))
        {
            TempData["Error"] = "This region has districts and cannot be deleted.";
            return RedirectToAction(nameof(Index));
        }

        entity.SoftDelete();
        await _db.SaveChangesAsync();
        TempData["Success"] = "Region deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
