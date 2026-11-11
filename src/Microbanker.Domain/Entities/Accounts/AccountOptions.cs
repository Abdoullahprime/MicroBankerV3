namespace Microbanker.Domain.Entities.Accounts;

public static class AccountOptions
{
    public const string Savings      = "SAVINGS";
    public const string Current      = "CURRENT";
    public const string FixedDeposit = "FIXED_DEPOSIT";

    public const string Pending = "PENDING";
    public const string Active  = "ACTIVE";
    public const string Dormant = "DORMANT";
    public const string Frozen  = "FROZEN";
    public const string Closed  = "CLOSED";

    public const string Approved  = "APPROVED";
    public const string Rejected  = "REJECTED";
    public const string Completed = "COMPLETED";

    public const string DefaultCurrency = "GMD";

    public static readonly string[] AccountTypes = { Savings, Current, FixedDeposit };
    public static readonly string[] AccountStatuses = { Pending, Active, Dormant, Frozen, Closed };

    /// <summary>Two-letter code used inside the account number.</summary>
    public static string TypePrefix(string accountType) => accountType switch
    {
        Savings => "SA",
        Current => "CA",
        FixedDeposit => "FD",
        _ => throw new ArgumentOutOfRangeException(nameof(accountType), accountType, "Unknown account type.")
    };
}
