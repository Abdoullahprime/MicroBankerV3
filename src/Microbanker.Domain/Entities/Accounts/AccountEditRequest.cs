namespace Microbanker.Domain.Entities.Accounts;

/// <summary>
/// A request to change an approved account. Once approved it lets the account be edited one time
/// (the edit completes the request).
/// </summary>
public class AccountEditRequest : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public Guid AccountId { get; private set; }
    public Guid RequestedBy { get; private set; }
    public string RequestReason { get; private set; } = string.Empty;
    /// <summary>PENDING, APPROVED, REJECTED or COMPLETED.</summary>
    public string Status { get; private set; } = AccountOptions.Pending;
    public Guid? ReviewedBy { get; private set; }
    public DateTime? ReviewedAt { get; private set; }
    public string? ReviewRemarks { get; private set; }

    public MemberAccount Account { get; private set; } = null!;

    private AccountEditRequest() { }

    public AccountEditRequest(Guid orgId, Guid accountId, Guid requestedBy, string requestReason)
    {
        OrganizationId = orgId;
        AccountId = accountId;
        RequestedBy = requestedBy;
        RequestReason = requestReason;
        Status = AccountOptions.Pending;
    }

    public void Approve(Guid reviewedBy, string? remarks) => Review(AccountOptions.Approved, reviewedBy, remarks);
    public void Reject(Guid reviewedBy, string? remarks) => Review(AccountOptions.Rejected, reviewedBy, remarks);

    public void Complete()
    {
        if (Status != AccountOptions.Approved)
            throw new InvalidOperationException("Only an approved request can be completed.");
        Status = AccountOptions.Completed;
        SetUpdatedAt();
    }

    private void Review(string outcome, Guid reviewedBy, string? remarks)
    {
        if (Status != AccountOptions.Pending)
            throw new InvalidOperationException("This request has already been reviewed.");
        Status = outcome;
        ReviewedBy = reviewedBy;
        ReviewedAt = DateTime.UtcNow;
        ReviewRemarks = remarks;
        SetUpdatedAt();
    }
}
