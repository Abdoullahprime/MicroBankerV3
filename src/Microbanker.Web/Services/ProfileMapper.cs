// src/Microbanker.Web/Services/ProfileMapper.cs
using Microbanker.Domain.Entities.Customers;
using Microbanker.Web.Models;

namespace Microbanker.Web.Services;

/// <summary>Moves customer / self-registration data between form models and domain entities.</summary>
public static class ProfileMapper
{
    private static string? Clean(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    /// <summary>
    /// Builds the domain profile from a posted form. Fields that belong to another customer type are
    /// dropped, so switching the type in the browser can never leave stale data behind.
    /// </summary>
    public static CustomerProfile ToProfile(ProfileFormModel m)
    {
        var t = m.CustomerType;
        return new CustomerProfile
        {
        BranchId = m.BranchId!.Value,
        CustomerType = t,
        Prefix = t == "INDIVIDUAL" ? Clean(m.Prefix) : null,
        FirstName = t == "INDIVIDUAL" ? Clean(m.FirstName) ?? string.Empty : string.Empty,
        MiddleName = t == "INDIVIDUAL" ? Clean(m.MiddleName) : null,
        LastName = t == "INDIVIDUAL" ? Clean(m.LastName) ?? string.Empty : string.Empty,
        DateOfBirth = t == "INDIVIDUAL" ? m.DateOfBirth : null,
        Sex = t == "INDIVIDUAL" ? Clean(m.Sex) : null,
        MaritalStatus = t == "INDIVIDUAL" ? Clean(m.MaritalStatus) : null,
        SpouseName = t == "INDIVIDUAL" ? Clean(m.SpouseName) : null,
        Dependants = t == "INDIVIDUAL" ? m.Dependants : 0,
        Nationality = t == "INDIVIDUAL" ? Clean(m.Nationality) : null,
        ResidenceStatus = t == "INDIVIDUAL" ? Clean(m.ResidenceStatus) : null,
        Language = t == "INDIVIDUAL" ? Clean(m.Language) : null,
        Occupation = t == "INDIVIDUAL" ? Clean(m.Occupation) : null,
        EmployerName = t == "INDIVIDUAL" ? Clean(m.EmployerName) : null,
        NationalId = t == "INDIVIDUAL" ? Clean(m.NationalId) : null,
        PassportNo = t == "INDIVIDUAL" ? Clean(m.PassportNo) : null,
        BusinessName = t == "BUSINESS" ? Clean(m.BusinessName) : null,
        BusinessRegDate = t == "BUSINESS" ? m.BusinessRegDate : null,
        BusinessRegNo = t == "BUSINESS" ? Clean(m.BusinessRegNo) : null,
        BusinessTelephone = t == "BUSINESS" ? Clean(m.BusinessTelephone) : null,
        NatureOfBusiness = t == "BUSINESS" ? Clean(m.NatureOfBusiness) : null,
        DirectorName = t == "BUSINESS" ? Clean(m.DirectorName) : null,
        GroupName = t == "GROUP" ? Clean(m.GroupName) : null,
        NoOfMembers = t == "GROUP" ? m.NoOfMembers : 0,
        NoOfMale = t == "GROUP" ? m.NoOfMale : 0,
        NoOfFemale = t == "GROUP" ? m.NoOfFemale : 0,
        Telephone = Clean(m.Telephone),
        MobileNo = Clean(m.MobileNo),
        Email = Clean(m.Email),
        Address1 = Clean(m.Address1),
        Address2 = Clean(m.Address2),
        Address3 = Clean(m.Address3),
        RegionId = m.RegionId,
        DistrictId = m.DistrictId,
        AreaId = m.AreaId,
        SectorId = m.SectorId,
        SubSectorId = m.SubSectorId,
        TinNo = Clean(m.TinNo),
        RelationshipOfficer = Clean(m.RelationshipOfficer),
        };
    }

    public static void Fill(ProfileFormModel m, PersonProfileBase e)
    {
        m.BranchId = e.BranchId;
        m.CustomerType = e.CustomerType;
        m.Prefix = e.Prefix;
        m.FirstName = e.FirstName;
        m.MiddleName = e.MiddleName;
        m.LastName = e.LastName;
        m.DateOfBirth = e.DateOfBirth;
        m.Sex = e.Sex;
        m.MaritalStatus = e.MaritalStatus;
        m.SpouseName = e.SpouseName;
        m.Dependants = e.Dependants;
        m.Nationality = e.Nationality;
        m.ResidenceStatus = e.ResidenceStatus;
        m.Language = e.Language;
        m.Occupation = e.Occupation;
        m.EmployerName = e.EmployerName;
        m.NationalId = e.NationalId;
        m.PassportNo = e.PassportNo;
        m.BusinessName = e.BusinessName;
        m.BusinessRegDate = e.BusinessRegDate;
        m.BusinessRegNo = e.BusinessRegNo;
        m.BusinessTelephone = e.BusinessTelephone;
        m.NatureOfBusiness = e.NatureOfBusiness;
        m.DirectorName = e.DirectorName;
        m.GroupName = e.GroupName;
        m.NoOfMembers = e.NoOfMembers;
        m.NoOfMale = e.NoOfMale;
        m.NoOfFemale = e.NoOfFemale;
        m.Telephone = e.Telephone;
        m.MobileNo = e.MobileNo;
        m.Email = e.Email;
        m.Address1 = e.Address1;
        m.Address2 = e.Address2;
        m.Address3 = e.Address3;
        m.RegionId = e.RegionId;
        m.DistrictId = e.DistrictId;
        m.AreaId = e.AreaId;
        m.SectorId = e.SectorId;
        m.SubSectorId = e.SubSectorId;
        m.TinNo = e.TinNo;
        m.RelationshipOfficer = e.RelationshipOfficer;
    }
}
