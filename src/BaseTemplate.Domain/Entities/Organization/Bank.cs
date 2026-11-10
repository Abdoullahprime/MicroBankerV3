// src/BaseTemplate.Domain/Entities/Organization/Bank.cs
using BaseTemplate.Domain.Entities;

namespace BaseTemplate.Domain.Entities.Organization;

public class Bank : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Code { get; private set; }
    public string? BranchCode { get; private set; }
    public string? SwiftCode { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Bank() { }

    public Bank(Guid organizationId, string name, string? code, string? branchCode, string? swiftCode)
    {
        OrganizationId = organizationId;
        Name = name;
        Code = code;
        BranchCode = branchCode;
        SwiftCode = swiftCode;
        IsActive = true;
    }

    public void Update(string name, string? code, string? branchCode, string? swiftCode, bool isActive)
    {
        Name = name;
        Code = code;
        BranchCode = branchCode;
        SwiftCode = swiftCode;
        IsActive = isActive;
        SetUpdatedAt();
    }

    public void SoftDelete() => SetDeleted();
}
