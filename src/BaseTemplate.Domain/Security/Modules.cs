// src/BaseTemplate.Domain/Security/Modules.cs
namespace BaseTemplate.Domain.Security;

/// <summary>
/// Permission module codes. Each one maps to a row in AppModules (seeded by DbSeeder)
/// and is used by [RequirePermission]. When you add a secured feature: add a constant here,
/// a seed row in DbSeeder, and a sidebar entry in _Layout.
/// </summary>
public static class Modules
{
    public const string Dashboard      = "DASHBOARD";
    public const string Organization   = "ORGANIZATION";
    public const string Branches       = "BRANCHES";
    public const string Departments    = "DEPARTMENTS";
    public const string Units          = "UNITS";
    public const string Positions      = "POSITIONS";
    public const string Banks          = "BANKS";
    public const string WorkCalendar   = "WORK_CALENDAR";
    public const string PublicHolidays = "PUBLIC_HOLIDAYS";
    public const string Users          = "USERS";
    public const string Roles          = "ROLES";
    public const string Permissions    = "PERMISSIONS";
    public const string EmailConfig    = "EMAIL_CONFIG";
    public const string SystemSettings = "SYSTEM_SETTINGS";
}

/// <summary>Permission action codes (columns of the permission matrix).</summary>
public static class PermissionActions
{
    public const string View    = "VIEW";
    public const string Create  = "CREATE";
    public const string Edit    = "EDIT";
    public const string Delete  = "DELETE";
    public const string Approve = "APPROVE";
    public const string Export  = "EXPORT";
}

/// <summary>Code of the built-in role that bypasses all permission checks.</summary>
public static class SystemRoles
{
    public const string SuperAdmin = "SUPER_ADMIN";
}
