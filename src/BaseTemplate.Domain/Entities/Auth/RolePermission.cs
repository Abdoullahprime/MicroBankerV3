// src/BaseTemplate.Domain/Entities/Auth/RolePermission.cs
using BaseTemplate.Domain.Entities;

namespace BaseTemplate.Domain.Entities.Auth;

/// <summary>Grants a Role permission to perform an Action on a Module.</summary>
public class RolePermission : BaseEntity
{
    public Guid RoleId { get; private set; }
    public Guid AppModuleId { get; private set; }
    public Guid AppActionId { get; private set; }
    public bool IsGranted { get; private set; } = true;

    // Navigation
    public Role Role { get; private set; } = null!;
    public AppModule AppModule { get; private set; } = null!;
    public AppAction AppAction { get; private set; } = null!;

    private RolePermission() { }

    public RolePermission(Guid roleId, Guid appModuleId, Guid appActionId)
    {
        RoleId = roleId;
        AppModuleId = appModuleId;
        AppActionId = appActionId;
        IsGranted = true;
    }

    public void SetGranted(bool granted)
    {
        IsGranted = granted;
        SetUpdatedAt();
    }
}
