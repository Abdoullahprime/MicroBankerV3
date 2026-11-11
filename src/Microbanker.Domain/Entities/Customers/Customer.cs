namespace Microbanker.Domain.Entities.Customers;

public class Customer : PersonProfileBase
{
    /// <summary>System-generated, CUST-{year}-{seq:D6}. Never reused, even after a soft delete.</summary>
    public string CustomerNo { get; private set; } = string.Empty;
    public string Status { get; private set; } = CustomerOptions.Pending;

    public bool IsApproved { get; private set; }
    public Guid? ApprovedBy { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public string? ApprovalRemarks { get; private set; }

    /// <summary>User who captured the record (null for records created before this was tracked).</summary>
    public Guid? CreatedByUserId { get; private set; }

    private Customer() { }

    public Customer(Guid orgId, Guid branchId, string customerNo, string customerType,
        string firstName, string lastName, Guid? createdByUserId = null)
    {
        OrganizationId = orgId;
        BranchId = branchId;
        CustomerNo = customerNo;
        CustomerType = customerType;
        FirstName = firstName;
        LastName = lastName;
        Status = CustomerOptions.Pending;
        CreatedByUserId = createdByUserId;
    }

    /// <summary>Replaces all editable details. The customer type cannot be changed after creation.</summary>
    public void Update(CustomerProfile profile) => ApplyProfile(profile);

    public void Approve(Guid approvedBy, string? remarks)
    {
        if (Status != CustomerOptions.Pending)
            throw new InvalidOperationException("Only a pending customer can be approved.");
        IsApproved = true;
        ApprovedBy = approvedBy;
        ApprovedAt = DateTime.UtcNow;
        ApprovalRemarks = remarks;
        Status = CustomerOptions.Active;
        SetUpdatedAt();
    }

    public void Suspend()
    {
        if (Status != CustomerOptions.Active)
            throw new InvalidOperationException("Only an active customer can be suspended.");
        Status = CustomerOptions.Suspended;
        SetUpdatedAt();
    }

    public void Reactivate()
    {
        if (Status != CustomerOptions.Suspended)
            throw new InvalidOperationException("Only a suspended customer can be reactivated.");
        Status = CustomerOptions.Active;
        SetUpdatedAt();
    }

    public void SoftDelete() => SetDeleted();
}
