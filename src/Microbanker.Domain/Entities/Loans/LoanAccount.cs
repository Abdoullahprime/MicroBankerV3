using Microbanker.Domain.Entities.Customers;
using Microbanker.Domain.Entities.Organization;
using Microbanker.Domain.Entities.Setup;

namespace Microbanker.Domain.Entities.Loans;

/// <summary>
/// A loan to a customer. Life cycle: PENDING → APPROVED → ACTIVE (disbursed) → CLOSED once every installment is paid.
///
/// <see cref="OutstandingBalance"/> is the principal still owed. Interest and charges still to be paid are the
/// unpaid parts of the installments.
///
/// This class tracks the loan only: it does not move money in member accounts or the general ledger.
/// </summary>
public class LoanAccount : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public Guid BranchId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid ProductDefinitionId { get; private set; }
    public Guid? LoanRequestId { get; private set; }

    /// <summary>LN-{year}-{seq:D6}; never changes once issued.</summary>
    public string LoanNo { get; private set; } = string.Empty;
    public string LoanName { get; private set; } = string.Empty;
    public string Currency { get; private set; } = "GMD";
    public string Status { get; private set; } = LoanOptions.Pending;

    public decimal LoanAmount { get; private set; }
    public decimal OutstandingBalance { get; private set; }
    public decimal DisbursedAmount { get; private set; }
    public decimal InterestRate { get; private set; }
    public string InterestCalcMethod { get; private set; } = LoanOptions.Reducing;
    public int GracePeriod { get; private set; }
    public int Tenure { get; private set; }
    public int NoOfInstallments { get; private set; }
    public string RepaymentFrequency { get; private set; } = LoanOptions.Monthly;
    public string RepaymentMode { get; private set; } = LoanOptions.Cash;
    public DateOnly? LoanStartDate { get; private set; }
    public DateOnly? LoanEndDate { get; private set; }
    public DateOnly? FirstEMIDate { get; private set; }
    public decimal Charges { get; private set; }
    public string? Collateral { get; private set; }
    public string? GuarantorsAccountNo { get; private set; }
    public string? SettlementAccountNo { get; private set; }
    public int SkipInstallments { get; private set; }

    public bool IsApproved { get; private set; }
    public Guid? ApprovedBy { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public bool IsDisbursed { get; private set; }
    public DateTime? DisbursedAt { get; private set; }
    public Guid? DisbursedBy { get; private set; }

    public Guid? SectorId { get; private set; }
    public Guid? SubSectorId { get; private set; }
    public Guid? CreatedByUserId { get; private set; }

    /// <summary>Replaced on every change so two users posting to the same loan cannot overwrite each other.</summary>
    public Guid ConcurrencyStamp { get; private set; } = Guid.NewGuid();

    public Customer Customer { get; private set; } = null!;
    public Branch Branch { get; private set; } = null!;
    public ProductDefinition ProductDefinition { get; private set; } = null!;

    private LoanAccount() { }

    public LoanAccount(Guid orgId, Guid branchId, Guid customerId, Guid productDefId, string loanNo, decimal loanAmount,
        Guid? createdByUserId = null, Guid? loanRequestId = null)
    {
        if (loanAmount <= 0) throw new ArgumentOutOfRangeException(nameof(loanAmount), "The loan amount must be greater than zero.");
        OrganizationId = orgId;
        BranchId = branchId;
        CustomerId = customerId;
        ProductDefinitionId = productDefId;
        LoanNo = loanNo;
        LoanAmount = loanAmount;
        CreatedByUserId = createdByUserId;
        LoanRequestId = loanRequestId;
        Status = LoanOptions.Pending;
    }

    // ── Terms ─────────────────────────────────────────────────────────────────

    /// <summary>Replaces the terms. Only possible while the loan is pending.</summary>
    public void UpdateTerms(LoanTerms t)
    {
        if (Status != LoanOptions.Pending)
            throw new InvalidOperationException("The terms of a loan cannot be changed once it has been approved.");
        ValidateTerms(t);

        ProductDefinitionId = t.ProductDefinitionId;
        LoanName = t.LoanName;
        Currency = t.Currency;
        LoanAmount = t.LoanAmount;
        InterestRate = t.InterestRate;
        InterestCalcMethod = t.InterestCalcMethod;
        GracePeriod = t.GracePeriod;
        Tenure = t.Tenure;
        NoOfInstallments = t.NoOfInstallments;
        RepaymentFrequency = t.RepaymentFrequency;
        RepaymentMode = t.RepaymentMode;
        FirstEMIDate = t.FirstEMIDate;
        Charges = t.Charges;
        Collateral = t.Collateral;
        GuarantorsAccountNo = t.GuarantorsAccountNo;
        SettlementAccountNo = t.SettlementAccountNo;
        SkipInstallments = t.SkipInstallments;
        SectorId = t.SectorId;
        SubSectorId = t.SubSectorId;
        Touch();
    }

    private static void ValidateTerms(LoanTerms t)
    {
        if (t.LoanAmount <= 0) throw new ArgumentOutOfRangeException(nameof(t), "The loan amount must be greater than zero.");
        if (t.InterestRate < 0 || t.InterestRate > 100) throw new ArgumentOutOfRangeException(nameof(t), "The interest rate must be between 0 and 100.");
        if (t.Tenure < 1) throw new ArgumentOutOfRangeException(nameof(t), "The tenure must be at least one month.");
        if (t.NoOfInstallments < 1) throw new ArgumentOutOfRangeException(nameof(t), "There must be at least one installment.");
        if (t.GracePeriod < 0 || t.SkipInstallments < 0) throw new ArgumentOutOfRangeException(nameof(t), "Grace period and skipped installments cannot be negative.");
        if (t.Charges < 0) throw new ArgumentOutOfRangeException(nameof(t), "Charges cannot be negative.");
        if (!LoanOptions.CalcMethods.Contains(t.InterestCalcMethod)) throw new ArgumentOutOfRangeException(nameof(t), "Unknown interest calculation method.");
        if (!LoanOptions.Frequencies.Contains(t.RepaymentFrequency)) throw new ArgumentOutOfRangeException(nameof(t), "Unknown repayment frequency.");
        if (!LoanOptions.RepaymentModes.Contains(t.RepaymentMode)) throw new ArgumentOutOfRangeException(nameof(t), "Unknown repayment mode.");
    }

    // ── Approval and disbursement ─────────────────────────────────────────────

    public void Approve(Guid approvedBy)
    {
        if (Status != LoanOptions.Pending)
            throw new InvalidOperationException("Only a pending loan can be approved.");
        if (NoOfInstallments < 1 || Tenure < 1 || string.IsNullOrWhiteSpace(LoanName))
            throw new InvalidOperationException("The loan terms are incomplete.");
        IsApproved = true;
        ApprovedBy = approvedBy;
        ApprovedAt = DateTime.UtcNow;
        Status = LoanOptions.Approved;
        Touch();
    }

    /// <summary>The schedule this loan would get if disbursed on <paramref name="disbursementDate"/>.</summary>
    public IReadOnlyList<ScheduledInstallment> PreviewSchedule(DateOnly disbursementDate, DateOnly? firstDueOverride = null)
    {
        var first = firstDueOverride ?? FirstEMIDate
            ?? LoanScheduleCalculator.DefaultFirstDueDate(disbursementDate, RepaymentFrequency, GracePeriod, SkipInstallments);
        return LoanScheduleCalculator.Build(LoanAmount, InterestRate, InterestCalcMethod, Tenure, NoOfInstallments,
            RepaymentFrequency, first, Charges);
    }

    /// <summary>
    /// Releases the loan: it becomes ACTIVE, the full amount is owed, and the repayment schedule is created
    /// and returned for the caller to save. Nothing is posted to any member account.
    /// </summary>
    public IReadOnlyList<LoanRepayment> Disburse(Guid disbursedBy, DateOnly disbursementDate, DateOnly? firstDueOverride = null)
    {
        if (Status != LoanOptions.Approved)
            throw new InvalidOperationException("Only an approved loan can be disbursed.");

        var first = firstDueOverride ?? FirstEMIDate
            ?? LoanScheduleCalculator.DefaultFirstDueDate(disbursementDate, RepaymentFrequency, GracePeriod, SkipInstallments);
        if (first <= disbursementDate)
            throw new InvalidOperationException("The first installment must fall after the disbursement date.");

        var schedule = LoanScheduleCalculator.Build(LoanAmount, InterestRate, InterestCalcMethod, Tenure, NoOfInstallments,
            RepaymentFrequency, first, Charges);

        LoanStartDate = disbursementDate;
        FirstEMIDate = first;
        LoanEndDate = schedule[^1].DueDate;
        DisbursedAmount = LoanAmount;
        OutstandingBalance = LoanAmount;
        IsDisbursed = true;
        DisbursedBy = disbursedBy;
        DisbursedAt = DateTime.UtcNow;
        Status = LoanOptions.Active;
        Touch();

        return schedule.Select(s => new LoanRepayment(OrganizationId, Id, s.No, s.DueDate, s.Principal, s.Interest, s.Charges)).ToList();
    }

    // ── Repayment ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Applies a payment to the schedule, oldest installment first. Inside an installment the payment goes to
    /// charges, then interest, then principal. Paying ahead is allowed: money beyond what is due now simply
    /// settles the following installments. The loan closes when everything is paid.
    /// </summary>
    public LoanPayment ApplyPayment(decimal amount, DateOnly paidOn, string mode, string? reference, Guid postedBy,
        IEnumerable<LoanRepayment> installments)
    {
        if (Status != LoanOptions.Active)
            throw new InvalidOperationException("Repayments can only be taken on an active loan.");
        if (amount <= 0 || decimal.Round(amount, 2) != amount)
            throw new ArgumentOutOfRangeException(nameof(amount), "The amount must be greater than zero with at most two decimal places.");
        if (!LoanOptions.RepaymentModes.Contains(mode))
            throw new ArgumentOutOfRangeException(nameof(mode), "Unknown repayment mode.");

        var schedule = installments.OrderBy(i => i.InstallmentNo).ToList();
        var owed = schedule.Sum(i => i.TotalRemaining);
        if (amount > owed)
            throw new InvalidOperationException($"The amount is more than the {owed:N2} still owed on this loan.");

        var left = amount;
        decimal chargesPaid = 0, interestPaid = 0, principalPaid = 0;
        var paidAt = DateTime.UtcNow;

        foreach (var inst in schedule)
        {
            if (left == 0) break;
            if (inst.TotalRemaining == 0) continue;

            var c = Math.Min(left, inst.ChargesRemaining); left -= c;
            var i = Math.Min(left, inst.InterestRemaining); left -= i;
            var p = Math.Min(left, inst.PrincipalRemaining); left -= p;

            inst.Apply(c, i, p, paidAt, postedBy);
            chargesPaid += c; interestPaid += i; principalPaid += p;
        }

        OutstandingBalance -= principalPaid;
        if (schedule.All(i => i.TotalRemaining == 0))
            Status = LoanOptions.Closed;
        Touch();

        return new LoanPayment(OrganizationId, Id, paidOn, principalPaid, interestPaid, chargesPaid, mode, reference, postedBy);
    }

    public void SoftDelete()
    {
        if (Status != LoanOptions.Pending)
            throw new InvalidOperationException("Only a pending loan can be deleted.");
        SetDeleted();
        Touch();
    }

    private void Touch()
    {
        SetUpdatedAt();
        ConcurrencyStamp = Guid.NewGuid();
    }
}
