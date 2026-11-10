// src/Microbanker.Domain/Entities/Organization/Organization.cs
using Microbanker.Domain.Entities;

namespace Microbanker.Domain.Entities.Organization;

/// <summary>The company the system is installed for. Single record per installation.</summary>
public class Organization : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string? ShortName { get; private set; }
    public string? Address { get; private set; }
    public string? Telephone { get; private set; }
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public string? Website { get; private set; }
    public string? RegistrationNumber { get; private set; }
    public string? TaxNumber { get; private set; }
    public string? Industry { get; private set; }
    public DateTime? EstablishedDate { get; private set; }
    public string? Logo { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Organization() { }

    public Organization(string name, string code)
    {
        Name = name;
        Code = code.ToUpper().Trim();
    }

    public void Update(string name, string code, string? shortName,
        string? address, string? telephone, string? phone,
        string? email, string? website, string? registrationNumber,
        string? taxNumber, string? industry, DateTime? establishedDate,
        bool isActive)
    {
        Name = name;
        Code = code.ToUpper().Trim();
        ShortName = shortName;
        Address = address;
        Telephone = telephone;
        Phone = phone;
        Email = email;
        Website = website;
        RegistrationNumber = registrationNumber;
        TaxNumber = taxNumber;
        Industry = industry;
        EstablishedDate = establishedDate;
        IsActive = isActive;
        SetUpdatedAt();
    }

    public void SoftDelete() => SetDeleted();
}
