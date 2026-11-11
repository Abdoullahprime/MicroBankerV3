namespace Microbanker.Domain.Entities.Loans;

/// <summary>A receipt: one repayment received, with how it was split. Installments only hold running totals.</summary>
public class LoanPayment : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public Guid LoanAccountId { get; private set; }
    public DateOnly PaidOn { get; private set; }
    public decimal Amount { get; private set; }
    public decimal PrincipalPaid { get; private set; }
    public decimal InterestPaid { get; private set; }
    public decimal ChargesPaid { get; private set; }
    public string Mode { get; private set; } = LoanOptions.Cash;
    public string? Reference { get; private set; }
    public Guid PostedBy { get; private set; }

    private LoanPayment() { }

    public LoanPayment(Guid orgId, Guid loanAccountId, DateOnly paidOn, decimal principal, decimal interest, decimal charges,
        string mode, string? reference, Guid postedBy)
    {
        OrganizationId = orgId;
        LoanAccountId = loanAccountId;
        PaidOn = paidOn;
        PrincipalPaid = principal;
        InterestPaid = interest;
        ChargesPaid = charges;
        Amount = principal + interest + charges;
        Mode = mode;
        Reference = reference;
        PostedBy = postedBy;
    }
}
