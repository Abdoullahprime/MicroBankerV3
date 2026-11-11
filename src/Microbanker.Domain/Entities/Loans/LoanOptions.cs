namespace Microbanker.Domain.Entities.Loans;

public static class LoanOptions
{
    // Loan status
    public const string Pending    = "PENDING";
    public const string Approved   = "APPROVED";
    public const string Active     = "ACTIVE";
    public const string Closed     = "CLOSED";
    public const string WrittenOff = "WRITTEN_OFF";

    // Request status
    public const string Rejected  = "REJECTED";
    public const string Converted = "CONVERTED";

    // Installment status
    public const string Partial = "PARTIAL";
    public const string Paid    = "PAID";

    public const string Flat     = "FLAT";
    public const string Reducing = "REDUCING";

    public const string Daily     = "DAILY";
    public const string Weekly    = "WEEKLY";
    public const string Monthly   = "MONTHLY";
    public const string Quarterly = "QUARTERLY";

    public const string Cash          = "CASH";
    public const string Deduction     = "DEDUCTION";
    public const string StandingOrder = "STANDING_ORDER";

    public static readonly string[] LoanStatuses       = { Pending, Approved, Active, Closed, WrittenOff };
    public static readonly string[] RequestStatuses    = { Pending, Approved, Rejected, Converted };
    public static readonly string[] CalcMethods        = { Flat, Reducing };
    public static readonly string[] Frequencies        = { Daily, Weekly, Monthly, Quarterly };
    public static readonly string[] RepaymentModes     = { Cash, Deduction, StandingOrder };
}
