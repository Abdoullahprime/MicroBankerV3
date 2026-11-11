namespace Microbanker.Domain.Entities.Customers;

/// <summary>Allowed values for the coded customer fields.</summary>
public static class CustomerOptions
{
    public const string Individual = "INDIVIDUAL";
    public const string Business   = "BUSINESS";
    public const string Group      = "GROUP";

    public const string Pending   = "PENDING";
    public const string Active    = "ACTIVE";
    public const string Suspended = "SUSPENDED";
    public const string Closed    = "CLOSED";

    public const string Approved = "APPROVED";
    public const string Rejected = "REJECTED";

    public static readonly string[] CustomerTypes     = { Individual, Business, Group };
    public static readonly string[] CustomerStatuses  = { Pending, Active, Suspended, Closed };
    public static readonly string[] SelfRegStatuses   = { Pending, Approved, Rejected };
    public static readonly string[] Prefixes          = { "Mr", "Mrs", "Ms", "Dr" };
    public static readonly string[] Sexes             = { "M", "F" };
    public static readonly string[] MaritalStatuses   = { "SINGLE", "MARRIED", "DIVORCED", "WIDOWED", "SEPARATED" };
    public static readonly string[] ResidenceStatuses = { "RESIDENT", "NON_RESIDENT" };
}
