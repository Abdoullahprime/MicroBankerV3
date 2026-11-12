namespace Microbanker.Domain.Entities.GeneralLedger;

public static class GLOptions
{
    public const string Debit  = "DR";
    public const string Credit = "CR";

    // GLEntry status
    public const string Posted   = "POSTED";
    public const string Reversed = "REVERSED";

    // Source module
    public const string Teller = "TELLER";
    public const string Loan   = "LOAN";
    public const string System = "SYSTEM";

    // Processing state
    public const string Open       = "OPEN";
    public const string EodRunning = "EOD_RUNNING";
    public const string Closed     = "CLOSED";

    // Process types and log status
    public const string Sod = "SOD";
    public const string Eod = "EOD";
    public const string Eom = "EOM";
    public const string Eoy = "EOY";
    public const string Running   = "RUNNING";
    public const string Completed = "COMPLETED";
    public const string Failed    = "FAILED";

    /// <summary>Journal references of year-end closing entries start with this.</summary>
    public const string ClosingJournalPrefix = "EOY-";

    public static readonly string[] EntryStatuses = { Posted, Reversed };
    public static readonly string[] SourceModules = { Teller, Loan, System };
    public static readonly string[] ProcessingStatuses = { Open, EodRunning, Closed };
    public static readonly string[] ProcessTypes = { Sod, Eod, Eom, Eoy };
    public static readonly string[] LogStatuses = { Running, Completed, Failed };

    /// <summary>The fiscal period of a date: "YYYY-MM". The fiscal year is the calendar year.</summary>
    public static string PeriodOf(DateOnly date) => $"{date.Year:D4}-{date.Month:D2}";

    public static DateOnly PeriodStart(string period) => new(int.Parse(period[..4]), int.Parse(period[5..7]), 1);
    public static DateOnly PeriodEnd(string period) => PeriodStart(period).AddMonths(1).AddDays(-1);
    public static string NextPeriod(string period) => PeriodOf(PeriodStart(period).AddMonths(1));
}
