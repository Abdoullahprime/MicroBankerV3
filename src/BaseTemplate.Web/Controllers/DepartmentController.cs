// src/BaseTemplate.Web/Controllers/DepartmentController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BaseTemplate.Domain.Entities.Organization;
using BaseTemplate.Infrastructure.Persistence;
using BaseTemplate.Web.Services;
using BaseTemplate.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BaseTemplate.Web.Controllers;

[RequirePermission(Modules.Departments)]
public class DepartmentController : BaseController
{
    private readonly AppDbContext _db;
    private readonly IEncryptionService _enc;

    public DepartmentController(AppDbContext db, IEncryptionService enc)
    {
        _db = db;
        _enc = enc;
    }

    public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
    {
        base.OnActionExecuting(context);
        ViewData["ActiveGroup"] = "Org";
        ViewData["ActivePage"]  = "Departments";
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
            Name        = dept.Name,
            Code        = dept.Code,
            Description = dept.Description,
            IsActive    = dept.IsActive,
            EncryptedId = id
        };
        await PopulateBranchesAsync(model, org.Id);
        // Set selected branch from the options list (same encrypted value, avoids IV mismatch)
        var branchOpt = model.BranchOptions.FirstOrDefault(o =>
            Guid.Parse(_enc.Decrypt(o.Value)) == dept.BranchId);
        if (branchOpt != null) model.EncryptedBranchId = branchOpt.Value;
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
