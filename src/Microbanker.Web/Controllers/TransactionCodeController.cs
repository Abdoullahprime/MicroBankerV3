// src/Microbanker.Web/Controllers/TransactionCodeController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microbanker.Domain.Entities.Setup;
using Microbanker.Infrastructure.Persistence;
using Microbanker.Web.Services;
using Microbanker.Web.Models;
using Microbanker.Domain.Security;
using Microbanker.Web.Security;

namespace Microbanker.Web.Controllers;

[RequirePermission(Modules.TransactionCodes)]
public class TransactionCodeController : SetupControllerBase
{
    public TransactionCodeController(MicrobankerDbContext db, IEncryptionService enc) : base(db, enc) { }

    private void SetNav()
    {
        ViewData["ActiveGroup"] = "Setup";
        ViewData["ActivePage"]  = "TransactionCodes";
    }

    public async Task<IActionResult> Index()
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var rows = await _db.TransactionCodes
            .Where(x => x.OrganizationId == org.Id && !x.IsDeleted)
            .OrderBy(x => x.Code).ToListAsync();

        var vm = rows.Select(x => new TransactionCodeViewModel
        {
            EncryptedId = _enc.Encrypt(x.Id.ToString()),
            Code = x.Code, Name = x.Name, DrCr = x.DrCr,
            RequiresApproval = x.RequiresApproval, IsActive = x.IsActive
        }).ToList();

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var model = new TransactionCodeFormModel();
        model.DrCrOptions = Options(TransactionCode.DrCrValues);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TransactionCodeFormModel model)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var self = Guid.Empty;
        var code = NormCode(model.Code)!;
        var drCr = (model.DrCr ?? "").Trim().ToUpperInvariant();
        if (!TransactionCode.DrCrValues.Contains(drCr))
            ModelState.AddModelError(nameof(model.DrCr), "Please select DR or CR.");
        if (await _db.TransactionCodes.AnyAsync(x => x.OrganizationId == org.Id && !x.IsDeleted && x.Id != self && x.Code == code))
            ModelState.AddModelError(nameof(model.Code), "Another record already uses this code.");

        if (!ModelState.IsValid)
        {
            model.DrCrOptions = Options(TransactionCode.DrCrValues);
            return View(model);
        }

        var entity = new TransactionCode(org.Id, code, model.Name.Trim(), drCr);
        entity.Update(code, model.Name.Trim(), drCr, model.Description?.Trim(), model.RequiresApproval, model.IsActive);
        _db.TransactionCodes.Add(entity);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Transaction code created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        if (!TryDecodeId(id, out var self)) return NotFound();

        var entity = await _db.TransactionCodes.FirstOrDefaultAsync(x => x.Id == self && x.OrganizationId == org.Id && !x.IsDeleted);
        if (entity == null) return NotFound();

        var model = new TransactionCodeFormModel
        {
            EncryptedId = id,
            Code = entity.Code, Name = entity.Name, DrCr = entity.DrCr, Description = entity.Description,
            RequiresApproval = entity.RequiresApproval, IsActive = entity.IsActive
        };
        model.DrCrOptions = Options(TransactionCode.DrCrValues);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, TransactionCodeFormModel model)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        if (!TryDecodeId(id, out var self)) return NotFound();

        var entity = await _db.TransactionCodes.FirstOrDefaultAsync(x => x.Id == self && x.OrganizationId == org.Id && !x.IsDeleted);
        if (entity == null) return NotFound();

        var code = NormCode(model.Code)!;
        var drCr = (model.DrCr ?? "").Trim().ToUpperInvariant();
        if (!TransactionCode.DrCrValues.Contains(drCr))
            ModelState.AddModelError(nameof(model.DrCr), "Please select DR or CR.");
        if (await _db.TransactionCodes.AnyAsync(x => x.OrganizationId == org.Id && !x.IsDeleted && x.Id != self && x.Code == code))
            ModelState.AddModelError(nameof(model.Code), "Another record already uses this code.");

        if (!ModelState.IsValid)
        {
            model.EncryptedId = id;
            model.DrCrOptions = Options(TransactionCode.DrCrValues);
            return View(model);
        }

        entity.Update(code, model.Name.Trim(), drCr, model.Description?.Trim(), model.RequiresApproval, model.IsActive);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Transaction code updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        if (!TryDecodeId(id, out var self)) return NotFound();

        var entity = await _db.TransactionCodes.FirstOrDefaultAsync(x => x.Id == self && x.OrganizationId == org.Id && !x.IsDeleted);
        if (entity == null) return NotFound();

        entity.SoftDelete();
        await _db.SaveChangesAsync();
        TempData["Success"] = "Transaction code deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
