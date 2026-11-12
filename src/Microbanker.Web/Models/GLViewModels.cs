// src/Microbanker.Web/Models/GLViewModels.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Microbanker.Web.Models;

// ─── GENERAL LEDGER ──────────────────────────────────────────────────────────

public class GLBalanceRowViewModel
{
    public string GlAccountNo { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Level { get; set; } = 1;
    public bool IsControlAccount { get; set; }
    public decimal DebitTotal { get; set; }
    public decimal CreditTotal { get; set; }
    /// <summary>Debits less credits. Positive = debit balance, negative = credit balance.</summary>
    public decimal Net => DebitTotal - CreditTotal;
}

public class GLListViewModel
{
    public DateOnly AsOf { get; set; }
    public string? Type { get; set; }
    public bool HideEmpty { get; set; }
    public List<GLBalanceRowViewModel> Rows { get; set; } = new();
    public decimal TotalDebits { get; set; }
    public decimal TotalCredits { get; set; }
    public List<SelectListItem> Types { get; set; } = new();
}

public class GLStatementLineViewModel
{
    public DateOnly ValueDate { get; set; }
    public string JournalRef { get; set; } = string.Empty;
    public string SourceRef { get; set; } = string.Empty;
    public string SourceModule { get; set; } = string.Empty;
    public string Narration { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    /// <summary>Running net balance (debits less credits).</summary>
    public decimal Running { get; set; }
}

public class GLStatementViewModel
{
    [Display(Name = "GL Account")]
    public string? GlAccountNo { get; set; }
    public string? Name { get; set; }
    public string? Type { get; set; }
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public bool Searched { get; set; }
    public bool NotFound { get; set; }
    public decimal OpeningNet { get; set; }
    public decimal ClosingNet { get; set; }
    public decimal TotalDebits { get; set; }
    public decimal TotalCredits { get; set; }
    public bool Truncated { get; set; }
    public List<GLStatementLineViewModel> Lines { get; set; } = new();
    public List<SelectListItem> Accounts { get; set; } = new();
}

// ─── REPORTS ─────────────────────────────────────────────────────────────────

public class TrialBalanceRowViewModel
{
    public string GlAccountNo { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
}

public class TrialBalanceViewModel
{
    public DateOnly AsOf { get; set; }
    /// <summary>Leave out the year-end closing entries, to see income and expenses before they were closed.</summary>
    public bool BeforeClosing { get; set; }
    public List<TrialBalanceRowViewModel> Rows { get; set; } = new();
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public bool InBalance => TotalDebit == TotalCredit;
}

public class AccountStatementLineViewModel
{
    public DateOnly ValueDate { get; set; }
    public string TransactionRef { get; set; } = string.Empty;
    public string Narration { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsReversal { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal Balance { get; set; }
}

public class AccountStatementViewModel
{
    [Display(Name = "Account No.")]
    public string? AccountNo { get; set; }
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public bool Searched { get; set; }
    public bool NotFound { get; set; }

    public string? AccountName { get; set; }
    public string? CustomerName { get; set; }
    public string? Currency { get; set; }
    public string? Status { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
    public decimal TotalDebits { get; set; }
    public decimal TotalCredits { get; set; }
    public List<AccountStatementLineViewModel> Lines { get; set; } = new();
}

public class LoanScheduleReportViewModel
{
    [Display(Name = "Loan No.")]
    public string? LoanNo { get; set; }
    public bool Searched { get; set; }
    public bool NotFound { get; set; }
    public bool IsPreview { get; set; }

    public string? LoanName { get; set; }
    public string? CustomerName { get; set; }
    public string? Currency { get; set; }
    public string? Status { get; set; }
    public decimal LoanAmount { get; set; }
    public decimal InterestRate { get; set; }
    public string? Method { get; set; }
    public List<ScheduleRowViewModel> Rows { get; set; } = new();
    public decimal TotalPrincipal { get; set; }
    public decimal TotalInterest { get; set; }
    public decimal TotalCharges { get; set; }
    public decimal TotalDue { get; set; }
    public decimal TotalPaid { get; set; }
}

// ─── PROCESSING ──────────────────────────────────────────────────────────────

public class EodLogRowViewModel
{
    public string ProcessType { get; set; } = string.Empty;
    public DateOnly ProcessDate { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? RunByName { get; set; }
    public string? Notes { get; set; }
}

public class ProcessingStateViewModel
{
    public bool Initialised { get; set; }
    public DateOnly CurrentDate { get; set; }
    public string Status { get; set; } = "CLOSED";
    public DateTime? LastSODAt { get; set; }
    public DateTime? LastEODAt { get; set; }
    public DateTime? LastEOMAt { get; set; }
    public DateTime? LastEOYAt { get; set; }
    public string? ClosedThroughPeriod { get; set; }
    /// <summary>The month month-end would close next, and whether it can be closed yet.</summary>
    public string? NextEomPeriod { get; set; }
    public bool EomPossible { get; set; }
    public List<EodLogRowViewModel> RecentLogs { get; set; } = new();
    /// <summary>What the last action did, carried across the redirect.</summary>
    public List<string> LastNotes { get; set; } = new();
}

public class SodViewModel : ProcessingStateViewModel
{
    [DataType(DataType.Date)]
    [Display(Name = "First Business Date")]
    public DateOnly? FirstDate { get; set; }
}

public class EoyViewModel : ProcessingStateViewModel
{
    public int? Year { get; set; }
    public string? Problem { get; set; }
    public List<GLBalanceRowViewModel> Lines { get; set; } = new();
    public decimal NetProfit { get; set; }

    [Display(Name = "Retained Earnings Account")]
    public string? RetainedEarningsGlNo { get; set; }
    public List<SelectListItem> EquityAccounts { get; set; } = new();
}
