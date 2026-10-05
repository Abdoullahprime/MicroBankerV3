// src/BaseTemplate.Domain/Entities/Organization/Unit.cs
using BaseTemplate.Domain.Entities;

namespace BaseTemplate.Domain.Entities.Organization;

public class Unit : BaseEntity
{
    public Guid DepartmentId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Code { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Navigation
    public Department Department { get; private set; } = null!;

    private Unit() { }

    public Unit(Guid departmentId, string name)
    {
        DepartmentId = departmentId;
        Name = name;
    }

    public void Update(Guid departmentId, string name, string? code,
        string? description, bool isActive)
    {
        DepartmentId = departmentId;
        Name = name;
        Code = code;
        Description = description;
        IsActive = isActive;
        SetUpdatedAt();
    }

    public void SoftDelete() => SetDeleted();
}
