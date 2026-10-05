// src/BaseTemplate.Web/Services/IPermissionService.cs
using System.Security.Claims;
using BaseTemplate.Domain.Security;
using BaseTemplate.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BaseTemplate.Web.Services;

/// <summary>Granted permissions of the signed-in user. Loaded once per request.</summary>
public class PermissionSet
{
    private readonly HashSet<string> _granted;
    public bool IsSuperAdmin { get; }

    public PermissionSet(bool isSuperAdmin, HashSet<string> granted)
    {
        IsSuperAdmin = isSuperAdmin;
        _granted = granted;
    }

    public bool Can(string module, string action = PermissionActions.View)
        => IsSuperAdmin || _granted.Contains($"{module}:{action}");

    /// <summary>True when the user may VIEW at least one of the modules (used for sidebar groups).</summary>
    public bool CanAny(params string[] modules) => modules.Any(m => Can(m));
}

public interface IPermissionService
{
    Task<PermissionSet> GetCurrentAsync();
}

public class PermissionService : IPermissionService
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _http;
    private PermissionSet? _cached;

    public PermissionService(AppDbContext db, IHttpContextAccessor http)
    {
        _db   = db;
        _http = http;
    }

    public async Task<PermissionSet> GetCurrentAsync()
    {
        if (_cached != null) return _cached;

        var user = _http.HttpContext?.User;
        var userIdClaim = user?.FindFirstValue("userId");
        if (user?.Identity?.IsAuthenticated != true || !Guid.TryParse(userIdClaim, out var userId))
            return _cached = new PermissionSet(false, new HashSet<string>());

        // Union of every role assigned to the user.
        var roleIds = await _db.UserRoles
            .Where(ur => ur.UserId == userId && !ur.IsDeleted)
            .Select(ur => ur.RoleId)
            .ToListAsync();

        var isSuper = await _db.Roles.AnyAsync(r =>
            roleIds.Contains(r.Id) && r.Code == SystemRoles.SuperAdmin && r.IsActive && !r.IsDeleted);
        if (isSuper)
            return _cached = new PermissionSet(true, new HashSet<string>());

        var granted = await _db.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId) && rp.IsGranted && !rp.IsDeleted)
            .Select(rp => rp.AppModule.Code + ":" + rp.AppAction.Code)
            .ToListAsync();

        return _cached = new PermissionSet(false, granted.ToHashSet());
    }
}
