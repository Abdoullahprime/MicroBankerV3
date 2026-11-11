namespace Microbanker.Domain.Entities.Customers;

/// <summary>A prospective customer's application, reviewed before a <see cref="Customer"/> is created from it.</summary>
public class SelfRegistration : PersonProfileBase
{
    public string Status { get; private set; } = CustomerOptions.Pending;
    public string? RejectionReason { get; private set; }
    public Guid? ReviewedBy { get; private set; }
    public DateTime? ReviewedAt { get; private set; }
    /// <summary>The customer created when this registration was approved.</summary>
    public Guid? CustomerId { get; private set; }

    private SelfRegistration() { }

    public SelfRegistration(Guid orgId, Guid branchId, string customerType)
    {
        OrganizationId = orgId;
        BranchId = branchId;
        CustomerType = customerType;
        Status = CustomerOptions.Pending;
    }

    public void Update(CustomerProfile profile)
    {
        EnsurePending();
        ApplyProfile(profile);
    }

    public void Approve(Guid reviewedBy, Guid customerId)
    {
        EnsurePending();
        Status = CustomerOptions.Approved;
        ReviewedBy = reviewedBy;
        ReviewedAt = DateTime.UtcNow;
        CustomerId = customerId;
        SetUpdatedAt();
    }

    public void Reject(Guid reviewedBy, string reason)
    {
        EnsurePending();
        Status = CustomerOptions.Rejected;
        RejectionReason = reason;
        ReviewedBy = reviewedBy;
        ReviewedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    public void SoftDelete() => SetDeleted();

    private void EnsurePending()
    {
        if (Status != CustomerOptions.Pending)
            throw new InvalidOperationException("Only a pending registration can be changed.");
    }
}
