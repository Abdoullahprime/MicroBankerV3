// src/Microbanker.Web/Controllers/PermissionsController.cs
using Microbanker.Domain.Entities.Auth;
using Microbanker.Infrastructure.Persistence;
using Microbanker.Web.Models;
using Microbanker.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microbanker.Domain.Security;
using Microbanker.Web.Security;

namespace Microbanker.Web.Controllers;

[RequirePermission(Modules.Permissions)]
public class PermissionsController : BaseController
{
    private readonly MicrobankerDbContext      _db;
    private readonly IEncryptionService _enc;
    private readonly IPermissionService _perms;

    public PermissionsController(MicrobankerDbContext db, IEncryptionService enc, IPermissionService perms)
    {
        _db    = db;
        _enc   = enc;
        _perms = perms;
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
        var orgId = GetCompanyId();
        Guid roleId;
        try { roleId = Guid.Parse(_enc.Decrypt(model.EncryptedRoleId)); }
        catch (Exception) { return NotFound(); }
        var role = await _db.Roles.FindAsync(roleId);
        if (role == null || role.OrganizationId != orgId) return NotFound();

        // Read what was ticked: only real (module, action) pairs, where the action belongs to the module.
        var submitted = new HashSet<(Guid Module, Guid Action)>();
        foreach (var entry in model.Granted)
        {
            var parts = entry.Split('|');
            if (parts.Length != 2 || !Guid.TryParse(parts[0], out var moduleId) || !Guid.TryParse(parts[1], out var actionId)) continue;
            submitted.Add((moduleId, actionId));
        }
        var known = await _db.AppActions.Where(a => a.IsActive).Select(a => new { a.Id, a.AppModuleId, ActionCode = a.Code, ModuleCode = a.AppModule.Code }).ToListAsync();
        var valid = known.Where(k => submitted.Contains((k.AppModuleId, k.Id))).ToList();

        var existing = await _db.RolePermissions.Where(rp => rp.RoleId == roleId).ToListAsync();

        // Nobody can hand out a permission they do not hold themselves. (Super Administrators hold them all.)
        // Only newly added grants are checked, so removing a permission from a role is always allowed.
        var me = await _perms.GetCurrentAsync();
        if (!me.IsSuperAdmin)
        {
            var already = existing.Where(e => e.IsGranted && !e.IsDeleted).Select(e => (e.AppModuleId, e.AppActionId)).ToHashSet();
            var notHeld = valid.Where(v => !already.Contains((v.AppModuleId, v.Id)) && !me.Can(v.ModuleCode, v.ActionCode)).ToList();
            if (notHeld.Count > 0)
            {
                TempData["Error"] = "You cannot grant permissions you do not hold yourself: "
                    + string.Join(", ", notHeld.Take(5).Select(n => $"{n.ModuleCode} {n.ActionCode}")) + (notHeld.Count > 5 ? ", …" : "") + ".";
                return RedirectToAction(nameof(Index), new { roleId = model.EncryptedRoleId });
            }
        }

        _db.RolePermissions.RemoveRange(existing);
        foreach (var v in valid)
            _db.RolePermissions.Add(new RolePermission(roleId, v.AppModuleId, v.Id));

        await _db.SaveChangesAsync();
        TempData["Success"] = $"Permissions saved for role '{role.Name}'.";
        return RedirectToAction(nameof(Index), new { roleId = model.EncryptedRoleId });
    }
}
