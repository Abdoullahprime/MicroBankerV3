// src/Microbanker.Web/Controllers/AreaController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microbanker.Domain.Entities.Setup;
using Microbanker.Infrastructure.Persistence;
using Microbanker.Web.Services;
using Microbanker.Web.Models;
using Microbanker.Domain.Security;
using Microbanker.Web.Security;

namespace Microbanker.Web.Controllers;

[RequirePermission(Modules.Areas)]
public class AreaController : SetupControllerBase
{
    public AreaController(MicrobankerDbContext db, IEncryptionService enc) : base(db, enc) { }

    private void SetNav()
    {
        ViewData["ActiveGroup"] = "Setup";
        ViewData["ActivePage"]  = "Areas";
    }

    public async Task<IActionResult> Index()
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var rows = await _db.Areas.Include(x => x.District).ThenInclude(d => d.Region)
            .Where(x => x.OrganizationId == org.Id && !x.IsDeleted)
            .OrderBy(x => x.District.Region.Name).ThenBy(x => x.District.Name).ThenBy(x => x.Name).ToListAsync();

        var vm = rows.Select(x => new AreaViewModel
        {
            EncryptedId = _enc.Encrypt(x.Id.ToString()),
            Name = x.Name, Code = x.Code, DistrictName = x.District.Name,
            RegionName = x.District.Region.Name, IsActive = x.IsActive
        }).ToList();

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var model = new AreaFormModel();
        model.Regions = await RegionItemsAsync(org.Id);
        model.Districts = await DistrictOptionsAsync(org.Id);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AreaFormModel model)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var self = Guid.Empty;
        var code = NormCode(model.Code);
        if (code != null && await _db.Areas.AnyAsync(x => x.OrganizationId == org.Id && !x.IsDeleted && x.Id != self && x.Code == code))
            ModelState.AddModelError(nameof(model.Code), "Another record already uses this code.");
        if (model.DistrictId != null && !await _db.Districts.AnyAsync(x => x.Id == model.DistrictId && x.OrganizationId == org.Id && !x.IsDeleted && x.IsActive))
            ModelState.AddModelError(nameof(model.DistrictId), "Please select a valid district.");

        if (!ModelState.IsValid)
        {
            model.Regions = await RegionItemsAsync(org.Id);
            model.Districts = await DistrictOptionsAsync(org.Id);
            return View(model);
        }

        var entity = new Area(org.Id, model.DistrictId!.Value, model.Name.Trim(), code);
        if (!model.IsActive) entity.Update(model.DistrictId!.Value, model.Name.Trim(), code, false);
        _db.Areas.Add(entity);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Area created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        if (!TryDecodeId(id, out var self)) return NotFound();

        var entity = await _db.Areas.Include(x => x.District).FirstOrDefaultAsync(x => x.Id == self && x.OrganizationId == org.Id && !x.IsDeleted);
        if (entity == null) return NotFound();

        var model = new AreaFormModel
        {
            EncryptedId = id,
            RegionId = entity.District.RegionId, DistrictId = entity.DistrictId,
            Name = entity.Name, Code = entity.Code, IsActive = entity.IsActive
        };
        model.Regions = await RegionItemsAsync(org.Id);
        model.Districts = await DistrictOptionsAsync(org.Id);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, AreaFormModel model)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        if (!TryDecodeId(id, out var self)) return NotFound();

        var entity = await _db.Areas.FirstOrDefaultAsync(x => x.Id == self && x.OrganizationId == org.Id && !x.IsDeleted);
        if (entity == null) return NotFound();

        var code = NormCode(model.Code);
        if (code != null && await _db.Areas.AnyAsync(x => x.OrganizationId == org.Id && !x.IsDeleted && x.Id != self && x.Code == code))
            ModelState.AddModelError(nameof(model.Code), "Another record already uses this code.");
        if (model.DistrictId != null && !await _db.Districts.AnyAsync(x => x.Id == model.DistrictId && x.OrganizationId == org.Id && !x.IsDeleted && x.IsActive))
            ModelState.AddModelError(nameof(model.DistrictId), "Please select a valid district.");

        if (!ModelState.IsValid)
        {
            model.EncryptedId = id;
            model.Regions = await RegionItemsAsync(org.Id);
            model.Districts = await DistrictOptionsAsync(org.Id);
            return View(model);
        }

        entity.Update(model.DistrictId!.Value, model.Name.Trim(), code, model.IsActive);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Area updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        if (!TryDecodeId(id, out var self)) return NotFound();

        var entity = await _db.Areas.FirstOrDefaultAsync(x => x.Id == self && x.OrganizationId == org.Id && !x.IsDeleted);
        if (entity == null) return NotFound();

        entity.SoftDelete();
        await _db.SaveChangesAsync();
        TempData["Success"] = "Area deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
