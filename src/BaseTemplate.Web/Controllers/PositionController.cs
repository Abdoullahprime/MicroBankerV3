// src/BaseTemplate.Web/Controllers/PositionController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BaseTemplate.Domain.Entities.Organization;
using BaseTemplate.Infrastructure.Persistence;
using BaseTemplate.Web.Services;
using BaseTemplate.Web.Models;

namespace BaseTemplate.Web.Controllers;

[RequirePermission(Modules.Positions)]
public class PositionController : BaseController
{
    private readonly AppDbContext _db;
    private readonly IEncryptionService _enc;

    public PositionController(AppDbContext db, IEncryptionService enc)
    {
        _db = db;
        _enc = enc;
    }

    public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
    {
        base.OnActionExecuting(context);
        ViewData["ActiveGroup"] = "Org";
        ViewData["ActivePage"]  = "Positions";
    }
    private async Task<Organization?> GetOrgAsync()
    {
        return await _db.Organizations
            .Where(o => !o.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<IActionResult> Index()
    {
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var positions = await _db.Positions
            .Where(p => p.OrganizationId == org.Id && !p.IsDeleted)
            .OrderBy(p => p.Name).ToListAsync();

        var vm = new PositionListViewModel
        {
            Positions = positions.Select(p => new PositionRowModel
            {
                Name = p.Name,
                Code = p.Code,
                Level = p.Level,
                IsActive = p.IsActive,
                EncryptedId = _enc.Encrypt(p.Id.ToString())
            }).ToList()
        };
        return View(vm);
    }

    public async Task<IActionResult> Create()
    {
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        var model = new PositionFormModel();
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PositionFormModel model)
    {
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        if (!ModelState.IsValid) return View(model);

        var position = new Position(org.Id, model.Name);
        position.Update(model.Name, model.Code, model.Description, model.Level, model.IsActive);

        _db.Positions.Add(position);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Position created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string id)
    {
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var posId = Guid.Parse(_enc.Decrypt(id));
        var pos = await _db.Positions.FindAsync(posId);
        if (pos == null || pos.IsDeleted) return NotFound();

        var model = new PositionFormModel
        {
            Name        = pos.Name,
            Code        = pos.Code,
            Description = pos.Description,
            Level       = pos.Level,
            IsActive    = pos.IsActive,
            EncryptedId = id
        };
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, PositionFormModel model)
    {
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        if (!ModelState.IsValid) { model.EncryptedId = id; return View(model); }

        var posId = Guid.Parse(_enc.Decrypt(id));
        var pos = await _db.Positions.FindAsync(posId);
        if (pos == null || pos.IsDeleted) return NotFound();

        pos.Update(model.Name, model.Code, model.Description, model.Level, model.IsActive);

        await _db.SaveChangesAsync();
        TempData["Success"] = "Position updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var posId = Guid.Parse(_enc.Decrypt(id));
        var pos = await _db.Positions.FindAsync(posId);
        if (pos == null || pos.IsDeleted) return NotFound();
        pos.SoftDelete();
        await _db.SaveChangesAsync();
        TempData["Success"] = "Position deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
