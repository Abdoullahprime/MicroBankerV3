// src/BaseTemplate.Web/Controllers/PermissionsController.cs
using BaseTemplate.Domain.Entities.Auth;
using BaseTemplate.Infrastructure.Persistence;
using BaseTemplate.Web.Models;
using BaseTemplate.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BaseTemplate.Web.Controllers;

[RequirePermission(Modules.Permissions)]
public class PermissionsController : BaseController
{
    private readonly AppDbContext      _db;
    private readonly IEncryptionService _enc;

    public PermissionsController(AppDbContext db, IEncryptionService enc)
    {
        _db  = db;
        _enc = enc;
    }

    // ── GET /Permissions?roleId={encrypted} ───────────────────────
    public async Task<IActionResult> Index(string? roleId = null)
    {
        var orgId = GetCompanyId();

        var roles = await _db.Roles
            .Where(r => r.OrganizationId == orgId && r.IsActive && !r.IsDeleted)
            .OrderBy(r => r.Name)
            .ToListAsync();

        if (!roles.Any())
        {
            TempData["Info"] = "No roles found. Please create roles first.";
            return RedirectToAction("Index", "Roles");
        }

        // Default to first role if none selected
        Role selectedRole;
        if (!string.IsNullOrEmpty(roleId))
            selectedRole = await _db.Roles.FindAsync(Guid.Parse(_enc.Decrypt(roleId))) ?? roles.First();
        else
            selectedRole = roles.First();

        var modules = await _db.AppModules
            .Where(m => m.IsActive && !m.IsDeleted)
            .OrderBy(m => m.SortOrder)
            .ToListAsync();

        var actions = await _db.AppActions
            .Where(a => a.IsActive && !a.IsDeleted)
            .OrderBy(a => a.SortOrder)
            .ToListAsync();

        var granted = await _db.RolePermissions
            .Where(rp => rp.RoleId == selectedRole.Id && rp.IsGranted && !rp.IsDeleted)
            .Select(rp => new { rp.AppModuleId, rp.AppActionId, rp.Id })
            .ToListAsync();

        // All unique action codes across all modules
        var allActionCodes = actions.Select(a => a.Code).Distinct().OrderBy(c => c).ToList();

        var moduleRows = modules.Select(m =>
        {
            var row = new ModulePermissionRow
            {
                ModuleId   = m.Id,
                ModuleName = m.Name,
                ModuleCode = m.Code,
                Group      = m.Group
            };

            var moduleActions = actions.Where(a => a.AppModuleId == m.Id);
            foreach (var a in moduleActions)
            {
                var perm = granted.FirstOrDefault(g => g.AppModuleId == m.Id && g.AppActionId == a.Id);
                row.ActionMap[a.Code] = (a.Id, perm != null);
            }

            return row;
        }).ToList();

        var vm = new PermissionViewModel
        {
            EncryptedRoleId = _enc.Encrypt(selectedRole.Id.ToString()),
            RoleName        = selectedRole.Name,
            IsSystemRole    = selectedRole.IsSystemRole,
            Modules         = moduleRows,
            Actions         = allActionCodes
        };

        ViewData["AllRoles"]    = roles.Select(r => new { Id = _enc.Encrypt(r.Id.ToString()), r.Name }).ToList();
        ViewData["SelectedRole"]= _enc.Encrypt(selectedRole.Id.ToString());
        ViewData["ActivePage"]  = "Permissions";
        ViewData["ActiveGroup"] = "Administration";
        return View(vm);
    }

    // ── POST /Permissions/Save ─────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(SavePermissionsModel model)
    {
        var orgId  = GetCompanyId();
        var roleId = Guid.Parse(_enc.Decrypt(model.EncryptedRoleId));
        var role   = await _db.Roles.FindAsync(roleId);
        if (role == null || role.OrganizationId != orgId) return NotFound();

        // Remove existing permissions for this role
        var existing = await _db.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync();
        _db.RolePermissions.RemoveRange(existing);

        // Add newly checked permissions
        foreach (var entry in model.Granted)
        {
            var parts = entry.Split('|');
            if (parts.Length != 2) continue;
            if (!Guid.TryParse(parts[0], out var moduleId)) continue;
            if (!Guid.TryParse(parts[1], out var actionId)) continue;

            _db.RolePermissions.Add(new RolePermission(roleId, moduleId, actionId));
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = $"Permissions saved for role '{role.Name}'.";
        return RedirectToAction(nameof(Index), new { roleId = model.EncryptedRoleId });
    }
}
