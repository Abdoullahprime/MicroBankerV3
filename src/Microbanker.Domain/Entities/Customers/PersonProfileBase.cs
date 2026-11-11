using Microbanker.Domain.Entities.Organization;
using Microbanker.Domain.Entities.Setup;

namespace Microbanker.Domain.Entities.Customers;

/// <summary>
/// Columns shared by <see cref="Customer"/> and <see cref="SelfRegistration"/>. Not mapped as an entity
/// itself: EF flattens these properties into each derived table.
/// </summary>
public abstract class PersonProfileBase : BaseEntity
{
    public Guid OrganizationId { get; protected set; }
    public string CustomerType { get; protected set; } = CustomerOptions.Individual;

    public Guid BranchId { get; protected set; }
    public string? Prefix { get; protected set; }
    public string FirstName { get; protected set; } = string.Empty;
    public string? MiddleName { get; protected set; }
    public string LastName { get; protected set; } = string.Empty;
    public DateOnly? DateOfBirth { get; protected set; }
    public string? Sex { get; protected set; }
    public string? MaritalStatus { get; protected set; }
    public string? SpouseName { get; protected set; }
    public int Dependants { get; protected set; }
    public string? Nationality { get; protected set; }
    public string? ResidenceStatus { get; protected set; }
    public string? Language { get; protected set; }
    public string? Occupation { get; protected set; }
    public string? EmployerName { get; protected set; }
    public string? NationalId { get; protected set; }
    public string? PassportNo { get; protected set; }
    public string? BusinessName { get; protected set; }
    public DateOnly? BusinessRegDate { get; protected set; }
    public string? BusinessRegNo { get; protected set; }
    public string? BusinessTelephone { get; protected set; }
    public string? NatureOfBusiness { get; protected set; }
    public string? DirectorName { get; protected set; }
    public string? GroupName { get; protected set; }
    public int NoOfMembers { get; protected set; }
    public int NoOfMale { get; protected set; }
    public int NoOfFemale { get; protected set; }
    public string? Telephone { get; protected set; }
    public string? MobileNo { get; protected set; }
    public string? Email { get; protected set; }
    public string? Address1 { get; protected set; }
    public string? Address2 { get; protected set; }
    public string? Address3 { get; protected set; }
    public Guid? RegionId { get; protected set; }
    public Guid? DistrictId { get; protected set; }
    public Guid? AreaId { get; protected set; }
    public Guid? SectorId { get; protected set; }
    public Guid? SubSectorId { get; protected set; }
    public string? TinNo { get; protected set; }
    public string? RelationshipOfficer { get; protected set; }

    public Branch Branch { get; private set; } = null!;
    public Region? Region { get; private set; }
    public District? District { get; private set; }
    public Area? Area { get; private set; }
    public Sector? Sector { get; private set; }
    public SubSector? SubSector { get; private set; }

    /// <summary>Name to show in lists: business or group name for those types, otherwise the person's full name.</summary>
    public string DisplayName => CustomerType switch
    {
        CustomerOptions.Business => BusinessName ?? string.Empty,
        CustomerOptions.Group    => GroupName ?? string.Empty,
        _ => string.Join(' ', new[] { FirstName, MiddleName, LastName }.Where(s => !string.IsNullOrWhiteSpace(s)))
    };

    /// <summary>Applies everything except the customer type, which is fixed at creation.</summary>
    protected void ApplyProfile(CustomerProfile p)
    {
        BranchId = p.BranchId;
        Prefix = p.Prefix;
        FirstName = p.FirstName;
        MiddleName = p.MiddleName;
        LastName = p.LastName;
        DateOfBirth = p.DateOfBirth;
        Sex = p.Sex;
        MaritalStatus = p.MaritalStatus;
        SpouseName = p.SpouseName;
        Dependants = p.Dependants;
        Nationality = p.Nationality;
        ResidenceStatus = p.ResidenceStatus;
        Language = p.Language;
        Occupation = p.Occupation;
        EmployerName = p.EmployerName;
        NationalId = p.NationalId;
        PassportNo = p.PassportNo;
        BusinessName = p.BusinessName;
        BusinessRegDate = p.BusinessRegDate;
        BusinessRegNo = p.BusinessRegNo;
        BusinessTelephone = p.BusinessTelephone;
        NatureOfBusiness = p.NatureOfBusiness;
        DirectorName = p.DirectorName;
        GroupName = p.GroupName;
        NoOfMembers = p.NoOfMembers;
        NoOfMale = p.NoOfMale;
        NoOfFemale = p.NoOfFemale;
        Telephone = p.Telephone;
        MobileNo = p.MobileNo;
        Email = p.Email;
        Address1 = p.Address1;
        Address2 = p.Address2;
        Address3 = p.Address3;
        RegionId = p.RegionId;
        DistrictId = p.DistrictId;
        AreaId = p.AreaId;
        SectorId = p.SectorId;
        SubSectorId = p.SubSectorId;
        TinNo = p.TinNo;
        RelationshipOfficer = p.RelationshipOfficer;
        SetUpdatedAt();
    }

    public CustomerProfile ToProfile() => new()
    {
        CustomerType = CustomerType,
            BranchId = BranchId,
            Prefix = Prefix,
            FirstName = FirstName,
            MiddleName = MiddleName,
            LastName = LastName,
            DateOfBirth = DateOfBirth,
            Sex = Sex,
            MaritalStatus = MaritalStatus,
            SpouseName = SpouseName,
            Dependants = Dependants,
            Nationality = Nationality,
            ResidenceStatus = ResidenceStatus,
            Language = Language,
            Occupation = Occupation,
            EmployerName = EmployerName,
            NationalId = NationalId,
            PassportNo = PassportNo,
            BusinessName = BusinessName,
            BusinessRegDate = BusinessRegDate,
            BusinessRegNo = BusinessRegNo,
            BusinessTelephone = BusinessTelephone,
            NatureOfBusiness = NatureOfBusiness,
            DirectorName = DirectorName,
            GroupName = GroupName,
            NoOfMembers = NoOfMembers,
            NoOfMale = NoOfMale,
            NoOfFemale = NoOfFemale,
            Telephone = Telephone,
            MobileNo = MobileNo,
            Email = Email,
            Address1 = Address1,
            Address2 = Address2,
            Address3 = Address3,
            RegionId = RegionId,
            DistrictId = DistrictId,
            AreaId = AreaId,
            SectorId = SectorId,
            SubSectorId = SubSectorId,
            TinNo = TinNo,
            RelationshipOfficer = RelationshipOfficer,
    };
}
