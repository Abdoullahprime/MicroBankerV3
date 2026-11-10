// src/Microbanker.Web/Models/AuthViewModels.cs
using System.ComponentModel.DataAnnotations;

namespace Microbanker.Web.Models;

// ── Users ──────────────────────────────────────────────────────────────────

public class UserListViewModel
{
    public List<UserRowModel> Users { get; set; } = new();
}

public class UserRowModel
{
    public string EncryptedId { get; set; } = "";
    public string Username    { get; set; } = "";
    public string FullName    { get; set; } = "";
    public string Email       { get; set; } = "";
    public string RoleName    { get; set; } = "";
    public bool   IsActive    { get; set; }
    public bool   MustChangePassword { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public class UserFormModel
{
    public string? EncryptedId { get; set; }

    [Required] [StringLength(100)]
    public string Username  { get; set; } = "";

    [Required] [EmailAddress] [StringLength(200)]
    public string Email     { get; set; } = "";

    [Required] [StringLength(100)]
    public string FirstName { get; set; } = "";

    [Required] [StringLength(100)]
    public string LastName  { get; set; } = "";

    [StringLength(50)]
    public string? Phone    { get; set; }

    // Password only required on Create
    [StringLength(100, MinimumLength = 8)]
    public string? Password { get; set; }

    public bool IsActive    { get; set; } = true;

    public Guid? RoleId     { get; set; }

    // For dropdown
    public List<RoleDropdownItem> AvailableRoles { get; set; } = new();
}

public class RoleDropdownItem
{
    public Guid   Id   { get; set; }
    public string Name { get; set; } = "";
}

// ── Roles ──────────────────────────────────────────────────────────────────

public class RoleListViewModel
{
    public List<RoleRowModel> Roles { get; set; } = new();
}

public class RoleRowModel
{
    public string EncryptedId  { get; set; } = "";
    public string Name         { get; set; } = "";
    public string Code         { get; set; } = "";
    public string? Description { get; set; }
    public bool   IsSystemRole { get; set; }
    public bool   IsActive     { get; set; }
    public int    UserCount    { get; set; }
}

public class RoleFormModel
{
    public string? EncryptedId { get; set; }

    [Required] [StringLength(100)]
    public string Name { get; set; } = "";

    [Required] [StringLength(50)]
    public string Code { get; set; } = "";

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}

// ── Permissions ─────────────────────────────────────────────────────────────

public class PermissionViewModel
{
    public string  EncryptedRoleId { get; set; } = "";
    public string  RoleName        { get; set; } = "";
    public bool    IsSystemRole    { get; set; }

    public List<ModulePermissionRow> Modules { get; set; } = new();
    public List<string>              Actions { get; set; } = new();
}

public class ModulePermissionRow
{
    public Guid   ModuleId   { get; set; }
    public string ModuleName { get; set; } = "";
    public string ModuleCode { get; set; } = "";
    public string? Group     { get; set; }

    /// <summary>ActionCode → (ActionId, IsGranted)</summary>
    public Dictionary<string, (Guid ActionId, bool IsGranted)> ActionMap { get; set; } = new();
}

public class SavePermissionsModel
{
    public string EncryptedRoleId { get; set; } = "";
    /// <summary>Flat list of "moduleId|actionId" strings that ARE checked.</summary>
    public List<string> Granted { get; set; } = new();
}
