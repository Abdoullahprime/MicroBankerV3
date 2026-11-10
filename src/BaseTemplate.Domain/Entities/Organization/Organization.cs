// src/BaseTemplate.Domain/Entities/Organization/Organization.cs
using BaseTemplate.Domain.Entities;

namespace BaseTemplate.Domain.Entities.Organization;

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
    public string? PostalCode { get; private set; }
    public string? GoogleCode { get; private set; }
    public string? MissionStatement { get; private set; }
    public string? VisionStatement { get; private set; }
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
        bool isActive,
        string? postalCode = null, string? googleCode = null,
        string? missionStatement = null, string? visionStatement = null)
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
        PostalCode = postalCode;
        GoogleCode = googleCode;
        MissionStatement = missionStatement;
        VisionStatement = visionStatement;
        SetUpdatedAt();
    }

    public void UpdateLogo(string? logoPath)
    {
        Logo = logoPath;
        SetUpdatedAt();
    }

    public void SoftDelete() => SetDeleted();
}
