// src/Microbanker.Web/Controllers/RolesController.cs
using Microbanker.Domain.Entities.Auth;
using Microbanker.Infrastructure.Persistence;
using Microbanker.Web.Models;
using Microbanker.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Microbanker.Web.Controllers;

public class RolesController : BaseController
{
    private readonly MicrobankerDbContext      _db;
    private readonly IEncryptionService _enc;

    public RolesController(MicrobankerDbContext db, IEncryptionService enc)
    {
        _db  = db;
        _enc = enc;
    }

    public async Task<IActionResult> Index()
    {
        var orgId = GetCompanyId();

        var roles = await _db.Roles
            .Where(r => r.OrganizationId == orgId && !r.IsDeleted)
            .OrderBy(r => r.Name)
            .ToListAsync();

        var userCounts = await _db.UserRoles
            .Where(ur => !ur.IsDeleted)
            .GroupBy(ur => ur.RoleId)
            .Select(g => new { RoleId = g.Key, Count = g.Count() })
            .ToListAsync();

        var vm = new RoleListViewModel
        {
            Roles = roles.Select(r => new RoleRowModel
            {
                EncryptedId = _enc.Encrypt(r.Id.ToString()),
                Name        = r.Name,
                Code        = r.Code,
                Description = r.Description,
                IsSystemRole = r.IsSystemRole,
                IsActive    = r.IsActive,
                UserCount   = userCounts.FirstOrDefault(u => u.RoleId == r.Id)?.Count ?? 0
            }).ToList()
        };

        ViewData["ActivePage"]  = "Roles";
        ViewData["ActiveGroup"] = "Administration";
        return View(vm);
    }

    public IActionResult Create()
    {
        ViewData["ActivePage"]  = "Roles";
        ViewData["ActiveGroup"] = "Administration";
        return View(new RoleFormModel { IsActive = true });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RoleFormModel model)
    {
        var orgId = GetCompanyId();
        if (!ModelState.IsValid)
        {
            ViewData["ActivePage"]  = "Roles";
            ViewData["ActiveGroup"] = "Administration";
            return View(model);
        }

        var code = model.Code.Trim().ToUpper().Replace(" ", "_");
        if (await _db.Roles.AnyAsync(r => r.OrganizationId == orgId && r.Code == code && !r.IsDeleted))
        {
            ModelState.AddModelError("Code", "A role with this code already exists.");
            ViewData["ActivePage"]  = "Roles";
            ViewData["ActiveGroup"] = "Administration";
            return View(model);
        }

        var role = new Role(orgId, model.Name, code, model.Description);
        _db.Roles.Add(role);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Role '{role.Name}' created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string id)
    {
        var orgId  = GetCompanyId();
        var roleId = Guid.Parse(_enc.Decrypt(id));
        var role   = await _db.Roles.FindAsync(roleId);
        if (role == null || role.IsDeleted || role.OrganizationId != orgId) return NotFound();

        var model = new RoleFormModel
        {
            EncryptedId = id,
            Name        = role.Name,
            Code        = role.Code,
            Description = role.Description,
            IsActive    = role.IsActive
        };

        ViewData["ActivePage"]  = "Roles";
        ViewData["ActiveGroup"] = "Administration";
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, RoleFormModel model)
    {
        var orgId  = GetCompanyId();
        var roleId = Guid.Parse(_enc.Decrypt(id));
        var role   = await _db.Roles.FindAsync(roleId);
        if (role == null || role.IsDeleted || role.OrganizationId != orgId) return NotFound();

        if (!ModelState.IsValid)
        {
            model.EncryptedId = id;
            ViewData["ActivePage"]  = "Roles";
            ViewData["ActiveGroup"] = "Administration";
            return View(model);
        }

        role.Update(model.Name, model.Description, model.IsActive);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Role updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var orgId  = GetCompanyId();
        var roleId = Guid.Parse(_enc.Decrypt(id));
        var role   = await _db.Roles.FindAsync(roleId);
        if (role == null || role.OrganizationId != orgId) return NotFound();

        if (role.IsSystemRole)
        {
            TempData["Error"] = "System roles cannot be deleted.";
            return RedirectToAction(nameof(Index));
        }

        role.SoftDelete();
        await _db.SaveChangesAsync();
        TempData["Success"] = "Role deleted.";
        return RedirectToAction(nameof(Index));
    }
}
