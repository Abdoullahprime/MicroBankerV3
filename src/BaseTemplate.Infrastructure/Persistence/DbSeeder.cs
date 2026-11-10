// src/BaseTemplate.Infrastructure/Persistence/DbSeeder.cs
using BaseTemplate.Domain.Entities.Auth;
using BaseTemplate.Domain.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BaseTemplate.Infrastructure.Persistence;

/// <summary>
/// Applies migrations, then seeds the permission modules/actions, default roles and the
/// first SuperAdmin user. Safe to call on every startup — every step is idempotent.
/// Admin credentials come from the "Seed" section of appsettings (change them per project).
/// ExecuteSqlAsync auto-parameterises every {hole} — never add surrounding quotes.
/// </summary>
public static class DbSeeder
{
    private static readonly Guid SeedOrgId      = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid RoleSuperAdmin = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid RoleAdmin      = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
    private static readonly Guid RoleUser       = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
    private static readonly Guid AdminUserId    = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");

    private static readonly string[] ActionCodes =
        { PermissionActions.View, PermissionActions.Create, PermissionActions.Edit,
          PermissionActions.Delete, PermissionActions.Approve, PermissionActions.Export };
    private static readonly string[] ActionNames = { "View", "Create", "Edit", "Delete", "Approve", "Export" };

    // Keep in sync with Modules.cs and the sidebar in _Layout.cshtml.
    private static readonly (string Code, string Name, string Icon, string Group, int Sort)[] ModuleSeed =
    {
        (Modules.Dashboard,      "Dashboard",        "fas fa-tachometer-alt", "Main",           1),
        (Modules.Organization,   "Organization",     "fas fa-building",       "Setup",         10),
        (Modules.Branches,       "Branches",         "fas fa-code-branch",    "Setup",         11),
        (Modules.Departments,    "Departments",      "fas fa-sitemap",        "Setup",         12),
        (Modules.Units,          "Units",            "fas fa-layer-group",    "Setup",         13),
        (Modules.Positions,      "Positions",        "fas fa-user-tag",       "Setup",         14),
        (Modules.Banks,          "Banks",            "fas fa-university",     "Setup",         15),
        (Modules.WorkCalendar,   "Work Calendar",    "fas fa-calendar-alt",   "Calendar",      20),
        (Modules.PublicHolidays, "Public Holidays",  "fas fa-flag",           "Calendar",      21),
        (Modules.Users,          "Users",            "fas fa-user-cog",       "Administration", 60),
        (Modules.Roles,          "Roles",            "fas fa-shield-alt",     "Administration", 61),
        (Modules.Permissions,    "Permissions",      "fas fa-key",            "Administration", 62),
        (Modules.EmailConfig,    "Email Config",     "fas fa-envelope",       "Administration", 63),
        (Modules.SystemSettings, "System Settings",  "fas fa-cogs",           "Administration", 64),
    };

    // ── Entry point ───────────────────────────────────────────────────────────

    public static async Task SeedAsync(AppDbContext db, IConfiguration config)
    {
        await db.Database.MigrateAsync();

        var orgId = await ResolveOrgIdAsync(db);

        await SeedModulesAndActionsAsync(db);
        await SeedRolesAsync(db, orgId);
        await SeedSuperAdminUserAsync(db, orgId, config);
        await SeedSuperAdminPermissionsAsync(db);
    }

    // ── Organization ──────────────────────────────────────────────────────────

    private static async Task<Guid> ResolveOrgIdAsync(AppDbContext db)
    {
        var existing = await db.Organizations
            .Where(o => !o.IsDeleted)
            .Select(o => (Guid?)o.Id)
            .FirstOrDefaultAsync();

        if (existing.HasValue)
            return existing.Value;

        // Fresh install — insert a placeholder; the admin fills in details via Organization setup.
        var now = DateTime.UtcNow;
        await db.Database.ExecuteSqlAsync(
            $"INSERT INTO Organizations (Id, Name, Code, IsActive, CreatedAt, IsDeleted) VALUES ({SeedOrgId}, {"My Organization"}, {"ORG"}, {true}, {now}, {false})");

        return SeedOrgId;
    }

    // ── Modules + Actions ─────────────────────────────────────────────────────

    private static async Task SeedModulesAndActionsAsync(AppDbContext db)
    {
        foreach (var (code, name, icon, group, sort) in ModuleSeed)
        {
            if (await db.AppModules.AnyAsync(m => m.Code == code))
                continue;

            var module = new AppModule(name, code, null, icon, group, sort);
            db.AppModules.Add(module);
            await db.SaveChangesAsync();

            for (int i = 0; i < ActionCodes.Length; i++)
                db.AppActions.Add(new AppAction(module.Id, ActionNames[i], ActionCodes[i], i + 1));

            await db.SaveChangesAsync();
        }
    }

    // ── Roles ─────────────────────────────────────────────────────────────────

    private static async Task SeedRolesAsync(AppDbContext db, Guid orgId)
    {
        var roles = new[]
        {
            (RoleSuperAdmin, "Super Administrator", SystemRoles.SuperAdmin, "Full system access (bypasses permission checks)", true),
            (RoleAdmin,      "Administrator",       "ADMIN",                "Administrative access",                           false),
            (RoleUser,       "User",                "USER",                 "Standard user — grant permissions as needed",     false),
        };

        foreach (var (id, name, code, desc, isSys) in roles)
        {
            if (await db.Roles.AnyAsync(r => r.Id == id))
                continue;

            var now = DateTime.UtcNow;
            await db.Database.ExecuteSqlAsync(
                $"INSERT INTO Roles (Id, OrganizationId, Name, Code, Description, IsSystemRole, IsActive, CreatedAt, IsDeleted) VALUES ({id}, {orgId}, {name}, {code}, {desc}, {isSys}, {true}, {now}, {false})");
        }
    }

    // ── SuperAdmin user ───────────────────────────────────────────────────────

    private static async Task SeedSuperAdminUserAsync(AppDbContext db, Guid orgId, IConfiguration config)
    {
        if (await db.AppUsers.AnyAsync(u => u.Id == AdminUserId))
        {
            // Re-link to the current org (handles a deleted-and-recreated organization).
            await db.Database.ExecuteSqlAsync(
                $"UPDATE AppUsers SET OrganizationId = {orgId} WHERE Id = {AdminUserId}");
            return;
        }

        var username = config["Seed:AdminUsername"] ?? "admin";
        var email    = config["Seed:AdminEmail"]    ?? "admin@example.com";
        var password = config["Seed:AdminPassword"] ?? "Admin@1234";

        var hash  = BCrypt.Net.BCrypt.HashPassword(password);
        var now   = DateTime.UtcNow;
        var fname = "System";
        var lname = "Administrator";

        // MustChangePassword = true forces a new password at first sign-in.
        await db.Database.ExecuteSqlAsync(
            $"INSERT INTO AppUsers (Id, OrganizationId, Username, Email, PasswordHash, FirstName, LastName, IsActive, MustChangePassword, CreatedAt, IsDeleted) VALUES ({AdminUserId}, {orgId}, {username}, {email}, {hash}, {fname}, {lname}, {true}, {true}, {now}, {false})");

        var userRoleId = Guid.NewGuid();
        await db.Database.ExecuteSqlAsync(
            $"INSERT INTO UserRoles (Id, UserId, RoleId, CreatedAt, IsDeleted) VALUES ({userRoleId}, {AdminUserId}, {RoleSuperAdmin}, {now}, {false})");
    }

    // ── SuperAdmin permissions (kept explicit so the matrix UI shows them ticked) ──

    private static async Task SeedSuperAdminPermissionsAsync(AppDbContext db)
    {
        var modules = await db.AppModules.Where(m => m.IsActive).ToListAsync();
        var actions = await db.AppActions.Where(a => a.IsActive).ToListAsync();

        foreach (var module in modules)
        foreach (var action in actions.Where(a => a.AppModuleId == module.Id))
        {
            bool exists = await db.RolePermissions.AnyAsync(rp =>
                rp.RoleId      == RoleSuperAdmin &&
                rp.AppModuleId == module.Id      &&
                rp.AppActionId == action.Id);

            if (!exists)
                db.RolePermissions.Add(new RolePermission(RoleSuperAdmin, module.Id, action.Id));
        }

        await db.SaveChangesAsync();
    }
}
