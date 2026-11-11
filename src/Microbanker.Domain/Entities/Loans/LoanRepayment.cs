namespace Microbanker.Domain.Entities.Loans;

/// <summary>One installment of a loan's repayment schedule.</summary>
public class LoanRepayment : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public Guid LoanAccountId { get; private set; }
    public int InstallmentNo { get; private set; }
    public DateOnly DueDate { get; private set; }

    public decimal PrincipalDue { get; private set; }
    public decimal InterestDue { get; private set; }
    public decimal ChargesDue { get; private set; }
    public decimal PrincipalPaid { get; private set; }
    public decimal InterestPaid { get; private set; }
    public decimal ChargesPaid { get; private set; }

    public DateTime? PaidAt { get; private set; }
    /// <summary>PENDING, PARTIAL or PAID. Overdue is derived from the due date, not stored.</summary>
    public string Status { get; private set; } = LoanOptions.Pending;
    public Guid? PostedBy { get; private set; }

    public LoanAccount LoanAccount { get; private set; } = null!;

    public decimal PrincipalRemaining => PrincipalDue - PrincipalPaid;
    public decimal InterestRemaining => InterestDue - InterestPaid;
    public decimal ChargesRemaining => ChargesDue - ChargesPaid;
    public decimal TotalDue => PrincipalDue + InterestDue + ChargesDue;
    public decimal TotalRemaining => PrincipalRemaining + InterestRemaining + ChargesRemaining;
    public bool IsOverdueOn(DateOnly today) => Status != LoanOptions.Paid && DueDate < today;

    private LoanRepayment() { }

    public LoanRepayment(Guid orgId, Guid loanAccountId, int installmentNo, DateOnly dueDate,
        decimal principalDue, decimal interestDue, decimal chargesDue)
    {
        OrganizationId = orgId;
        LoanAccountId = loanAccountId;
        InstallmentNo = installmentNo;
        DueDate = dueDate;
        PrincipalDue = principalDue;
        InterestDue = interestDue;
        ChargesDue = chargesDue;
        Status = TotalRemaining == 0 ? LoanOptions.Paid : LoanOptions.Pending;
    }

    /// <summary>Records a payment against this installment. The caller decides the split.</summary>
    public void Apply(decimal charges, decimal interest, decimal principal, DateTime paidAt, Guid postedBy)
    {
        if (charges < 0 || interest < 0 || principal < 0)
            throw new ArgumentOutOfRangeException(nameof(principal), "Payment parts cannot be negative.");
        if (charges > ChargesRemaining || interest > InterestRemaining || principal > PrincipalRemaining)
            throw new InvalidOperationException("The payment is more than what is due on this installment.");

        ChargesPaid += charges;
        InterestPaid += interest;
        PrincipalPaid += principal;
        PaidAt = paidAt;
        PostedBy = postedBy;
        Status = TotalRemaining == 0 ? LoanOptions.Paid : LoanOptions.Partial;
        SetUpdatedAt();
    }
}
