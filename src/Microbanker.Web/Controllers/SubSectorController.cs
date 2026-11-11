// src/Microbanker.Web/Controllers/SubSectorController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microbanker.Domain.Entities.Setup;
using Microbanker.Infrastructure.Persistence;
using Microbanker.Web.Services;
using Microbanker.Web.Models;

namespace Microbanker.Web.Controllers;

public class SubSectorController : SetupControllerBase
{
    public SubSectorController(MicrobankerDbContext db, IEncryptionService enc) : base(db, enc) { }

    private void SetNav()
    {
        ViewData["ActiveGroup"] = "Setup";
        ViewData["ActivePage"]  = "SubSectors";
    }

    public async Task<IActionResult> Index()
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var rows = await _db.SubSectors.Include(x => x.Sector)
            .Where(x => x.OrganizationId == org.Id && !x.IsDeleted)
            .OrderBy(x => x.Sector.Name).ThenBy(x => x.Name).ToListAsync();

        var vm = rows.Select(x => new SubSectorViewModel
        {
            EncryptedId = _enc.Encrypt(x.Id.ToString()),
            Name = x.Name, Code = x.Code, SectorName = x.Sector.Name, IsActive = x.IsActive
        }).ToList();

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var model = new SubSectorFormModel();
        model.Sectors = await SectorItemsAsync(org.Id);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SubSectorFormModel model)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var self = Guid.Empty;
        var code = NormCode(model.Code);
        if (code != null && await _db.SubSectors.AnyAsync(x => x.OrganizationId == org.Id && !x.IsDeleted && x.Id != self && x.Code == code))
            ModelState.AddModelError(nameof(model.Code), "Another record already uses this code.");
        if (model.SectorId != null && !await _db.Sectors.AnyAsync(x => x.Id == model.SectorId && x.OrganizationId == org.Id && !x.IsDeleted && x.IsActive))
            ModelState.AddModelError(nameof(model.SectorId), "Please select a valid sector.");

        if (!ModelState.IsValid)
        {
            model.Sectors = await SectorItemsAsync(org.Id);
            return View(model);
        }

        var entity = new SubSector(org.Id, model.SectorId!.Value, model.Name.Trim(), code);
        if (!model.IsActive) entity.Update(model.SectorId!.Value, model.Name.Trim(), code, false);
        _db.SubSectors.Add(entity);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Sub-sector created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        if (!TryDecodeId(id, out var self)) return NotFound();

        var entity = await _db.SubSectors.FirstOrDefaultAsync(x => x.Id == self && x.OrganizationId == org.Id && !x.IsDeleted);
        if (entity == null) return NotFound();

        var model = new SubSectorFormModel
        {
            EncryptedId = id,
            SectorId = entity.SectorId, Name = entity.Name, Code = entity.Code, IsActive = entity.IsActive
        };
        model.Sectors = await SectorItemsAsync(org.Id);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, SubSectorFormModel model)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        if (!TryDecodeId(id, out var self)) return NotFound();

        var entity = await _db.SubSectors.FirstOrDefaultAsync(x => x.Id == self && x.OrganizationId == org.Id && !x.IsDeleted);
        if (entity == null) return NotFound();

        var code = NormCode(model.Code);
        if (code != null && await _db.SubSectors.AnyAsync(x => x.OrganizationId == org.Id && !x.IsDeleted && x.Id != self && x.Code == code))
            ModelState.AddModelError(nameof(model.Code), "Another record already uses this code.");
        if (model.SectorId != null && !await _db.Sectors.AnyAsync(x => x.Id == model.SectorId && x.OrganizationId == org.Id && !x.IsDeleted && x.IsActive))
            ModelState.AddModelError(nameof(model.SectorId), "Please select a valid sector.");

        if (!ModelState.IsValid)
        {
            model.EncryptedId = id;
            model.Sectors = await SectorItemsAsync(org.Id);
            return View(model);
        }

        entity.Update(model.SectorId!.Value, model.Name.Trim(), code, model.IsActive);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Sub-sector updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        if (!TryDecodeId(id, out var self)) return NotFound();

        var entity = await _db.SubSectors.FirstOrDefaultAsync(x => x.Id == self && x.OrganizationId == org.Id && !x.IsDeleted);
        if (entity == null) return NotFound();

        entity.SoftDelete();
        await _db.SaveChangesAsync();
        TempData["Success"] = "Sub-sector deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
