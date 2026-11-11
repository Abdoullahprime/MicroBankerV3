namespace Microbanker.Domain.Entities.Transactions;

public static class TransactionOptions
{
    public const string Debit  = "DR";
    public const string Credit = "CR";

    // Transaction status
    public const string Pending  = "PENDING";
    public const string Posted   = "POSTED";
    public const string Reversed = "REVERSED";

    // Withdrawal request status
    public const string Approved  = "APPROVED";
    public const string Rejected  = "REJECTED";
    public const string Completed = "COMPLETED";

    public const string Loan = "LOAN";

    public static readonly string[] DrCrValues = { Debit, Credit };
    public static readonly string[] TransactionStatuses = { Pending, Posted, Reversed };
    public static readonly string[] AccountKinds = { "SAVINGS", "CURRENT", "FIXED_DEPOSIT", Loan };
    public static readonly string[] WithdrawalStatuses = { Pending, Approved, Rejected, Completed };
}
