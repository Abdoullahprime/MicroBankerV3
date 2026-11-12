// src/Microbanker.Web/Controllers/UnitController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microbanker.Domain.Entities.Organization;
using Microbanker.Infrastructure.Persistence;
using Microbanker.Web.Services;
using Microbanker.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrgUnit = Microbanker.Domain.Entities.Organization.Unit;
using Microbanker.Domain.Security;
using Microbanker.Web.Security;

namespace Microbanker.Web.Controllers;

[RequirePermission(Modules.Units)]
public class UnitController : BaseController
{
    private readonly MicrobankerDbContext _db;
    private readonly IEncryptionService _enc;

    public UnitController(MicrobankerDbContext db, IEncryptionService enc)
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

    private async Task PopulateDepartmentsAsync(UnitFormModel model, Guid orgId)
    {
        var departments = await _db.Departments
            .Include(d => d.Branch)
            .Where(d => d.Branch.OrganizationId == orgId && !d.IsDeleted && d.IsActive)
            .OrderBy(d => d.Branch.Name).ThenBy(d => d.Name)
            .ToListAsync();

        model.DepartmentOptions = departments.Select(d => new SelectListItem
        {
            Value = _enc.Encrypt(d.Id.ToString()),
            Text  = $"{d.Branch.Name} → {d.Name}"
        }).ToList();
    }

    public async Task<IActionResult> Index()
    {
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var units = await _db.Units
            .Include(u => u.Department).ThenInclude(d => d.Branch)
            .Where(u => u.Department.Branch.OrganizationId == org.Id && !u.IsDeleted)
            .OrderBy(u => u.Department.Branch.Name)
            .ThenBy(u => u.Department.Name)
            .ThenBy(u => u.Name)
            .ToListAsync();

        var vm = new UnitListViewModel
        {
            Units = units.Select(u => new UnitRowModel
            {
                Name           = u.Name,
                Code           = u.Code,
                DepartmentName = u.Department.Name,
                BranchName     = u.Department.Branch.Name,
                IsActive       = u.IsActive,
                EncryptedId    = _enc.Encrypt(u.Id.ToString())
            }).ToList()
        };
        return View(vm);
    }

    public async Task<IActionResult> Create()
    {
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        var model = new UnitFormModel();
        await PopulateDepartmentsAsync(model, org.Id);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UnitFormModel model)
    {
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        if (!ModelState.IsValid) { await PopulateDepartmentsAsync(model, org.Id); return View(model); }

        var deptId = Guid.Parse(_enc.Decrypt(model.EncryptedDepartmentId!));
        var unit   = new OrgUnit(deptId, model.Name);
        unit.Update(deptId, model.Name, model.Code, model.Description, model.IsActive);

        _db.Units.Add(unit);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Unit created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string id)
    {
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var unitId = Guid.Parse(_enc.Decrypt(id));
        var unit   = await _db.Units.FindAsync(unitId);
        if (unit == null || unit.IsDeleted) return NotFound();

        var model = new UnitFormModel
        {
            Name                  = unit.Name,
            Code                  = unit.Code,
            Description           = unit.Description,
            IsActive              = unit.IsActive,
            EncryptedDepartmentId = _enc.Encrypt(unit.DepartmentId.ToString()),
            EncryptedId           = id
        };
        await PopulateDepartmentsAsync(model, org.Id);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, UnitFormModel model)
    {
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        if (!ModelState.IsValid) { model.EncryptedId = id; await PopulateDepartmentsAsync(model, org.Id); return View(model); }

        var unitId = Guid.Parse(_enc.Decrypt(id));
        var unit   = await _db.Units.FindAsync(unitId);
        if (unit == null || unit.IsDeleted) return NotFound();

        var deptId = Guid.Parse(_enc.Decrypt(model.EncryptedDepartmentId!));
        unit.Update(deptId, model.Name, model.Code, model.Description, model.IsActive);

        await _db.SaveChangesAsync();
        TempData["Success"] = "Unit updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var unitId = Guid.Parse(_enc.Decrypt(id));
        var unit   = await _db.Units.FindAsync(unitId);
        if (unit == null || unit.IsDeleted) return NotFound();
        unit.SoftDelete();
        await _db.SaveChangesAsync();
        TempData["Success"] = "Unit deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
