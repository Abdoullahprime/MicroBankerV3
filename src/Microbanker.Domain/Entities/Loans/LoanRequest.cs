using Microbanker.Domain.Entities.Customers;
using Microbanker.Domain.Entities.Organization;
using Microbanker.Domain.Entities.Setup;

namespace Microbanker.Domain.Entities.Loans;

/// <summary>A customer's request for a loan, reviewed before a <see cref="LoanAccount"/> is created from it.</summary>
public class LoanRequest : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public Guid BranchId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid ProductDefinitionId { get; private set; }
    public decimal RequestedAmount { get; private set; }
    public string? Purpose { get; private set; }
    /// <summary>PENDING, APPROVED, REJECTED or CONVERTED (a loan was created from it).</summary>
    public string Status { get; private set; } = LoanOptions.Pending;
    public Guid? ReviewedBy { get; private set; }
    public DateTime? ReviewedAt { get; private set; }
    public string? ReviewRemarks { get; private set; }
    public Guid? CreatedByUserId { get; private set; }

    public Customer Customer { get; private set; } = null!;
    public Branch Branch { get; private set; } = null!;
    public ProductDefinition ProductDefinition { get; private set; } = null!;

    private LoanRequest() { }

    public LoanRequest(Guid orgId, Guid branchId, Guid customerId, Guid productDefId, decimal requestedAmount,
        string? purpose, Guid? createdByUserId = null)
    {
        if (requestedAmount <= 0) throw new ArgumentOutOfRangeException(nameof(requestedAmount), "The amount must be greater than zero.");
        OrganizationId = orgId;
        BranchId = branchId;
        CustomerId = customerId;
        ProductDefinitionId = productDefId;
        RequestedAmount = requestedAmount;
        Purpose = purpose;
        CreatedByUserId = createdByUserId;
        Status = LoanOptions.Pending;
    }

    public void Approve(Guid reviewedBy, string? remarks) => Review(LoanOptions.Approved, reviewedBy, remarks);
    public void Reject(Guid reviewedBy, string? remarks) => Review(LoanOptions.Rejected, reviewedBy, remarks);

    public void MarkConverted()
    {
        if (Status != LoanOptions.Approved)
            throw new InvalidOperationException("Only an approved request can be turned into a loan.");
        Status = LoanOptions.Converted;
        SetUpdatedAt();
    }

    /// <summary>The loan created from this request was deleted before approval, so the request can be used again.</summary>
    public void ReopenAfterLoanDeleted()
    {
        if (Status != LoanOptions.Converted) return;
        Status = LoanOptions.Approved;
        SetUpdatedAt();
    }

    public void SoftDelete()
    {
        if (Status == LoanOptions.Converted)
            throw new InvalidOperationException("A request that became a loan cannot be deleted.");
        SetDeleted();
    }

    private void Review(string outcome, Guid reviewedBy, string? remarks)
    {
        if (Status != LoanOptions.Pending)
            throw new InvalidOperationException("This request has already been reviewed.");
        Status = outcome;
        ReviewedBy = reviewedBy;
        ReviewedAt = DateTime.UtcNow;
        ReviewRemarks = remarks;
        SetUpdatedAt();
    }
}
