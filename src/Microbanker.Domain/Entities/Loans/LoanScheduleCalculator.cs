namespace Microbanker.Domain.Entities.Loans;

/// <summary>One line of a repayment schedule.</summary>
public sealed record ScheduledInstallment(int No, DateOnly DueDate, decimal Principal, decimal Interest, decimal Charges)
{
    public decimal Total => Principal + Interest + Charges;
}

/// <summary>
/// Builds repayment schedules. Pure arithmetic, no I/O.
/// All amounts are rounded to two places; the last installment absorbs the rounding so that the
/// principal always adds up to exactly the loan amount.
/// </summary>
public static class LoanScheduleCalculator
{
    public static int PeriodsPerYear(string frequency) => frequency switch
    {
        LoanOptions.Daily => 365,
        LoanOptions.Weekly => 52,
        LoanOptions.Monthly => 12,
        LoanOptions.Quarterly => 4,
        _ => throw new ArgumentOutOfRangeException(nameof(frequency), frequency, "Unknown repayment frequency.")
    };

    /// <summary>The due date of installment <paramref name="index"/> (0-based) counted from the first due date.</summary>
    public static DateOnly DueDate(DateOnly firstDue, string frequency, int index) => frequency switch
    {
        LoanOptions.Daily => firstDue.AddDays(index),
        LoanOptions.Weekly => firstDue.AddDays(7 * index),
        // Anchored on the first date, so 31 Jan + 1 month = 28 Feb but + 2 months = 31 Mar.
        LoanOptions.Monthly => firstDue.AddMonths(index),
        LoanOptions.Quarterly => firstDue.AddMonths(3 * index),
        _ => throw new ArgumentOutOfRangeException(nameof(frequency), frequency, "Unknown repayment frequency.")
    };

    /// <summary>
    /// The default first due date: one repayment period after the start, pushed back by the grace period
    /// (months) and by any skipped installments.
    /// </summary>
    public static DateOnly DefaultFirstDueDate(DateOnly start, string frequency, int graceMonths, int skipInstallments)
    {
        var d = start.AddMonths(graceMonths);
        return DueDate(d, frequency, 1 + skipInstallments);
    }

    public static IReadOnlyList<ScheduledInstallment> Build(
        decimal principal, decimal annualRatePercent, string method, int tenureMonths, int installments,
        string frequency, DateOnly firstDueDate, decimal totalCharges)
    {
        if (principal <= 0) throw new ArgumentOutOfRangeException(nameof(principal));
        if (annualRatePercent < 0) throw new ArgumentOutOfRangeException(nameof(annualRatePercent));
        if (installments < 1) throw new ArgumentOutOfRangeException(nameof(installments));
        if (tenureMonths < 1) throw new ArgumentOutOfRangeException(nameof(tenureMonths));
        if (totalCharges < 0) throw new ArgumentOutOfRangeException(nameof(totalCharges));

        var principals = new decimal[installments];
        var interests = new decimal[installments];

        switch (method)
        {
            case LoanOptions.Flat:
            {
                var totalInterest = R(principal * annualRatePercent / 100m * tenureMonths / 12m);
                Spread(principal, principals);
                Spread(totalInterest, interests);
                break;
            }
            case LoanOptions.Reducing:
            {
                var r = annualRatePercent / 100m / PeriodsPerYear(frequency);
                if (r == 0m)
                {
                    Spread(principal, principals);
                    break;
                }
                var emi = R(principal * r / (1m - 1m / Pow(1m + r, installments)));
                var balance = principal;
                for (var i = 0; i < installments; i++)
                {
                    var interest = R(balance * r);
                    var princ = i == installments - 1 ? balance : Math.Min(balance, emi - interest);
                    if (princ < 0) princ = 0;
                    principals[i] = princ;
                    interests[i] = interest;
                    balance -= princ;
                }
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(method), method, "Unknown interest calculation method.");
        }

        var charges = new decimal[installments];
        Spread(totalCharges, charges);

        var list = new List<ScheduledInstallment>(installments);
        for (var i = 0; i < installments; i++)
            list.Add(new ScheduledInstallment(i + 1, DueDate(firstDueDate, frequency, i), principals[i], interests[i], charges[i]));
        return list;
    }

    /// <summary>Splits an amount evenly; the last slot takes whatever rounding leaves over.</summary>
    private static void Spread(decimal total, decimal[] slots)
    {
        var each = R(total / slots.Length);
        var used = 0m;
        for (var i = 0; i < slots.Length - 1; i++)
        {
            slots[i] = each;
            used += each;
        }
        slots[^1] = total - used;
    }

    private static decimal R(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);

    private static decimal Pow(decimal b, int n)
    {
        var result = 1m;
        for (var i = 0; i < n; i++) result *= b;
        return result;
    }
}
