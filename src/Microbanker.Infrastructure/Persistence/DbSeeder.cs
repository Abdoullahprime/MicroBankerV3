// src/Microbanker.Infrastructure/Persistence/DbSeeder.cs
using Microbanker.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace Microbanker.Infrastructure.Persistence;

/// <summary>
/// Seeds default roles, modules, actions, and SuperAdmin user.
/// Safe to call on every startup — all checks are idempotent.
/// ExecuteSqlAsync auto-parameterises every {hole} — never add surrounding quotes.
/// </summary>
public static class DbSeeder
{
    private static readonly Guid SeedOrgId      = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid RoleSuperAdmin = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid RoleAdmin      = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
    private static readonly Guid AdminUserId    = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");

    private static readonly string[] ActionCodes = { "VIEW", "CREATE", "EDIT", "DELETE", "APPROVE", "EXPORT" };
    private static readonly string[] ActionNames = { "View", "Create", "Edit", "Delete", "Approve", "Export" };

    private static readonly (string Code, string Name, string Icon, string Group, int Sort)[] Modules =
    {
        ("DASHBOARD",    "Dashboard",    "fas fa-tachometer-alt", "Main",           1),
        ("ORGANIZATION", "Organization", "fas fa-building",       "Setup",         10),
        ("BRANCHES",     "Branches",     "fas fa-code-branch",    "Setup",         11),
        ("DEPARTMENTS",  "Departments",  "fas fa-sitemap",        "Setup",         12),
        ("UNITS",        "Units",        "fas fa-layer-group",    "Setup",         13),
        ("BANKS",        "Banks",        "fas fa-university",     "Setup",         14),
        ("REGIONS",             "Regions",              "fas fa-globe-africa",   "Setup", 20),
        ("DISTRICTS",           "Districts",            "fas fa-map",            "Setup", 21),
        ("AREAS",               "Areas",                "fas fa-map-marker-alt", "Setup", 22),
        ("SECTORS",             "Sectors",              "fas fa-industry",       "Setup", 23),
        ("SUB_SECTORS",         "Sub-Sectors",          "fas fa-industry",       "Setup", 24),
        ("PRODUCT_TYPES",       "Product Types",        "fas fa-tags",           "Setup", 25),
        ("PRODUCT_DEFINITIONS", "Product Definitions",  "fas fa-box-open",       "Setup", 26),
        ("TRANSACTION_CODES",   "Transaction Codes",    "fas fa-exchange-alt",   "Setup", 27),
        ("CHART_OF_ACCOUNTS",   "Chart of Accounts",    "fas fa-book",           "Setup", 28),
        ("CUSTOMERS",           "Customers",            "fas fa-users",          "Banking", 30),
        ("SELF_REG",            "Self Registration",    "fas fa-user-plus",      "Banking", 31),
        ("ACCOUNTS",            "Accounts",             "fas fa-piggy-bank",     "Banking", 32),
        ("BAL_ENQUIRY",         "Balance Enquiry",      "fas fa-search-dollar",  "Banking", 33),
        ("LOANS",               "Loans",                "fas fa-hand-holding-usd",   "Lending", 40),
        ("LOAN_REQ",            "Loan Requests",        "fas fa-file-invoice-dollar","Lending", 41),
        ("USERS",        "Users",        "fas fa-user-cog",       "Administration", 60),
        ("ROLES",        "Roles",        "fas fa-shield-alt",     "Administration", 61),
        ("PERMISSIONS",  "Permissions",  "fas fa-key",            "Administration", 62),
    };

    // ── Entry point ───────────────────────────────────────────────────────────

    public static async Task SeedAsync(MicrobankerDbContext db)
    {
        await db.Database.MigrateAsync();

        var orgId = await ResolveOrgIdAsync(db);

        await SeedModulesAndActionsAsync(db);
        await SeedRolesAsync(db, orgId);
        await SeedSuperAdminUserAsync(db, orgId);
        await SeedSuperAdminPermissionsAsync(db);
    }

    // ── Organisation ──────────────────────────────────────────────────────────

    private static async Task<Guid> ResolveOrgIdAsync(MicrobankerDbContext db)
    {
        var existing = await db.Organizations
            .Where(o => !o.IsDeleted)
            .Select(o => (Guid?)o.Id)
            .FirstOrDefaultAsync();

        if (existing.HasValue)
            return existing.Value;

        // Fresh install — insert placeholder; admin fills in details via Organisation Setup
        var now = DateTime.UtcNow;
        await db.Database.ExecuteSqlAsync(
            $"INSERT INTO Organizations (Id, Name, Code, IsActive, CreatedAt, IsDeleted) VALUES ({SeedOrgId}, {"My Organisation"}, {"ORG"}, {true}, {now}, {false})");

        return SeedOrgId;
    }

    // ── Modules + Actions ─────────────────────────────────────────────────────

    private static async Task SeedModulesAndActionsAsync(MicrobankerDbContext db)
    {
        foreach (var (code, name, icon, group, sort) in Modules)
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

    private static async Task SeedRolesAsync(MicrobankerDbContext db, Guid orgId)
    {
        var roles = new[]
        {
            (RoleSuperAdmin, "Super Administrator", "SUPER_ADMIN", "Full system access",    true),
            (RoleAdmin,      "Administrator",       "ADMIN",       "Administrative access", false),
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

    private static async Task SeedSuperAdminUserAsync(MicrobankerDbContext db, Guid orgId)
    {
        if (await db.AppUsers.AnyAsync(u => u.Id == AdminUserId))
        {
            // Re-link to current org (handles deleted-and-recreated DB)
            await db.Database.ExecuteSqlAsync(
                $"UPDATE AppUsers SET OrganizationId = {orgId} WHERE Id = {AdminUserId}");
            return;
        }

        var hash     = BCrypt.Net.BCrypt.HashPassword("Admin@1234");
        var now      = DateTime.UtcNow;
        var username = "admin";
        var email    = "admin@microbanker.local";
        var fname    = "System";
        var lname    = "Administrator";

        await db.Database.ExecuteSqlAsync(
            $"INSERT INTO AppUsers (Id, OrganizationId, Username, Email, PasswordHash, FirstName, LastName, IsActive, MustChangePassword, CreatedAt, IsDeleted) VALUES ({AdminUserId}, {orgId}, {username}, {email}, {hash}, {fname}, {lname}, {true}, {true}, {now}, {false})");

        var userRoleId = Guid.NewGuid();
        await db.Database.ExecuteSqlAsync(
            $"INSERT INTO UserRoles (Id, UserId, RoleId, CreatedAt, IsDeleted) VALUES ({userRoleId}, {AdminUserId}, {RoleSuperAdmin}, {now}, {false})");
    }

    // ── SuperAdmin permissions ────────────────────────────────────────────────

    private static async Task SeedSuperAdminPermissionsAsync(MicrobankerDbContext db)
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
