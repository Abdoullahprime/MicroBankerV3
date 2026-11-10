// src/Microbanker.Domain/Entities/Organization/Branch.cs
using Microbanker.Domain.Entities;

namespace Microbanker.Domain.Entities.Organization;

public class Branch : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string? Location { get; private set; }
    public string? Address { get; private set; }
    public string? Telephone { get; private set; }
    public string? Phone { get; private set; }
    public string? Fax { get; private set; }
    public string? Email { get; private set; }
    public bool IsHeadOffice { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Branch() { }

    public Branch(Guid organizationId, string name, string code)
    {
        OrganizationId = organizationId;
        Name = name;
        Code = code.ToUpper().Trim();
    }

    public void Update(string name, string code, string? location,
        string? address, string? telephone, string? phone,
        string? fax, string? email, bool isHeadOffice, bool isActive)
    {
        Name = name;
        Code = code.ToUpper().Trim();
        Location = location;
        Address = address;
        Telephone = telephone;
        Phone = phone;
        Fax = fax;
        Email = email;
        IsHeadOffice = isHeadOffice;
        IsActive = isActive;
        SetUpdatedAt();
    }

    public void ClearHeadOffice() { IsHeadOffice = false; SetUpdatedAt(); }
    public void SoftDelete() => SetDeleted();
}
