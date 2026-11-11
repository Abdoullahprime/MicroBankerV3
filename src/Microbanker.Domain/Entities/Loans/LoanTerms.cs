namespace Microbanker.Domain.Entities.Loans;

/// <summary>The editable terms of a loan, passed to <see cref="LoanAccount.UpdateTerms"/> as one unit.</summary>
public sealed class LoanTerms
{
    public Guid ProductDefinitionId { get; init; }
    public string LoanName { get; init; } = string.Empty;
    public string Currency { get; init; } = "GMD";
    public decimal LoanAmount { get; init; }
    /// <summary>Annual rate in percent.</summary>
    public decimal InterestRate { get; init; }
    public string InterestCalcMethod { get; init; } = LoanOptions.Reducing;
    /// <summary>Months before the first repayment falls due.</summary>
    public int GracePeriod { get; init; }
    /// <summary>Loan term in months.</summary>
    public int Tenure { get; init; }
    public int NoOfInstallments { get; init; }
    public string RepaymentFrequency { get; init; } = LoanOptions.Monthly;
    public string RepaymentMode { get; init; } = LoanOptions.Cash;
    public DateOnly? FirstEMIDate { get; init; }
    /// <summary>Total charges, collected in equal parts with the installments.</summary>
    public decimal Charges { get; init; }
    public string? Collateral { get; init; }
    public string? GuarantorsAccountNo { get; init; }
    public string? SettlementAccountNo { get; init; }
    /// <summary>Number of repayment periods to leave out at the start of the schedule.</summary>
    public int SkipInstallments { get; init; }
    public Guid? SectorId { get; init; }
    public Guid? SubSectorId { get; init; }
}
