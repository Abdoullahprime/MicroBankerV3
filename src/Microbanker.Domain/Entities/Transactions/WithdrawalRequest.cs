namespace Microbanker.Domain.Entities.Transactions;

/// <summary>
/// A withdrawal that needs approval before the teller may pay it out (the transaction code is flagged
/// "requires approval"). PENDING → APPROVED → COMPLETED once the teller has paid and the debit is posted.
/// </summary>
public class WithdrawalRequest : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public Guid BranchId { get; private set; }
    public Guid AccountId { get; private set; }
    public Guid TransactionCodeId { get; private set; }
    public decimal Amount { get; private set; }
    public Guid RequestedBy { get; private set; }
    public string? Purpose { get; private set; }
    public string Status { get; private set; } = TransactionOptions.Pending;
    public Guid? ApprovedBy { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public string? ReviewRemarks { get; private set; }
    /// <summary>Reference of the debit transaction once the withdrawal has been paid.</summary>
    public string? CompletedTransactionRef { get; private set; }

    private WithdrawalRequest() { }

    public WithdrawalRequest(Guid orgId, Guid branchId, Guid accountId, Guid transactionCodeId, decimal amount,
        Guid requestedBy, string? purpose)
    {
        if (amount <= 0 || decimal.Round(amount, 2) != amount)
            throw new ArgumentOutOfRangeException(nameof(amount), "The amount must be greater than zero with at most two decimal places.");
        OrganizationId = orgId;
        BranchId = branchId;
        AccountId = accountId;
        TransactionCodeId = transactionCodeId;
        Amount = amount;
        RequestedBy = requestedBy;
        Purpose = purpose;
        Status = TransactionOptions.Pending;
    }

    public void Approve(Guid approvedBy, string? remarks)
    {
        EnsurePending();
        Status = TransactionOptions.Approved;
        ApprovedBy = approvedBy;
        ApprovedAt = DateTime.UtcNow;
        ReviewRemarks = remarks;
        SetUpdatedAt();
    }

    public void Reject(Guid reviewedBy, string reason)
    {
        EnsurePending();
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("A reason is required.", nameof(reason));
        Status = TransactionOptions.Rejected;
        ApprovedBy = reviewedBy;
        ApprovedAt = DateTime.UtcNow;
        ReviewRemarks = reason;
        SetUpdatedAt();
    }

    public void Complete(string transactionRef)
    {
        if (Status != TransactionOptions.Approved)
            throw new InvalidOperationException("Only an approved withdrawal can be paid out.");
        Status = TransactionOptions.Completed;
        CompletedTransactionRef = transactionRef;
        SetUpdatedAt();
    }

    private void EnsurePending()
    {
        if (Status != TransactionOptions.Pending)
            throw new InvalidOperationException("This request has already been reviewed.");
    }
}
