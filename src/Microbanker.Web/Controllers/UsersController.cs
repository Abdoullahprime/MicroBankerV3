// src/Microbanker.Web/Controllers/UsersController.cs
using Microbanker.Domain.Entities.Auth;
using Microbanker.Infrastructure.Persistence;
using Microbanker.Web.Models;
using Microbanker.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microbanker.Domain.Security;
using Microbanker.Web.Security;

namespace Microbanker.Web.Controllers;

[RequirePermission(Modules.Users)]
public class UsersController : BaseController
{
    private readonly MicrobankerDbContext     _db;
    private readonly IEncryptionService _enc;
    private readonly IPermissionService _perms;

    public UsersController(MicrobankerDbContext db, IEncryptionService enc, IPermissionService perms)
    {
        _db    = db;
        _enc   = enc;
        _perms = perms;
    }

    /// <summary>
    /// Why the signed-in user may not give <paramref name="roleId"/> to someone, or null if they may.
    /// Without this, anyone allowed to edit users could make themselves (or anyone) a Super Administrator.
    /// A Super Administrator may assign any role; everyone else may only assign an active role of this
    /// organisation whose permissions they hold themselves.
    /// </summary>
    private async Task<string?> RoleAssignmentProblemAsync(Guid? roleId, Guid orgId)
    {
        if (roleId == null) return null;

        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Id == roleId && r.OrganizationId == orgId && !r.IsDeleted && r.IsActive);
        if (role == null) return "Please select a valid role.";

        var me = await _perms.GetCurrentAsync();
        if (me.IsSuperAdmin) return null;

        if (role.Code == SystemRoles.SuperAdmin)
            return "Only a Super Administrator can assign the Super Administrator role.";

        var grants = await _db.RolePermissions
            .Where(rp => rp.RoleId == role.Id && rp.IsGranted && !rp.IsDeleted)
            .Select(rp => new { Module = rp.AppModule.Code, Action = rp.AppAction.Code }).ToListAsync();
        if (grants.Any(g => !me.Can(g.Module, g.Action)))
            return "That role grants permissions you do not hold yourself, so you cannot assign it.";
        return null;
    }

    /// <summary>True when the user is a Super Administrator and the signed-in user is not: they may not be changed or deleted.</summary>
    private async Task<bool> IsProtectedSuperAdminAsync(Guid userId)
    {
        var me = await _perms.GetCurrentAsync();
        if (me.IsSuperAdmin) return false;
        return await _db.UserRoles.AnyAsync(ur => ur.UserId == userId && !ur.IsDeleted
            && ur.Role.Code == SystemRoles.SuperAdmin && !ur.Role.IsDeleted);
    }

    public async Task<IActionResult> Index()
    {
        var orgId = GetCompanyId();

        var users = await _db.AppUsers
            .Where(u => u.OrganizationId == orgId && !u.IsDeleted)
            .OrderBy(u => u.FirstName)
            .ToListAsync();

        var userRoles = await _db.UserRoles
            .Include(ur => ur.Role)
            .Where(ur => !ur.IsDeleted)
            .ToListAsync();

        var vm = new UserListViewModel
        {
            Users = users.Select(u =>
            {
                var role = userRoles.FirstOrDefault(ur => ur.UserId == u.Id)?.Role;
                return new UserRowModel
                {
                    EncryptedId      = _enc.Encrypt(u.Id.ToString()),
                    Username         = u.Username,
                    FullName         = u.FullName,
                    Email            = u.Email,
                    RoleName         = role?.Name ?? "—",
                    IsActive         = u.IsActive,
                    MustChangePassword = u.MustChangePassword,
                    LastLoginAt      = u.LastLoginAt
                };
            }).ToList()
        };

        ViewData["ActivePage"]  = "Users";
        ViewData["ActiveGroup"] = "Administration";
        return View(vm);
    }

    public async Task<IActionResult> Create()
    {
        var orgId = GetCompanyId();
        ViewData["ActivePage"]  = "Users";
        ViewData["ActiveGroup"] = "Administration";
        return View(await BuildFormModel(orgId));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserFormModel model)
    {
        var orgId = GetCompanyId();

        if (string.IsNullOrWhiteSpace(model.Password))
            ModelState.AddModelError("Password", "Password is required for new users.");

        if (!ModelState.IsValid)
        {
            model.AvailableRoles = await GetRoleDropdown(orgId);
            ViewData["ActivePage"]  = "Users";
            ViewData["ActiveGroup"] = "Administration";
            return View(model);
        }

        if (await _db.AppUsers.AnyAsync(u => u.Username == model.Username.ToLower() && !u.IsDeleted))
        {
            ModelState.AddModelError("Username", "Username already exists.");
            model.AvailableRoles = await GetRoleDropdown(orgId);
            ViewData["ActivePage"]  = "Users";
            ViewData["ActiveGroup"] = "Administration";
            return View(model);
        }

        var roleProblem = await RoleAssignmentProblemAsync(model.RoleId, orgId);
        if (roleProblem != null)
        {
            ModelState.AddModelError("RoleId", roleProblem);
            model.AvailableRoles = await GetRoleDropdown(orgId);
            ViewData["ActivePage"]  = "Users";
            ViewData["ActiveGroup"] = "Administration";
            return View(model);
        }

        var hash = BCrypt.Net.BCrypt.HashPassword(model.Password!);
        var user = new AppUser(orgId, model.Username, model.Email, hash, model.FirstName, model.LastName);
        user.Update(model.Email, model.FirstName, model.LastName, model.Phone, model.IsActive);

        _db.AppUsers.Add(user);
        await _db.SaveChangesAsync();

        if (model.RoleId.HasValue)
            _db.UserRoles.Add(new UserRole(user.Id, model.RoleId.Value));

        await _db.SaveChangesAsync();
        TempData["Success"] = $"User '{user.FullName}' created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string id)
    {
        var orgId  = GetCompanyId();
        var userId = Guid.Parse(_enc.Decrypt(id));
        var user   = await _db.AppUsers.FindAsync(userId);
        if (user == null || user.IsDeleted || user.OrganizationId != orgId) return NotFound();

        var userRole = await _db.UserRoles.Where(ur => ur.UserId == userId && !ur.IsDeleted).FirstOrDefaultAsync();

        var model = new UserFormModel
        {
            EncryptedId     = id,
            Username        = user.Username,
            Email           = user.Email,
            FirstName       = user.FirstName,
            LastName        = user.LastName,
            Phone           = user.Phone,
            IsActive        = user.IsActive,
            RoleId          = userRole?.RoleId,
            AvailableRoles  = await GetRoleDropdown(orgId)
        };

        ViewData["ActivePage"]  = "Users";
        ViewData["ActiveGroup"] = "Administration";
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, UserFormModel model)
    {
        var orgId  = GetCompanyId();
        var userId = Guid.Parse(_enc.Decrypt(id));
        var user   = await _db.AppUsers.FindAsync(userId);
        if (user == null || user.IsDeleted || user.OrganizationId != orgId) return NotFound();

        if (await IsProtectedSuperAdminAsync(userId))
        {
            TempData["Error"] = "Only a Super Administrator can change a Super Administrator.";
            return RedirectToAction(nameof(Index));
        }

        var roleProblem = await RoleAssignmentProblemAsync(model.RoleId, orgId);
        if (roleProblem != null) ModelState.AddModelError("RoleId", roleProblem);

        if (!ModelState.IsValid)
        {
            model.EncryptedId    = id;
            model.AvailableRoles = await GetRoleDropdown(orgId);
            ViewData["ActivePage"]  = "Users";
            ViewData["ActiveGroup"] = "Administration";
            return View(model);
        }

        user.Update(model.Email, model.FirstName, model.LastName, model.Phone, model.IsActive);

        // Update password if provided
        if (!string.IsNullOrWhiteSpace(model.Password))
            user.SetPassword(BCrypt.Net.BCrypt.HashPassword(model.Password));

        // Update role
        var existing = await _db.UserRoles.Where(ur => ur.UserId == userId && !ur.IsDeleted).ToListAsync();
        _db.UserRoles.RemoveRange(existing);
        if (model.RoleId.HasValue)
            _db.UserRoles.Add(new UserRole(userId, model.RoleId.Value));

        await _db.SaveChangesAsync();
        TempData["Success"] = "User updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var orgId  = GetCompanyId();
        var userId = Guid.Parse(_enc.Decrypt(id));
        var user   = await _db.AppUsers.FindAsync(userId);
        if (user == null || user.OrganizationId != orgId) return NotFound();

        if (await IsProtectedSuperAdminAsync(userId))
        {
            TempData["Error"] = "Only a Super Administrator can delete a Super Administrator.";
            return RedirectToAction(nameof(Index));
        }

        // Prevent deleting yourself
        if (userId == GetUserId())
        {
            TempData["Error"] = "You cannot delete your own account.";
            return RedirectToAction(nameof(Index));
        }

        user.SoftDelete();
        await _db.SaveChangesAsync();
        TempData["Success"] = "User deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    // ── Helpers ────────────────────────────────────────────────────

    private async Task<UserFormModel> BuildFormModel(Guid orgId)
        => new() { AvailableRoles = await GetRoleDropdown(orgId), IsActive = true };

    private async Task<List<RoleDropdownItem>> GetRoleDropdown(Guid orgId)
        => await _db.Roles
            .Where(r => r.OrganizationId == orgId && r.IsActive && !r.IsDeleted)
            .OrderBy(r => r.Name)
            .Select(r => new RoleDropdownItem { Id = r.Id, Name = r.Name })
            .ToListAsync();
}
