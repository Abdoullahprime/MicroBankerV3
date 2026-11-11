namespace Microbanker.Domain.Entities.Customers;

/// <summary>
/// The personal / business / group / contact data captured for a customer or a self-registration.
/// A plain carrier so that <see cref="Customer"/> and <see cref="SelfRegistration"/> keep private setters
/// while still taking one cohesive argument in <c>Update()</c>.
/// </summary>
public sealed class CustomerProfile
{
    public Guid BranchId { get; init; }
    public string CustomerType { get; init; } = CustomerOptions.Individual;

    // Individual
    public string? Prefix { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string? MiddleName { get; init; }
    public string LastName { get; init; } = string.Empty;
    public DateOnly? DateOfBirth { get; init; }
    public string? Sex { get; init; }
    public string? MaritalStatus { get; init; }
    public string? SpouseName { get; init; }
    public int Dependants { get; init; }
    public string? Nationality { get; init; }
    public string? ResidenceStatus { get; init; }
    public string? Language { get; init; }
    public string? Occupation { get; init; }
    public string? EmployerName { get; init; }
    public string? NationalId { get; init; }
    public string? PassportNo { get; init; }

    // Business
    public string? BusinessName { get; init; }
    public DateOnly? BusinessRegDate { get; init; }
    public string? BusinessRegNo { get; init; }
    public string? BusinessTelephone { get; init; }
    public string? NatureOfBusiness { get; init; }
    public string? DirectorName { get; init; }

    // Group
    public string? GroupName { get; init; }
    public int NoOfMembers { get; init; }
    public int NoOfMale { get; init; }
    public int NoOfFemale { get; init; }

    // Contact (all types)
    public string? Telephone { get; init; }
    public string? MobileNo { get; init; }
    public string? Email { get; init; }
    public string? Address1 { get; init; }
    public string? Address2 { get; init; }
    public string? Address3 { get; init; }
    public Guid? RegionId { get; init; }
    public Guid? DistrictId { get; init; }
    public Guid? AreaId { get; init; }
    public Guid? SectorId { get; init; }
    public Guid? SubSectorId { get; init; }
    public string? TinNo { get; init; }
    public string? RelationshipOfficer { get; init; }
}
