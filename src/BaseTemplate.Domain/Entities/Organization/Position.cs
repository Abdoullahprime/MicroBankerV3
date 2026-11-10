// src/BaseTemplate.Domain/Entities/Organization/Position.cs
using BaseTemplate.Domain.Entities;

namespace BaseTemplate.Domain.Entities.Organization;

public class Position : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Code { get; private set; }
    public string? Description { get; private set; }
    public int? Level { get; private set; }
    public bool IsActive { get; private set; } = true;


    private Position() { }

    public Position(Guid organizationId, string name)
    {
        OrganizationId = organizationId;
        Name = name;
    }

    public void Update(string name, string? code,
        string? description, int? level, bool isActive)
    {
        Name = name;
        Code = code;
        Description = description;
        Level = level;
        IsActive = isActive;
        SetUpdatedAt();
    }

    public void SoftDelete() => SetDeleted();
}
