// src/Microbanker.Domain/Entities/Auth/Role.cs
using Microbanker.Domain.Entities;

namespace Microbanker.Domain.Entities.Auth;

public class Role : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsSystemRole { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Role() { }

    public Role(Guid organizationId, string name, string code, string? description = null, bool isSystemRole = false)
    {
        OrganizationId = organizationId;
        Name = name.Trim();
        Code = code.Trim().ToUpper();
        Description = description;
        IsSystemRole = isSystemRole;
    }

    public void Update(string name, string? description, bool isActive)
    {
        Name = name.Trim();
        Description = description;
        IsActive = isActive;
        SetUpdatedAt();
    }

    public void SoftDelete() => SetDeleted();
}
