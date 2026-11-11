using Microbanker.Domain.Entities.Loans;

namespace Microbanker.Tests;

public class LoanDomainTests
{
    private static LoanTerms Terms(decimal amount = 1200m, decimal rate = 12m, string method = LoanOptions.Flat, int n = 12, decimal charges = 0m) => new()
    {
        ProductDefinitionId = Guid.NewGuid(), LoanName = "Test loan", Currency = "GMD", LoanAmount = amount, InterestRate = rate,
        InterestCalcMethod = method, Tenure = 12, NoOfInstallments = n, RepaymentFrequency = LoanOptions.Monthly,
        RepaymentMode = LoanOptions.Cash, Charges = charges
    };

    private static LoanAccount Pending(LoanTerms? terms = null)
    {
        var l = new LoanAccount(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "LN-2026-000001", 1200m);
        l.UpdateTerms(terms ?? Terms());
        return l;
    }

    private static (LoanAccount Loan, List<LoanRepayment> Schedule) Active(LoanTerms? terms = null)
    {
        var l = Pending(terms);
        l.Approve(Guid.NewGuid());
        var rows = l.Disburse(Guid.NewGuid(), new DateOnly(2026, 1, 1)).ToList();
        return (l, rows);
    }

    private static DateOnly Day => new(2026, 6, 1);

    [Fact]
    public void Life_cycle_runs_pending_approved_active()
    {
        var l = Pending();
        Assert.Equal(LoanOptions.Pending, l.Status);
        l.Approve(Guid.NewGuid());
        Assert.Equal(LoanOptions.Approved, l.Status);
        Assert.True(l.IsApproved);
        Assert.Throws<InvalidOperationException>(() => l.Approve(Guid.NewGuid()));
        l.Disburse(Guid.NewGuid(), new DateOnly(2026, 1, 1));
        Assert.Equal(LoanOptions.Active, l.Status);
        Assert.True(l.IsDisbursed);
    }

    [Fact]
    public void Terms_lock_after_approval_and_cannot_be_invalid()
    {
        var l = Pending();
        Assert.Throws<ArgumentOutOfRangeException>(() => l.UpdateTerms(Terms(amount: 0m)));
        Assert.Throws<ArgumentOutOfRangeException>(() => l.UpdateTerms(Terms(rate: 101m)));
        Assert.Throws<ArgumentOutOfRangeException>(() => l.UpdateTerms(Terms(n: 0)));
        l.Approve(Guid.NewGuid());
        Assert.Throws<InvalidOperationException>(() => l.UpdateTerms(Terms()));
    }

    [Fact]
    public void Only_an_approved_loan_can_be_disbursed()
    {
        Assert.Throws<InvalidOperationException>(() => Pending().Disburse(Guid.NewGuid(), new DateOnly(2026, 1, 1)));
    }

    [Fact]
    public void Disburse_creates_the_schedule_and_sets_the_dates_and_balance()
    {
        var (l, rows) = Active();
        Assert.Equal(12, rows.Count);
        Assert.All(rows, r => Assert.Equal(l.Id, r.LoanAccountId));
        Assert.Equal(1200m, l.OutstandingBalance);
        Assert.Equal(1200m, l.DisbursedAmount);
        Assert.Equal(new DateOnly(2026, 1, 1), l.LoanStartDate);
        Assert.Equal(new DateOnly(2026, 2, 1), l.FirstEMIDate);
        Assert.Equal(rows[^1].DueDate, l.LoanEndDate);
        Assert.Equal(1200m, rows.Sum(r => r.PrincipalDue));
        Assert.Equal(144m, rows.Sum(r => r.InterestDue));       // flat 12% for a year
    }

    [Fact]
    public void The_first_installment_must_fall_after_the_disbursement_date()
    {
        var l = Pending();
        l.Approve(Guid.NewGuid());
        Assert.Throws<InvalidOperationException>(() => l.Disburse(Guid.NewGuid(), new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 1)));
    }

    [Fact]
    public void Preview_does_not_change_the_loan()
    {
        var l = Pending();
        var preview = l.PreviewSchedule(new DateOnly(2026, 1, 1));
        Assert.Equal(12, preview.Count);
        Assert.Equal(LoanOptions.Pending, l.Status);
        Assert.Equal(0m, l.OutstandingBalance);
    }

    // ── Repayment ─────────────────────────────────────────────────────────────

    [Fact]
    public void A_payment_goes_to_charges_then_interest_then_principal_on_the_oldest_installment()
    {
        // 1,200 flat 12% over 12 months + 120 charges: each installment = 100 principal + 12 interest + 10 charges = 122
        var (l, rows) = Active(Terms(charges: 120m));
        var p = l.ApplyPayment(15m, Day, LoanOptions.Cash, "R1", Guid.NewGuid(), rows);

        Assert.Equal(10m, p.ChargesPaid);
        Assert.Equal(5m, p.InterestPaid);
        Assert.Equal(0m, p.PrincipalPaid);
        Assert.Equal(15m, p.Amount);
        Assert.Equal(LoanOptions.Partial, rows[0].Status);
        Assert.Equal(LoanOptions.Pending, rows[1].Status);
        Assert.Equal(1200m, l.OutstandingBalance);                 // no principal paid yet
    }

    [Fact]
    public void A_payment_can_settle_several_installments_and_pay_ahead()
    {
        var (l, rows) = Active();                                  // each installment: 100 + 12
        var p = l.ApplyPayment(250m, Day, LoanOptions.Cash, null, Guid.NewGuid(), rows);

        Assert.Equal(LoanOptions.Paid, rows[0].Status);
        Assert.Equal(LoanOptions.Paid, rows[1].Status);
        Assert.Equal(LoanOptions.Partial, rows[2].Status);         // 250 - 224 = 26 left over: 12 interest + 14 principal
        Assert.Equal(214m, p.PrincipalPaid);
        Assert.Equal(36m, p.InterestPaid);
        Assert.Equal(1200m - 214m, l.OutstandingBalance);
        Assert.Equal(LoanOptions.Active, l.Status);
    }

    [Fact]
    public void Paying_everything_closes_the_loan()
    {
        var (l, rows) = Active();
        l.ApplyPayment(1344m, Day, LoanOptions.Cash, null, Guid.NewGuid(), rows);    // 1200 + 144 interest
        Assert.Equal(LoanOptions.Closed, l.Status);
        Assert.Equal(0m, l.OutstandingBalance);
        Assert.All(rows, r => Assert.Equal(LoanOptions.Paid, r.Status));
    }

    [Fact]
    public void Overpaying_or_bad_amounts_are_rejected_and_change_nothing()
    {
        var (l, rows) = Active();
        Assert.Throws<InvalidOperationException>(() => l.ApplyPayment(1344.01m, Day, LoanOptions.Cash, null, Guid.NewGuid(), rows));
        Assert.Throws<ArgumentOutOfRangeException>(() => l.ApplyPayment(0m, Day, LoanOptions.Cash, null, Guid.NewGuid(), rows));
        Assert.Throws<ArgumentOutOfRangeException>(() => l.ApplyPayment(10.005m, Day, LoanOptions.Cash, null, Guid.NewGuid(), rows));
        Assert.Throws<ArgumentOutOfRangeException>(() => l.ApplyPayment(10m, Day, "BARTER", null, Guid.NewGuid(), rows));
        Assert.Equal(1200m, l.OutstandingBalance);
        Assert.All(rows, r => Assert.Equal(LoanOptions.Pending, r.Status));
    }

    [Fact]
    public void Repayments_need_an_active_loan()
    {
        var l = Pending();
        Assert.Throws<InvalidOperationException>(() => l.ApplyPayment(10m, Day, LoanOptions.Cash, null, Guid.NewGuid(), Array.Empty<LoanRepayment>()));
    }

    [Fact]
    public void Reducing_balance_loans_repay_to_exactly_zero()
    {
        var (l, rows) = Active(Terms(amount: 10000m, rate: 12m, method: LoanOptions.Reducing));
        var total = rows.Sum(r => r.TotalDue);
        l.ApplyPayment(total, Day, LoanOptions.Cash, null, Guid.NewGuid(), rows);
        Assert.Equal(LoanOptions.Closed, l.Status);
        Assert.Equal(0m, l.OutstandingBalance);
    }

    [Fact]
    public void Overdue_is_derived_from_the_due_date_and_status()
    {
        var (_, rows) = Active();
        Assert.True(rows[0].IsOverdueOn(new DateOnly(2026, 2, 2)));
        Assert.False(rows[0].IsOverdueOn(new DateOnly(2026, 2, 1)));
    }

    [Fact]
    public void Only_pending_loans_can_be_deleted_and_every_change_replaces_the_stamp()
    {
        var l = Pending();
        var before = l.ConcurrencyStamp;
        l.Approve(Guid.NewGuid());
        Assert.NotEqual(before, l.ConcurrencyStamp);
        Assert.Throws<InvalidOperationException>(() => l.SoftDelete());
        var p = Pending();
        p.SoftDelete();
        Assert.True(p.IsDeleted);
    }

    // ── Requests ──────────────────────────────────────────────────────────────

    [Fact]
    public void A_loan_request_is_reviewed_once_and_converted_only_when_approved()
    {
        var r = new LoanRequest(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 500m, "Stock");
        Assert.Throws<InvalidOperationException>(() => r.MarkConverted());
        r.Approve(Guid.NewGuid(), null);
        Assert.Throws<InvalidOperationException>(() => r.Reject(Guid.NewGuid(), "no"));
        r.MarkConverted();
        Assert.Equal(LoanOptions.Converted, r.Status);
        Assert.Throws<InvalidOperationException>(() => r.SoftDelete());
        r.ReopenAfterLoanDeleted();
        Assert.Equal(LoanOptions.Approved, r.Status);
        Assert.Throws<ArgumentOutOfRangeException>(() => new LoanRequest(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 0m, null));
    }
}
