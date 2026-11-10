// src/BaseTemplate.Domain/Entities/Auth/UserRole.cs
using BaseTemplate.Domain.Entities;

namespace BaseTemplate.Domain.Entities.Auth;

public class UserRole : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }

    // Navigation
    public AppUser User { get; private set; } = null!;
    public Role Role { get; private set; } = null!;

    private UserRole() { }

    public UserRole(Guid userId, Guid roleId)
    {
        UserId = userId;
        RoleId = roleId;
    }
}
