namespace Microbanker.Domain.Entities.Transactions;

/// <summary>
/// One leg of a posting, on one account. Posted transactions are never edited or deleted: a mistake is
/// corrected by reversing it, which marks the original REVERSED and adds an offsetting transaction
/// (<see cref="IsReversal"/>) that points back at it.
///
/// <see cref="GLAccountNo"/> is the general-ledger account posted against when the leg has no matching leg
/// on another account (cash for a deposit or withdrawal, for example). It is null when the legs of a batch
/// offset each other.
/// </summary>
public class Transaction : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public Guid BranchId { get; private set; }
    /// <summary>TXN-{yyyyMMdd}-{seq:D6}; never reused.</summary>
    public string TransactionRef { get; private set; } = string.Empty;
    public Guid TransactionCodeId { get; private set; }
    public Guid AccountId { get; private set; }
    public string AccountType { get; private set; } = string.Empty;
    public string DrCr { get; private set; } = TransactionOptions.Debit;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "GMD";
    public string Narration { get; private set; } = string.Empty;
    public DateOnly ValueDate { get; private set; }
    public DateTime PostingDate { get; private set; }
    public Guid PostedBy { get; private set; }
    public string? TellerRef { get; private set; }
    /// <summary>Shared by the legs posted together; null for a single-leg posting.</summary>
    public string? BatchRef { get; private set; }
    public string Status { get; private set; } = TransactionOptions.Posted;
    public bool IsReversal { get; private set; }
    /// <summary>On a reversed original: the reference of its offsetting transaction. On a reversal: the reference of the original.</summary>
    public string? ReversalRef { get; private set; }
    public string? GLAccountNo { get; private set; }
    /// <summary>The account's book balance straight after this transaction, for statements.</summary>
    public decimal BalanceAfter { get; private set; }

    public Guid? ReversedBy { get; private set; }
    public DateTime? ReversedAt { get; private set; }
    public string? ReversalReason { get; private set; }

    private Transaction() { }

    public Transaction(Guid orgId, Guid branchId, string txnRef, Guid txnCodeId, Guid accountId, string accountType,
        string drCr, decimal amount, string narration, DateOnly valueDate, Guid postedBy,
        string currency = "GMD", decimal balanceAfter = 0m, string? tellerRef = null, string? batchRef = null,
        string? glAccountNo = null, DateTime? postingDate = null)
    {
        if (amount <= 0 || decimal.Round(amount, 2) != amount)
            throw new ArgumentOutOfRangeException(nameof(amount), "The amount must be greater than zero with at most two decimal places.");
        if (!TransactionOptions.DrCrValues.Contains(drCr))
            throw new ArgumentOutOfRangeException(nameof(drCr), drCr, "DrCr must be DR or CR.");
        if (!TransactionOptions.AccountKinds.Contains(accountType))
            throw new ArgumentOutOfRangeException(nameof(accountType), accountType, "Unknown account type.");

        OrganizationId = orgId;
        BranchId = branchId;
        TransactionRef = txnRef;
        TransactionCodeId = txnCodeId;
        AccountId = accountId;
        AccountType = accountType;
        DrCr = drCr;
        Amount = amount;
        Currency = currency;
        Narration = narration;
        ValueDate = valueDate;
        PostedBy = postedBy;
        BalanceAfter = balanceAfter;
        TellerRef = tellerRef;
        BatchRef = batchRef;
        GLAccountNo = glAccountNo;
        PostingDate = postingDate ?? DateTime.UtcNow;
        Status = TransactionOptions.Posted;
    }

    /// <summary>Builds the offsetting transaction for a reversal. It is itself a normal, posted transaction.</summary>
    public static Transaction CreateReversalOf(Transaction original, string reversalRef, Guid postedBy, DateOnly valueDate,
        decimal balanceAfter, string? batchRef, string reason, DateTime? postingDate = null)
    {
        var t = new Transaction(original.OrganizationId, original.BranchId, reversalRef, original.TransactionCodeId,
            original.AccountId, original.AccountType,
            original.DrCr == TransactionOptions.Debit ? TransactionOptions.Credit : TransactionOptions.Debit,
            original.Amount, $"Reversal of {original.TransactionRef}: {reason}", valueDate, postedBy,
            original.Currency, balanceAfter, original.TellerRef, batchRef, original.GLAccountNo, postingDate)
        {
            IsReversal = true,
            ReversalRef = original.TransactionRef
        };
        return t;
    }

    /// <summary>Marks this transaction as reversed by the transaction with reference <paramref name="reversalRef"/>.</summary>
    public void Reverse(string reversalRef, Guid reversedBy, string reason)
    {
        if (IsReversal)
            throw new InvalidOperationException("A reversal cannot itself be reversed.");
        if (Status != TransactionOptions.Posted)
            throw new InvalidOperationException("Only a posted transaction can be reversed.");
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("A reason is required.", nameof(reason));

        Status = TransactionOptions.Reversed;
        ReversalRef = reversalRef;
        ReversedBy = reversedBy;
        ReversedAt = DateTime.UtcNow;
        ReversalReason = reason;
        SetUpdatedAt();
    }
}
