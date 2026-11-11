using Microbanker.Domain.Entities.Loans;

namespace Microbanker.Tests;

public class LoanScheduleCalculatorTests
{
    private static readonly DateOnly First = new(2026, 2, 1);

    private static IReadOnlyList<ScheduledInstallment> Build(decimal principal, decimal rate, string method, int tenure, int n,
        string freq = LoanOptions.Monthly, decimal charges = 0m)
        => LoanScheduleCalculator.Build(principal, rate, method, tenure, n, freq, First, charges);

    [Fact]
    public void Flat_interest_is_rate_times_term_spread_evenly()
    {
        var s = Build(12000m, 10m, LoanOptions.Flat, 12, 12);
        Assert.Equal(12, s.Count);
        Assert.Equal(12000m, s.Sum(x => x.Principal));
        Assert.Equal(1200m, s.Sum(x => x.Interest));          // 12000 * 10% * 12/12
        Assert.All(s, x => { Assert.Equal(1000m, x.Principal); Assert.Equal(100m, x.Interest); });
    }

    [Fact]
    public void Flat_interest_scales_with_the_tenure_in_months()
    {
        var s = Build(12000m, 10m, LoanOptions.Flat, 6, 6);
        Assert.Equal(600m, s.Sum(x => x.Interest));            // half a year
    }

    [Fact]
    public void The_last_installment_absorbs_rounding_so_principal_adds_up_exactly()
    {
        var s = Build(1000m, 0m, LoanOptions.Flat, 3, 3);
        Assert.Equal(new[] { 333.33m, 333.33m, 333.34m }, s.Select(x => x.Principal).ToArray());
        Assert.Equal(1000m, s.Sum(x => x.Principal));
    }

    [Fact]
    public void Reducing_balance_matches_the_standard_emi_and_clears_the_loan()
    {
        var s = Build(10000m, 12m, LoanOptions.Reducing, 12, 12);
        Assert.Equal(100.00m, s[0].Interest);                   // 1% of 10,000
        Assert.Equal(888.49m, s[0].Principal + s[0].Interest);  // standard EMI for 10,000 at 12% over 12 months
        Assert.Equal(10000m, s.Sum(x => x.Principal));          // exactly repaid
        Assert.InRange(s.Sum(x => x.Interest), 661.0m, 663.0m);
        for (var i = 1; i < s.Count; i++) Assert.True(s[i].Interest < s[i - 1].Interest);   // interest falls as the balance falls
        Assert.All(s, x => Assert.True(x.Principal >= 0));
    }

    [Fact]
    public void Reducing_balance_at_zero_rate_is_equal_principal_and_no_interest()
    {
        var s = Build(1200m, 0m, LoanOptions.Reducing, 12, 12);
        Assert.All(s, x => { Assert.Equal(100m, x.Principal); Assert.Equal(0m, x.Interest); });
    }

    [Fact]
    public void Charges_are_spread_across_the_installments()
    {
        var s = Build(1000m, 0m, LoanOptions.Flat, 3, 3, charges: 100m);
        Assert.Equal(100m, s.Sum(x => x.Charges));
        Assert.Equal(33.33m, s[0].Charges);
        Assert.Equal(33.34m, s[2].Charges);
        Assert.Equal(1100m, s.Sum(x => x.Total));
    }

    [Fact]
    public void Monthly_dates_stay_anchored_to_the_first_date()
    {
        var s = LoanScheduleCalculator.Build(300m, 0m, LoanOptions.Flat, 3, 3, LoanOptions.Monthly, new DateOnly(2026, 1, 31), 0m);
        Assert.Equal(new[] { new DateOnly(2026, 1, 31), new DateOnly(2026, 2, 28), new DateOnly(2026, 3, 31) },
            s.Select(x => x.DueDate).ToArray());
    }

    [Theory]
    [InlineData(LoanOptions.Daily, 3, "2026-02-04")]
    [InlineData(LoanOptions.Weekly, 3, "2026-02-22")]
    [InlineData(LoanOptions.Monthly, 3, "2026-05-01")]
    [InlineData(LoanOptions.Quarterly, 3, "2026-11-01")]
    public void Due_dates_step_by_the_frequency(string frequency, int index, string expected)
        => Assert.Equal(DateOnly.Parse(expected), LoanScheduleCalculator.DueDate(First, frequency, index));

    [Fact]
    public void Default_first_due_date_adds_grace_months_and_skipped_periods()
    {
        var start = new DateOnly(2026, 1, 15);
        Assert.Equal(new DateOnly(2026, 2, 15), LoanScheduleCalculator.DefaultFirstDueDate(start, LoanOptions.Monthly, 0, 0));
        Assert.Equal(new DateOnly(2026, 5, 15), LoanScheduleCalculator.DefaultFirstDueDate(start, LoanOptions.Monthly, 2, 1));
        Assert.Equal(new DateOnly(2026, 1, 22), LoanScheduleCalculator.DefaultFirstDueDate(start, LoanOptions.Weekly, 0, 0));
    }

    [Fact]
    public void Invalid_inputs_are_rejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Build(0m, 10m, LoanOptions.Flat, 12, 12));
        Assert.Throws<ArgumentOutOfRangeException>(() => Build(100m, -1m, LoanOptions.Flat, 12, 12));
        Assert.Throws<ArgumentOutOfRangeException>(() => Build(100m, 10m, LoanOptions.Flat, 12, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => Build(100m, 10m, "BULLET", 12, 12));
        Assert.Throws<ArgumentOutOfRangeException>(() => Build(100m, 10m, LoanOptions.Flat, 12, 12, "YEARLY"));
    }
}
