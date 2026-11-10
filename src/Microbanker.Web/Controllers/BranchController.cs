// src/Microbanker.Web/Controllers/BranchController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microbanker.Domain.Entities.Organization;
using Microbanker.Infrastructure.Persistence;
using Microbanker.Web.Services;
using Microbanker.Web.Models;

namespace Microbanker.Web.Controllers;

public class BranchController : BaseController
{
    private readonly MicrobankerDbContext _db;
    private readonly IEncryptionService _enc;

    public BranchController(MicrobankerDbContext db, IEncryptionService enc)
    {
        _db = db;
        _enc = enc;
    }

    private async Task<Organization?> GetOrgAsync()
    {
        var companyId = GetCompanyId();
        return await _db.Organizations
            .Where(o => !o.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<IActionResult> Index()
    {
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var branches = await _db.Branches
            .Where(b => b.OrganizationId == org.Id && !b.IsDeleted)
            .OrderBy(b => b.Name)
            .ToListAsync();

        var vm = new BranchListViewModel
        {
            Branches = branches.Select(b => new BranchRowModel
            {
                Name        = b.Name,
                Code        = b.Code,
                Location    = b.Location,
                IsHeadOffice = b.IsHeadOffice,
                IsActive    = b.IsActive,
                EncryptedId = _enc.Encrypt(b.Id.ToString())
            }).ToList()
        };
        return View(vm);
    }

    public async Task<IActionResult> Create()
    {
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        return View(new BranchFormModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BranchFormModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        if (model.IsHeadOffice)
        {
            var existing = await _db.Branches
                .Where(b => b.OrganizationId == org.Id && b.IsHeadOffice && !b.IsDeleted)
                .ToListAsync();
            existing.ForEach(b => b.ClearHeadOffice());
        }

        var code = string.IsNullOrEmpty(model.Code) ? "BR" : model.Code;
        var branch = new Branch(org.Id, model.Name, code);
        branch.Update(model.Name, code, model.Location, model.Address,
            model.Phone, model.Phone, null, model.Email,
            model.IsHeadOffice, model.IsActive);

        _db.Branches.Add(branch);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Branch created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string id)
    {
        var branchId = Guid.Parse(_enc.Decrypt(id));
        var branch   = await _db.Branches.FindAsync(branchId);
        if (branch == null || branch.IsDeleted) return NotFound();

        var model = new BranchFormModel
        {
            Name        = branch.Name,
            Code        = branch.Code,
            Location    = branch.Location,
            Address     = branch.Address,
            Phone       = branch.Phone,
            Email       = branch.Email,
            IsHeadOffice = branch.IsHeadOffice,
            IsActive    = branch.IsActive,
            EncryptedId = id
        };
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, BranchFormModel model)
    {
        if (!ModelState.IsValid) { model.EncryptedId = id; return View(model); }

        var branchId = Guid.Parse(_enc.Decrypt(id));
        var branch   = await _db.Branches.FindAsync(branchId);
        if (branch == null || branch.IsDeleted) return NotFound();

        if (model.IsHeadOffice)
        {
            var existing = await _db.Branches
                .Where(b => b.OrganizationId == branch.OrganizationId
                         && b.IsHeadOffice && b.Id != branchId && !b.IsDeleted)
                .ToListAsync();
            existing.ForEach(b => b.ClearHeadOffice());
        }

        var code = string.IsNullOrEmpty(model.Code) ? branch.Code : model.Code;
        branch.Update(model.Name, code, model.Location, model.Address,
            model.Phone, model.Phone, null, model.Email,
            model.IsHeadOffice, model.IsActive);

        await _db.SaveChangesAsync();
        TempData["Success"] = "Branch updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var branchId = Guid.Parse(_enc.Decrypt(id));
        var branch   = await _db.Branches.FindAsync(branchId);
        if (branch == null || branch.IsDeleted) return NotFound();
        branch.SoftDelete();
        await _db.SaveChangesAsync();
        TempData["Success"] = "Branch deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
