// src/Microbanker.Web/Controllers/DepartmentController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microbanker.Domain.Entities.Organization;
using Microbanker.Infrastructure.Persistence;
using Microbanker.Web.Services;
using Microbanker.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Microbanker.Web.Controllers;

public class DepartmentController : BaseController
{
    private readonly MicrobankerDbContext _db;
    private readonly IEncryptionService _enc;

    public DepartmentController(MicrobankerDbContext db, IEncryptionService enc)
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

    private async Task PopulateBranchesAsync(DepartmentFormModel model, Guid orgId)
    {
        var branches = await _db.Branches
            .Where(b => b.OrganizationId == orgId && !b.IsDeleted && b.IsActive)
            .OrderBy(b => b.Name).ToListAsync();

        model.BranchOptions = branches.Select(b => new SelectListItem
        {
            Value = _enc.Encrypt(b.Id.ToString()),
            Text  = b.Name
        }).ToList();
    }

    public async Task<IActionResult> Index()
    {
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var departments = await _db.Departments
            .Include(d => d.Branch)
            .Where(d => d.Branch.OrganizationId == org.Id && !d.IsDeleted)
            .OrderBy(d => d.Branch.Name).ThenBy(d => d.Name)
            .ToListAsync();

        var vm = new DepartmentListViewModel
        {
            Departments = departments.Select(d => new DepartmentRowModel
            {
                Name        = d.Name,
                Code        = d.Code,
                BranchName  = d.Branch.Name,
                IsActive    = d.IsActive,
                EncryptedId = _enc.Encrypt(d.Id.ToString())
            }).ToList()
        };
        return View(vm);
    }

    public async Task<IActionResult> Create()
    {
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        var model = new DepartmentFormModel();
        await PopulateBranchesAsync(model, org.Id);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DepartmentFormModel model)
    {
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        if (!ModelState.IsValid) { await PopulateBranchesAsync(model, org.Id); return View(model); }

        var branchId = Guid.Parse(_enc.Decrypt(model.EncryptedBranchId!));
        var code = string.IsNullOrEmpty(model.Code) ? "DEPT" : model.Code;
        var dept = new Department(branchId, model.Name, code);
        dept.Update(branchId, model.Name, code, model.Description, null, model.IsActive);

        _db.Departments.Add(dept);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Department created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string id)
    {
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var deptId = Guid.Parse(_enc.Decrypt(id));
        var dept   = await _db.Departments.FindAsync(deptId);
        if (dept == null || dept.IsDeleted) return NotFound();

        var model = new DepartmentFormModel
        {
            Name              = dept.Name,
            Code              = dept.Code,
            Description       = dept.Description,
            IsActive          = dept.IsActive,
            EncryptedBranchId = _enc.Encrypt(dept.BranchId.ToString()),
            EncryptedId       = id
        };
        await PopulateBranchesAsync(model, org.Id);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, DepartmentFormModel model)
    {
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        if (!ModelState.IsValid) { model.EncryptedId = id; await PopulateBranchesAsync(model, org.Id); return View(model); }

        var deptId   = Guid.Parse(_enc.Decrypt(id));
        var dept     = await _db.Departments.FindAsync(deptId);
        if (dept == null || dept.IsDeleted) return NotFound();

        var branchId = Guid.Parse(_enc.Decrypt(model.EncryptedBranchId!));
        var code = string.IsNullOrEmpty(model.Code) ? dept.Code : model.Code;
        dept.Update(branchId, model.Name, code, model.Description, null, model.IsActive);

        await _db.SaveChangesAsync();
        TempData["Success"] = "Department updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var deptId = Guid.Parse(_enc.Decrypt(id));
        var dept   = await _db.Departments.FindAsync(deptId);
        if (dept == null || dept.IsDeleted) return NotFound();
        dept.SoftDelete();
        await _db.SaveChangesAsync();
        TempData["Success"] = "Department deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
