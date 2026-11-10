// src/Microbanker.Domain/Entities/Organization/Department.cs
using Microbanker.Domain.Entities;

namespace Microbanker.Domain.Entities.Organization;

public class Department : BaseEntity
{
    public Guid BranchId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid? ManagerId { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Navigation
    public Branch Branch { get; private set; } = null!;

    private Department() { }

    public Department(Guid branchId, string name, string code)
    {
        BranchId = branchId;
        Name = name;
        Code = code.ToUpper().Trim();
    }

    public void Update(Guid branchId, string name, string code,
        string? description, Guid? managerId, bool isActive)
    {
        BranchId = branchId;
        Name = name;
        Code = code.ToUpper().Trim();
        Description = description;
        ManagerId = managerId;
        IsActive = isActive;
        SetUpdatedAt();
    }

    public void SoftDelete() => SetDeleted();
}
