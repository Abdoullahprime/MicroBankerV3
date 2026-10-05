// src/BaseTemplate.Web/Controllers/UsersController.cs
using BaseTemplate.Domain.Entities.Auth;
using BaseTemplate.Infrastructure.Persistence;
using BaseTemplate.Web.Models;
using BaseTemplate.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BaseTemplate.Web.Controllers;

[RequirePermission(Modules.Users)]
public class UsersController : BaseController
{
    private readonly AppDbContext     _db;
    private readonly IEncryptionService _enc;

    public UsersController(AppDbContext db, IEncryptionService enc)
    {
        _db  = db;
        _enc = enc;
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
